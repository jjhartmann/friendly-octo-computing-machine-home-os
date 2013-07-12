using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HomeOS.Hub.Common.DataStore;
using HomeOS.Shared;

namespace HomeOS.Hub.UnitTests.Common.DataStore
{
    [TestClass]
    public class DirStreamTest
    {
        IStream dds_byte_val;
        StrKey k1;
        StrKey k2;

        [TestInitialize]
        public void Setup()
        {
            StreamFactory sf = StreamFactory.Instance;
            dds_byte_val = sf.createDirStream<StrKey, ByteValue>(new FqStreamID("99-2729", "A0", "TestDS"),
                                                                StreamFactory.StreamOp.Write,
                                                                new CallerInfo(null, "A0", "A0", 1),
                                                                null, SynchronizerType.None);

            k1 = new StrKey("k1");
            k2 = new StrKey("k2");
        }

        [TestCleanup]
        public void Cleanup()
        {
            dds_byte_val.Close();
        }

        [TestMethod]
        public void DirStreamTest_TestUpdateByteValue()
        {
            dds_byte_val.Update(k1, new ByteValue(StreamFactory.GetBytes("k1-cmu")));
            dds_byte_val.Update(k2, new ByteValue(StreamFactory.GetBytes("k2-msr")));
            dds_byte_val.Update(k1, new ByteValue(StreamFactory.GetBytes("k1-msr")));
        }

        [TestMethod]
        public void DirStreamTest_TestGetByteValue()
        {
            Assert.IsTrue("k1-msr" == dds_byte_val.Get(k1).ToString());
            Assert.IsTrue("k2-msr" == dds_byte_val.Get(k2).ToString());
        }

        [TestMethod]
        public void DirStreamTest_TestGetLatestStrValue()
        {
            Assert.IsTrue("k1-msr" == dds_byte_val.GetLatest().Item2.ToString());
        }
    }
}
