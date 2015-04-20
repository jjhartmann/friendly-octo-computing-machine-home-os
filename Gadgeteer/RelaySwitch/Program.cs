using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
//using Gadgeteer.Modules.Seeed;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO.Ports;

namespace LightSensor
{
    public partial class Program
    {
        HomeOSGadgeteer.HomeOSGadgeteerDevice hgd;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            hgd = new HomeOSGadgeteer.HomeOSGadgeteerDevice("MicrosoftResearch", "RelaySwitch", "abcdefgh", wifi,
                null, null, /*usbSerial.SerialLine.PortName*/null, null, null, () => { return GT.Timer.GetMachineTime() < RemoteControlLedEndTime; }, true);

            hgd.SetupWebEvent("IsOn").WebEventReceived += this.RelayWebEventReceived;
           // hgd.SetupWebEvent("Switch").WebEventReceived += Program_WebEventReceived;

           // this.joystick.JoystickPressed += joystick_JoystickPressed;

            this.SwitchAndWait();

            GT.Timer relayTimer = new GT.Timer(RelayCheckPeriod);
            relayTimer.Tick += relayTimer_Tick;
            relayTimer.Start();

            Debug.Print("Program Started");
        }
        //void relayTimer_Tick(GT.Timer timer)
        //{
        //    //this.relay_X1.Enabled = !this.relay_X1.Enabled;
        //    Debug.Print(this.response);
        //}
      

        public void Switch()
        {   
            this.relay_X1.Enabled = !this.relay_X1.Enabled;
        }

        public void SwitchAndWait()
        {
            Switch();
            Thread.Sleep(500);
        }

        bool ok = false;
        void Program_WebEventReceived(string path, HomeOSGadgeteer.Networking.WebServer.HttpMethod method, HomeOSGadgeteer.Networking.Responder responder)
        {
            //throw new NotImplementedException();
            //this.relay_X1.Enabled = !this.relay_X1.Enabled;
            //Reboot();
            //if (this.relay_X1)
            //{
            //    this.relay_X1.TurnOff();
            //}
            //else
            //{
            //    this.relay_X1.TurnOn();
            //}
            //ok = !ok;
            //this.relay_X1.TurnOn();
            responder.Respond("OK");
        }

        TimeSpan RemoteControlLedEndTime = TimeSpan.Zero;
        #region Device behaviour

        /// <summary>
        /// The time between checking light sensor
        /// </summary>
        const int RelayCheckPeriod = 2000;
        TimeSpan LightBlinkTimeSpan = new TimeSpan(0, 0, 1);

        /// <summary>
        /// This is called when a wifi network connection to a home network (not setup network) is made (or remade) 
        /// </summary>
        private void HomeNetworkConnectionMade()
        {
            // this is just for test
            //WebClient.GetFromWeb("http://research.microsoft.com/en-us/").ResponseReceived += new HttpRequest.ResponseHandler(TestResponseReceived);
        }

        void RelayWebEventReceived(string path, 
            HomeOSGadgeteer.Networking.WebServer.HttpMethod method, 
            HomeOSGadgeteer.Networking.Responder responder)
        {
            this.relay_X1.Enabled = !this.relay_X1.Enabled;
            Debug.Print("Relay web event from " + responder.ClientEndpoint + " - response " + this.response);
            responder.Respond(this.webResponse);
        }

        void relayTimer_Tick(GT.Timer timer)
        {
            //this.relay_X1.Enabled = !this.relay_X1.Enabled;
            Debug.Print(this.response);
        }

        private int IsOn()
        {
            return this.relay_X1.Enabled ? 1 : 0;
        }

        private string webResponse
        {
            get
            {
                return "{\"DeviceId\":\"" + 
                    hgd.IdentifierString + "\","
                    + "\"IsOn\":" + IsOn() + 
                    "}";
            }
        }

        private string response
        {
            get
            {
                return "{" +
                "\"IsOn\" : " + IsOn() + "\n" +
                 "\"DeviceIP\" : \"" + this.wifi.NetworkSettings.IPAddress + "\", " +
                 "\"DeviceId\" : \"" + hgd.IdentifierString + "\", " +
                "}";
            }
        }

        #endregion

    }
}
