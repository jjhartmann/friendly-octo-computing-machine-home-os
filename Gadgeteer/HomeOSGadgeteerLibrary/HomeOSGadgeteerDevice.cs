using System;
using Microsoft.SPOT;
using GHI.Premium.Net;
using System.Collections;
using GT=Gadgeteer;
using GTM=Gadgeteer.Modules;
using System.Threading;
using System.Text;
using System.Net.Sockets;
using System.Net;
using HomeOSGadgeteer.Networking;
using System.IO.Ports;

namespace HomeOSGadgeteer
{
    /// <summary>
    /// A HomeOS device library supporting assocation with HomeOS hubs, using wifi, serial or storage devices (USB host, SD card)
    /// </summary>
    public class HomeOSGadgeteerDevice
    {
        static HomeOSGadgeteerDevice()
        {
            // this waits at boot time.  The wifi hogs the processor when it starts up, so by waiting here we can allow reprogramming (since the processor hogging blocks reprogramming)
            Debug.Print("Static pausing 1s");
            Thread.Sleep(1000);
        }

        
        /// <summary>
        /// A delegate which returns a boolean 
        /// </summary>
        /// <returns></returns>
        public delegate bool BoolDelegate();

        /// <summary>
        /// A delegate which returns void
        /// </summary>
        public delegate void VoidDelegate();

        GTM.GHIElectronics.WiFi_RS21 wifi;
        GTM.GHIElectronics.MulticolorLed led;
        GTM.Module.DisplayModule display;

        BoolDelegate SkipWifiTimer;
        BoolDelegate DoNotControlScreen;
        BoolDelegate DoNotControlLed;

        BoolDelegate defaultFalse = new BoolDelegate( () => false );

        /// <summary>
        /// Creates a HomeOS device library instance supporting assocation with HomeOS hubs, using wifi, serial or storage devices (USB host, SD card) 
        /// to transfer credentials.   
        /// </summary>
        /// <param name="manufacturer">A string identifying the manufacturer, e.g. "Microsoft Research"</param>
        /// <param name="deviceTypeIdentifier">A string identifying the device type, e.g. "MoistureSensor"</param>
        /// <param name="setupWifiAuthCode">The authentication code used for wifi setup networks to prove that the caller has physical access to this device (it should be written on the device)</param>
        /// <param name="wifi">The instance of the wifi module.  Cannot be null.</param>
        /// <param name="led">The instance of an LED module to show the wifi connection state.  Can be null.  The LED will not be controlled if the doNotControlLed delegate returns true.</param>
        /// <param name="display">The instance of a display module to show the wifi connection state.  Can be null.  The display will not be controlled if the doNotControlLed delegate returns true.</param>
        /// <param name="serialPort">The serial port to use to receive wifi network credentials.  Can be null.</param>
        /// <param name="doNotManageWifi">A delegate specifying when NOT to manage wifi, e.g. allowing the program to stop wifi scanning and assocation for a time.  Can be null.</param>
        /// <param name="doNotControlScreen">A delegate specifying when NOT to control the screen (i.e. if your program wants to control the screen)</param>
        /// <param name="doNotControlLed">A delegate specifying when NOT to control the led (i.e. if your program wants to control the led)</param>
        /// <param name="enableWifiSetupNetworks">Enable wifi-based discovery using "setup" network</param>
        /// <param name="doUDPDiscovery">Listens and responds to UDP packets from HomeOS trying to discover this device, over both "setup" and home networks.  If this is off, the device will still beacon periodically over UDP.  Normally leave it on.</param>
        public HomeOSGadgeteerDevice(
            string manufacturer,
            string deviceTypeIdentifier,
            string setupWifiAuthCode,

            GTM.GHIElectronics.WiFi_RS21 wifi,
            GTM.GHIElectronics.MulticolorLed led = null,
            GTM.Module.DisplayModule display = null,
            string serialPort = null,
            BoolDelegate doNotManageWifi = null,
            BoolDelegate doNotControlScreen = null,
            BoolDelegate doNotControlLed = null,

            bool enableWifiSetupNetworks = true,
            bool doUDPDiscovery = true

            )
        {
            if (manufacturer == null) throw new ArgumentNullException("manufacturer");
            if (deviceTypeIdentifier == null) throw new ArgumentNullException("deviceTypeIdentifier");
            if (setupWifiAuthCode == null) throw new ArgumentNullException("setupAuthCode");
            if (wifi == null) throw new ArgumentNullException("wifi");
            this.wifi = wifi;
            this.led = led;
            this.display = display;
            if (display != null)
            {
                display.SimpleGraphics.AutoRedraw = false;
            }

            if (serialPort != null)
            {
                new Thread(() => SerialReadThread(serialPort)).Start();
            }

            this.SkipWifiTimer = doNotManageWifi ?? defaultFalse;
            this.DoNotControlScreen = doNotControlScreen ?? defaultFalse;
            this.DoNotControlLed = doNotControlLed ?? defaultFalse;

            this.DoUDPDiscovery = doUDPDiscovery;
            EnableWifiSetupNetworks = enableWifiSetupNetworks;
            this.SetupAuthCode = setupWifiAuthCode;
            this.TypeIdentifier = deviceTypeIdentifier;
            this.Manufacturer = manufacturer;

            wifi.Interface.NetworkInterface.EnableDhcp();
            wifi.Interface.WirelessConnectivityChanged += Interface_WirelessConnectivityChanged;
            wifi.Interface.NetworkAddressChanged += Interface_NetworkAddressChanged;

            if (!wifi.Interface.IsOpen)
                wifi.Interface.Open();
            NetworkInterfaceExtension.AssignNetworkingStackTo(wifi.Interface);


            Thread thread = new Thread(DiscoveryThread);
            thread.Start();

            LoadData();

            wifiTimer = new GT.Timer(WifiScanPeriod);
            wifiTimer.Tick += wifiTimer_Tick;
            wifiTimer.Start();

            WebServer.SetupWebEvent(webPath).WebEventReceived += new WebEvent.ReceivedWebEventHandler(CredentialsWebEventReceived);

            SetLed();
            SetScreen();

            GT.Program.BeginInvoke(new VoidDelegate(() => wifiTimer_Tick(wifiTimer)), null);
        }

        #region parameters

        /// <summary>
        /// Determines whether we do UDP discovery (we respond to polls on the DiscoveryPort by broadcasting our ID on DiscoveryResponsePort)
        /// Even if this is off, we still do periodic beacons on BroadcastPort
        /// </summary>
        bool DoUDPDiscovery = true;

        /// <summary>
        /// Enables wifi to use setup networks to get password
        /// </summary>
        internal static bool EnableWifiSetupNetworks = true;

        /// <summary>
        /// A unique key, used as the secure setup network WPA key
        /// </summary>
        string SetupAuthCode = "abcdefgh";

        /// <summary>
        /// The class identifier (stays constant)
        /// </summary>
        string ClassIdentifier = "HomeOSGadgeteerDevice";

        /// <summary>
        /// The identifier of the type of device
        /// </summary>
        string TypeIdentifier = "";


        /// <summary>
        /// The manufacturer of this type of device
        /// </summary>
        string Manufacturer = "MicrosoftResearch";

        public const char IdentifierStringSeparator = '_';

        /// <summary>
        /// Gets the full identifier (including unique ID) for this device
        /// </summary>
        public string IdentifierString 
        { 
            get
            { 
                return ClassIdentifier + IdentifierStringSeparator + 
                        TypeIdentifier + IdentifierStringSeparator + 
                        Manufacturer + IdentifierStringSeparator + 
                        DeviceUniqueID; 
            } 
        }

        /// <summary>
        /// The UDP message sent periodically when connected to the home network
        /// </summary>
        byte[] BroadcastMessage { get { return Encoding.UTF8.GetBytes(IdentifierString); } }

        /// <summary>
        /// The serial message sent periodically
        /// </summary>
        byte[] BroadcastMessageWithNewline { get { return Encoding.UTF8.GetBytes(IdentifierString + "\n"); } }

        /// <summary>
        /// The port to use for broadcast UDP packets for discovery. Port number is "gthos" in telephone digits
        /// </summary>
        const int BeaconPort = 48469;

        /// <summary>
        /// This UDP port is listened to; if any traffic received them a UDP broadcast response on BroadcastPort is sent.
        /// </summary>
        const int DiscoveryPort = 48468;

        /// <summary>
        /// This UDP port is listened to; if any traffic received them a UDP broadcast response on BroadcastPort is sent.
        /// </summary>
        const int DiscoveryResponsePort = 48467;

        /// <summary>
        /// The time between WiFi scans when unassociated, and discovery broadcasts when connected to a home network
        /// </summary>
        TimeSpan WifiScanPeriod = new TimeSpan(0, 0, 5);

        /// <summary>
        /// Filename on storage device that we look for credentials in.
        /// </summary>
        public readonly string CredentialsFilename = "credentials.txt";

        /// <summary>
        /// Web path that we use to receive credentials 
        /// </summary>
        string webPath = "credentials";

        #endregion


        [Serializable]
        internal class APDetails
        {
            public string SSID;
            public string Key;

            /// <summary>
            /// Whether this AP name is a prefix - i.e. any real AP's SSID which starts with this.SSID matches 
            /// </summary>
            public bool Prefix;

            public APDetails(string ssid = "", string key = "", bool prefix = false)
            {
                if (ssid.Length > 32)
                {
                    var truncated = ssid.Substring(0, 32);
                    Debug.Print("Truncating SSID " + ssid + " to 32 chars: " + truncated);
                    ssid = truncated;
                }
                SSID = ssid;
                Key = key;
                Prefix = prefix;
            }

            internal bool Matches(WiFiNetworkInfo ap)
            {
                if (ap == null) return false;
                if (!Prefix)
                {
                    return ap.SSID == SSID;
                }
                return ap.SSID.Length >= SSID.Length && ap.SSID.Substring(0, SSID.Length) == SSID;
            }


            /// <summary>
            /// The number of "setup" related APs
            /// </summary>
            public static int NumSetupAPs { get { return HomeOSGadgeteerDevice.EnableWifiSetupNetworks ? 3 : 0; } } 

            /// <summary>
            /// A setup related AP with no key
            /// </summary>
            public static APDetails SetupOpenAP = new APDetails("setupopen", "");

            /// <summary>
            /// A setup related AP with a well-known secret
            /// </summary>
            public static APDetails SetupSecretAP = new APDetails("setup", "helloworld");

            /// <summary>
            /// A setup related AP which is a prefix, allowing each HomeOS hub to use a distinct AP name with this prefix
            /// </summary>
            public static APDetails SetupSecretAPNew = new APDetails("HomeOS-setup", "helloworld", true);


            internal static bool IsSetupAP(APDetails ap)
            {
                if (ap == null) return false;
                return ap == APDetails.SetupSecretAPNew || ap == APDetails.SetupSecretAP || ap == APDetails.SetupOpenAP;
            }

            internal static bool IsSetupAP(WiFiNetworkInfo ap)
            {
                return SetupSecretAPNew.Matches(ap) || SetupSecretAP.Matches(ap) || SetupOpenAP.Matches(ap);
            }

            internal static void AddSetupAPs(ArrayList wifiNetworks)
            {
                wifiNetworks.Insert(0, APDetails.SetupSecretAPNew);
                wifiNetworks.Insert(1, APDetails.SetupSecretAP);
                wifiNetworks.Insert(2, APDetails.SetupOpenAP);
            }
        }


        [Serializable]
        private class Config
        {
            public string UniqueID;
            public APDetails[] APDetails;
        }

        
        #region Save/Load

        private ExtendedWeakReference ConfigEWR;

        private void SaveData()
        {
            Config config = new Config();
            config.UniqueID = DeviceUniqueID;
            if (wifiNetworks.Count > APDetails.NumSetupAPs)
            {
                config.APDetails = new APDetails[wifiNetworks.Count - APDetails.NumSetupAPs];
                for (int i = 0; i < wifiNetworks.Count - APDetails.NumSetupAPs; i++)
                {
                    config.APDetails[i] = wifiNetworks[i] as APDetails;
                }
            }
            ConfigEWR.Target = config;
            Debug.Print("Credentials data saved");
        }

        private void LoadData()
        {
            ConfigEWR = ExtendedWeakReference.RecoverOrCreate(typeof(Config), 0, ExtendedWeakReference.c_SurviveBoot | ExtendedWeakReference.c_SurvivePowerdown);
            ConfigEWR.Priority = (int)ExtendedWeakReference.PriorityLevel.Critical;

            wifiNetworks.Clear();
            if(EnableWifiSetupNetworks) APDetails.AddSetupAPs(wifiNetworks);

            Config config = ConfigEWR.Target as Config;
            if (config == null)
            {
                config = new Config();
                config.UniqueID = "";
                var random = new Random();
                for (int i = 0; i < 20; i++) { config.UniqueID += random.Next(10).ToString(); }
                ConfigEWR.Target = config;
                Debug.Print("Created new configuration in flash");
            }
            else
            {
                if (config.APDetails != null) for (var i = 0; i < config.APDetails.Length; i++)
                    {
                        wifiNetworks.Insert(i, config.APDetails[i]);
                        Debug.Print("Loaded WiFi config for SSID " + config.APDetails[i].SSID);
                    }
            }
            DeviceUniqueID = config.UniqueID;
            Debug.Print("Device unique ID is " + DeviceUniqueID);
        }

        /// <summary>
        /// The device's unique ID, which is automatically and randomly generated at first boot time, and then is stored persistently. 
        /// </summary>
        public string DeviceUniqueID { get; private set; } 

        #endregion

        #region WiFi
        WiFiNetworkInfo currentAP = null;
        readonly ArrayList wifiNetworks = new ArrayList();
        int wifiUpTime = 0;
        const int wifiUpTimeout = 5;
        TimeSpan maxTimeOnSetupNetwork = new TimeSpan(0, 5, 0);

        TimeSpan joinTime = TimeSpan.Zero;
        void wifiTimer_Tick(GT.Timer timer)
        {
            Debug.Print("wifiTimer start - currentAP " + (currentAP != null ? currentAP.SSID : "(null)") + " wifiUp " + wifiConnected + (wifiConnected ? " IP " + wifi.Interface.NetworkInterface.IPAddress : ""));
            if (sp != null)
            {
                var spMessage = BroadcastMessageWithNewline;
                sp.Write(spMessage, 0, spMessage.Length);
            }
            if (SkipWifiTimer())
            {
                Debug.Print("aborting wifitimer tick since SkipWifiTimer delegate is true");
                return;
            }

            if (wifiConnected && currentAP == null)
            {
                Debug.Print("ERROR: wifi is up but current AP is null! Disconnecting.");
                currentAP = null;
                wifiConnected = false;
                SetLed();
                SetScreen();
                wifi.Interface.Disconnect();
                WebServer.StopLocalServer();
                return;
            }

            if (currentAP != null && !wifiConnected)
            {
                wifiUpTime++;
                if (wifiUpTime == wifiUpTimeout)
                {
                    Debug.Print("WARN: starting wifi for " + currentAP.SSID + " timeout - resetting currentAP");
                    currentAP = null;
                    wifiConnected = false;
                    SetLed();
                    SetScreen();
                    wifi.Interface.Disconnect();
                    WebServer.StopLocalServer();
                    return;
                }
            }

            if (wifiConnected && currentAP != null && APDetails.IsSetupAP(currentAP))
            {
                if (GT.Timer.GetMachineTime() - joinTime > maxTimeOnSetupNetwork)
                {
                    Debug.Print("Disconnecting from setup AP due to timeout (setup should not take this long)");
                    currentAP = null;
                    wifiConnected = false;
                    SetLed();
                    SetScreen();
                    wifi.Interface.Disconnect();
                    WebServer.StopLocalServer();
                    return;
                }
                else
                {
                    // only sending unsolicited beacons when we're not on the home network
                    Debug.Print("Sending unsolicited UDP beacon since we're on a setup network");
                    SendUDPBroadcast(BeaconPort);
                }
            }

            if (wifiConnected && currentAP != null)
            {
                if (!WebServer.IsRunning()) WebServer.StartLocalServer(wifi.Interface.NetworkInterface.IPAddress, 80);
            }

            // skip scanning when connected since it fails
            if (currentAP == null)
            {

                try
                {
                    var scan = wifi.Interface.Scan();
                    Thread.Sleep(1);
                    foreach (var ap in scan)
                    {
                        Debug.Print("Scanned AP " + ap.SSID + " rssi " + ap.RSSI);
                    }
                    if (scan != null) foreach (APDetails knownAP in wifiNetworks)
                        {
                            // APs are in priority order, so break once we see the current ap
                            if (knownAP.Matches(currentAP)) break;
                            WiFiNetworkInfo joinAP = null;
                            foreach (var scanAP in scan)
                            {
                                if (!knownAP.Matches(scanAP)) continue;
                                if (joinAP == null) joinAP = scanAP;
                                if (joinAP.RSSI > scanAP.RSSI) joinAP = scanAP; // lower RSSI is better
                            }
                            if (joinAP == null) continue;

                            try
                            {

                                if (currentAP != null)
                                {
                                    Debug.Print("Disconnecting from WiFi network " + currentAP + " to join " + joinAP.SSID);
                                    wifi.Interface.Disconnect();
                                }

                                //// stop pictimeout and any streaming since wifi join operation hogs processor 
                                //picTimeout.Stop();
                                //camera.StopStreamingBitmaps();

                                joinTime = GT.Timer.GetMachineTime();
                                Debug.Print("Joining WiFi network " + joinAP.SSID + " rssi " + joinAP.RSSI);
                                //wifi.Interface.();
                                currentAP = joinAP;
                                SetLed();
                                SetScreen();
                                wifi.Interface.Join(joinAP, knownAP.Key);
                                //Debug.Print("Joined WiFi network " + scanAP.SSID);
                                wifiUpTime = 0;
                                break;
                            }
                            catch
                            {
                                Debug.Print("Error joining wifi network: " + joinAP.SSID);
                                wifi.Interface.Disconnect();
                                timer.Stop();
                                timer.Start();
                                currentAP = null;
                                SetLed();
                                SetScreen();
                                break;
                            }
                        }
                }
                catch (GHI.Premium.Net.NetworkInterfaceExtensionException wex)
                {
                    wifi.Interface.Disconnect();
                    // restart the timer to prevent it immediately firing again
                    timer.Stop();
                    timer.Start();
                    Debug.Print("Error scanning wifi networks: " + wex);
                    currentAP = null;
                    setLedError();
                    setScreenError();
                }
            }
            Debug.Print("wifiTimer end - currentAP " + (currentAP != null ? currentAP.SSID : "(null)") + " wifiUp " + wifiConnected);
        }

        GT.Timer wifiTimer;

        bool wifiConnected = false;

        void Interface_WirelessConnectivityChanged(object sender, GHI.Premium.Net.WiFiRS9110.WirelessConnectivityEventArgs e)
        {
            Debug.Print("wireless connectivity changed. IP address " + wifi.Interface.NetworkInterface.IPAddress + " network connected = " + wifi.Interface.IsLinkConnected + " activated = " + wifi.Interface.IsActivated + " open= " + wifi.Interface.IsOpen);
        }


        void Interface_NetworkAddressChanged(object sender, EventArgs e)
        {
            try
            {
                if (wifi.Interface.IsLinkConnected)
                {
                    Debug.Print("WiFi up with AP " + currentAP.SSID);
                    Debug.Print("IP Address: " + wifi.Interface.NetworkInterface.IPAddress);
                    Debug.Print("Subnet Mask: " + wifi.Interface.NetworkInterface.SubnetMask);
                    Debug.Print("Gateway: " + wifi.Interface.NetworkInterface.GatewayAddress);

                    var dnsAddresses = wifi.Interface.NetworkInterface.DnsAddresses;
                    Debug.Print("DNS Server: " + (dnsAddresses == null ? "NULL" : (dnsAddresses.Length == 0 ? "none" : dnsAddresses[0])));
                    Debug.Print("Elapsed time from join: " + (GT.Timer.GetMachineTime() - joinTime).Seconds + "s");
                    wifiConnected = true;
                    SetScreen();
                    SetLed();
                    //SendUDPBroadcast();

                    WebServer.StartLocalServer(wifi.Interface.NetworkInterface.IPAddress, 80);

                    if (ConnectedToHomeNetwork)
                    {
                        OnHomeNetworkJoined();
                    }
                }
                else
                {
                    wifiConnected = false;
                    WebServer.StopLocalServer();
                    SetLed();
                    SetScreen();
                    Debug.Print("Wifi down");
                    if (firstDown)
                    {
                        firstDown = false;
                    }
                    else
                    {
                        currentAP = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Exception in Interface_WirelessConnectivityChanged: " + ex);
            }
        }

        /// <summary>
        /// This event is called when the home network is connected to, enabling actions that need to happen when network connectivity is available.
        /// </summary>
        public event VoidDelegate HomeNetworkJoined;
        private VoidDelegate onHomeNetworkJoined;

        private void OnHomeNetworkJoined()
        {
            if (onHomeNetworkJoined == null) onHomeNetworkJoined = new VoidDelegate(OnHomeNetworkJoined);
            if (GT.Program.CheckAndInvoke(HomeNetworkJoined, onHomeNetworkJoined, null)) HomeNetworkJoined();
        }

        void DiscoveryThread()
        {
            if (!DoUDPDiscovery)
            {
                Debug.Print("Not setting up UDP beacon request listener (beacons still sent periodically on port " + BeaconPort + ")");
                return;
            }
            Debug.Print("Setting up UDP beacon request listener on port " + DiscoveryPort);

            byte[] buf = new byte[2000];

            var DiscoverySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //DiscoverySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            DiscoverySocket.Bind(new IPEndPoint(IPAddress.Any, DiscoveryPort));
            DiscoverySocket.ReceiveTimeout = Timeout.Infinite;

            while (true)
            {
                try
                {
                    if (!wifiConnected)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    EndPoint ep = new IPEndPoint(IPAddress.Any, DiscoveryPort);
                    DiscoverySocket.ReceiveFrom(buf, ref ep);
                    if (ep is IPEndPoint)
                    {
                        var iep = ep as IPEndPoint;
                        if (iep.Address.GetAddressBytes()[0] == 127) continue;
                        Debug.Print("Sending UDP broadcast on port " + DiscoveryResponsePort + " in response to UDP packet from " + iep.Address + ":" + iep.Port);
                        SendUDPBroadcast(DiscoveryResponsePort);
                    }
                    else
                    {
                        Debug.Print("Received UDP packet from non-IP endpoint?!");
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print("DiscoveryThread: Exception " + ex);
                }
            }
        }

        Socket BroadcastSocket = null;
        void SendUDPBroadcast(int port)
        {
            if (BroadcastSocket == null)
            {
                BroadcastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                BroadcastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            }
            BroadcastSocket.SendTo(BroadcastMessage, new IPEndPoint(IPAddress.Parse("255.255.255.255"), port));
            //BroadcastSocket.Close();
        }

        /// <summary>
        /// Whether the device has been provided by the Home Hub with a set of Home AP credentials
        /// </summary>
        public bool WiFiCredentialsKnown
        {
            get
            {
                if (wifiNetworks.Count == 0) return false;
                APDetails apd = wifiNetworks[0] as APDetails;
                if (apd == null) return false;
                if (APDetails.IsSetupAP(apd)) return false;
                return true;
            }
        }

        /// <summary>
        /// Whether the device is connected to a home network (i.e. it is connected to wifi and not just connected to a "setup" network)
        /// </summary>
        public bool ConnectedToHomeNetwork
        {
            get
            {
                return wifiConnected && currentAP != null && !APDetails.IsSetupAP(currentAP);
            }
        }

        bool firstDown = true;

        #endregion


        #region screen control
        Font font = Resources.GetFont(Resources.FontResources.small);

        /// <summary>
        /// Causes the screen to be redrawn, showing the current WiFi status.  The screen is not touched if the DoNotControlScreen delegate returns true.
        /// This method is called internally by the library, but is exposed for use when the conditions for DoNotControlScreen stop, i.e. when control is
        /// returned to this library from the application. 
        /// </summary>
        public void SetScreen()
        {
            if (display == null) return;
            if (DoNotControlScreen()) return;

            //GT.Color border = GT.Color.Green; // can press button
            string text1 = "", text2 = "", text3 = "";
            //Bitmap image;
            if (currentAP == null)
            {
                // no suitable AP seen
                if (WiFiCredentialsKnown)
                {
                    //image = uiScanningForHomeAP;
                    text1 = "Searching for";
                    text2 = "home AP"; 
                }
                else
                {
                    if (EnableWifiSetupNetworks)
                    {
                        //image = uiScanningForSetupAP;
                        text1 = "Searching for";
                        text2 = "setup AP";
                    }
                    else 
                    {
                        text1 = "Waiting for";
                        text2 = "credentials";
                    }
                }
            }
            else if (APDetails.IsSetupAP(currentAP))
            {
                if (wifiConnected)
                {
                    // joined setup AP
                    if (WiFiCredentialsKnown)
                    {
                        //image = uiReceivedHomeAPDetails;
                        text1 = "Got credentials";
                        text2 = "for AP:";
                        text3 = ((APDetails)wifiNetworks[0]).SSID;
                    }
                    else
                    {
                        //image = uiJoinedSetupAP;
                        text1 = "Joined";
                        text2 = "setup AP";
                    }
                }
                else
                {
                    //image = uiJoiningSetupNetwork;
                    text1 = "Joining";
                    text2 = "setup AP";
                    //border = GT.Color.Gray;
                }
            }
            else
            {
                if (wifiConnected)
                {
                    //image = uiJoinedHomeAP;
                    // joined a real AP
                    text1 = "Joined AP:";
                    text2 = currentAP.SSID;
                }
                else
                {
                    // trying to join a real AP
                    //image = uiJoiningHomeNetwork;
                    text1 = "Joining AP:";
                    text2 = currentAP.SSID;
                    //border = GT.Color.Gray;
                }
            }
            //oledDisplay.SimpleGraphics.DisplayRectangle(border, 10, GT.Color.Black, 0, 0, 128, 128);
            display.SimpleGraphics.Clear();
            display.SimpleGraphics.DisplayText(text1, font, GT.Color.White, 20, 20);
            if (text2 != "") display.SimpleGraphics.DisplayText(text2, font, GT.Color.White, 20, 50);
            if (text3 != "") display.SimpleGraphics.DisplayText(text3, font, GT.Color.White, 20, 80);
            //oledDisplay.SimpleGraphics.DisplayImage(image, 0, 0);
            display.SimpleGraphics.Redraw();

        }

        
            //uiScanningForHomeAP = Resources.GetBitmap(Resources.BitmapResources.ScanningforhomeAP128x128);
            //uiScanningForSetupAP = Resources.GetBitmap(Resources.BitmapResources.ScanningforsetupAP128x128);
            //uiReceivedHomeAPDetails = Resources.GetBitmap(Resources.BitmapResources.ReceivedhomeAPdetails128x128);
            //uiJoiningHomeNetwork = Resources.GetBitmap(Resources.BitmapResources.Joininghomenetwork128x128);
            //uiJoiningSetupNetwork = Resources.GetBitmap(Resources.BitmapResources.Joiningsetupnetwork128x128);
            //uiJoinedHomeAP = Resources.GetBitmap(Resources.BitmapResources.JoinedhomeAP);
            //uiJoinedSetupAP = Resources.GetBitmap(Resources.BitmapResources.JoinedsetupAP);


        void setScreenError()
        {

        }

        #endregion
        #region LED control
        void setLedError()
        {
            if (led == null) return;
            led.BlinkRepeatedly(GT.Color.Cyan, new TimeSpan(0, 0, 1), GT.Color.Yellow, new TimeSpan(0, 0, 1));
        }

        /// <summary>
        /// Causes the led to be controlled, abstractly showing the current WiFi status.  The screen is not touched if the DoNotControlLed delegate returns true.
        /// This method is called internally by the library, but is exposed for use when the conditions for DoNotControlLed stop, i.e. when control is
        /// returned to this library from the application. 
        /// </summary>
        public void SetLed()
        {
            if (led == null) return;
            if (DoNotControlLed()) return;

            if (currentAP == null)
            {
                // no suitable AP seen
                if (WiFiCredentialsKnown)
                {
                    led.FadeRepeatedly(GT.Color.Blue);
                }
                else
                {
                    led.FadeRepeatedly(GT.Color.Red);
                }
            }
            else if (APDetails.IsSetupAP(currentAP))
            {
                if (wifiConnected)
                {
                    // joined setup AP
                    if (WiFiCredentialsKnown)
                    {
                        led.TurnBlue();
                    }
                    else
                    {
                        led.TurnRed();
                    }
                }
                else
                {
                    // trying to join setup AP
                    if (WiFiCredentialsKnown)
                    {
                        led.BlinkRepeatedly(GT.Color.Blue, blinkTime, GT.Color.Black, blinkTime);
                    }
                    else
                    {
                        led.BlinkRepeatedly(GT.Color.Red, blinkTime, GT.Color.Black, blinkTime);
                    }
                }
            }
            else
            {
                if (wifiConnected)
                {
                    // joined a real AP
                    led.TurnGreen();
                }
                else
                {
                    // trying to join a real AP
                    led.BlinkRepeatedly(GT.Color.Green, blinkTime, GT.Color.Black, blinkTime);
                }
            }

        }

        TimeSpan blinkTime = new TimeSpan(0, 0, 0, 0, 250);
        #endregion

        #region methods for receiving credentials

        bool ParseSSIDKey(string line)
        {
            if (line == "") return false;
            var items = line.Split(' ');
            if (items.Length < 2 || items[0].ToLower() != "ssid" || (items.Length > 2 && items[2].ToLower() != "key"))
            {
                return false;
            }
            return ReceivedCredentials(items[1], items.Length < 4 ? "" : items[3]);
        }

        private bool ReceivedCredentials(string ssid, string key)
        {
            foreach (APDetails net in wifiNetworks)
            {
                if (net.SSID == ssid)
                {
                    if (net.Key == key)
                    {
                        Debug.Print("Received duplicate of existing network credentials: ssid " + ssid);
                        return false;
                    }
                    Debug.Print("Updating key for existing SSID " + ssid);
                    net.Key = key;
                    return true;
                }
            }
            Debug.Print("Received new network credentials: ssid " + ssid + " key " + key);
            wifiNetworks.Insert(0, new APDetails(ssid, key));
            return true;
        }

        // secure reception of wifi credentials over wifi network - secured by receiving secret 
        void CredentialsWebEventReceived(string path, WebServer.HttpMethod method, Responder responder)
        {
            if (responder.UrlParameters == null || !responder.UrlParameters.Contains("ssid") || !responder.UrlParameters.Contains("setupauthcode"))
            {
                responder.Respond("ERROR::Syntax: " + path + "?ssid=SSID&setupauthcode=SETUPAUTHCODE[&key=WIFIKEY]");
                return;
            }
            if (((string)responder.UrlParameters["setupauthcode"]) != SetupAuthCode)
            {
                responder.Respond("ERROR::Incorrect setupauthcode");
                return;
            }
            var ssid = responder.UrlParameters["ssid"] as string;
            if (ssid == null || ssid == "")
            {
                responder.Respond("ERROR::Invalid SSID");
                return;
            }
            var key = "";
            if (responder.UrlParameters.Contains("key"))
            {
                key = responder.UrlParameters["key"] as string;
            }
            Debug.Print("Received (via web event) credentials for network " + ssid);
            bool newcreds = ReceivedCredentials(ssid, key);
            if (newcreds)
            {
                responder.Respond("OK::Got new credentials");
                SaveData();
                Thread.Sleep(10);
                WebServer.StopLocalServer();
                currentAP = null;
                SetLed();
                SetScreen();
                wifi.Interface.Disconnect();
            }
            else
            {
                responder.Respond("OK::Already had those");
            }
        }

        SerialPort sp;
        private void SerialReadThread(string portName)
        {
            StringDelegate lineReceivedDelegate = new StringDelegate(SerialLineReceived);
            string outstandingText = String.Empty;
            byte[] buf = new byte[1];
            sp = new SerialPort(portName, 9600, Parity.None, 8, StopBits.One);
            sp.ReadTimeout = 1000;
            sp.Open();

            while (true)
            {
                try
                {
                    int res = sp.Read(buf, 0, 1);

                    if (res <= 0) continue;
                    if (buf[0] == 10)
                    {
                        GT.Program.BeginInvoke(lineReceivedDelegate, outstandingText);
                        outstandingText = String.Empty;
                    }
                    else if (buf[0] < 128)
                    {
                        outstandingText = outstandingText + (char)buf[0];
                    }
                }
                catch
                {
                    Debug.Print("Exception reading serial line");
                    Thread.Sleep(100);
                }
            }
        }

        delegate void StringDelegate(string s);
        string serialInstructions = "\nSyntax: \"ssid <ssid> key <key>\" with newline at end. \"key <key>\" is optional\n";
        void SerialLineReceived(string line)
        {
            if (!ParseSSIDKey(line))
            {
                var instructions = UTF8Encoding.UTF8.GetBytes(serialInstructions);
                var message = BroadcastMessageWithNewline;
                sp.Write(message, 0, message.Length);
                sp.Write(instructions, 0, instructions.Length);
            }
            else
            {
                SaveData();
            }
        }

        string storageInstructions = "// Put SSID and key for home wifi access point(s) below in the format \"ssid <ssid> key <key>\" with newline at end. \"key <key>\" is optional. Lines starting // are ignored.\n";
        
        /// <summary>
        /// The users program should call this if a flash storage device has been attached which may contain credentials for the home network in the file specified by CredentialsFilename  
        /// The flash storage wil be searched for the CredentialsFilename file, and if this does not exist it will be created with instructions for use.
        /// </summary>
        /// <param name="storageDevice">The storage device.</param>
        public void StorageAttached(GT.StorageDevice storageDevice)
        {
            var file = storageDevice.Open(CredentialsFilename, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite);
            if (file.Length == 0)
            {
                // write to file
                file.Write(Encoding.UTF8.GetBytes(storageInstructions), 0, storageInstructions.Length);
                file.Close();
                Debug.Print("No credentials file on storage device; wrote instructions");
            }
            else
            {
                byte[] buf = new byte[(int)file.Length];
                int len = file.Read(buf, 0, (int)file.Length);
                if (len != file.Length)
                {
                    Debug.Print("Error: cannot read credentials file " + CredentialsFilename);
                }
                else
                {
                    string fileContents = new string(Encoding.UTF8.GetChars(buf));
                    bool newnetwork = false;
                    foreach (string s in fileContents.Split('\n'))
                    {
                        var strim = s.Trim();
                        if (strim.Length < 2 || strim.Substring(0, 2) == "//") continue;
                        if (ParseSSIDKey(strim))
                        {
                            newnetwork = true;
                        }
                        else
                        {
                            Debug.Print("Error: invalid line in credentials file: " + strim);
                        }
                    }
                    if (newnetwork)
                    {
                        SaveData();
                        SetLed();
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// Should be called by an application when the home network credentials should be forgotten, e.g. after a reset button is held down.
        /// </summary>
        public void ResetNetworkCredentials()
        {
            Debug.Print("Forgetting home wireless network credentials");

            wifiNetworks.Clear();
            if(EnableWifiSetupNetworks) APDetails.AddSetupAPs(wifiNetworks);
            SaveData();

            WebServer.StopLocalServer();
            currentAP = null;
            SetLed();
            wifi.Interface.Disconnect();
        }

        /// <summary>
        /// Configures the web server to handle GET requests to a particular path, e.g. http://{device's IP addres}/path.
        /// The resulting WebEvent has a "WebEventReceived" event which is fired when a web request is received.
        /// </summary>
        /// <param name="path">The path to handle</param>
        /// <returns></returns>
        public WebEvent SetupWebEvent(string path)
        {
            return WebServer.SetupWebEvent(path);
        }

        /// <summary>
        /// Configures the web server to handle GET requests to a particular path, e.g. http://{device's IP addres}/path.
        /// The resulting WebEvent has a "WebEventReceived" event which is fired when a web request is received.
        /// All responses to this event automatically have an HTTP header added asking for auto-refresh after a delay
        /// </summary>
        /// <param name="path">The path to handle</param>
        /// <param name="refreshAfter">The time in seconds</param>
        /// <returns></returns>
        public WebEvent SetupWebEvent(string path, uint refreshAfter)
        {
            return WebServer.SetupWebEvent(path, refreshAfter);
        }
    }
}
