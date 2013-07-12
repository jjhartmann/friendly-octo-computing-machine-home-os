using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace HomeOS.Cloud.Platform.RedirectWebRole
{
    [ServiceContract]
    public interface IRedirectService
    {        
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/redirect")]
        Stream Redirect(Stream input);
        
        [OperationContract]
        string RedirectString();
    }

}
