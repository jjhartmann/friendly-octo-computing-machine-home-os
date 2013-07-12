using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.IO;
using System.Text;
using Microsoft.Synchronization;
using Microsoft.Synchronization.Files;

using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using HomeOS.Hub.Common.DataStore;

namespace HomeOS.Hub.Tools.HDS.HDS_Sync
{
    public class TestDataSync
    {
        static void Main(string[] args)
        {
            //
            // Setup Store and Provider
            //
            string accountName = ConfigurationManager.AppSettings.Get("AccountName");
            string accountKey = ConfigurationManager.AppSettings.Get("AccountSharedKey");
            RemoteInfo ri = new RemoteInfo(accountName, accountKey);
            
            StreamFactory sf = StreamFactory.Instance;
            IStream dfs = sf.createFileStream<StrKey, ByteValue>(new FqStreamID("99-2729", "A0", "TestBS"),
                                                                 StreamFactory.StreamOp.Write,
                                                                 new CallerInfo(null, "A0", "A0", 1),
                                                                 ri, SynchronizerType.Azure);

            StrKey akey = new StrKey("amar");
            StrKey rkey = new StrKey("ratul");

            dfs.Update(akey, new ByteValue(StreamFactory.GetBytes("phanishayee")));
            dfs.Update(rkey, new ByteValue(StreamFactory.GetBytes("mahajan")));
            dfs.Update(akey, new ByteValue(StreamFactory.GetBytes("CMU")));

            Console.WriteLine("amar ==> " + dfs.Get(akey));
            Console.WriteLine("ratul ==> " + dfs.Get(rkey));

            dfs.Close();

            Console.ReadKey();

            /////

            IStream dfs2 = sf.createFileStream<StrKey, StrValue>(new FqStreamID("99-2729", "A0", "TestSS"),
                                                                 StreamFactory.StreamOp.Write,
                                                                 new CallerInfo(null, "A0", "A0", 1),
                                                                 ri, SynchronizerType.Azure);

            dfs2.Update(akey, new StrValue("phanishayee"));
            dfs2.Update(rkey, new StrValue("mahajan"));
            dfs2.Update(akey, new StrValue("CMU"));

            Console.WriteLine("amar ==> " + dfs2.Get(akey));
            Console.WriteLine("ratul ==> " + dfs2.Get(rkey));

            dfs2.Close();

            Console.ReadKey();

            /////

            IStream dds = sf.createDirStream<StrKey, ByteValue>(new FqStreamID("99-2729", "A0", "TestDS"),
                                                                StreamFactory.StreamOp.Write,
                                                                new CallerInfo(null, "A0", "A0", 1),
                                                                ri, SynchronizerType.Azure);

            dds.Update(akey, new ByteValue(StreamFactory.GetBytes("phanishayee")));
            dds.Update(rkey, new ByteValue(StreamFactory.GetBytes("mahajan")));
            dds.Update(akey, new ByteValue(StreamFactory.GetBytes("CMU")));

            Console.WriteLine("amar ==> " + dds.Get(akey));
            Console.WriteLine("ratul ==> " + dds.Get(rkey));

            dds.Close();

            Console.ReadKey();
        }
    }
}
