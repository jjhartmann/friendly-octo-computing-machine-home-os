using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HomeOS.Hub.Common.DataStore;
using HomeOS.Shared;
using System.Collections.Generic;
using System.Collections;
using System.Threading;

namespace HomeOS.Hub.UnitTests.Common.DataStore
{
    [TestClass]
    public class SyncFileStreamTest
    {
        StrKey k1;
        StrKey k2;

        [TestInitialize]
        public void Setup()
        {
            k1 = new StrKey("k1");
            k2 = new StrKey("k2");
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void SyncFileStreamTest_TestRepeatedClose()
        {
            for (int i = 0; i < 10; ++i)
            {
                StreamFactory sf = StreamFactory.Instance;
                IStream dfs_byte_val = sf.createFileStream<StrKey, ByteValue>(new FqStreamID("99-2729", "A0", "TestMultiClose"),
                            StreamFactory.StreamOp.Write,
                            new CallerInfo(null, "A0", "A0", 1),
                            new RemoteInfo("homelab", "123"),
                            SynchronizerType.Azure);
                dfs_byte_val.Append(k1, new ByteValue(StreamFactory.GetBytes("k1-cmu-" + i)));
                dfs_byte_val.Close();
                Thread.Sleep(5000);
            }
        }
    }
}
