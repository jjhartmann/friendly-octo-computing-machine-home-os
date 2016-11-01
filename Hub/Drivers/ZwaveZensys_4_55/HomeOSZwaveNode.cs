using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeOS.Hub.Platform.Views;
using HomeOS.Hub.Common;
using Zensys.ZWave;
using Zensys.ZWave.Devices;
using Zensys.ZWave.Enums;
using Zensys.ZWave.Events;
using Zensys.ZWave.Application;

namespace HomeOS.Hub.Drivers.ZwaveZensys
{
    public class HomeOSZwaveNode
    {
        private IDeviceInfo deviceInfo;
        private string deviceType;
        private byte[] supportedCommandClasses;        
        private Port port = null;
        private DriverZwaveZensys driver;
        private VLogger logger;

        private Dictionary<byte, HomeOSCommandClass> cmdClasses = new Dictionary<byte, HomeOSCommandClass>();
        private Dictionary<string, HomeOSCommandClass> roles = new Dictionary<string, HomeOSCommandClass>();

        public HomeOSZwaveNode(IDeviceInfo info, string deviceType, byte[] supportedClasses, Port port, DriverZwaveZensys driver, VLogger logger)
        {
            this.deviceInfo = info;
            this.deviceType = deviceType;
            this.supportedCommandClasses = supportedClasses;

            this.port = port;
            this.driver = driver;
            this.logger = logger;

            HomeOSCommandClass basicTarget = null;

            //first add command classes based on generic device type
            if (deviceInfo.Generic == driver.XmlDataManager.FindGenericDevice("GENERIC_TYPE_SWITCH_BINARY").KeyId)
            {
                basicTarget = new SwitchBinary(this, logger);
                cmdClasses.Add(driver.XmlDataManager.GetCommandClassKey("COMMAND_CLASS_SWITCH_BINARY"), basicTarget);
            }
            else if (deviceInfo.Generic == driver.XmlDataManager.FindGenericDevice("GENERIC_TYPE_SWITCH_MULTILEVEL").KeyId)
            {
                basicTarget = new SwitchMultiLevel(this, logger);
                cmdClasses.Add(driver.XmlDataManager.GetCommandClassKey("COMMAND_CLASS_SWITCH_MULTILEVEL"), basicTarget);                              
            }
            else if (deviceInfo.Generic == driver.XmlDataManager.FindGenericDevice("GENERIC_TYPE_SENSOR_BINARY").KeyId)
            {
                basicTarget = new SensorBinary(this, logger);
                cmdClasses.Add(driver.XmlDataManager.GetCommandClassKey("COMMAND_CLASS_SENSOR_BINARY"), basicTarget);
            }
            else
            {
                logger.Log("Do not know how to use zwave node {0} of generic type {1}", deviceInfo.Id.ToString(), deviceInfo.Generic.ToString());
            }

            //stitch in the target for the basic command class
            if (basicTarget != null && 
                !cmdClasses.ContainsKey(driver.XmlDataManager.GetCommandClassKey("COMMAND_CLASS_BASIC")))
            {
                cmdClasses.Add(driver.XmlDataManager.GetCommandClassKey("COMMAND_CLASS_BASIC"), basicTarget);
            }

            //now add command classes based on supported command classes
            foreach (byte cc in supportedCommandClasses)
            {
                if (cmdClasses.ContainsKey(cc))
                    continue;

                if (cc == driver.XmlDataManager.GetCommandClassKey("COMMAND_CLASS_SWITCH_BINARY"))
                {
                    cmdClasses.Add(cc, new SwitchBinary(this, logger));
                }
                else if (cc == driver.XmlDataManager.GetCommandClassKey("COMMAND_CLASS_SWITCH_MULTILEVEL"))
                {
                    cmdClasses.Add(cc, new SwitchMultiLevel(this, logger));
                }
                else if (cc == driver.XmlDataManager.GetCommandClassKey("COMMAND_CLASS_SENSOR_BINARY"))
                {
                    cmdClasses.Add(cc, new SensorBinary(this, logger));
                }
                else if (cc == driver.XmlDataManager.GetCommandClassKey("COMMAND_CLASS_SENSOR_MULTILEVEL"))
                {
                    cmdClasses.Add(cc, new SensorMultiLevel(this, logger));
                }
                else if (cc == driver.XmlDataManager.GetCommandClassKey("COMMAND_CLASS_BATTERY"))
                {
                    cmdClasses.Add(cc, new BatteryLevel(this, logger));
                }
                else
                {
                    logger.Log("Do not know how to use command class {0}", string.Join(" ", driver.XmlDataManager.FindCommandClasses(cc).Select(i => i.ToString())));
                }
            }

            List<VRole> roleList = new List<VRole>();

            //we use distinct in case two keys point to the same class (as it may for basic)
            foreach (var cmdClass in cmdClasses.Values.Distinct())
            {
                foreach (var role in cmdClass.GetRoleList())
                {
                    if (roles.ContainsKey(role.Name()))
                    {
                        logger.Log("Suppressing duplicate role {0} in a port by {1}", role.Name(), cmdClass.GetType().ToString());
                    }

                    roleList.Add(role);

                    roles.Add(role.Name(), cmdClass);
                }
            }

            driver.BindRolesHelper(port, roleList);

            foreach (VRole role in roleList)
            {
                foreach (Operation operation in role.GetOperations())

                    port.SetOperationDelegate(role.Name(), operation.Name(), roles[role.Name()].OnOperationInvoke);
            }
        }

        public void Init()
        {
            foreach (var cmdClassKey in cmdClasses.Keys)
            {
                //send Init() to everyone but basic command class, which we doubly added
                if (!cmdClassKey.Equals(driver.XmlDataManager.GetCommandClassKey("COMMAND_CLASS_BASIC")))
                    cmdClasses[cmdClassKey].Init();
            }
        }

        public void ProcessCommandFromDevice(CommandClass cmdClass, Command command, CommandClassValue[] values)
        {
            if (cmdClasses.ContainsKey(cmdClass.KeyId))
                cmdClasses[cmdClass.KeyId].ProcessCommandFromDevice(values);
            else
                logger.Log("Got command for unhandled command class {0}", cmdClass.Name);
        }

        public void ProcessUnsolicitedFrame(UnsolititedFrameReceivedArgs args)
        {
            foreach (var cmdClassKey in cmdClasses.Keys)
            {
                //send ProcessUnsolicitedFrame to everyone but basic command class, which we doubly added
                if (!cmdClassKey.Equals(driver.XmlDataManager.GetCommandClassKey("COMMAND_CLASS_BASIC")))
                       cmdClasses[cmdClassKey].ProcessUnsolicitedFrame(args);
            }
        }

        public IDeviceInfo DeviceInfo
        {
            get { return deviceInfo; }
        }

        public string DeviceType
        {
            get { return deviceType; }
        }

        public DriverZwaveZensys Driver
        {
            get { return driver; }
        }

        public Port Port
        {
            get { return port; }
        }

   }
}
