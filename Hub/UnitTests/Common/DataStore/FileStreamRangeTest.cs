using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HomeOS.Hub.Common.DataStore;
using HomeOS.Shared;
using System.Collections.Generic;

namespace HomeOS.Hub.UnitTests.Common.DataStore
{
    [TestClass]
    public class FileStreamRangeTest
    {
        IStream dfs_str_val;
        List<IKey> keys;

        [TestInitialize]
        public void Setup()
        {
            StreamFactory sf = StreamFactory.Instance;
            dfs_str_val = sf.createFileStream<StrKey, StrValue>(new FqStreamID("99-2729", "A0", "TestRange"),
                                                                 StreamFactory.StreamOp.Write,
                                                                 new CallerInfo(null, "A0", "A0", 1),
                                                                 null, SynchronizerType.None);
            keys = new List<IKey>();
            for (int i = 0; i < 10; i++) 
            {
                keys.Add(new StrKey("k" + i));
                for (int j = 0; j < 100; j++)
                {
                    dfs_str_val.Append(keys[i], new StrValue("k" + i + "_value" + j));
                }
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            dfs_str_val.Close();
        }

        [TestMethod]
        public void FileStreamRangeTest_TestGetLatestStrValue()
        {
            Assert.IsTrue("k9_value99" == dfs_str_val.GetLatest().Item2.ToString());
        }
    }
}
