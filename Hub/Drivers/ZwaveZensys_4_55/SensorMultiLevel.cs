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
using HomeOS.Hub.Drivers.ZwaveZensys.Configuration;

namespace HomeOS.Hub.Drivers.ZwaveZensys
{
    public class SensorValue
    {
        public VRole SensorType { get; set; }
        public bool IsCurrValueSet {get; set;}
        public double CurrValue { get; set; }

        public SensorValue(VRole role)
        {
            SensorType = role;
            IsCurrValueSet = false;
            CurrValue = 0;
        }
    }

    class SensorMultiLevel : HomeOSCommandClass
    {
        const bool ReportOnlyOnValueChange = false;

        Dictionary<VRole, SensorValue> sensorValues = new Dictionary<VRole, SensorValue>();

        public SensorMultiLevel(HomeOSZwaveNode zwaveNode, VLogger logger)
            : base(zwaveNode, logger)
        {
            var deviceConfiguration = zwaveNode.Driver.DriverConfiguration.Devices[zwaveNode.DeviceType];

            if (deviceConfiguration == null)
            {
                sensorValues.Add(RoleSensorMultiLevel.Instance, new SensorValue(RoleSensorMultiLevel.Instance));
            }
            else
            {
                foreach (RoleElement role in deviceConfiguration.RoleList)
                {
                    switch (role.Name) 
                    {
                        case RolePowerSensor.RoleName:
                            sensorValues.Add(RolePowerSensor.Instance, new SensorValue(RolePowerSensor.Instance));
                            break;
                        case RoleTemperatureSensor.RoleName:
                            sensorValues.Add(RoleTemperatureSensor.Instance, new SensorValue(RoleTemperatureSensor.Instance));
                            break;
                        case RoleHumiditySensor.RoleName:
                            sensorValues.Add(RoleHumiditySensor.Instance, new SensorValue(RoleHumiditySensor.Instance));
                            break;
                        case RoleLuminositySensor.RoleName:
                            sensorValues.Add(RoleLuminositySensor.Instance, new SensorValue(RoleLuminositySensor.Instance));
                            break;
                        default:
                            logger.Log("Unknown multilevel role in driver configruation: {0}", role.Name);
                            sensorValues.Add(RoleSensorMultiLevel.Instance, new SensorValue(RoleSensorMultiLevel.Instance));
                            break;
                    }
                }
            }
        }

        public override void Init()
        {
            SendGetRequest();

            //this is purely for testing
            //System.Timers.Timer timer = new System.Timers.Timer(30 * 1000);
            //timer.Elapsed += timer_Elapsed;
            //timer.Start();
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SensorValue sensorValue = new SensorValue(RoleSensorMultiLevel.Instance);
            sensorValue.CurrValue = 1.0;
            sensorValue.IsCurrValueSet = true;

            Notify(sensorValue);
        }


        public override List<VRole> GetRoleList()
        {
            return sensorValues.Keys.ToList();
        }

        public override IList<VParamType> OnOperationInvoke(string roleName, string opName, IList<VParamType> list)
        {
            SensorValue sensorValue = null;
            
            foreach (VRole role in sensorValues.Keys)
            {
                if (role.Name().Equals(roleName))
                {
                    sensorValue = sensorValues[role];
                    break;
                }
            }

            if (sensorValue == null)
            {
                logger.Log("Got reqeust for unsupported sensor role: roleName");
                return null;
            }

            switch (opName.ToLower())
            {
                case RoleSensorMultiLevel.OpGetName:
                    {
                        double value = sensorValue.IsCurrValueSet ? sensorValue.CurrValue : 0.0;

                        logger.Log("SensorMultiLevel-{0} for {1} returning {2}", zwaveNode.DeviceInfo.Id.ToString(), sensorValue.SensorType.Name(), value.ToString());

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

            foreach (var ccv in values)
            {
                //now extract the value we care about
                var cmdClass = zwaveNode.Driver.XmlDataManager.FindCommandClass("COMMAND_CLASS_SENSOR_MULTILEVEL", 1);
                var command = zwaveNode.Driver.XmlDataManager.FindCommand(cmdClass, "SENSOR_MULTILEVEL_REPORT");

                if (ccv.CommandClassDefinition.KeyId.Equals(cmdClass.KeyId) &&   //matching on KeyId makes the match neutral to version numbers
                    ccv.CommandValue.CommandDefinition.Equals(command))
                {
                    List<ParamValue> newPValues = ccv.CommandValue.ParamValues;

                    if (newPValues.Count != 5)
                    {
                        logger.Log("Unexpected pvalues in multilevel report. count = {0}", newPValues.Count.ToString());
                        return;
                    }

                    byte reportedSensorType = newPValues[0].ByteValueList[0];
                    byte reportedSize = newPValues[1].ByteValueList[0];
                    byte reportedScale = newPValues[2].ByteValueList[0];
                    byte reportedPrecision = newPValues[3].ByteValueList[0];
                    double reportedSensorValue = ByteArrayToDouble(newPValues[4]);

                    reportedSensorValue = reportedSensorValue / Math.Pow(10.0, reportedPrecision);

                    VRole sensorRole = MapSensorTypeToRole(reportedSensorType);
                    SensorValue sensorValue = null;

                    if (sensorRole == null)
                    {
                        logger.Log("Got unknown sensor type {0} in report", reportedSensorType.ToString());

                        //we can only match a generic role
                        if (sensorValues.ContainsKey(RoleSensorMultiLevel.Instance))
                            sensorValue = sensorValues[RoleSensorMultiLevel.Instance];
                    }
                    else if (sensorValues.ContainsKey(sensorRole))
                    {
                        sensorValue = sensorValues[sensorRole];
                    }
                    else
                    {
                        //see if we have the generic role
                        if (sensorValues.ContainsKey(RoleSensorMultiLevel.Instance))
                            sensorValue = sensorValues[RoleSensorMultiLevel.Instance];
                    }

                    if (sensorValue == null)
                    {
                        logger.Log("No matching sensor value found. Role = {0}", (sensorRole == null) ? "null" : sensorRole.ToString());
                        return;
                    }

                    //we notify if reportonlyonvaluechange is false, or this was the first time we got a value for this sensor, or new value != old value
                    if (!ReportOnlyOnValueChange || !sensorValue.IsCurrValueSet || reportedSensorValue != sensorValue.CurrValue)
                    {
                        sensorValue.CurrValue = reportedSensorValue;
                        Notify(sensorValue);
                    }
                    else
                    {
                        logger.Log("No change in value ({0}, {1})", sensorValue.SensorType.Name(), reportedSensorValue.ToString());
                    }

                    sensorValue.IsCurrValueSet = true;
                    
                }
            }
        }

        private double ByteArrayToDouble(ParamValue value)
        {
            string valueStr = value.TextValue.Replace(" ", "");
            double valueDbl = (double) Convert.ToInt32(valueStr,16);
            return valueDbl;
        }

        private VRole MapSensorTypeToRole(byte sensorType)
        {
            switch (sensorType)
            {
                case 0x01:
                    return RoleTemperatureSensor.Instance;
                case 0x03:
                    return RoleLuminositySensor.Instance;
                case 0x04:
                    return RolePowerSensor.Instance;
                case 0x05:
                    return RoleHumiditySensor.Instance;
                default:
                    return null;
            }
        }

        private void Notify(SensorValue sValue)
        {
            IList<VParamType> retVals = new List<VParamType>() { new ParamType(sValue.CurrValue) };

            zwaveNode.Port.Notify(sValue.SensorType.Name(), RoleSensorMultiLevel.OpGetName, retVals);

            logger.Log("{0}: issued notification for sensor {1}, value {2}", ToString(), zwaveNode.DeviceInfo.Id.ToString(), sValue.CurrValue.ToString());
        }

        public override void ProcessUnsolicitedFrame(UnsolititedFrameReceivedArgs args)
        {
            SendGetRequest();
        }

        private void SendGetRequest()
        {
            //versions 1-4 do not have any arguments for multilevel_get
            var cmdClass = zwaveNode.Driver.XmlDataManager.FindCommandClass("COMMAND_CLASS_SENSOR_MULTILEVEL", 1);
            Command cmd = zwaveNode.Driver.XmlDataManager.FindCommand(cmdClass, "SENSOR_MULTILEVEL_GET");
            byte[] dataToSend = cmd.FillPayload(null);
            zwaveNode.Driver.QueueRequest(new OutboundRequest(zwaveNode.DeviceInfo.Id, dataToSend, "SensorMultilevelGet"));
        }
    }

}
