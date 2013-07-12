using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Modules.Seeed;
using System.Text;
using System.IO.Ports;
using HomeOSGadgeteer;

namespace WindowCamera
{
    public partial class Program
    {
        HomeOSGadgeteerDevice hgd;
        
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            picTimeout = new GT.Timer(30000, GT.Timer.BehaviorType.RunOnce);
            picTimeout.Tick += new GT.Timer.TickEventHandler(picTimeout_Tick);

            hgd = new HomeOSGadgeteerDevice("MicrosoftResearch", "WindowCamera", "abcdefgh", wifi, multicolorLed, oledDisplay, null, () => button.IsPressed, () => picTimeout.IsRunning);
            hgd.HomeNetworkJoined += hgd_HomeNetworkJoined;

            button.LEDMode = Button.LEDModes.OnWhilePressed;
            button.ButtonPressed += new Button.ButtonEventHandler(resetButton_ButtonPressed);
            button.ButtonReleased += new Button.ButtonEventHandler(resetButton_ButtonReleased);

            //usbHost.USBDriveConnected += (sender, storagedevice) => hgd.StorageAttached(storagedevice);
            //sdCard.SDCardMounted += (sender, storagedevice) => hgd.StorageAttached(storagedevice);

            camera.CurrentPictureResolution = Camera.PictureResolution.Resolution160x120;
            camera.BitmapStreamed += new Camera.BitmapStreamedEventHandler(camera_BitmapStreamed);

            hgd.SetupWebEvent("webcam").WebEventReceived += WebcamEventReceived;
            hgd.SetupWebEvent("webcamauto", 1).WebEventReceived += WebcamEventReceived;

            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
            Debug.Print("Program Started");
        }


        Bitmap lastBitmap = new Bitmap(160, 120);

        void picTimeout_Tick(GT.Timer timer)
        {
            picTimeout.Stop();
            hgd.SetScreen();
            Debug.Print("Stream to screen stopped due to timeout");
        }

        TimeSpan WebcamStreamTime = new TimeSpan(0, 0, 0);
        TimeSpan webcamStreamStopTime = TimeSpan.Zero;

        int numresponses = 0;

        void camera_BitmapStreamed(Camera sender, Bitmap bitmap)
        {
            if (!picTimeout.IsRunning && GT.Timer.GetMachineTime() >= webcamStreamStopTime)
            {
                //Debug.Print("Stopping bitmap streaming");
                camera.StopStreamingBitmaps();
            }

            if (responders.Count > 0)
            {
                byte[] bmpFile = new byte[bitmap.Width * bitmap.Height * 3 + 54];
                GHI.Premium.System.Util.BitmapToBMPFile(bitmap.GetBitmap(), bitmap.Width, bitmap.Height, bmpFile);
                GT.Picture picture = new GT.Picture(bmpFile, GT.Picture.PictureEncoding.BMP);

                numresponses++;
                Debug.Print("Sending webcam response " + numresponses  + " to " + responders.Count + " clients");
                foreach (HomeOSGadgeteer.Networking.Responder responder in responders)
                {
                    try
                    {
                        responder.Respond(picture);
                    }
                    catch { }
                }
                responders.Clear();
            }

            if (picTimeout.IsRunning) 
            {
                oledDisplay.SimpleGraphics.DisplayImage(lastBitmap, 0, 0);
                oledDisplay.SimpleGraphics.Redraw();
                Debug.Print("Picture streamed -> screen");
            } 
        }

        /// <summary>
        /// This is called when a wifi network connection to a home network (not setup network) is made (or remade) 
        /// </summary>
        void hgd_HomeNetworkJoined()
        {
        }


        #region button
        TimeSpan resetPressMinDuration = new TimeSpan(0, 0, 10);
        DateTime resetButtonPressTime = DateTime.MaxValue;
        void resetButton_ButtonPressed(Button sender, Button.ButtonState state)
        {
            Debug.Print("Button pressed");
            resetButtonPressTime = DateTime.Now;
        }

        void resetButton_ButtonReleased(Button sender, Button.ButtonState state)
        {
            if (resetButtonPressTime == DateTime.MaxValue)
            {
                Debug.Print("Error: badly-debounced button press detected - ignoring");
                return;
            }

            Debug.Print("Button released");

            if (DateTime.Now - resetButtonPressTime >= resetPressMinDuration)
            {
                hgd.ResetNetworkCredentials();
            }
            else
            {
                if (picTimeout.IsRunning)
                {
                    picTimeout.Stop();
                    hgd.SetScreen();
                    //                oledDisplay.SimpleGraphics.Clear();
                    Debug.Print("Stream to screen stopped due to button");
                }
                else
                {
                    Debug.Print("Stream to screen started");
                    picTimeout.Stop();
                    picTimeout.Start();
                    camera.StartStreamingBitmaps(lastBitmap);
                }
            }
            resetButtonPressTime = DateTime.MaxValue;
        }

        #endregion


        GT.Timer picTimeout;

        ArrayList responders = new ArrayList();
        void WebcamEventReceived(string path, HomeOSGadgeteer.Networking.WebServer.HttpMethod method, HomeOSGadgeteer.Networking.Responder responder)
        {
            if (!hgd.ConnectedToHomeNetwork)
            {
                responder.Respond("Security error: not on home network");
                return;
            }
            responders.Add(responder);
            camera.StartStreamingBitmaps(lastBitmap);
            webcamStreamStopTime = GT.Timer.GetMachineTime() + WebcamStreamTime;
            Debug.Print("Received webcam request - " + responders.Count + " client in queue");
        }
        
    }
}
