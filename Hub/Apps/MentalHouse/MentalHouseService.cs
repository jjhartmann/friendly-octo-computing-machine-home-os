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

namespace HomeOS.Hub.Apps.MentalHouse
{
    //Nicholas Kostriken

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class MentalHouseService : IMentalHouseContract
    {
        protected VLogger logger;
        MentalHouse mentalHouse;

        public MentalHouseService(VLogger logger, MentalHouse mentalHouse)
        {
            this.logger = logger;
            this.mentalHouse = mentalHouse;
        }


        public List<string> GetConnection()
        {
            List<string> retVal = new List<string>();
            try
            {
                retVal.Add(mentalHouse.GetConnection().ToString());
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetConnection: " + e);
            }
            return retVal;
        }

        public List<string> GetAttention()
        {
            List<string> retVal = new List<string>();
            try
            {
                retVal.Add(mentalHouse.GetAttention().ToString());
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetAttention: " + e);
            }
            return retVal;
        }

        public List<string> GetMeditation()
        {
            List<string> retVal = new List<string>();
            try
            {
                retVal.Add(mentalHouse.GetMeditation().ToString());
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetMeditation: " + e);
            }
            return retVal;
        }

        public List<string> GetWaves()
        {
            List<string> retVal = new List<string>();
            try
            {
                var waves = mentalHouse.GetWaves();
                for (int i = 0; i < waves.Count; i++)
                    retVal.Add(waves[i].ToString());
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetWaves: " + e);
            }
            return retVal;
        }

        public List<string> GetBlink()
        {
            List<string> retVal = new List<string>();
            try
            {
                retVal.Add(mentalHouse.GetBlink().ToString());
            }
            catch (Exception e)
            {
                logger.Log("Got exception in GetBlink: " + e);
            }
            return retVal;
        }


    }

     [ServiceContract]
    public interface IMentalHouseContract
    {
         [OperationContract]
         [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
         List<string> GetConnection();

         [OperationContract]
         [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
         List<string> GetAttention();

         [OperationContract]
         [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
         List<string> GetMeditation();

         [OperationContract]
         [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
         List<string> GetWaves();

         [OperationContract]
         [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, ResponseFormat = WebMessageFormat.Json)]
         List<string> GetBlink();

    }
}