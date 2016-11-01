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
    public abstract class HomeOSCommandClass
    {
        protected HomeOSZwaveNode zwaveNode;
        protected VLogger logger;

        protected Dictionary<Command, List<ParamValue>> values = new Dictionary<Command, List<ParamValue>>();

        public HomeOSCommandClass(HomeOSZwaveNode zwaveNode, VLogger logger)
        {
            this.zwaveNode = zwaveNode;
            this.logger = logger;
        }

        internal void UpdateCommandClassValues(CommandClassValue[] newCCValues)
        {
            foreach (var newCCValue in newCCValues)
            {
                ////if we don't have this command class, add it
                //if (!values.ContainsKey(newCCValue.CommandClassDefinition))
                //    values.Add(newCCValue.CommandClassDefinition, new Dictionary<Command, List<ParamValue>>());

                //if we don't have this command, add it 
                if (!values.ContainsKey(newCCValue.CommandValue.CommandDefinition))
                    values.Add(newCCValue.CommandValue.CommandDefinition, new List<ParamValue>());

                values[newCCValue.CommandValue.CommandDefinition] = newCCValue.CommandValue.ParamValues;
            }
        }

        internal bool ValueChanged(CommandClassValue[] newCCValues)
        {
            foreach (var newCCValue in newCCValues)
            {
                //if (!values.ContainsKey(newCCValue.CommandClassDefinition))
                //    return true;

                //var curCmdValue = values[newCCValue.CommandClassDefinition];

                CommandValue newCmdValue = newCCValue.CommandValue;

                if (!values.ContainsKey(newCmdValue.CommandDefinition))
                    return true;

                var curParamValues = values[newCmdValue.CommandDefinition];

                List<ParamValue> newParamValues = newCCValue.CommandValue.ParamValues;

                if (curParamValues.Count != newParamValues.Count)
                    return true;

                for (int index = 0; index < curParamValues.Count; index++)
                {
                    ParamValue curPVal = curParamValues[index];
                    ParamValue newPVal = newParamValues[index];

                    if (curPVal.ParamDefinition.Name != newPVal.ParamDefinition.Name)
                        return true;

                    if (!curPVal.TextValue.Equals(newPVal.TextValue))
                        return true;
                }
            }

            return false;
        }

        public abstract void Init();
        public abstract List<VRole> GetRoleList();
        public abstract IList<VParamType> OnOperationInvoke(string roleName, string opName, IList<VParamType> list);
        public abstract void ProcessCommandFromDevice(CommandClassValue[] values);
        public abstract void ProcessUnsolicitedFrame(UnsolititedFrameReceivedArgs args);
    }
}
