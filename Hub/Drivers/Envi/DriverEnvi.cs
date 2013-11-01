using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeOS.Hub.Common;
using System.IO.Ports;
using System.Text.RegularExpressions;
using HomeOS.Hub.Platform.Views;

namespace HomeOS.Hub.Drivers.Envi
{
    /// <summary>
    /// An envi driver module that 
    /// 1. opens and registers a port
    /// 2. reads the active power consumption of an appliance from the USB port whenever it is available
    /// 3. Notifies applications that subscribed to this port
    /// </summary>

   [System.AddIn.AddIn("HomeOS.Hub.Drivers.Envi")]
    public class Envi : ModuleBase
    {
        private Port enviPort;
        private SerialPort serialport;
        private string SerialPortName;
        private const string Prolific = "Prolific";

        public override void Start()
        {
            logger.Log("Started: {0}", ToString());

            List<COMPortInfo> comportList = COMPortInfo.GetCOMPortsInfo();

            foreach (COMPortInfo comPortInfo in comportList)
            {
                if (comPortInfo.Description.Contains(Prolific))
                {
                    this.SerialPortName = comPortInfo.Name;
                    break;
                }
            }
            logger.Log("Discovered envi sensor on COM port: "+SerialPortName);


            // ..... initialize the list of roles we are going to export
            List<VRole> listRole = new List<VRole>(){RoleSensor.Instance};


            //.................instantiate the port
            VPortInfo portInfo = GetPortInfoFromPlatform("envi");
            enviPort = InitPort(portInfo);

            //..... bind the port to roles and delegates
            BindRoles(enviPort, listRole, null);

            //.................register the port after the binding is complete
            RegisterPortWithPlatform(enviPort);

            ReadFromSerialPort();
        }

        private void ReadFromSerialPort()
        {
            serialport = new SerialPort(SerialPortName, 57600, Parity.None, 8, StopBits.One);
            try
            {
                // Close the serial port if it is open
                if (serialport.IsOpen)
                {
                    serialport.Close();
                }
                // Open the serial port again
                serialport.Open();
            }
            catch (System.IO.IOException)
            {

                logger.Log("Driver Envi Error: failed to open {0} port", SerialPortName);
                return;
            }

            // Defining the event handler function
            serialport.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int value = 0;
            Regex measurements = new Regex(@".+<watts>(\d+)</watts>.+<watts>(\d+)</watts>.+");
            String str = serialport.ReadLine();
            Match m = measurements.Match(str);
            if (m.Success)
            {
                int ch1 = Convert.ToInt32(m.Groups[1].Value);
                //Console.WriteLine(ch1);
                int ch2 = Convert.ToInt32(m.Groups[2].Value);
                //Console.WriteLine(ch2);

                // Adding power consumptions of both channels, the result is the total power consumption
                value = ch1 + ch2;

                // Setting the return parameter
                IList<VParamType> retVals = new List<VParamType>();
                retVals.Add(new ParamType(value));

                // Notifying modules (e.g., AppEnvi) subscribed to the enviPort
                enviPort.Notify(RoleSensor.RoleName, RoleSensor.OpGetName, retVals);

                logger.Log("{0}: issued notification on port. read value {1}", SerialPortName, value.ToString());
            }
            else
            {
                // Pattern mismatch
                logger.Log("{0} is not a valid measurment data", str);
                //Console.WriteLine(str + " is not a valid measurment data");
            }
        }

        public override void Stop()
        {
            this.serialport.Close();
            Finished();
        }

        /// <summary>
        ///  Called when a new port is registered with the platform
        ///        the dummy driver does not care about other ports in the system
        /// </summary>
        public override void PortRegistered(VPort port) { }

        /// <summary>
        ///  Called when a new port is deregistered with the platform
        ///        the dummy driver does not care about other ports in the system
        /// </summary>
        public override void PortDeregistered(VPort port) { }

    }

}
