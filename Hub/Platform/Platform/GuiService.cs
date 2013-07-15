using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.IO;
using System.ServiceModel.Web;
using System.ServiceModel.Description;
using HomeOS.Hub.Platform.ManagedWifi;
using HomeOS.Hub.Platform.DeviceScout;
using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;

using System.Net.Mail;

namespace HomeOS.Hub.Platform
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class GuiService : IGuiServiceWeb, IGuiServiceWebSec, IDisposable
    {
        const int ZWaveAddRemoveTimeoutSecs = 10;

        Platform platform;
        Configuration config;
        HomeStoreInfo homeStoreInfo;
        VLogger logger;

        ServiceHost serviceHostWeb;
        WebFileServer webFileServer;

        SafeServiceHost serviceHostWebHomeId;
        WebFileServer webFileServerHomeId;

        SafeServiceHost serviceHostWebSecHomeId;

        public GuiService(Platform platform, Configuration config, HomeStoreInfo hsInfo, VLogger logger)
        {
            this.platform = platform;
            this.config = config;
            this.homeStoreInfo = hsInfo;
            this.logger = logger;

            string svcBase = Constants.InfoServiceAddress + "/" ;

            //let us start serving the files
            string webBase = svcBase + Common.Constants.GuiServiceSuffixWeb;
            webFileServer = new WebFileServer(Constants.DashboardRoot, webBase, logger);

            serviceHostWeb = OpenUnsafeServiceWeb(webBase + Constants.AjaxSuffix);
        }

        public void ConfiguredStart()
        {
            string svcBase = Constants.InfoServiceAddress + "/" + Settings.HomeId + "/";

            //let us start serving the files
            string webBase = svcBase + Common.Constants.GuiServiceSuffixWeb;
            webFileServerHomeId = new WebFileServer(Constants.DashboardRoot, webBase, logger);

            serviceHostWebHomeId = OpenSafeServiceWeb(webBase + Constants.AjaxSuffix);

            string webSecBase = svcBase + Common.Constants.GuiServiceSuffixWebSec;

            serviceHostWebSecHomeId = OpenSafeServiceWebSec(webSecBase + Constants.AjaxSuffix);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != serviceHostWeb)
                {
                    serviceHostWeb.Close();
                }
                if (null != webFileServer)
                {
                    webFileServer.Dispose();
                }
                if (null != serviceHostWebHomeId)
                {
                    serviceHostWebHomeId.Close();
                }
                if (null != serviceHostWebSecHomeId)
                {
                    serviceHostWebSecHomeId.Close();
                }
                if (null != webFileServerHomeId)
                {
                    webFileServerHomeId.Dispose();
                }
            }
        }

        private ServiceHost OpenUnsafeServiceWeb(string baseUrl)
        {
            var svcHost = new ServiceHost(this, new Uri(baseUrl));

            var contract = ContractDescription.GetContract(typeof(IGuiServiceWeb));
            var webEndPoint = new ServiceEndpoint(contract, new WebHttpBinding(), new EndpointAddress(baseUrl));
            webEndPoint.EndpointBehaviors.Add(new WebHttpBehavior());

            svcHost.AddServiceEndpoint(webEndPoint);

            svcHost.Description.Behaviors.Add(new ServiceMetadataBehavior());
            svcHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

            svcHost.Open();

            return svcHost;
        }

        private SafeServiceHost OpenSafeServiceWeb(string baseUrl)
        {
            var svcHost = new SafeServiceHost(logger, platform, this, baseUrl);

            var contract = ContractDescription.GetContract(typeof(IGuiServiceWeb));
            var webEndPoint = new ServiceEndpoint(contract, new WebHttpBinding(), new EndpointAddress(baseUrl));
            webEndPoint.EndpointBehaviors.Add(new WebHttpBehavior());

            svcHost.AddServiceEndpoint(webEndPoint);

            svcHost.AddServiceMetadataBehavior(new ServiceMetadataBehavior());
            //svcHost.Description.Behaviors.Add(new ServiceMetadataBehavior());
            //svcHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

            svcHost.Open();

            return svcHost;
        }

        private SafeServiceHost OpenSafeServiceWebSec(string baseUrl)
        {
            var svcHost = new SafeServiceHost(logger, platform, this, baseUrl);

            var contract = ContractDescription.GetContract(typeof(IGuiServiceWebSec));
            var webEndPoint = new ServiceEndpoint(contract, new WebHttpBinding(), new EndpointAddress(baseUrl));
            webEndPoint.EndpointBehaviors.Add(new WebHttpBehavior());

            svcHost.AddServiceEndpoint(webEndPoint);

            svcHost.AddServiceMetadataBehavior(new ServiceMetadataBehavior());
            //svcHost.Description.Behaviors.Add(new ServiceMetadataBehavior());
            //svcHost.AddServiceEndpoint(typeof(IMetadataExchange), MetadataExchangeBindings.CreateMexHttpBinding(), "mex");

            svcHost.Open();

            return svcHost;
        }


        #region functions for setting and querying global configuration
        public List<string> GetVersion()
        {
            try
            {

                logger.Log("UICalled:GetVersion: 42");
                return new List<string>() { "", "42" };
            }
            catch (Exception e)
            {
                logger.Log("Exception in GetUnconfiguredDevicesForScout: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        //public Tuple<bool, string> SetHomeId(string homeId, string password)
        //{
        //    logger.Log("UICalled: SetHomeId: " + homeId, " " + password);

        //    if (IsConfigNeeded())
        //    {
        //        if (HomeOS.Shared.HomeId.IsValidHomeId(homeId))
        //        {
        //            config.UpdateConfSetting("HomeId", homeId);

        //            HomeOS.Shared.Globals.HomeId = new HomeOS.Shared.HomeId(homeId);

        //            platform.ConfiguredStart();

        //            return new Tuple<bool, string>(true, "");
        //        }
        //        else
        //        {
        //            return new Tuple<bool, string>(false, "Invalid HomeId!");
        //        }
        //    }
        //    else
        //    {
        //        logger.Log("Attempt to reconfigure HomeId! Foiled!");

        //        return new Tuple<bool, string>(false, "HomeId is already configured!");
        //    }
        //}

        public List<string> SetHomeIdWeb(string homeId, string password)
        {
            try
            {
                logger.Log("UICalled: SetHomeIdWeb: " + homeId, " " + password);

                if (IsConfigNeeded())
                {
                    if (Utils.IsValidHomeId(homeId))
                    {
                        config.UpdateConfSetting("HomeId", homeId);

                        platform.ConfiguredStart();

                        return new List<string>() { "" };
                    }
                    else
                    {
                        return new List<string>() { "Invalid HomeId!" };
                    }
                }
                else
                {
                    logger.Log("Attempt to reconfigure HomeId! Foiled!");

                    return new List<string>() { "HomeId is already configured!" };
                }
            }
            catch (Exception e)
            {
                logger.Log("Exception in SetHomeIdWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        //public Tuple<bool, string> SetNotificationEmail(string emailAddress)
        //{
        //    logger.Log("UICalled:SetNotificationEmail " + emailAddress);

        //    try
        //    {
        //        config.UpdateConfSetting("NotificationEmail", emailAddress);

        //        return new Tuple<bool, string>(true, "");
        //    }
        //    catch (Exception)
        //    {
        //        return new Tuple<bool, string>(false, "Got exception while updating the notification email. Try again?");
        //    }
        //}

        public List<string> SetNotificationEmailWeb(string emailAddress)
        {
            try
            {
                logger.Log("UICalled:SetNotificationEmail " + emailAddress);

                config.UpdateConfSetting("NotificationEmail", emailAddress);

                return new List<string>() { "", "true" };
            }
            catch (Exception e)
            {
                logger.Log("Exception in SetNotificationEmailWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        public bool IsConfigNeeded()
        {
            return string.IsNullOrEmpty(Settings.HomeId);
        }

        public List<string> IsConfigNeededWeb()
        {
            try
            {
                logger.Log("UICalled:IsConfigNeededWeb");

            return new List<string>() {"", IsConfigNeeded().ToString()};
            }
            catch (Exception e)
            {
                logger.Log("Exception in IsConfigNeededWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }

        }

       // public Tuple<string, string> GetConfSetting(string confKey)
       // {
       //     logger.Log("UICalled:GetConfSetting " + confKey);

        //    string confValue = config.GetConfSetting(confKey);

        //    if (confValue == null)
        //        return new Tuple<string, string>(null, "Key not found!");
        //    else
        //        return new Tuple<string, string>(confValue, "");
        //}

        public List<string> GetConfSettingWeb(string confKey)
        {
            try
            {
                logger.Log("UICalled:GetConfSettingWeb " + confKey);

                string confValue = config.GetConfSetting(confKey);

                if (confValue == null)
                    return new List<string>() { "Key not found!" };
                else
                    return new List<string>() { "", confValue };
            }
            catch (Exception e)
            {
                logger.Log("Exception in GetConfSetting: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }
        #endregion

        #region Wifi related functions
        ///// <summary>
        ///// The UI calls this functions to get the list of devices that the HomeHub can see
        ///// </summary>
        ///// <returns></returns>
        //public Tuple<List<string>, string> GetVisibleWifiNetworks()
        //{
        //     logger.Log("UICalled:GetVisibleWifiNetworks ");
        //    List<string> retList = new List<string>();

        //    try
        //    {
        //        WlanClient client = new WlanClient();
        //        foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
        //        {
        //            Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
        //            foreach (Wlan.WlanAvailableNetwork network in networks)
        //            {
        //                string ssid = WifiHelper.GetStringForSSID(network.dot11Ssid);

        //                if (!retList.Contains(ssid) && !String.IsNullOrWhiteSpace(ssid))
        //                {
        //                    retList.Add(WifiHelper.GetStringForSSID(network.dot11Ssid));
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Log("Error while scanning for Wifi networks: {0}", e.ToString());
        //        return new Tuple<List<string>, string>(retList, "Error while scanning for Wifi networks");
        //    }

        //    return new Tuple<List<string>, string>(retList, "");
        //}

        /// <summary>
        /// The UI calls this functions to get the list of devices that the HomeHub can see
        /// </summary>
        /// <returns></returns>
        public List<string> GetVisibleWifiNetworksWeb()
        {
            logger.Log("UICalled:GetVisibleWifiNetworksWeb ");
            List<string> retList = new List<string>();

            try
            {
                WlanClient client = new WlanClient();
                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {
                    Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                    foreach (Wlan.WlanAvailableNetwork network in networks)
                    {
                        string ssid = WifiHelper.GetStringForSSID(network.dot11Ssid);

                        if (!retList.Contains(ssid) && !String.IsNullOrWhiteSpace(ssid))
                        {
                            retList.Add(WifiHelper.GetStringForSSID(network.dot11Ssid));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Log("Error while scanning for Wifi networks: {0}", e.ToString());
                retList.Insert(0, "Error while scanning WiFi networks");
                return retList;
            }

            retList.Insert(0, "");
            return retList;
        }

        /// <summary>
        /// The UI calls this function to ask the hub to connect to a particular WiFi network
        /// </summary>
        /// <param name="targetSsid"></param>
        /// <param name="passPhrase"></param>
        /// <returns></returns>
        public List<string> ConnectToWifiNetworkWeb(string targetSsid, string passPhrase)
        {
            try
            {
                bool result = false;
                string retString = "Network " + targetSsid + " not found";

                logger.Log("UICalled:ConnectToWifiNetworkWeb " + targetSsid + " " + passPhrase);
                WlanClient client = new WlanClient();
                foreach (WlanClient.WlanInterface wlanIface in client.Interfaces)
                {
                    // Lists all networks with WEP security
                    Wlan.WlanAvailableNetwork[] networks = wlanIface.GetAvailableNetworkList(0);
                    foreach (Wlan.WlanAvailableNetwork network in networks)
                    {
                        string ssid = WifiHelper.GetStringForSSID(network.dot11Ssid);

                        if (targetSsid.Equals(ssid))
                        {
                            string profileName = ssid;
                            string profile = WifiHelper.MakeProfile(profileName, network, passPhrase);

                            //    Console.WriteLine("Made profile:\n{0}", profile);

                            Wlan.WlanReasonCode reasonCode = wlanIface.SetProfile(Wlan.WlanProfileFlags.AllUser, profile, true);

                            if (reasonCode == Wlan.WlanReasonCode.Success)
                            {
                                result = wlanIface.ConnectSynchronously(Wlan.WlanConnectionMode.Profile, Wlan.Dot11BssType.Any, profileName, 10000);

                                if (result == true)
                                {
                                    retString = "Connection succeeded!";

                                    config.UpdateConfSetting("WifiSsid", targetSsid);
                                    config.UpdateConfSetting("WifiKey", passPhrase);

                                    return new List<string>() { "", "true" };
                                }
                                else
                                {
                                    return new List<string>() { "Could not connect. Is the passphrase correct?", "false" };
                                }
                            }
                            else
                            {
                                return new List<string>() { "Could not set profile. ReasonCode = " + reasonCode.ToString(), "false" };
                            }
                        }
                    }
                }

                return new List<string>() { retString, result.ToString() };
            }
            catch (Exception e)
            {
                logger.Log("Exception in ConnectToWifiNetworkWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }

        }

        #endregion


        #region Zwave related functions
        public Tuple<bool, string> ResetZwaveController()
        {
            logger.Log("UICalled: ResetZwaveController");
            VModule driverZwave = platform.GetDriverZwave();

            if (driverZwave == null)
                return new Tuple<bool, string>(false, "Is the Zwave driver running?");

            string result = (string) driverZwave.OpaqueCall("Reset");

            logger.Log("result of reset call = " + result);

            return new Tuple<bool,string>(true, "");
        }

        public Tuple<bool, string> RemoveFailedZwaveNode(byte nodeId)
        {
            logger.Log("UICalled: RemoveFailedZwaveNode");
            VModule driverZwave = platform.GetDriverZwave();

            if (driverZwave == null)
                return new Tuple<bool, string>(false, "Is the Zwave driver running?");

            string result = (string)driverZwave.OpaqueCall("RemoveFailedNode", nodeId);

            logger.Log("result of removefailednode call = " + result);

            return new Tuple<bool, string>(true, "");
        }

        //public Tuple<string, string> AddZwave()
        //{
        //    logger.Log("UICalled: AddZwave");
        //    VModule driverZwave = platform.GetDriverZwave();

        //    if (driverZwave == null)
        //        return new Tuple<string, string>("", "Is the Zwave driver running?");

        //    string result = (string)driverZwave.OpaqueCall("AddDevice");

        //    logger.Log("Result of add zwave = " + result);
            
        //    if (result.Contains("ZwaveNode::"))
        //         return new Tuple<string, string>(result, "");
        //    else 
        //        return new Tuple<string,string>("", result);
        //}

        public List<string> AddZwaveWeb()
        {
            try
            {
                logger.Log("UICalled: AddZwaveWeb");
                VModule driverZwave = platform.GetDriverZwave();

                if (driverZwave == null)
                    return new List<string>() { "Is the Zwave driver running?" };

                string addResult = null;

                SafeThread addThread = new SafeThread(delegate() { addResult = (string)driverZwave.OpaqueCall("AddDevice"); },
                                                      "zwave node adding", logger);
                addThread.Start();
             addThread.Join(new TimeSpan(0, 0, ZWaveAddRemoveTimeoutSecs));

                logger.Log("Result of add zwave = " + addResult);

                if (addResult == null)
                {
                    string abortResult = (string)driverZwave.OpaqueCall("AbortAddDevice");

                    logger.Log("Result of AbortAddDevice = " + abortResult);

                    return new List<string>() { "Add operation timed out" };
                }

                if (addResult.Contains("ZwaveNode::"))
                    return new List<string>() { "", addResult };

                return new List<string>() { addResult };
            }
            catch (Exception e)
            {
                logger.Log("Exception in AddZwaveWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }

        }

        public List<string> AbortAddZwaveWeb()
        {
            try
            {
                logger.Log("UICalled: AbortAddZwaveWeb");

                VModule driverZwave = platform.GetDriverZwave();
                if (driverZwave == null)
                    return new List<string>() { "Is the Zwave driver running?" };

                string result = (string)driverZwave.OpaqueCall("AbortAddDevice");
                logger.Log("Result of abort add zwave = " + result);

                return new List<string>() { "" };
            }
            catch (Exception e)
            {
                logger.Log("Exception in AbortZwaveWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        //if you are removing a device you have, the parameters are (deviceFriendlyName, “false”) 
        //if you are removing a device that does not exist any more, the parameters should be (nodeId, “true”)
        public List<string> RemoveZwaveWeb(string deviceFriendlyName, string failedNode)
        {
            try
            {
                logger.Log("UICalled: RemoveZwaveDeviceWeb: {0} {1}", deviceFriendlyName, failedNode);

                // get and check the zwave driver
                VModule driverZwave = platform.GetDriverZwave();

                if (driverZwave == null)
                    return new List<string>() { "Is the Zwave driver running?" };

                //get the service port and check if the node is indeed zwave
                var servicePort = config.GetConfiguredPortUsingFriendlyName(deviceFriendlyName);

                if (servicePort == null)
                    return new List<string>() { "Port not found for " + deviceFriendlyName };

                if (!servicePort.ModuleFacingName().Contains("ZwaveNode::"))
                    return new List<string>() { deviceFriendlyName + " doesn't appear to be a zwave node. its modulefacingname is " + servicePort.ModuleFacingName() };

                //now get removing
                if (failedNode.Equals("false", StringComparison.CurrentCultureIgnoreCase))
                {
                    string result = null;

                    SafeThread addThread = new SafeThread(delegate() { result = (string)driverZwave.OpaqueCall("RemoveDevice"); },
                                                          "zwave node removal", logger);

                    addThread.Start();

                addThread.Join(new TimeSpan(0, 0, ZWaveAddRemoveTimeoutSecs));

                    logger.Log("Result of remove zwave = " + result);

                    if (result == null)
                    {
                        string abortResult = (string)driverZwave.OpaqueCall("AbortRemoveDevice");

                        logger.Log("Result of AbortAddDevice = " + abortResult);

                        return new List<string>() { "Add operation timed out" };
                    }
                }
                else
                {
                    int startIndex = servicePort.ModuleFacingName().IndexOf("ZwaveNode::");
                    string subString = servicePort.ModuleFacingName().Substring(startIndex);

                    int nodeId;
                    bool success = Int32.TryParse(subString, out nodeId);

                    if (!success)
                    {
                        logger.Log("Could not extract node id from {0}", servicePort.ModuleFacingName());
                        return new List<string>() { "Could not extract nodeId from " + servicePort.ModuleFacingName() };
                    }

                    string result = (string)driverZwave.OpaqueCall("RemoveFailedNode", nodeId);

                    logger.Log("result of removefailednode call = " + result);
                }

                //now remove the port
                bool removePortResult = config.RemovePort(servicePort);

                if (!removePortResult)
                    return new List<string>() { String.Format("Could not remove port {0}", servicePort.ToString()) };

                platform.RemoveAccessRulesForDevice(deviceFriendlyName);

                return new List<string>() { "" };
            }
            catch (Exception e)
            {
                logger.Log("Exception in RemoveZwaveWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        public List<string> AbortRemoveZwaveWeb()
        {
            try
            {

                logger.Log("UICalled: AbortRemoveZwave");

                VModule driverZwave = platform.GetDriverZwave();
                if (driverZwave == null)
                    return new List<string>() { "Is the Zwave driver running?" };

                string result = (string)driverZwave.OpaqueCall("AbortRemoveDevice");
                logger.Log("Result of abort remove zwave = " + result);

                return new List<string>() { "" };
            }
            catch (Exception e)
            {
                logger.Log("Exception in AbortRemoveZwaveWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

      //  public Tuple<bool, string> ConfigureZwave(string nodeId, string friendlyName, bool highSecurity, string location, string[] apps)
      //  {
      //      logger.Log("UICalled:ConfigureZwave " + nodeId + " " + friendlyName + " " + highSecurity.ToString() + " " + location + " " + apps.ToString());
      //      return ConfigureDevice(nodeId, friendlyName, highSecurity, location, apps);
      //  }

        #endregion

        #region general utility functions
        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="dst">to:</param>
        /// <param name="subject">subject</param>
        /// <param name="body">body of the message</param>
        /// <returns>A tuple with true/false success and string exception message (if any)</returns>
        public List<string> SendEmail(string dst, string subject, string body)
        {
            logger.Log("UICalled:SendEmail " + dst + " " + subject + " " + body);

            //   <!--smtp server, smtp user, smtp password, email target, sms target -->
            // <Args Count="5" val1="smtp.live.com" val2="homeos@live.com" val3="home123$" val4="homeos@live.com" val5="1234567890@txt.att.net" />

            string smtpUser = "homeos@live.com";
            string smtpPassword = "home123$";
            string smtpServer = "smtp.live.com";
            
            MailMessage message = new MailMessage();

            string from = string.IsNullOrEmpty(smtpUser) ? "homeos.placeholder@microsoft.com" : smtpUser;
            message.From = new MailAddress(from);
            message.To.Add(dst);

            message.Subject = subject;
            message.Body = body;
            
            try
            {
                SmtpClient smtpClient = new SmtpClient(smtpServer);
                smtpClient.Credentials = new System.Net.NetworkCredential(smtpUser, smtpPassword);
                smtpClient.EnableSsl = true;

                smtpClient.Send(message);
            }
            catch (Exception exception)
            {
                logger.Log("Exception while sending message: {0}", exception.ToString());
                return new List<string>() {exception.ToString()};
            }
            
            return new List<string>() {""};
        }
        #endregion

        #region functions related to apps
        /// <summary>
        /// returns apps in homestore that are compatible with the deviceid
        /// </summary>
        /// <param name="uniqueDeviceId"></param>
        /// <returns></returns>
        public List<string> GetCompatibleAppsNotInstalledWeb(string uniqueDeviceId)
        {
            try
            {

                logger.Log("UICalled:GetCompatibleAppsNotInstalled " + uniqueDeviceId);
                List<PortInfo> pInfoList = config.GetPorts(uniqueDeviceId);

                if (pInfoList.Count != 1)
                    return new List<string>() { String.Format("Incorrect number of ports ({0}) found", pInfoList.Count) };

                List<string> appList = new List<string>();

                //the set of roles that we should check compatibility with is those already configured + those we are now configuring
                //List<VRole> roleList = System.Linq.Enumerable.ToList<VRole>(config.configuredRolesInHome.Keys);
                List<VRole> roleList = config.GetAllRolesInHome();
                roleList.AddRange(pInfoList[0].GetRoles());

                foreach (HomeStoreApp homeStoreApp in homeStoreInfo.GetAllModules())
                {
                    if (!homeStoreApp.Manifest.IsCompatibleWithHome(roleList))
                        continue;

                    //assume that we end up using homeStoreApp.AppName as friendly name in the new gui
                    if (config.allModules.ContainsKey(homeStoreApp.AppName))
                        continue;

                    //string triplet ugliness
                    appList.Add(homeStoreApp.AppName);
                    appList.Add(homeStoreApp.Description);
                    appList.Add(homeStoreApp.IconUrl);
                }

                appList.Insert(0, "");
                return appList;
            }
            catch (Exception e)
            {
                logger.Log("Exception in GetCompatibleAppsNotInstalled: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        /// <summary>
        /// returns installed apps that are compatible with a device
        /// </summary>
        /// <param name="uniqueDeviceId"></param>
        /// <returns></returns>
        public List<string> GetCompatibleAppsInstalledWeb(string uniqueDeviceId)
        {
            try
            {
                logger.Log("UICalled:GetCompatibleAppsInstalledWeb " + uniqueDeviceId);
                List<PortInfo> pInfoList = config.GetPorts(uniqueDeviceId);

                if (pInfoList.Count != 1)
                    return new List<string>() { String.Format("Incorrect number of ports ({0}) found", pInfoList.Count) };

                List<ModuleInfo> modList = config.GetCompatibleModules(pInfoList[0]);

                var retList = new List<string>();
                //string triplet ugliness
                foreach (var mInfo in modList)
                {
                    retList.Add(mInfo.AppName());

                    HomeStoreApp app = homeStoreInfo.GetHomeStoreAppByName(mInfo.AppName());

                    if (app == null)
                    {
                        retList.Add("Unknown");
                        retList.Add("Unknown");
                    }
                    else
                    {
                        retList.Add(app.Description);
                        retList.Add(app.IconUrl);
                    }
                }

                retList.Insert(0, "");
                return retList;
            }
            catch (Exception e)
            {
                logger.Log("Exception in GetCompatibleAppsInstalledWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }

        }

        /// <summary>
        /// Returns all installed modules that are foreground, as 3-tuples [appname,description,iconurl]
        /// </summary>
        /// <returns></returns>
        public List<string> GetInstalledAppsWeb()
        {
            try
            {
                logger.Log("UICalled:GetInstalledAppsWeb ");
                List<ModuleInfo> mInfoList = config.GetAllForegroundModules();

                List<string> mNameList = new List<string>();

                foreach (var mInfo in mInfoList)
                {
                    mNameList.Add(mInfo.AppName());

                    HomeStoreApp app = homeStoreInfo.GetHomeStoreAppByName(mInfo.AppName());

                    if (app == null)
                    {
                        mNameList.Add("Unknown");
                        mNameList.Add("Unknown");
                    }
                    else
                    {
                        mNameList.Add(app.Description);
                        mNameList.Add(app.IconUrl);
                    }
                }

                mNameList.Insert(0, "");
                return mNameList;
            }
            catch (Exception e)
            {
                logger.Log("Exception in GetInstalledAppsWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }


        public List<string> InstallAppWeb(string appName)
        {
            try
            {
                logger.Log("UICalled:InstallAppWeb " + appName);

                HomeStoreApp app = homeStoreInfo.GetHomeStoreAppByName(appName);

                if (app == null)
                {
                    logger.Log("HomeStore app {0} was not found", appName);
                    return new List<string>() { "HomeStore app not found" };
                }

                //by default, we make the app auto start
                ModuleInfo moduleInfo = new ModuleInfo(app.AppName, app.AppName, app.BinaryName, null, true);
                moduleInfo.SetManifest(app.Manifest);
                moduleInfo.SetVersion(app.Version);

                AccessRule accessRule = new AccessRule();
                accessRule.ModuleName = moduleInfo.FriendlyName();
                accessRule.RuleName = "Access for " + moduleInfo.FriendlyName();
                accessRule.UserGroup = "everyone";
                accessRule.AccessMode = AccessMode.Allow;
                accessRule.DeviceList = new List<string> { "*" };
                accessRule.TimeList = new List<TimeOfWeek> { new TimeOfWeek(-1, 0, 2400) };
                accessRule.Priority = 0;

                platform.AddAccessRule(accessRule);

                //we now call startmodule: if we don't already have the binaries, this will download them as well
                var startedModule = platform.StartModule(moduleInfo, true);

                if (startedModule != null)
                {
                    //add this to our configuration
                    config.AddModule(moduleInfo);

                    return new List<string>() { "" };
                }
                else
                {
                    return new List<string>() { "Could not start module. Perhaps because we didn't find the binaries (with the right version)" };
                }
            }
            catch (Exception e)
            {
                logger.Log("Exception in InstallAppWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        public List<string> GetApps()
        {
            try
            {
                logger.Log("UICalled:GetApps ");

                List<string> appList = new List<string>();

                foreach (HomeStoreApp homeStoreApp in homeStoreInfo.GetAllModules())
                {
                    appList.Add(homeStoreApp.AppName);
                    appList.Add(homeStoreApp.Description);
                    appList.Add(homeStoreApp.Rating.ToString());

                    //add the icon url
                    if (homeStoreApp.IconUrl == null)
                        appList.Add("unknown");
                    else
                        appList.Add(homeStoreApp.IconUrl);

                    bool compatibleWithHome = homeStoreApp.Manifest.IsCompatibleWithHome(config.GetAllRolesInHome());
                    appList.Add(compatibleWithHome.ToString());

                    string missingRolesString = homeStoreApp.Manifest.MissingRolesString(config.GetAllRolesInHome());
                    appList.Add(missingRolesString);

                    bool installed = config.IsAppInstalled(homeStoreApp.AppName);
                    appList.Add(installed.ToString());
                }

                appList.Insert(0, "");
                return appList;
            }
            catch (Exception e)
            {
                logger.Log("Exception in GetApps: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        #endregion

        #region device conf related functions
        
        //returns the following tuples [friendly name, module facing name, location]
        public List<string> GetConfiguredDevicesWeb()
        {
            try
            {
                logger.Log("UICalled:GetConfiguredDevices ");
                List<string> retList = new List<string>();

                List<PortInfo> portList = config.GetConfiguredPorts();

                foreach (PortInfo portInfo in portList)
                {
                    retList.Add(portInfo.GetFriendlyName());
                    retList.Add(portInfo.ModuleFacingName());
                    retList.Add(portInfo.GetLocation().ToString());
                }

                retList.Insert(0, "");
                return retList;
            }
            catch (Exception e)
            {
                logger.Log("Exception in GetConfiguredDevicesWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        //public List<string> GetUnconfiguredDevicesWeb()
        //{
        //    try
        //    {
        //        logger.Log("UICalled:GetUnconfiguredDevices ");

        //        List<string> retList = new List<string>();

        //        var deviceList = config.GetUnconfiguredDevices();

        //        foreach (var device in deviceList)
        //        {
        //            retList.Add(device.UniqueName);
        //        }

        //        retList.Insert(0, "");
        //        return retList;
        //    }
        //    catch (Exception e)
        //    {
        //        logger.Log("Exception in GetUnconfiguredDevicesWeb: " + e.ToString());

        //        return new List<string>() { "Got exception: " + e.Message };
        //    }
        //}

        /// <summary>
        /// returns all unconfigured devices for the given scoutName
        /// </summary>
        /// <param name="scoutName"></param>
        /// <returns></returns>
        public List<string> GetUnconfiguredDevicesForScout(string scoutName)
        {
            try
            {
                logger.Log("UIcalled: GetUnconfiguredDeviceForScout {0}", scoutName);

                List<string> retString = new List<string>();

                List<Device> deviceList = platform.GetDevicesForScout(scoutName);

                if (deviceList == null)
                {
                    string errorString = String.Format("Scout {0} not found or not running", scoutName);

                    logger.Log(errorString);

                    retString.Add(errorString);

                    return retString;
                }

                //make sure that the configuration knows about these devices
                config.ProcessNewDiscoveryResults(deviceList);

                //now add uncofigured devices to the list
                foreach (var device in deviceList)
                {
                    if (!config.IsConfiguredDevice(device.UniqueName))
                    {
                        retString.Add(device.UniqueName);
                        retString.Add(scoutName);
                    }
                }

                retString.Insert(0, "");
                return retString;
            }
            catch (Exception e)
            {
                logger.Log("Exception in GetUnconfiguredDevicesForScout: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        /// <summary>
        /// returns all unconfigured devices for the given scoutName
        /// </summary>
        public List<string> GetAllUnconfiguredDevices()
        {
            try
            {
                logger.Log("UIcalled: GetAllUnconfiguredDevices");

                List<string> retString = new List<string>();

                var runningScoutNames = platform.GetAllScoutNames();

                //first, get all scouted devices
                foreach (var scoutName in runningScoutNames)
                {
                    try
                    {
                        var deviceList = platform.GetDevicesForScout(scoutName);

                        if (deviceList == null)
                            logger.Log("Got null device list for scoutName {0}", scoutName);

                        //make sure that the configuration knows about these devices
                        config.ProcessNewDiscoveryResults(deviceList);

                        //now add uncofigured devices to the list
                        foreach (var device in deviceList)
                        {
                            if (!config.IsConfiguredDevice(device.UniqueName))
                            {
                                retString.Add(device.UniqueName);
                                retString.Add(scoutName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Log("Exception in getting or processing device list from scout {0}", scoutName);
                    }
                }

                //second, get unconfigured devices for which driver is running but they aren't configured
                var portList = config.GetUnconfiguredPorts();

                foreach (var device in portList)
                {
                    retString.Add(device.ModuleFacingName());
                    retString.Add(Constants.OrphanedDeviceIndicator);
                }

                retString.Insert(0, "");
                return retString;
            }
            catch (Exception e)
            {
                logger.Log("Exception in GetAllUnconfiguredDevices: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }

        }

        /// <summary>
        /// returns all unconfigured devices for the given scoutName
        /// </summary>
        public List<string> GetOrphanedDevicesWeb()
        {
            try
            {
                logger.Log("UIcalled: GetOrphanedDevicesWeb");

                List<string> retString = new List<string>();

                //get unconfigured devices for which driver is running but they aren't configured
                var portList = config.GetUnconfiguredPorts();

                foreach (var port in portList)
                {
                    retString.Add(port.ModuleFacingName());
                }

                retString.Insert(0, "");
                return retString;
            }
            catch (Exception e)
            {
                logger.Log("Exception in GetOrphanedDevices: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        public List<string> StartDriver(string uniqueDeviceId)
        {
            try
            {
                logger.Log("UIcalled: StartDriver {0}", uniqueDeviceId);

                List<string> retList = new List<string>();

                Device device = config.GetDevice(uniqueDeviceId);

                if (device == null)
                {
                    string errorString = String.Format("device corresponding to name {0} not found", uniqueDeviceId);

                    logger.Log(errorString);

                    retList.Add(errorString);

                    return retList;
                }

                var result = platform.StartDriverForDevice(device, device.Details.DriverParams);

                //check for success or failure
                if (result.Item1)
                    retList.Add("");
                else
                    retList.Add(result.Item2);

                return retList;
            }
            catch (Exception e)
            {
                logger.Log("Exception in StartDriver " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        public List<string> IsDeviceReady(string uniqueDeviceId)
        {
            try
            {
                logger.Log("UICalled:IsDeviceReady " + uniqueDeviceId);

                List<string> retList = new List<string>();

                List<PortInfo> pInfo = config.GetUnconfiguredPorts(uniqueDeviceId);

                if (pInfo.Count == 0)
                {
                    string errorString = "Device not ready yet. Try again.";
                    logger.Log(errorString);
                    retList.Add(errorString);
                    return retList;
                }
                else if (pInfo.Count > 1)
                {
                    string errorString = "Multiple services found for device";
                    logger.Log(errorString);
                    retList.Add(errorString);
                    return retList;
                }

                retList.Add("");
                return retList;
            }
            catch (Exception e)
            {
                logger.Log("Exception in IsDeviceReady: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }

        }

        public List<string> GetDeviceDetails(string uniqueDeviceId) 
        {

            try
            {
                logger.Log("UICalled:GetDeviceDetails " + uniqueDeviceId);

                List<PortInfo> pInfoList = config.GetPorts(uniqueDeviceId);

                if (pInfoList.Count != 1)
                    return new List<string>() { String.Format("Incorrect number of ports ({0}) found", pInfoList.Count) };

                string imageUrl = platform.GetImageUrlFromModule(pInfoList[0].ModuleFriendlyName(), uniqueDeviceId);

                string description = platform.GetDescriptionFromModule(pInfoList[0].ModuleFriendlyName(), uniqueDeviceId);

                return new List<string>() { "", imageUrl, description };

            }
            catch (Exception e)
            {
                logger.Log("Exception in GetDeviceImage: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="uniqueDeviceId">uniqueDeviceId or ModuleFacingName</param>
        /// <param name="friendlyName"></param>
        /// <param name="highSecurity"></param>
        /// <param name="location"></param>
        /// <param name="apps"></param>
        /// <returns></returns>
        public List<string> ConfigureDeviceWeb(string uniqueDeviceId, string friendlyName, bool highSecurity, string location, string[] apps)
        {
            try
            {
                logger.Log("UICalled:ConfigureDeviceWeb " + uniqueDeviceId + " " + friendlyName + " " + highSecurity.ToString() + " " + location + " " + apps.ToString());

                List<string> retList = new List<string>();

                List<PortInfo> pInfo = config.GetUnconfiguredPorts(uniqueDeviceId);

                if (pInfo.Count == 0)
                {
                    string errorString = "Service for device not found. Try again";
                    logger.Log(errorString);
                    retList.Add(errorString);
                    return retList;
                }
                else if (pInfo.Count > 1)
                {
                    string errorString = "Multiple services found for device";
                    logger.Log(errorString);
                    retList.Add(errorString);
                    return retList;
                }

                platform.AddService(pInfo[0], friendlyName, highSecurity, location, apps);

                retList.Add("");
                return retList;
            }
            catch (Exception e)
            {
                logger.Log("Exception in ConfiguredDeviceWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        public List<string> AllowAppAccessToDevice(string appFriendlyName, string deviceFriendlyName)
        {
            try
            {
                logger.Log("UICalled:AllowAppAccessToDevice {0} {1}", appFriendlyName, deviceFriendlyName);

                if (!config.CanAppAccessDevice(appFriendlyName, deviceFriendlyName))
                {
                    platform.AllowAppAcccessToDevice(appFriendlyName, deviceFriendlyName);
                    return new List<string>() { "", "newly allowed" };
                }
                else
                {
                    return new List<string>() { "", "already allowed" };
                }
            }
            catch (Exception exception)
            {
                logger.Log("Exception in AllowAppAccessToDevice: {0}", exception.ToString());
                return new List<string>() { exception.Message };
            }
        }

        public List<string> DisallowAppAccessToDevice(string appFriendlyName, string deviceFriendlyName)
        {
            try
            {
                logger.Log("UICalled:DisallowAppAccessToDevice {0} {1}", appFriendlyName, deviceFriendlyName);

                if (config.CanAppAccessDevice(appFriendlyName, deviceFriendlyName))
                {

                    bool result = platform.DisallowAppAccessToDevice(appFriendlyName, deviceFriendlyName);

                    if (!result)
                        return new List<string>() { "Failed" };

                    return new List<string>() { "", "newly disallowed" };
                }
                else
                {
                    return new List<string>() { "", "already disallowed" };
                }
            }
            catch (Exception exception)
            {
                logger.Log("Exception in DisallowAppAccessToDevice: {0}", exception.ToString());
                return new List<string>() { exception.Message };
            }
        }

        //this returns triplets [deviceFriendlyName, location]
        public List<string> GetCompatibleDevicesForHomestoreApp(string appName)
        {
            try
            {
                logger.Log("UICalled:GetCompatibleDevicesForHomestoreApp {0}", appName);

                HomeStoreApp app = homeStoreInfo.GetHomeStoreAppByName(appName);

                if (app == null)
                {
                    logger.Log("HomeStore app {0} was not found", appName);
                    return new List<string>() { "HomeStore app not found" };
                }

                List<PortInfo> pInfoList = config.GetCompatiblePorts(app.Manifest);

                var retList = new List<string>();

                foreach (var pInfo in pInfoList)
                {
                    retList.Add(pInfo.GetFriendlyName());
                    retList.Add(pInfo.GetLocation().ToString());
                }

                retList.Insert(0, "");
                return retList;
            }
            catch (Exception exception)
            {
                logger.Log("Exception in GetCompatibleDevicesForHomestoreApp: {0}", exception.ToString());
                return new List<string>() { exception.Message };
            }
        }

        //this returns triplets [deviceFriendlyName, whether app has access to device, location]
        public List<string> GetCompatibleDevicesForInstalledApp(string appFriendlyName)
        {
            try
            {
                logger.Log("UICalled:GetCompatibleDevicesForInstalledApp {0}", appFriendlyName);

                List<PortInfo> pInfoList = config.GetCompatiblePorts(appFriendlyName);

                if (pInfoList == null)
                    return new List<string>() { "appFriendlyName " + appFriendlyName + " not found" };

                var retList = new List<string>();

                foreach (var pInfo in pInfoList)
                {
                    retList.Add(pInfo.GetFriendlyName());

                    bool allowed = config.CanAppAccessDevice(appFriendlyName, pInfo.GetFriendlyName());
                    retList.Add(allowed.ToString());

                    retList.Add(pInfo.GetLocation().ToString());
                }

                retList.Insert(0, "");
                return retList;
            }
            catch (Exception exception)
            {
                logger.Log("Exception in GetCompatibleDevicesForInstalledApp: {0}", exception.ToString());
                return new List<string>() { exception.Message };
            }
        }

        //this returns pairs of strings [appFriendlyName, whether the device has access to the app]
        public List<string> GetCompatibleInstalledAppsForDevice(string deviceFriendlyName)
        {
            try
            {
                logger.Log("UICalled:GetCompatibleAppsForDevices {0}", deviceFriendlyName);

                List<ModuleInfo> mInfoList = config.GetCompatibleModules(deviceFriendlyName);

                if (mInfoList == null)
                    return new List<string>() { "deviceFriendlyName " + deviceFriendlyName + " not found" };

                var retList = new List<string>();

                foreach (var mInfo in mInfoList)
                {
                    retList.Add(mInfo.FriendlyName());

                    bool allowed = config.CanAppAccessDevice(mInfo.FriendlyName(), deviceFriendlyName);
                    retList.Add(allowed.ToString());
                }

                retList.Insert(0, "");
                return retList;
            }
            catch (Exception exception)
            {
                logger.Log("Exception in GetCompatibleAppsForDevice: {0}", exception.ToString());
                return new List<string>() { exception.Message };
            }
        }

        public List<string> RemoveDeviceWeb(string deviceFriendlyName)
        {
            try
            {
                logger.Log("UICalled:RemoveDeviceWeb: {0}", deviceFriendlyName);

                //to remove a configured device, we need to remove its trace from four things
                // 1. in services.xml -- there must be a service with the provided friendlyname
                // 2. in modules.xml -- there must be a module for the driver, join using the modulename
                // 3. in devices.xml -- there must be a device, join using modulename
                // 4. in rules.xml and policyEngine -- remove everything with the deviceFriendlyName

                //step 1
                var servicePort = config.GetConfiguredPortUsingFriendlyName(deviceFriendlyName);

                if (servicePort == null)
                    return new List<string>() { "Service port not found for device " + deviceFriendlyName };

                //step 2
                var driverModule = config.GetModule(servicePort.ModuleFriendlyName());

                if (driverModule == null)
                    return new List<string>() { "Driver module not found for device " + deviceFriendlyName + " modulefriendlyname is " + servicePort.ModuleFriendlyName() };

                //step 3
                var deviceList = config.GetDevicesForModule(servicePort.ModuleFriendlyName());

                if (deviceList.Count != 1)
                    return new List<string>() { String.Format("Unexpected number of devices ({0}) found for the driver {1}", deviceList.Count, servicePort.ModuleFriendlyName()) };

                //everything appears in order, now lets erase things
                bool stopModuleResult = platform.StopModule(driverModule.FriendlyName());

                if (!stopModuleResult)
                    return new List<string>() { String.Format("Could not stop module with name {0}", driverModule.FriendlyName()) };

                bool removePortResult = config.RemovePort(servicePort);

                if (!removePortResult)
                    return new List<string>() { String.Format("Could not remove port {0}", servicePort.ToString()) };

                bool removeModuleResult = config.RemoveModule(driverModule.FriendlyName());

                if (!removeModuleResult)
                    return new List<string>() { String.Format("Could not remove module with name {0}", driverModule.FriendlyName()) };

                bool removeDeviceResult = config.RemoveDevice(deviceList[0].UniqueName);

                if (!removeDeviceResult)
                    return new List<string>() { String.Format("Could not remove device {0} with uniqueName {1}", deviceFriendlyName, deviceList[0].UniqueName) };

                platform.RemoveAccessRulesForDevice(deviceFriendlyName);

                return new List<string>() { "" };
            }
            catch (Exception e)
            {
                logger.Log("Exception in RemoveDeviceWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        public List<string> RemoveOrphanedDeviceWeb(string moduleFacingName)
        {
            try
            {
                logger.Log("UICalled:RemoveOrphanedDeviceWeb: {0}", moduleFacingName);

                //to remove a configured device, we need to remove its trace from four things
                // 1. in services.xml -- there must be a service with the provided moduleFacingName
                // 2. in modules.xml -- there must be a module for the driver, join using the modulename
                // 3. in devices.xml -- there must be a device, join using modulename

                //step 1
                var pInfo = config.GetUnconfiguredPorts(moduleFacingName);

                if (pInfo.Count != 1)
                    return new List<string>() { "Incorrect number of ports found for " + moduleFacingName };

                var servicePort = pInfo[0];

                //step 2
                var driverModule = config.GetModule(servicePort.ModuleFriendlyName());

                if (driverModule == null)
                    return new List<string>() { "Driver module not found for device " + moduleFacingName + " modulefriendlyname is " + servicePort.ModuleFriendlyName() };

                //step 3
                var deviceList = config.GetDevicesForModule(servicePort.ModuleFriendlyName());

                if (deviceList.Count != 1)
                    return new List<string>() { String.Format("Unexpected number of devices ({0}) found for the driver {1}", deviceList.Count, servicePort.ModuleFriendlyName()) };

                //everything appears in order, now lets erase things
                bool stopModuleResult = platform.StopModule(driverModule.FriendlyName());

                if (!stopModuleResult)
                    return new List<string>() { String.Format("Could not stop module with name {0}", driverModule.FriendlyName()) };

                bool removePortResult = config.RemovePort(servicePort);

                if (!removePortResult)
                    return new List<string>() { String.Format("Could not remove port {0}", servicePort.ToString()) };

                bool removeModuleResult = config.RemoveModule(driverModule.FriendlyName());

                if (!removeModuleResult)
                    return new List<string>() { String.Format("Could not remove module with name {0}", driverModule.FriendlyName()) };

                bool removeDeviceResult = config.RemoveDevice(deviceList[0].UniqueName);

                if (!removeDeviceResult)
                    return new List<string>() { String.Format("Could not remove device {0} with moduleFacingName {1}", moduleFacingName, deviceList[0].UniqueName) };

                //there shouldn't be any access rules for an orphaned device
                //platform.RemoveAccessRulesForDevice(deviceFriendlyName);

                return new List<string>() { "" };
            }
            catch (Exception e)
            {
                logger.Log("Exception in RemoveDeviceWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        public List<string> RemoveAppWeb(string appFriendlyName)
        {
            try
            {
                logger.Log("UICalled:RemoveAppWeb: {0}", appFriendlyName);

                //to remove an installed app, we need to remove its trace from four things
                // 1. in modules.xml -- there must be a module for the driver, join using the modulename
                // 2. in services.xml -- in case the app exports any ports
                // 2. in rules.xml -- remove everything with the deviceFriendlyName

                var appModule = config.GetModule(appFriendlyName);

                if (appModule == null)
                    return new List<string>() { "App module not found for " + appFriendlyName };

                //in case this app exports any ports
                var ports = config.GetPortsUsingModuleFriendlyName(appFriendlyName);

                bool stopModuleResult = platform.StopModule(appFriendlyName);

                if (!stopModuleResult)
                    return new List<string>() { String.Format("Could not stop module with name {0}", appFriendlyName) };

                foreach (var pInfo in ports)
                {
                    bool removePortResult = config.RemovePort(pInfo);

                    if (!removePortResult)
                        return new List<string>() { String.Format("Could not remove port {0}", pInfo.ToString()) };
                }

                bool removeModuleResult = config.RemoveModule(appFriendlyName);

                if (!removeModuleResult)
                    return new List<string>() { String.Format("Could not remove module with name {0}", appFriendlyName) };

                platform.RemoveAccessRulesForModule(appFriendlyName);

                return new List<string>() { "" };
            }
            catch (Exception e)
            {
                logger.Log("Exception in RemoveAppWeb: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }

        }

        #endregion

        #region location related  functions

        public List<string> GetAllLocations()
        {
            try
            {
                logger.Log("UICalled: GetAllLocations");

                var locations = config.GetAllLocations();
                locations.Insert(0, "");
                return locations;
            }
            catch (Exception e)
            {
                logger.Log("Exception in GetAllLocations: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }
        }

        public List<string> AddLocation(string locationToAdd, string parentLocation)
        {
            try
            {
                logger.Log("UICalled: AllLocation: {0} {1}", locationToAdd, parentLocation);

                bool result = config.AddLocation(locationToAdd, parentLocation);

                if (result)
                    return new List<string>() { "" };
                else
                    return new List<string>() { "failed, likely because parent location does not exist or you are adding a duplicate" };
            }
            catch (Exception e)
            {
                logger.Log("Exception in AddLocation: " + e.ToString());

                return new List<string>() { "Got exception: " + e.Message };
            }

        }

        #endregion


        public List<string> GetRemoteAccessUrlWeb()
        {
            string url = String.Format("http://{0}:{1}/{2}/{3}/index.html", Settings.GatekeeperUri, HomeOS.Shared.Gatekeeper.Settings.ClientPort, Settings.HomeId, Constants.GuiServiceSuffixWeb);
            return new List<string>() {"", url};
        }

        public List<string> IsServiceReady(string absoluteUrl)
        {
            try
            {
                logger.Log("UICalled: IsServiceReady {0}", absoluteUrl);
                
                string url = Constants.InfoServiceAddress + "/" + new Uri(absoluteUrl).LocalPath; 
                var request = System.Net.WebRequest.Create(url);
                try
                {
                    var response = request.GetResponse();

                    //succeeded in getting a response; the service must exist
                    logger.Log("UICalled: IsServiceReady = true for  {0}", absoluteUrl);
                    return new List<string>() { "", "true" };
                }
                catch (System.Net.WebException webEx)
                {
                    if (webEx.Message.Contains("404"))
                    {
                        logger.Log("UICalled: IsServiceReady = false for {0}", absoluteUrl);
                        return new List<string>() { "", "false" };
                    }
                    else if (webEx.Message.Contains("405"))
                    {
                        logger.Log("UICalled: IsServiceReady = true for {0}", absoluteUrl);
                        return new List<string>() { "", "true" };
                    }
                    else
                    {
                        logger.Log("UICalled: IsServiceReady webexception for {0}", absoluteUrl);
                        return new List<string>() { "Unknown WebException: " + webEx.Message };
                    }
                }
            }
            catch (Exception exception)
            {
                logger.Log("Exception in IsServiceReady: " + exception.ToString());
                return new List<string>() { exception.Message };
            }
        }

        public List<string> AddLiveIdUser(string userName, string groupName, string liveId, string liveIdUniqueUserToken)
        {
            try
            {
                logger.Log("UICalled: AddLiveIdUser {0} {1} {2} {3}", userName, groupName, liveId, liveIdUniqueUserToken);

                var result = platform.AddLiveIdUser(userName, groupName, liveId, liveIdUniqueUserToken);

                if (result.Item1)
                    return new List<string>() { "" };
                else
                    return new List<string>() { result.Item2 };
            }
            catch (Exception exception)
            {
                logger.Log("Exception in AddLiveIdUser: " + exception.ToString());
                return new List<string>() { exception.Message };
            }
        }

        /// <summary>
        /// Returns the list of all users as [name,liveid] tuples
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllUsersWeb()
        {
            try
            {
                logger.Log("UICalled: GetAllUsersWeb");

                var users = config.GetAllUsers();

                List<string> retList = new List<string>();

                foreach (var user in users)
                {
                    if (!user.Name.Equals(Constants.SystemLow) && !user.Name.Equals(Constants.SystemHigh))
                    {
                        retList.Add(user.Name);
                        retList.Add(user.LiveId);
                    }
                }

                retList.Insert(0, "");
                return retList;
            }
            catch (Exception exception)
            {
                logger.Log("Exception in GetAllUsersWeb: " + exception.ToString());
                return new List<string>() { exception.Message };
            }
        }

        /// <summary>
        /// Returns the list of all groups as list of [names]
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllGroupsWeb()
        {
            try
            {
                logger.Log("UICalled: GetAllGroupsWeb");

                var groups = config.GetAllGroups();

                List<string> retList = new List<string>();

                foreach (var group in groups)
                {
                    UserInfo user = group as UserInfo;
                    if (user == null)
                    {
                        retList.Add(group.Name);
                    }
                }

                retList.Insert(0, "");
                return retList;
            }
            catch (Exception exception)
            {
                logger.Log("Exception in GetAllUsersWeb: " + exception.ToString());
                return new List<string>() { exception.Message };
            }
        }

        public List<string> RemoveLiveIdUser(string userName)
        {
            try
            {
                logger.Log("UICalled: RemoveLiveIdUser {0}", userName);

                var result = platform.RemoveLiveIdUser(userName);

                if (result.Item1)
                    return new List<string>() { "" };
                else
                    return new List<string>() { result.Item2 };
            }
            catch (Exception exception)
            {
                logger.Log("Exception in AddLiveIdUser: " + exception.ToString());
                return new List<string>() { exception.Message };
            }
        }

    }

    [ServiceContract]
    public interface IGuiServiceWeb
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetVersion();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> SendEmail(string dst, string subject, string body);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetApps();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetUnconfiguredDevicesForScout(string scoutName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetAllUnconfiguredDevices();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> StartDriver(string uniqueDeviceId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetDeviceDetails(string uniqueDeviceId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> IsDeviceReady(string uniqueDeviceId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> ConfigureDeviceWeb(string uniqueDeviceId, string friendlyName, bool highSecurity, string location, string[] apps);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetCompatibleDevicesForHomestoreApp(string appName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetCompatibleAppsNotInstalledWeb(string uniqueDeviceId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetCompatibleAppsInstalledWeb(string uniqueDeviceId);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> InstallAppWeb(string appName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetAllLocations();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> AddLocation(string locationToAdd, string parentLocation);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetConfSettingWeb(string confKey);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> SetHomeIdWeb(string homeId, string password);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetInstalledAppsWeb();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> IsConfigNeededWeb();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetConfiguredDevicesWeb();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetOrphanedDevicesWeb();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetVisibleWifiNetworksWeb();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> ConnectToWifiNetworkWeb(string targetSsid, string passPhrase);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> SetNotificationEmailWeb(string emailAddress);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> AddZwaveWeb();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> RemoveZwaveWeb(string friendlyName, string failedNode);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> AbortAddZwaveWeb();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> AbortRemoveZwaveWeb();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetRemoteAccessUrlWeb();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> AllowAppAccessToDevice(string appFriendlyName, string deviceFriendlyName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> DisallowAppAccessToDevice(string appFriendlyName, string deviceFriendlyName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetCompatibleDevicesForInstalledApp(string appFriendlyName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetCompatibleInstalledAppsForDevice(string deviceFriendlyName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> RemoveDeviceWeb(string deviceFriendlyName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> RemoveOrphanedDeviceWeb(string moduleFacingName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> RemoveAppWeb(string appFriendlyName);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> IsServiceReady(string absoluteUrl);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetAllUsersWeb();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetAllGroupsWeb();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> AddLiveIdUser(string userName, string groupName, string liveId, string liveIdUniqueUserToken);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> RemoveLiveIdUser(string userName);
    }

    [ServiceContract]
    public interface IGuiServiceWebSec
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetVersion();
    }

}
