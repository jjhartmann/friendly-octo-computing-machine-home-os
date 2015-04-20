using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;

using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.Seeed;
using Gadgeteer.Modules.GHIElectronics;
using GHI.Premium.Net;

namespace TempHumiditySensor
{
    public partial class Program
    {
        HomeOSGadgeteer.HomeOSGadgeteerDevice hgd;

        /// <summary>
        /// The time between checking temp&humidity sensor
        /// </summary>
        const int CheckPeriod = 5 * 1000;

        /// <summary>
        /// The time between restarting device (This device crashed every periodic time)
        /// </summary>
        const int ResetPeriod = 60 * 1000;
        TimeSpan RemoteControlLedEndTime = TimeSpan.Zero;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            hgd = new HomeOSGadgeteer.HomeOSGadgeteerDevice("MicrosoftResearch", "TempHumiditySensor", "abcdefgh",wifi,
                null, null, /*usbSerial.SerialLine.PortName*/null, null, null, () => { return GT.Timer.GetMachineTime() < RemoteControlLedEndTime; }, true);

            //this.joystick.JoystickPressed += joystick_JoystickPressed;

            //hgd.SetupWebEvent("temp").WebEventReceived += this.LightWebEventReceived;
            this.button.TurnLEDOff();
            this.button.ButtonPressed += button_ButtonPressed;

            GT.Timer timer = new GT.Timer(CheckPeriod);
            timer.Tick += Timer_Tick;
            timer.Start();

            GT.Timer timerReboot = new GT.Timer(ResetPeriod);
            timerReboot.Tick += timerReboot_Tick;
            timerReboot.Start();
         

            hgd.SetupWebEvent("temperature").WebEventReceived += TempHumidityWebEventReceived;

            Debug.Print("Program Started");

            this.temperatureHumidity.MeasurementComplete += temperatureHumidity_MeasurementComplete;
            this.temperatureHumidity.StartContinuousMeasurements();

            /*******************************************************************************************
            Modules added in the Program.gadgeteer designer view are used by typing 
            their name followed by a period, e.g.  button.  or  camera.
            
            Many modules generate useful events. Type +=<tab><tab> to add a handler to an event, e.g.:
                button.ButtonPressed +=<tab><tab>
            
            If you want to do something periodically, use a GT.Timer and handle its Tick event, e.g.:
                GT.Timer timer = new GT.Timer(1000); // every second (1000ms)
                timer.Tick +=<tab><tab>
                timer.Start();
            *******************************************************************************************/


            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }

        void button_ButtonPressed(Button sender, Button.ButtonState state)
        {
            Reboot();
        }

        void timerReboot_Tick(GT.Timer timer)
        {
            Reboot();
        }

        void temperatureHumidity_MeasurementComplete(TemperatureHumidity sender, double temperature, double relativeHumidity)
        {
            this.temperature = temperature;
            this.relativeHumidity = relativeHumidity;

            Debug.Print(this.response);
        }

        void TempHumidityWebEventReceived(string path,
            HomeOSGadgeteer.Networking.WebServer.HttpMethod method,
            HomeOSGadgeteer.Networking.Responder responder)
        {
            Debug.Print("Temp&Humidity web event from " + responder.ClientEndpoint + " - response " + this.response);
            responder.Respond(this.webResponse);
        }

        void Timer_Tick(GT.Timer timer)
        {
            Debug.Print(this.response);
        }

        private double temperature;
        private double relativeHumidity;

        private string webResponse
        {
            get
            {
                return "{\"DeviceId\":\"" +
                    hgd.IdentifierString + "\","
                    + "\"temperature\":" + this.temperature.ToString() +
                    "}";
            }
        }

        private string response
        {
            get
            {
                return "{" +
                "\"temperature\" : " + this.temperature.ToString() + "\n" +
                 "\"DeviceIP\" : \"" + this.wifi.NetworkSettings.IPAddress + "\", " +
                 "\"DeviceId\" : \"" + hgd.IdentifierString + "\", " +
                "}";
            }
        }

    }
}
