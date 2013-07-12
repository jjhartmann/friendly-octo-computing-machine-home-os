// -
// <copyright file="Settings.cs" company="Microsoft Corporation">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -

namespace HomeOS.Shared.Gatekeeper
{
    using System;
    using System.Xml;
    using HomeOS.Shared;

    /// <summary>
    /// Defines various settings
    /// The class has default values.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// home id
        /// </summary>
        private static string homeId = null;

        /// <summary>
        /// The name of the host running the HomeOS.Cloud.Platform.Gatekeeper cloud service.
        /// </summary>
        private static string serviceHost = "localhost";

        /// <summary>
        /// The main connection port for the HomeOS.Cloud.Platform.Gatekeeper cloud service.
        /// </summary>
        private static int servicePort = 5002;

        /// <summary>
        /// The port at which client services are available
        /// </summary>
        private static int clientPort = 51431;

        /// <summary>
        /// A hard-coded password to use with basic authentication.
        /// </summary>
        /// <remarks>
        /// ToDo: add a real authentication scheme.
        /// </remarks>
        private static uint homePassword = 7033568;

        /// <summary>
        /// Gets the home id; relevant only for the home side of the service
        /// </summary>
        public static string HomeId
        {
            get { return homeId; }
            set { homeId = value; }
        }

        /// <summary>
        /// Gets the name of the host running the HomeOS.Cloud.Platform.Gatekeeper cloud service.
        /// </summary>
        public static string ServiceHost
        {
            get { return serviceHost; }
            set {serviceHost = value;}
        }

        /// <summary>
        /// Gets the main connection port for the HomeOS.Cloud.Platform.Gatekeeper cloud service.
        /// </summary>
        public static int ServicePort
        {
            get { return Settings.servicePort; }
        }

        /// <summary>
        /// Gets the main client facing port for the HomeOS.Cloud.Platform.Gatekeeper cloud service.
        /// </summary>
        public static int ClientPort
        {
            get { return Settings.clientPort; }
        }

        /// <summary>
        /// Gets a hard-coded password to use with basic authentication.
        /// </summary>
        public static uint HomePassword
        {
            get { return Settings.homePassword; }
        }

        ///// <summary>
        ///// Configures various settings based on the contents of configuration files.
        ///// </summary>
        ///// <param name="directoryName">Path to the directory holding the configuration files.</param>
        //public static void Configure(string directoryName)
        //{
        //    Settings.ReadSettings(directoryName + "\\" + Globals.SettingsFileName);

        //    if (Globals.HomeId == null)
        //    {
        //        Globals.HomeId = new HomeId("DefaultHomeId");
        //    }
        //}

        /// <summary>
        /// Reads the settings from the XML file.
        /// </summary>
        /// <param name="fileName">Path to the settings XML file.</param>
        public static void InitSettingsFromFile(string fileName)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreComments = true;

            XmlReader reader = XmlReader.Create(fileName, xmlReaderSettings);
            xmlDoc.Load(reader);

            XmlElement root = xmlDoc.FirstChild as XmlElement;

            if (!root.Name.Equals("Settings"))
            {
                throw new Exception(fileName + " doesn't start with Settings");
            }

            foreach (XmlElement xmlParam in root.ChildNodes)
            {
                if (!xmlParam.Name.Equals("Param"))
                {
                    throw new Exception("child is not a Param in " + fileName);
                }

                string name = xmlParam.GetAttribute("Name");
                string value = xmlParam.GetAttribute("Value");

                switch (name)
                {
                    case "HomeId":
                        Settings.homeId = value;
                        break;

                    case "HomePassword":
                        Settings.homePassword = uint.Parse(value);
                        break;

                    case "GatekeeperServicePort":
                        int.TryParse(value, out Settings.servicePort);
                        break;

                    case "GatekeeperServiceHost":
                        Settings.serviceHost = value;
                        break;
                }
            }

            reader.Close();
        }
    }
}
