//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MoistureSensor {
    using Gadgeteer;
    using GTM = Gadgeteer.Modules;
    
    
    public partial class Program : Gadgeteer.Program {
        
        /// <summary>The MulticolorLed module using socket 11 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.MulticolorLed multicolorLed;
        
        /// <summary>The MoistureSensor module using socket 10 of the mainboard.</summary>
        private Gadgeteer.Modules.Seeed.MoistureSensor moistureSensor;
        
        /// <summary>The WiFi_RS21 (Premium) module using socket 9 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.WiFi_RS21 wifi;
        
        /// <summary>The Button module using socket 4 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.Button resetButton;
        
        /// <summary>The UsbClientDP module using socket 1 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.UsbClientDP usbClientDP;
        
        /// <summary>The UsbHost (Premium) module using socket 3 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.UsbHost usbHost;
        
        /// <summary>The UsbSerial module using socket 8 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.UsbSerial usbSerial;
        
        /// <summary>The SDCard module using socket 5 of the mainboard.</summary>
        private Gadgeteer.Modules.GHIElectronics.SDCard sdCard;
        
        /// <summary>This property provides access to the Mainboard API. This is normally not necessary for an end user program.</summary>
        protected new static GHIElectronics.Gadgeteer.FEZSpider Mainboard {
            get {
                return ((GHIElectronics.Gadgeteer.FEZSpider)(Gadgeteer.Program.Mainboard));
            }
            set {
                Gadgeteer.Program.Mainboard = value;
            }
        }
        
        /// <summary>This method runs automatically when the device is powered, and calls ProgramStarted.</summary>
        public static void Main() {
            // Important to initialize the Mainboard first
            Program.Mainboard = new GHIElectronics.Gadgeteer.FEZSpider();
            Program p = new Program();
            p.InitializeModules();
            p.ProgramStarted();
            // Starts Dispatcher
            p.Run();
        }
        
        private void InitializeModules() {
            this.usbClientDP = new GTM.GHIElectronics.UsbClientDP(1);
            this.usbHost = new GTM.GHIElectronics.UsbHost(3);
            this.resetButton = new GTM.GHIElectronics.Button(4);
            this.sdCard = new GTM.GHIElectronics.SDCard(5);
            this.usbSerial = new GTM.GHIElectronics.UsbSerial(8);
            this.wifi = new GTM.GHIElectronics.WiFi_RS21(9);
            this.moistureSensor = new GTM.Seeed.MoistureSensor(10);
            this.multicolorLed = new GTM.GHIElectronics.MulticolorLed(11);
        }
    }
}
