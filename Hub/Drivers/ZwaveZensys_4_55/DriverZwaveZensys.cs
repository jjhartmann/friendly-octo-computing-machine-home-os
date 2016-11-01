using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;
using System.IO;
using System.Configuration;
using HomeOS.Hub.Drivers.ZwaveZensys.Configuration;

namespace HomeOS.Hub.Drivers.ZwaveZensys
{

    using Zensys.ZWave;
    using Zensys.ZWave.Devices;
    using Zensys.ZWave.Enums;
    using Zensys.ZWave.Events;
    using Zensys.ZWave.Application;

    [System.AddIn.AddIn("HomeOS.Hub.Drivers.ZwaveZensys_4_55")]
    public class DriverZwaveZensys : Common.ModuleBase
    {
        private static string mXmlZWaveDefinitionFile = @"ZWave_custom_cmd_classes.xml";
        private XmlDataManager mXmlDataManager;
        public XmlDataManager XmlDataManager
        {
            get { return mXmlDataManager; }
            set { mXmlDataManager = value; }
        }

        private IController controller;
        byte controllerId = 1;

        Queue<OutboundRequest> outboundRequests = new Queue<OutboundRequest>();

        private Dictionary<byte, HomeOSZwaveNode> zwaveNodes = new Dictionary<byte, HomeOSZwaveNode>();

        List<byte> excludedNodeIds = new List<byte>();

        bool foundController = false;

        UsbNotifier usbNotifier = null;
        SafeThread zwaveIniter = null;

        private WebFileServer imageServer;

        private string deviceMemoryFile;

        public DeviceSettingsSection DriverConfiguration {get; private set;}

        public override void Start()
        {
            //unsuccessful attempts to not having to chagne the signature of the init function
            //AppDomain.CurrentDomain.AppendPrivatePath(Globals.AddInRoot + "\\AddIns\\DriverZwaveZensys\\");
            //logger.Log("1: {0}", AppDomain.CurrentDomain.SetupInformation.PrivateBinPath);
            //AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = AppDomain.CurrentDomain.BaseDirectory;
            //logger.Log("2: {0}", AppDomain.CurrentDomain.SetupInformation.PrivateBinPath);
            //logger.Log("3: {0}", AppDomain.CurrentDomain.SetupInformation.PrivateBinPathProbe);


            //exclude the nodes that we don't want to manage

            DriverConfiguration = Configuration.ConfigurationHelper.GetCurrentComponentConfiguration();

            PrintConfiguration();

            foreach (string nodeId in moduleInfo.Args())
            {
                excludedNodeIds.Add(byte.Parse(nodeId));
            }

            XmlDataManager = new XmlDataManager();

            string fullPathDefinitionFile = moduleInfo.BinaryDir() + "\\" + mXmlZWaveDefinitionFile;

            if (!File.Exists(fullPathDefinitionFile))
            {
                logger.Log("ZwaveDriver: ERROR: cannot find the zwave definitions file at " + fullPathDefinitionFile);
                return;
            }

            DefinitionConverter dconv = new DefinitionConverter(fullPathDefinitionFile, null);
            dconv.UpgradeConvert(false);
            XmlDataManager.ZWaveDefinition = dconv.ZWaveDefinition;

            Directory.CreateDirectory(moduleInfo.WorkingDir());

            deviceMemoryFile = moduleInfo.WorkingDir() + "\\" + "devicemem.txt";

            imageServer = new WebFileServer(moduleInfo.BinaryDir(), moduleInfo.BaseURL(), logger);

            zwaveIniter = new SafeThread(delegate()
            {
                Init();
            }, "zwaveiniter", logger);
            zwaveIniter.Start();

        }

        private void Init()
        {
            if (FindController())
            {
                InitController();

                #region //code for generating fake notifications -- should normally be commented out
                //while (true)
                //{
                //    foreach (ZwaveNode node in zwaveNodes.Values)
                //    {
                //        if (node.deviceInfo.Generic == GenericDevices.Instance.GENERIC_TYPE_SENSOR_BINARY.Key)
                //        {
                //            IList<VParamType> retVals = new List<VParamType>();
                //            retVals.Add(new ParamType(ParamType.SimpleType.integer, "8", (byte) 255, "value"));
                //            node.port.Notify(RoleSensor.RoleName, RoleSensor.OpGetName, retVals);
                //            logger.Log("Issued fake notification for " + node.port.ToString());
                //        }
                //    }
                //    System.Threading.Thread.Sleep(20 * 1000);
                //}
                #endregion
            }
            else
            {
                //the controller wasn't found
                //we add a notification for when USB devices are inserted and look again
                usbNotifier = new UsbNotifier(logger);
                usbNotifier.AddInsertUSBHandler(UsbInserted);

                //we check periodically if the controller has been found and then remove the event handler
                //removing the event handler from within the handler was leading to the Stop() call never returning

                while (true)
                {
                    if (foundController)
                    {
                        usbNotifier.DeleteInsertUSBHandler();
                        break;
                    }

                    System.Threading.Thread.Sleep(10 * 1000);
                }
            }
        }


        private void PrintConfiguration()
        {
            foreach (DeviceElement device in DriverConfiguration.Devices)
            {
                logger.Log("ZwaveDriver:device = {0}", device.Name);

                foreach (var setting in device.DeviceSettings)
                {
                    logger.Log("   setting = {0}", setting.ToString());
                }

                foreach (var role in device.RoleList)
                {
                    logger.Log("   role = {0}", role.ToString());
                }
            }
        }

        private bool FindController()
        {
            //initialize the manager
            ZWaveManager zwaveManager = new ZWaveManager();
            zwaveManager.Init(LibraryModes.PcController, true, moduleInfo.BinaryDir() + "\\");
            controller = zwaveManager.ApplicationLayer.CreateController();

            //get the list of serial interfaces
            List<Zensys.Framework.Win32PnPEntityClass>
            interfaces = Zensys.Framework.ComputerSystemHardwareHelper.GetWin32PnPEntityClassSerialPortDevices();

            foreach (Zensys.Framework.Win32PnPEntityClass serialPortInfo in interfaces)
            {
                if (serialPortInfo.Caption.StartsWith("USB Serial Port"))
                {
                    logger.Log("ZwaveDriver: skipping {0} while search for zwave controller", serialPortInfo.Caption);
                    continue;
                }

                try
                {
                    controller.Open(serialPortInfo.DeviceID);
                    controller.GetVersion();
                    Libraries deviceLibrary = controller.Version.Library;

                    logger.Log("ZwaveDriver: found controller at {0} with lib {1}", serialPortInfo.Caption, deviceLibrary.ToString());

                    foundController = true;

                    break;
                }
                catch (Exception)
                {
                    logger.Log("ZwaveDriver: couldn't find controller at {0}", serialPortInfo.Caption);
                    controller.Close();
                }
            }

            return foundController;
        }

        public IDeviceInfo AddNodeAction(IController controller)
        {
            IDeviceInfo addedNode = null;

            try
            {
                if (controller.ConnectionStatus == ConnectionStatuses.Closed)
                {
                    controller.Open(controller.SerialPort);
                }

                addedNode = controller.AddNodeToNetwork();
                if (addedNode != null)
                {
                    IDeviceInfo _addedNode = controller.GetProtocolInfo(addedNode.Id, false);

                    addedNode.Capability = _addedNode.Capability;
                    addedNode.Security = _addedNode.Security;
                    addedNode.Reserved = _addedNode.Reserved;

                    if (addedNode.Generic == XmlDataManager.FindGenericDevice("GENERIC_TYPE_SENSOR_BINARY").KeyId ||
                         addedNode.Generic == XmlDataManager.FindGenericDevice("GENERIC_TYPE_SWITCH_BINARY").KeyId ||
                       addedNode.Generic == XmlDataManager.FindGenericDevice("GENERIC_TYPE_SWITCH_MULTILEVEL").KeyId)
                    {
                        addedNode.CommandQueueOverrided = true;

                        byte[] nodeIds = new byte[] { controller.Id };

                        MakeAssociation(addedNode.Id);

                    }

                }
            }
            catch (Exception)
            {
                //mExceptionManager.RegisterException(ex);
            }

            return addedNode;
        }

        private void InitController()
        {
            controller.ApplicationCommandHandlerEvent += new DeviceAppCommandHandlerEventHandler(ApplicationCommandHandlerEvent);
            controller.UnsolititedFrameReceived += new UnsolititedFrameReceivedEventHandler(UnsolititedFrameReceived);

            //controller.NodeStatusChanged += new NodeStatusChangedEventHandler(NodeStatusChanged);
            //controller.ApplicationSlaveCommandHandlerEvent += new DeviceAppSlaveCommandHandlerEventHandler(ApplicationSlaveCommandHandlerEvent);
            //controller.ControllerUpdated += new ControllerUpdatedEventHandler(ControllerUpdated);

            //AddNodeAction(controller);

            //these commands are copied from SetUpController in CommonActions.cs
            controller.CommandClassesStore.Load();
            controller.GetNodes();
            controller.GetCapabilities();
            controller.GetControllerCapabilities();
            controller.Memory.GetId();
            controller.GetSUCNodeID();

            List<IDeviceInfo> devicesToInit = new List<IDeviceInfo>();

            List<Zensys.ZWave.Devices.IDeviceInfo> nodeList = controller.GetNodes();
            foreach (IDeviceInfo nodeInfo in nodeList)
            {

                if (nodeInfo.Generic == XmlDataManager.FindGenericDevice("GENERIC_TYPE_STATIC_CONTROLLER").KeyId)
                {
                    controllerId = nodeInfo.Id;
                    logger.Log("ZwaveDriver: Setting Zwave controller Id to " + nodeInfo.Id);
                    continue;
                }

                if (excludedNodeIds.Contains(nodeInfo.Id))
                    continue;

                //this should not be there under normal circumstances
                //if (nodeInfo.Id < 30)
                //    continue;

                IDeviceInfo deviceInfo = controller.RequestNodeInfo(nodeInfo.Id);

                if (deviceInfo != null)
                    devicesToInit.Add(deviceInfo);
                else
                    devicesToInit.Add(nodeInfo);
            }

            foreach (IDeviceInfo deviceInfo in devicesToInit)
            {
                InitDevice(deviceInfo);
            }

            
    //while (true)
            //{
            //    byte nodeId = 9;
            //    byte paramNumber = 1;

            //    GetConfiguration(nodeId, paramNumber);

            //    System.Threading.Thread.Sleep(30 * 1000);

            //    byte[] configValues = new byte[] { 1 };
            //    SetConfiguration(nodeId, paramNumber, configValues);

            //    System.Threading.Thread.Sleep(30 * 1000);

            //}
        }

        public override object OpaqueCall(string callName, params object[] args)
        {

            switch (callName)
            {
                case "AddDevice":
                    if (args.Count() > 0)
                        return AddDevice((string)args[0]);
                    else
                        return AddDevice("unknown"); 
                case "AbortAddDevice":
                    return AbortAddDevice();
                case "RemoveDevice":
                    return RemoveDevice();
                case "AbortRemoveDevice":
                    return AbortRemoveDevice();
                case "Reset":
                    return ResetController();
                case "RemoveFailedNode":
                    return RemoveFailedNode(byte.Parse(args[0].ToString()));
                default:
                    return "Unknown callname";
            }
        }

        private string ResetController()
        {

            throw new NotImplementedException("ResetController is not implemented");

            //if (controller.ConnectionStatus != ConnectionStatuses.Opened)
            //{
            //    return "zwave controller is not attached or is in strange state: " + controller.ConnectionStatus;
            //}

            //the controller SDK does not implement this
            //controller.Reset();

            //TODO: we could call RemoveFailedNode on all devices

            // return "Done";
        }

        private string RemoveFailedNode(byte nodeId)
        {
            if (controller.ConnectionStatus != ConnectionStatuses.Opened)
            {
                return "zwave controller is not attached or is in strange state: " + controller.ConnectionStatus;
            }

            int numRemainingTries = 3;
            bool success = false;

            while (numRemainingTries > 0)
            {
                try
                {
                    FailedNodeStatus status = controller.RemoveFailedNodeID(nodeId);

                    success = true;

                    break;
                }
                catch (Exception e)
                {
                    numRemainingTries--;

                    logger.Log("ZwaveDriver: Exception in removing {0}: {1}. NumRemainingTries = {2}", nodeId.ToString(), e.Message, numRemainingTries.ToString());
                }

            }

            return success.ToString();

        }

        private string AddDevice(string deviceType)
        {
            if (controller.ConnectionStatus != ConnectionStatuses.Opened)
            {
                return "zwave controller is not attached or is in strange state: " + controller.ConnectionStatus;
            }

            int maxtries = 1;

            string result = "Device not found";

            while (maxtries > 0)
            {
                try
                {
                    IDeviceInfo deviceInfo = controller.AddNodeToNetwork(Mode.NodeAny);

                    if (deviceInfo != null)
                    {
                        ConfigureDevice(deviceInfo, deviceType);

                        //we must remember device before calling InitDevice, as InitDevice reads from the memory file
                        RememberDevice(deviceInfo, deviceType);

                        InitDevice(deviceInfo);

                        result = GetNameFromId(deviceInfo.Id);

                        break;

                    }
                }
                catch (Exception e)
                {
                    logger.Log("ZwaveDriver: Got exception while finding device: " + e.ToString());
                }

                maxtries--;
            }

            return result;
        }

        private void RememberDevice(IDeviceInfo deviceInfo, string deviceType)
        {
            lock (this)
            {
                var deviceMemory = File.AppendText(deviceMemoryFile);

                string ccString = string.Join(" ", deviceInfo.SupportedCommandClasses.Select(i => i.ToString()));
                deviceMemory.WriteLine("{0}::{1}::{2}", deviceInfo.Id, deviceType, ccString);

                deviceMemory.Close();
            }
        }

        private void ForgetDevice(byte deviceId)
        {
            lock (this)
            {
                if (!File.Exists(deviceMemoryFile))
                {
                    return;
                }

                var deviceMemoryReader = File.OpenText(deviceMemoryFile);

                List<string> linesToRemember = new List<string>();

                while (deviceMemoryReader.Peek() >= 0)
                {
                    string line = deviceMemoryReader.ReadLine();

                    string[] words = line.Split(new string[] {"::"}, StringSplitOptions.None);

                    int devId = int.Parse(words[0]);

                    if (devId != deviceId)
                    {
                        linesToRemember.Add(line);
                    }
                }

                deviceMemoryReader.Close();

                var deviceMemoryWriter = File.CreateText(deviceMemoryFile);

                linesToRemember.ForEach(line => deviceMemoryWriter.WriteLine(line));

                deviceMemoryWriter.Close();
            }
        }

        private Tuple<string, byte[]> GetDeviceDetailsFromMemory(byte deviceId)
        {
            lock (this)
            {
                if (!File.Exists(deviceMemoryFile))
                {
                    return null;
                }

                var deviceMemoryReader = File.OpenText(deviceMemoryFile);

                while (deviceMemoryReader.Peek() >= 0)
                {
                    string line = deviceMemoryReader.ReadLine();

                    string[] words = line.Split(new string[] { "::" }, StringSplitOptions.None);

                    int devId = int.Parse(words[0]);

                    if (devId == deviceId)
                    {
                        string[] supClasses = words[2].Split(' ');
                        byte[] supClassesBytes = supClasses.Select(i => byte.Parse(i)).ToArray();

                        deviceMemoryReader.Close();

                        return new Tuple<string, byte[]>(words[1], supClassesBytes);
                    }
                }

                deviceMemoryReader.Close();

                return null;
            }
        }

        private string AbortAddDevice()
        {
            controller.StopRequest((byte)CommandTypes.CmdZWaveAddNodeToNetwork);

            return "add device aborted";
        }

        private string RemoveDevice()
        {
            if (controller.ConnectionStatus != ConnectionStatuses.Opened)
            {
                return "zwave controller is not attached or is in strange state: " + controller.ConnectionStatus;
            }

            int maxtries = 1;

            string result = "Device not found";

            while (maxtries > 0)
            {
                try
                {
                    IDeviceInfo deviceInfo = controller.RemoveNodeFromNetwork();

                    if (deviceInfo != null)
                    {
                        //let us remove the port for this device
                        lock (this)
                        {
                            if (zwaveNodes.ContainsKey(deviceInfo.Id))
                            {
                                DeregisterPortWithPlatform(zwaveNodes[deviceInfo.Id].Port);
                                zwaveNodes.Remove(deviceInfo.Id);
                                ForgetDevice(deviceInfo.Id);
                            }
                        }

                        result = GetNameFromId(deviceInfo.Id);

                        break;

                    }
                }
                catch (Exception e)
                {
                    logger.Log("ZwaveDriver: Got exception while finding device: " + e.ToString());
                }

                maxtries--;
            }

            return result;
        }

        private string AbortRemoveDevice()
        {
            controller.StopRequest((byte)CommandTypes.CmdZWaveRemoveNodeFromNetwork);
            return "remove device aborted";
        }


        private void InitDevice(IDeviceInfo deviceInfo)
        {
            if (!zwaveNodes.ContainsKey(deviceInfo.Id))
            {
                var details = GetDeviceDetailsFromMemory(deviceInfo.Id);

                string deviceType = "unknown";
                byte[] supportedCommandClasses = new byte[0];

                if (details == null) 
                {
                    logger.Log("ZwaveDriver: Details not found in memory for device {0}", deviceInfo.Id.ToString());
                    supportedCommandClasses = deviceInfo.SupportedCommandClasses;
                }
                else 
                {
                    deviceType = details.Item1;
                    supportedCommandClasses = details.Item2;
                }

                VPortInfo pInfo = GetPortInfoFromPlatform(GetNameFromId(deviceInfo.Id));
                Port port = InitPort(pInfo);

                HomeOSZwaveNode node = new HomeOSZwaveNode(deviceInfo, deviceType, supportedCommandClasses, port, this, logger);

                zwaveNodes.Add(deviceInfo.Id, node);

                node.Init();

                RegisterPortWithPlatform(port);
            }
        }

        public void BindRolesHelper(Port port, List<VRole> roles)
        {
            BindRoles(port, roles);
        }

        private void ConfigureDevice(IDeviceInfo deviceInfo, string deviceType)
        {
            //make association if the device supports it
            if (deviceInfo.SupportedCommandClasses.Contains(XmlDataManager.GetCommandClassKey("COMMAND_CLASS_ASSOCIATION")))
            { 
                        MakeAssociation(deviceInfo.Id);
            }

            var deviceConfiguration = DriverConfiguration.Devices[deviceType];

            if (deviceConfiguration != null)
            {
                foreach (DeviceSettingElement setting in deviceConfiguration.DeviceSettings)
                {
                    string[] bytes = setting.Value.Split(' ');

                    byte[] byteVals = bytes.Select(i => byte.Parse(i)).ToArray();

                    if (byteVals.Length != setting.Level)
                        logger.Log("ZwaveDriver: zwave device configuration seems broken!");
                    else
                        SetConfiguration(deviceInfo.Id, setting.ParamNum, byteVals);
                }
            }
            else
            {
                if (deviceType != "unknown")
                    logger.Log("ZwaveDriver: zwave device of type {0} not found in configuration", deviceType);
            }
        }


        private string GetNameFromId(byte nodeId)
        {
            return "ZwaveNode::" + nodeId;
        }


        public void QueueRequest(OutboundRequest request)
        {
            lock (outboundRequests)
            {
                outboundRequests.Enqueue(request);

                if (outboundRequests.Count == 1)
                {
                    SafeThread newThread = new SafeThread(delegate() { SendRequests(); },
                                                              "SendRequests",
                                                              logger);
                    newThread.Start();
                }
            }
        }

        private void SendRequests()
        {
            while (true)
            {
                lock (outboundRequests)
                {
                    if (outboundRequests.Count == 0)
                        break;

                    SendRequest(outboundRequests.Dequeue());
                }
            }
        }

        //private void SendRequests()
        //{
        //    OutboundRequest requestToSend = null;

        //    lock (outboundRequests)
        //    {
        //        if (outboundRequests.Count > 0)
        //            requestToSend = outboundRequests.Dequeue();
        //    }

        //    if (requestToSend != null)
        //    {
        //        SendRequest(requestToSend);
        //    }

        //    int remainingCount = 0;
        //    lock (outboundRequests)
        //    {
        //        remainingCount = outboundRequests.Count;
        //    }

        //    if (remainingCount > 0)
        //        SendRequests();
        //}

        private bool SendRequest(OutboundRequest request)
        {
            logger.Log("zwave: about to send request {0} to {1}: {2}", request.Description, request.NodeId.ToString(), BitConverter.ToString(request.Data));

            TransmitStatuses ret = controller.SendData(request.NodeId,
                                                       request.Data,
                                TransmitOptions.TransmitOptionAutoRoute |
                                TransmitOptions.TransmitOptionAcknowledge);

            logger.Log("ZwaveDriver: status of request {0} to {1}: {2}", request.Description, request.NodeId.ToString(), ret.ToString());

            if (ret == TransmitStatuses.CompleteOk)
                return true;

            return false;
        }

        //IList<VParamType> OnOperationInvoke(byte nodeId, string roleName, string opName, IList<VParamType> list)
        //{
        //    lock (this)
        //    {
        //        if (!zwaveNodes.ContainsKey(nodeId))
        //            return null;

        //        HomeOSZwaveNode node = zwaveNodes[nodeId];

        //        return node.OnOperationInvoke(roleName, opName, list);
        //    }
        //}

        private void ApplicationCommandHandlerEvent(DeviceAppCommandHandlerEventArgs args)
        {
            //AJB commented out to reduce data in logs
           // logger.Log("zwave application command handler called");

            SafeThread worker = new SafeThread(delegate { ACH_helper(args); }, "ACHE", logger);
            worker.Start();

            //ACH_helper(args);
        }

        private void ACH_helper(DeviceAppCommandHandlerEventArgs args) 
        {
            lock (this)
            {
                if (args.SourceNodeId == 0 || !zwaveNodes.ContainsKey(args.SourceNodeId))
                    return;

                HomeOSZwaveNode node = zwaveNodes[args.SourceNodeId];

                CommandClass commandClass = XmlDataManager.FindCommandClass(args.CommandClassKey);
                if (commandClass == null)
                    return;

                Command command = XmlDataManager.FindCommand(commandClass, args.CommandKey);
                if (command == null)
                    return;

                List<byte> payload = new List<byte>();
                payload.Add(args.CommandClassKey);
                payload.Add(args.CommandKey);
                if (args.CommandBuffer != null && args.CommandBuffer.Length > 0)
                {
                    payload.AddRange(args.CommandBuffer);
                }

                CommandClassValue[] values = null;
                XmlDataManager.ParseApplicationObject(payload.ToArray(), out values);

                //AJB commented out to reduce data sent to logs
                //PrintCommandResponse(values);

                node.ProcessCommandFromDevice(commandClass, command, values);
            }
        }

        private void PrintCommandResponse(CommandClassValue[] values)
        {
            try
            {
                foreach (var ccv in values)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(string.Format("  received: {0} {1}", ccv.CommandClassDefinition.Name, ccv.CommandValue.CommandDefinition.Name));

                    var pvList = ccv.CommandValue.ParamValues;

                    if (pvList != null && pvList.Count > 0)
                    {
                        sb.Append(", parameters: ");
                        foreach (var pval in pvList)
                        {
                            
                            sb.Append(string.Format("    {0}={1}; ", pval.ParamDefinition.Name, pval.TextValue));
                        }
                    }
                    logger.Log(sb.ToString());
                }
            }
            catch (Exception)
            { }
        }


        private void UnsolititedFrameReceived(UnsolititedFrameReceivedArgs args)
        {
                StringBuilder sb = new StringBuilder();
                foreach (byte val in args.Data)
                {
                    sb.Append(val + " ");
                }

               //AJB reduce data written in log
               //logger.Log("zwave unsolicited frame received: {0}", sb.ToString());

                lock (this)
                {
                    if (args.Data[3] == (byte)CommandTypes.CmdApplicationControllerUpdate &&
                    args.Data[4] == (byte)ApplicationControllerUpdateStatuses.NODE_INFO_RECEIVED &&
                    zwaveNodes.ContainsKey(args.Data[5]))
                {

                    HomeOSZwaveNode node = zwaveNodes[args.Data[5]];

                    node.ProcessUnsolicitedFrame(args);
                }
            }
        }

        public void MakeAssociation(byte nodeId)
        {
            CommandClass cmdClass = XmlDataManager.FindCommandClass("COMMAND_CLASS_ASSOCIATION", 1);
            Command command = XmlDataManager.FindCommand(cmdClass, "ASSOCIATION_SET");
            byte[] dataToSend = command.FillPayload(1, controller.Id);
            QueueRequest(new OutboundRequest(nodeId, dataToSend, "makeassoc-"+nodeId));
        }

        public void GetConfiguration(byte nodeId, byte paramNumber)
        {
            CommandClass cmdClass = XmlDataManager.FindCommandClass("COMMAND_CLASS_CONFIGURATION", 1);
            Command command = XmlDataManager.FindCommand(cmdClass, "CONFIGURATION_GET");
            byte[] dataToSend = command.FillPayload(paramNumber);
            QueueRequest(new OutboundRequest(nodeId, dataToSend, "getconfig-" + nodeId + " param " + paramNumber));
        }

        private void SetConfiguration(byte nodeId, byte paramNumber, byte[] configValues)
        {
            CommandClass cmdClass = XmlDataManager.FindCommandClass("COMMAND_CLASS_CONFIGURATION", 1);
            Command command = XmlDataManager.FindCommand(cmdClass, "CONFIGURATION_SET");

            byte[] payload = new byte[2 + configValues.Length];
            payload[0] = paramNumber;
            payload[1] = (byte)configValues.Length;
            configValues.CopyTo(payload, 2);

            byte[] dataToSend = command.FillPayload(payload);

            QueueRequest(new OutboundRequest(nodeId, dataToSend, "setconfig-" + nodeId + " param " + paramNumber));
        }


        public override void Stop()
        {
            if (zwaveIniter != null)
                zwaveIniter.Abort();

            if (imageServer != null)
                imageServer.Dispose();
        }

        public override string GetDescription(string hint)
        {
            logger.Log("DriverZwave.GetDescription for {0}", hint);
            return "Zwave Device";
        }

        //we have nothing to do with other ports
        public override void PortRegistered(VPort port) { }
        public override void PortDeregistered(VPort port) { }


        public void UsbInserted(object sender, EventArgs eArgs)
        {
            logger.Log("ZwaveDriver: Zwave detected USB insertion. Current value of foundController = {0}", foundController.ToString());

            if (!foundController)
            {
                if (FindController())
                {
                    //we found the controller!
                    InitController();
                }
            }
        }
    }
}
