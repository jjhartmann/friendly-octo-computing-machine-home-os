using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web;

namespace HomeOS.Cloud.Platform.RedirectWebRole
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class RedirectService : IRedirectService
    {
        public string RedirectString()
        {
            return "foo";
        }

        public Stream Redirect(Stream input)
        {
            var streamReader = new StreamReader(input);
            string streamString = streamReader.ReadToEnd();
            streamReader.Close();
            System.Collections.Specialized.NameValueCollection nvc = HttpUtility.ParseQueryString(streamString);
            string action = string.IsNullOrEmpty(nvc["action"]) ? "" : nvc["action"];
            string stoken = string.IsNullOrEmpty(nvc["stoken"]) ? "" : nvc["stoken"];
            string appctx = string.IsNullOrEmpty(nvc["appctx"]) ? "" : nvc["appctx"];



            string html = ""; 
            if (action == "login")
            {
                string token = HttpUtility.UrlEncode(stoken);
                Uri uri = new Uri(appctx);
                string homeID = uri.LocalPath.Split('/')[1];
                string newURL = uri.Scheme + "://" + uri.Host + ":" + uri.Port + "/" + homeID + "/auth/redirect?stoken=" + token + "&appctx=" + appctx + "&action=" + action + "&scheme=liveid";
                html = "<html><meta http-equiv='refresh' content='0; url="+newURL+"'> </html>";

            }
            else if (action == "logout")
            {

            }
            else
            {

            }

            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            byte[] htmlBytes = Encoding.UTF8.GetBytes(html);
            return new MemoryStream(htmlBytes);
            
        }
    }
}
