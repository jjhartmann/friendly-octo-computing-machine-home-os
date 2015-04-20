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
            hgd = new HomeOSGadgeteer.HomeOSGadgeteerDevice("MicrosoftResearch", "LightSensor", "abcdefgh", wifi,
                null, null, /*usbSerial.SerialLine.PortName*/null, null, null, () => { return GT.Timer.GetMachineTime() < RemoteControlLedEndTime; }, true);

            this.joystick.JoystickPressed += joystick_JoystickPressed;

            hgd.SetupWebEvent("light").WebEventReceived += this.LightWebEventReceived;

            GT.Timer lightTimer = new GT.Timer(LightCheckPeriod);
            lightTimer.Tick += new GT.Timer.TickEventHandler(this.lightTimer_Tick);
            lightTimer.Start();

            Debug.Print("Program Started");
        }

        void joystick_JoystickPressed(Joystick sender, Joystick.JoystickState state)
        {
            this.led7r.Animate(500, true, true, false);
            Debug.Print("Joystick pressed! Reseting Network Credentials!");
            hgd.ResetNetworkCredentials();
        }

        TimeSpan RemoteControlLedEndTime = TimeSpan.Zero;
        #region Device behaviour

        /// <summary>
        /// The time between checking light sensor
        /// </summary>
        const int LightCheckPeriod = 5000;
        //const int MinBadLightLevel = 200;
        TimeSpan LightBlinkTimeSpan = new TimeSpan(0, 0, 1);

        /// <summary>
        /// This is called when a wifi network connection to a home network (not setup network) is made (or remade) 
        /// </summary>
        private void HomeNetworkConnectionMade()
        {
            // this is just for test
            //WebClient.GetFromWeb("http://research.microsoft.com/en-us/").ResponseReceived += new HttpRequest.ResponseHandler(TestResponseReceived);
        }

        void LightWebEventReceived(string path, 
            HomeOSGadgeteer.Networking.WebServer.HttpMethod method, 
            HomeOSGadgeteer.Networking.Responder responder)
        {
            Debug.Print("Light web event from " + responder.ClientEndpoint + " - response " + this.response);
            responder.Respond(this.webResponse);
        }

        void lightTimer_Tick(GT.Timer timer)
        {
            Debug.Print(this.response);
        }

        private string webResponse
        {
            get
            {
                return "{\"DeviceId\":\"" + 
                    hgd.IdentifierString + "\"," 
                    + "\"light\":" + this.lightSensor.GetIlluminance().ToString() + 
                    "}";
            }
        }

        private string response
        {
            get
            {
                return "{" +
                "\"light\" : " + this.lightSensor.GetIlluminance() + "\n" +
                 "\"DeviceIP\" : \"" + this.wifi.NetworkSettings.IPAddress + "\", " +
                 "\"DeviceId\" : \"" + hgd.IdentifierString + "\", " +
                "}";
            }
        }

        #endregion

    }
}
