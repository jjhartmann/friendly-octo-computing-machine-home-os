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
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO.Ports;

namespace RelaySensor
{
    public partial class Program
    {
        /// <summary>
        /// Workaround on #### Exception System.Net.Sockets.SocketException - CLR_E_FAIL (1) ####
        /// </summary>
        private bool RebootAfterEachDeviceAction = true;

        HomeOSGadgeteer.HomeOSGadgeteerDevice hgd;
        TimeSpan RemoteControlLedEndTime = TimeSpan.Zero;

        /// <summary>
        /// The time between checking light sensor
        /// </summary>
        const int RelayCheckPeriod = 1000;
        TimeSpan LightBlinkTimeSpan = new TimeSpan(0, 0, 1);

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            hgd = new HomeOSGadgeteer.HomeOSGadgeteerDevice("MicrosoftResearch", "RelaySwitch", "abcdefgh", wifi,
                null, null, /*usbSerial.SerialLine.PortName*/null, null, null, () => { return GT.Timer.GetMachineTime() < RemoteControlLedEndTime; }, true);

            hgd.SetupWebEvent("IsOn").WebEventReceived += this.RelayWebEventReceived;

            this.SwitchAndWait();

            GT.Timer relayTimer = new GT.Timer(RelayCheckPeriod);
            relayTimer.Tick += relayTimer_Tick;
            relayTimer.Start();

            Debug.Print("Program Started");
        }

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
            Debug.Print("Relay web event from " + responder.ClientEndpoint + " - response " + this.response);
            responder.Respond(this.webResponse);

            int amount = 1;
            if (responder.UrlParameters.Count > 0)
            {
                string a = responder.UrlParameters["amount"].ToString();
                amount = Int32.Parse(a);
            }
            this.FireLight(amount);
        }

        #region Device behaviour

        void relayTimer_Tick(GT.Timer timer)
        {
            Debug.Print(this.response);
        }

        /// <summary>
        /// Turns off & off <c>amount</c> times
        /// </summary>
        /// <param name="amount"></param>
        private void FireLight(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                this.SwitchAndWait();
            }
            if (amount > 0 && RebootAfterEachDeviceAction)
            {
                Reboot();
            }
        }

        private void Switch()
        {
            this.relay_X1.Enabled = !this.relay_X1.Enabled;
        }

        private void SwitchAndWait()
        {
            Switch();
            Thread.Sleep(500);
        }

        private int IsOn()
        {
            return this.relay_X1.Enabled ? 1 : 0;
        }

        #endregion

        #region #REST

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
