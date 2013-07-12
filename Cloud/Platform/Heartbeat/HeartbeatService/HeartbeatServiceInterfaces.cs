using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using HomeOS.Shared;
using System.ComponentModel;

namespace HomeOS.Cloud.Platform.Heartbeat
{
    [ServiceContract]
    public interface IHeartbeatListenerService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare,
                RequestFormat=WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("Log the Heartbeat Information in the Cloud Storage")]
        void SetHeartbeatInfo(HeartbeatInfo hbi);

        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Bare,
                RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("Checks if the specified Home OS identifier is appropriate for use by client by making sure it is unique per hardware.")]
        bool CanClaimHomeId(ClaimHomeIdInfo chi);
    }

    [ServiceContract]
    public interface IHeartbeatMonitorService
    {
        /// <summary>
        /// Returns a list of HeartbeatInfo tuples specified by using the Hub Org Id
        /// </summary>
        /// <param name="orgId"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("Retrieve the Heartbeat Information by providing the Org Id")]
        List<Tuple<string /* orgId */, string /* homeId */, DateTime /* UtcTime */, HeartbeatInfo>> GetHeartbeatInfoRangeByOrg(string orgId);

        /// <summary>
        /// Returns a list of HeartbeatInfo samples range specified using the Hub Org Id and write time stamps of the cloud server. 
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="timeOffset"></param>
        /// <returns></returns>
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("Retrieve the Heartbeat Information by providing the Org Id and start time (Cloud Side in UTC) and the range as a time offset (+ or -)")]
        List<Tuple<string /* orgId */, string /* homeId */, DateTime /* UtcTime */, HeartbeatInfo>> GetHeartbeatInfoRangeByOrgAndCloudTime(string orgId, string startTimeUtc, string timeOffset);
    }
}
