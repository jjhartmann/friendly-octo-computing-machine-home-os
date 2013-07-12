using System;
using System.Diagnostics;
using System.IO;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace HomeOS.Cloud.Platform.Heartbeat
{
    public class HeartbeatLocalStorageTraceListener : XmlWriterTraceListener
    {
        public HeartbeatLocalStorageTraceListener()
            : base(Path.Combine(HeartbeatLocalStorageTraceListener.GetLogDirectory().Path, "HeartbeatService.svclog"))
        {
        }

        public static DirectoryConfiguration GetLogDirectory()
        {
            DirectoryConfiguration directory = new DirectoryConfiguration();
            directory.Container = "wad-tracefiles";
            directory.DirectoryQuotaInMB = 10;
            directory.Path = RoleEnvironment.GetLocalResource("HeartbeatService.svclog").RootPath;
            return directory;
        }

        public override void WriteLine(string s)
        {
            base.WriteLine(DateTime.Now.ToUniversalTime().ToLongTimeString() + " " + s);
        }
    }
}
