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
using Gadgeteer.Modules.Seeed;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO.Ports;

namespace MoistureSensor
{
    public partial class Program
    {
        HomeOSGadgeteer.HomeOSGadgeteerDevice hgd;
        
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            hgd = new HomeOSGadgeteer.HomeOSGadgeteerDevice("MicrosoftResearch", "MoistureSensor", "abcdefgh", wifi,
                multicolorLed, null, usbSerial.SerialLine.PortName, null, null, () => { return GT.Timer.GetMachineTime() < RemoteControlLedEndTime; },false);

            resetButton.LEDMode = Button.LEDModes.OnWhilePressed;
            resetButton.ButtonPressed += new Button.ButtonEventHandler(resetButton_ButtonPressed);
            resetButton.ButtonReleased += new Button.ButtonEventHandler(resetButton_ButtonReleased);

            usbHost.USBDriveConnected += (sender,storagedevice) => hgd.StorageAttached(storagedevice);
            sdCard.SDCardMounted += (sender, storagedevice) => hgd.StorageAttached(storagedevice);

            hgd.SetupWebEvent("moisture").WebEventReceived += MoistureWebEventReceived;
            hgd.SetupWebEvent("led").WebEventReceived += LedWebEventReceived;

            GT.Timer moistureTimer = new GT.Timer(MoistureCheckPeriod);
            moistureTimer.Tick += new GT.Timer.TickEventHandler(moistureTimer_Tick);
            moistureTimer.Start();
            
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        #region reset button
        TimeSpan resetPressMinDuration = new TimeSpan(0, 0, 5);
        DateTime resetButtonPressTime = DateTime.MaxValue;
        void resetButton_ButtonPressed(Button sender, Button.ButtonState state)
        {
            resetButtonPressTime = DateTime.Now;
        }

        void resetButton_ButtonReleased(Button sender, Button.ButtonState state)
        {
            if (resetButtonPressTime != DateTime.MaxValue && DateTime.Now - resetButtonPressTime >= resetPressMinDuration)
            {
                hgd.ResetNetworkCredentials();
            }
            resetButtonPressTime = DateTime.MaxValue;
        }

        #endregion


        #region Device behaviour
        
        /// <summary>
        /// The time between checking moisture sensor
        /// </summary>
       const int MoistureCheckPeriod = 5000;
       const int MinBadMoistureLevel = 200;
       TimeSpan MoistureBlinkTimeSpan = new TimeSpan(0, 0, 1);

        void LedWebEventReceived(string path, HomeOSGadgeteer.Networking.WebServer.HttpMethod method, HomeOSGadgeteer.Networking.Responder responder)
        {
            byte r=0, g=0, b=0;
            int time = 0;

            try
            {
                string rstring = responder.GetParameterValueFromURL("r");
                string gstring = responder.GetParameterValueFromURL("g");
                string bstring = responder.GetParameterValueFromURL("b");
                string timestring = responder.GetParameterValueFromURL("t");

                if (rstring != null && rstring != "")
                {
                    try
                    {
                        int rint = int.Parse(rstring);
                        if (rint >= 0 && rint <= 255)
                        {
                            r = (byte)rint;
                        }
                    }
                    catch { }
                }

                if (gstring != null && gstring != "")
                {
                    try
                    {
                        int gint = int.Parse(gstring);
                        if (gint >= 0 && gint <= 255)
                        {
                            g = (byte)gint;
                        }
                    }
                    catch { }
                }

                if (bstring != null && bstring != "")
                {
                    try
                    {
                        int bint = int.Parse(bstring);
                        if (bint >= 0 && bint <= 255)
                        {
                            b = (byte)bint;
                        }
                    }
                    catch { }
                }

                if (timestring != null && timestring != "")
                {
                    try
                    {
                        time = int.Parse(timestring);
                        if (time < 0) time = 0;
                    }
                    catch { }
                }
            }
            catch { }

            TimeSpan duration = new TimeSpan(0, 0, time);
            RemoteControlLedEndTime = GT.Timer.GetMachineTime() + duration;
            GT.Timer rcEndTimer = new GT.Timer(duration);
            rcEndTimer.Behavior = GT.Timer.BehaviorType.RunOnce;
            rcEndTimer.Tick += (t) => hgd.SetLed();
            rcEndTimer.Start();

            multicolorLed.TurnColor(new GT.Color(r, g, b));

            responder.Respond("Setting LED to r=" + r + " g=" + g + " b=" + b + " for t=" + time + " secs");
        }
        TimeSpan RemoteControlLedEndTime = TimeSpan.Zero;

        /// <summary>
        /// This is called when a wifi network connection to a home network (not setup network) is made (or remade) 
        /// </summary>
        private void HomeNetworkConnectionMade()
        {
            // this is just for test
            WebClient.GetFromWeb("http://research.microsoft.com/en-us/").ResponseReceived += new HttpRequest.ResponseHandler(TestResponseReceived);
        }

        void TestResponseReceived(HttpRequest sender, HttpResponse response)
        {
            if (response.StatusCode.Substring(0, 3) == "200")
            {
                multicolorLed.FadeOnce(GT.Color.White, new TimeSpan(0, 0, 1), GT.Color.Green);
            }
            else
            {
                multicolorLed.FadeOnce(GT.Color.Black, new TimeSpan(0, 0, 1), GT.Color.Green);
            }

            Debug.Print("Test web request: response status " + response.StatusCode);
        }

        void MoistureWebEventReceived(string path, HomeOSGadgeteer.Networking.WebServer.HttpMethod method, HomeOSGadgeteer.Networking.Responder responder)
        {
            string response = "{ \"DeviceId\" : \"" + hgd.IdentifierString + "\", \"moisture\" : " + moistureSensor.GetMoistureReading() + "}";
            Debug.Print("Moisture web event from " + responder.ClientEndpoint + " - response " + response);
            responder.Respond(response); 
        }

        void moistureTimer_Tick(GT.Timer timer)
        {
            int moisture = moistureSensor.GetMoistureReading();
            Debug.Print("Moisture: " + moisture.ToString());
        }
        
        #endregion

    }
}
