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
    class SwitchMultiLevel : HomeOSCommandClass
    {
        private bool isCurrValueSet = false;
        private double currValue;

        public SwitchMultiLevel(HomeOSZwaveNode zwaveNode, VLogger logger)
            : base(zwaveNode, logger)
        {
        }

        public override void Init()
        {
            SendGetRequest();
        }

        public override List<VRole> GetRoleList()
        {
            return new List<VRole>() { RoleSwitchMultiLevel.Instance };
        }

        public override IList<VParamType> OnOperationInvoke(string roleName, string opName, IList<VParamType> list)
        {
            if (!roleName.Equals(RoleSwitchMultiLevel.RoleName, StringComparison.CurrentCultureIgnoreCase))
                return null;

            switch (opName.ToLower())
            {
                case RoleSwitchMultiLevel.OpSetName:
                    {
                        double valToSet = (double) list[0].Value();

                        if (valToSet < 0) valToSet = 0;
                        if (valToSet > 1) valToSet = 1;

                        if (!isCurrValueSet || currValue != valToSet)
                        {
                            logger.Log("queueing switch level set for {0}", valToSet.ToString());

                            var cmdClass = zwaveNode.Driver.XmlDataManager.FindCommandClass("COMMAND_CLASS_SWITCH_MULTILEVEL", 2);
                            Command cmd = zwaveNode.Driver.XmlDataManager.FindCommand(cmdClass, "SWITCH_MULTILEVEL_SET");
                            byte[] dataToSend = cmd.FillPayload( (byte) (valToSet * 255));

                            zwaveNode.Driver.QueueRequest(new OutboundRequest(zwaveNode.DeviceInfo.Id, dataToSend, "SwitchMultiLevelSet-" + valToSet));

                            Notify(valToSet);
                        }

                        isCurrValueSet = true;
                        currValue = valToSet;

                        return new List<VParamType>();
                    }
                case RoleSwitchMultiLevel.OpGetName:
                    {
                        double value = isCurrValueSet ? currValue : 0;

                        logger.Log("SwitchMultiLevel-{0}: OpGetName returning {1}", zwaveNode.DeviceInfo.Id.ToString(), value.ToString());

                        IList<VParamType> retVals = new List<VParamType>() { new ParamType(value) };

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

            //now extract the value we care about
            var cmdClass = zwaveNode.Driver.XmlDataManager.FindCommandClass("COMMAND_CLASS_SWITCH_MULTILEVEL", 2);
            var command = zwaveNode.Driver.XmlDataManager.FindCommand(cmdClass, "SWITCH_MULTILEVEL_REPORT");

            foreach (var ccv in values)
            {
                if (ccv.CommandClassDefinition.Equals(cmdClass) && 
                    ccv.CommandValue.CommandDefinition.Equals(command))
                {
                    List<ParamValue> newPValues = ccv.CommandValue.ParamValues;

                    byte newValueByte = newPValues[0].ByteValueList[0];

                    double newValue = newValueByte / 255.0;

                    Notify(newValue);

                    isCurrValueSet = true;
                    currValue = newValue;
                }
            }
        }

        private void Notify(double value)
        {
            IList<VParamType> retVals = new List<VParamType>() {new ParamType(value)};

            zwaveNode.Port.Notify(RoleSwitchMultiLevel.RoleName, RoleSwitchMultiLevel.OpGetName, retVals);

            logger.Log("{0}: issued notification for switch {1}, value {2}", ToString(), zwaveNode.DeviceInfo.Id.ToString(), value.ToString());
        }

        public override void ProcessUnsolicitedFrame(UnsolititedFrameReceivedArgs args)
        {
            SendGetRequest();
        }

        private void SendGetRequest()
        {
            var cmdClass = zwaveNode.Driver.XmlDataManager.FindCommandClass("COMMAND_CLASS_SWITCH_MULTILEVEL", 2);
            Command cmd = zwaveNode.Driver.XmlDataManager.FindCommand(cmdClass, "SWITCH_MULTILEVEL_GET");
            byte[] dataToSend = cmd.FillPayload(null);
            zwaveNode.Driver.QueueRequest(new OutboundRequest(zwaveNode.DeviceInfo.Id, dataToSend, "SwithMultiLevelGet"));
        }
    }

}
