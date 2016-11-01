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
    class SensorBinary : HomeOSCommandClass
    {
        private bool isCurrValueSet = false;
        private byte currValue;

        public SensorBinary(HomeOSZwaveNode zwaveNode, VLogger logger)
            : base(zwaveNode, logger)
        {
            zwaveNode.DeviceInfo.CommandQueueOverrided = true;
        }

        public override void Init()
        {
            SendGetRequest();

            // this is purely for testing
            //System.Timers.Timer timer = new System.Timers.Timer(30 * 1000);
            //timer.Elapsed += timer_Elapsed;
            //timer.Start();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Notify(255);
        }

        public override List<VRole> GetRoleList()
        {
            return new List<VRole>() { RoleSensor.Instance };
        }

        public override IList<VParamType> OnOperationInvoke(string roleName, string opName, IList<VParamType> list)
        {
            if (!roleName.Equals(RoleSensor.RoleName, StringComparison.CurrentCultureIgnoreCase))
                return null;

            switch (opName.ToLower())
            {
                case RoleSensor.OpGetName:
                    {
                        byte value = isCurrValueSet ? currValue : (byte)0;

                        logger.Log("Sensor-{0}: OpGetName returning {1}", zwaveNode.DeviceInfo.Id.ToString(), value.ToString());

                        IList<VParamType> retVals = new List<VParamType>();
                        retVals.Add(new ParamType(value));

                        return retVals;
                    }
                default:
                    logger.Log("Unknown operation {0} for role {1}", opName, roleName);
                    return null;
            }
        }

        public override void ProcessCommandFromDevice(CommandClassValue[] values)
        {
            //first, we should update the full store
            UpdateCommandClassValues(values);

            foreach (var ccv in values)
            {
                //now extract the value we care about
                var cmdClassSb = zwaveNode.Driver.XmlDataManager.FindCommandClass("COMMAND_CLASS_SENSOR_BINARY", 1);
                var commandSb = zwaveNode.Driver.XmlDataManager.FindCommand(cmdClassSb, "SENSOR_BINARY_REPORT");

                var cmdClassBs = zwaveNode.Driver.XmlDataManager.FindCommandClass("COMMAND_CLASS_BASIC", 1);
                var commandBs = zwaveNode.Driver.XmlDataManager.FindCommand(cmdClassBs, "BASIC_SET");

                if (ccv.CommandClassDefinition.Equals(cmdClassSb) && ccv.CommandValue.CommandDefinition.Equals(commandSb) ||
                    ccv.CommandClassDefinition.Equals(cmdClassBs) && ccv.CommandValue.CommandDefinition.Equals(commandBs))
                {
                    List<ParamValue> newPValues = ccv.CommandValue.ParamValues;

                    byte newValue = newPValues[0].ByteValueList[0];

             
                    Notify(newValue);

                    isCurrValueSet = true;
                    currValue = newValue;
                }
            }
        }


        private void Notify(byte value)
        {
            logger.Log("{0}: about to issue notification for sensor {1}, value {2}", ToString(), zwaveNode.DeviceInfo.Id.ToString(), value.ToString());

            IList<VParamType> retVals = new List<VParamType>();
            retVals.Add(new ParamType(value));

            zwaveNode.Port.Notify(RoleSensor.RoleName, RoleSensor.OpGetName, retVals);
        }

        public override void ProcessUnsolicitedFrame(UnsolititedFrameReceivedArgs args)
        {
            SendGetRequest();
        }

        private void SendGetRequest()
        {
            var cmdClass = zwaveNode.Driver.XmlDataManager.FindCommandClass("COMMAND_CLASS_SENSOR_BINARY", 1);
            Command cmd = zwaveNode.Driver.XmlDataManager.FindCommand(cmdClass, "SENSOR_BINARY_GET");
            byte[] dataToSend = cmd.FillPayload(null);
            zwaveNode.Driver.QueueRequest(new OutboundRequest(zwaveNode.DeviceInfo.Id, dataToSend, "SensorBinaryGet"));
        }

    }
}




