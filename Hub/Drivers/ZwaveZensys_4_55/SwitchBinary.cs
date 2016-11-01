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
    class SwitchBinary : HomeOSCommandClass
    {
        private bool isCurrValueSet = false;
        private bool currValue;

        public SwitchBinary(HomeOSZwaveNode zwaveNode, VLogger logger)
            : base(zwaveNode, logger)
        {}

        public override void Init()
        {
            SendGetRequest();
        }

        public override List<VRole> GetRoleList()
        {
            return new List<VRole>() { RoleSwitchBinary.Instance };
        }

        public override IList<VParamType> OnOperationInvoke(string roleName, string opName, IList<VParamType> list)
        {
            if (!roleName.Equals(RoleSwitchBinary.RoleName, StringComparison.CurrentCultureIgnoreCase))
                return null;

            switch (opName.ToLower())
            {
                case RoleSwitchBinary.OpSetName:
                    {
                        bool valToSet = (bool) list[0].Value();

                        //if (valToSet > 0) valToSet = 255;

                        if (!isCurrValueSet || currValue != valToSet)
                        {
                            logger.Log("queueing binary switch set for {0}", valToSet.ToString());

                            var cmdClass = zwaveNode.Driver.XmlDataManager.FindCommandClass("COMMAND_CLASS_SWITCH_BINARY", 1);
                            Command cmd = zwaveNode.Driver.XmlDataManager.FindCommand(cmdClass, "SWITCH_BINARY_SET");
                            byte[] dataToSend = cmd.FillPayload((valToSet)? (byte) 255 : (byte) 0);

                            zwaveNode.Driver.QueueRequest(new OutboundRequest(zwaveNode.DeviceInfo.Id, dataToSend, "SwitchBinarySet-" + valToSet));

                            Notify(valToSet);
                        }

                        isCurrValueSet = true;
                        currValue = valToSet;

                        return new List<VParamType>();
                    }
                case RoleSwitchBinary.OpGetName:
                    {
                        bool value = isCurrValueSet ? currValue : false;

                        logger.Log("SwitchBinary-{0}: OpGetName returning {1}", zwaveNode.DeviceInfo.Id.ToString(), value.ToString());

                        IList<VParamType> retVals = new List<VParamType>() {new ParamType(value)};

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
                var cmdClassSb = zwaveNode.Driver.XmlDataManager.FindCommandClass("COMMAND_CLASS_SWITCH_BINARY", 1);
                var commandSb = zwaveNode.Driver.XmlDataManager.FindCommand(cmdClassSb, "SWITCH_BINARY_REPORT");

                var cmdClassBs = zwaveNode.Driver.XmlDataManager.FindCommandClass("COMMAND_CLASS_BASIC", 1);
                var commandBs = zwaveNode.Driver.XmlDataManager.FindCommand(cmdClassBs, "BASIC_SET");

                if (ccv.CommandClassDefinition.Equals(cmdClassSb) && ccv.CommandValue.CommandDefinition.Equals(commandSb) ||
                    ccv.CommandClassDefinition.Equals(cmdClassBs) && ccv.CommandValue.CommandDefinition.Equals(commandBs))
                {
                    List<ParamValue> newPValues = ccv.CommandValue.ParamValues;

                    byte newValueByte = newPValues[0].ByteValueList[0];

                    bool newValue = (newValueByte > 0)? true : false;

                    Notify(newValue);

                    isCurrValueSet = true;
                    currValue = newValue;
                }
            }
        }


        private void Notify(bool value)
        {
            IList<VParamType> retVals = new List<VParamType>() { new ParamType(value) };

            zwaveNode.Port.Notify(RoleSwitchBinary.RoleName, RoleSwitchBinary.OpGetName, retVals);

            logger.Log("{0}: issued notification for switch {1}, value {2}", ToString(), zwaveNode.DeviceInfo.Id.ToString(), value.ToString());
        }

        public override void ProcessUnsolicitedFrame(UnsolititedFrameReceivedArgs args)
        {
            SendGetRequest();
        }

        private void SendGetRequest()
        {
            var cmdClass = zwaveNode.Driver.XmlDataManager.FindCommandClass("COMMAND_CLASS_SWITCH_BINARY", 1);
            Command cmd = zwaveNode.Driver.XmlDataManager.FindCommand(cmdClass, "SWITCH_BINARY_GET");
            byte[] dataToSend = cmd.FillPayload(null);
            zwaveNode.Driver.QueueRequest(new OutboundRequest(zwaveNode.DeviceInfo.Id, dataToSend, "SwithBinaryGet"));
        }

    }
}


