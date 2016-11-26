using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Threading;
using System.ServiceModel.Web;
using HomeOS.Hub.Platform.Views;
using HomeOS.Hub.Common;

namespace HomeOS.Hub.Apps.TapTap
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TapTapService : ITapTapContract
    {
        protected VLogger logger;
        TapTap TapTap;

        public TapTapService(VLogger logger, TapTap TapTap)
        {
            this.logger = logger;
            this.TapTap = TapTap;
        }

        public List<string> GetReceivedMessages()
        {
            List<string> retVal = new List<string>();
            try
            {
                retVal = TapTap.GetReceivedMessages();
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetReceivedMessages: " + e);
            }
            return retVal;
        }

        public List<string> SetupDevice()
        {
            List<string> retVal = new List<string>();
            try
            {
                retVal = TapTap.GetReceivedMessages();
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetReceivedMessages: " + e);
            }
            return retVal;
        }


        public Dictionary<string, string> GetAllDevices()
        {
            Dictionary<string, string> retVal = new Dictionary<string, string>();
            try
            {
                retVal = TapTap.GetAllDevices();
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetReceivedMessages: " + e);
            }
            return retVal;
        }

        public Dictionary<string, string> GetAllThings()
        {
            Dictionary < string, string> retVal = new Dictionary<string, string> ();
            try
            {
                retVal = TapTap.GetAllThings();
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetReceivedMessages: " + e);
            }
            return retVal;
        }

        public bool SaveDeviceName(string id, string name)
        {
            bool retVal = false;
            try
            {
                retVal = TapTap.SaveDeviceName(id, name);
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetReceivedMessages: " + e);
            }
            return retVal;
        }

        public bool SaveThingTag(string id, string tag)
        {
            bool retVal = false;
            try
            {
                retVal = TapTap.SaveThingTag(id, tag);
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetReceivedMessages: " + e);
            }
            return retVal;
        }

        public List<string> GetDeviceRequests()
        {
            List<string> retVal = new List<string>();
            try
            {
                retVal = TapTap.GetDeviceRequests();
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetReceivedMessages: " + e);
            }
            return retVal;
        }
    }

    [ServiceContract]
    public interface ITapTapContract
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetReceivedMessages();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> SetupDevice();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        Dictionary<string, string> GetAllDevices();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        Dictionary<string, string> GetAllThings();

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        bool SaveDeviceName(string id, string name);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        bool SaveThingTag(string id, string tag);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetDeviceRequests();
    }
}