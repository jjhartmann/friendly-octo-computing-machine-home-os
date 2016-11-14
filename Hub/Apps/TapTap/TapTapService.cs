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

    }

     [ServiceContract]
    public interface ITapTapContract
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.WrappedRequest, ResponseFormat = WebMessageFormat.Json)]
        List<string> GetReceivedMessages();


    }
}