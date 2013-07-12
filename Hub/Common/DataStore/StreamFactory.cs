using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using HomeOS.Shared;

namespace HomeOS.Hub.Common.DataStore
{
    public sealed class StreamFactory
    {
        private static volatile StreamFactory instance;
        private static object syncRoot = new Object();

        public enum StreamOp : byte { Read = 0, Write }

        private StreamFactory() { }

        public static StreamFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new StreamFactory();
                    }

                    if (!Stopwatch.IsHighResolution)
                    {
                        throw new PlatformNotSupportedException("High frequency timer not available!");
                    }
                }

                return instance;
            }
        }

        public IStream createFileStream<KeyType, ValType>(FqStreamID FQSID, StreamFactory.StreamOp Op,
                                                          CallerInfo Ci, RemoteInfo ri, SynchronizerType st)
        {
            ISync synchronizer = null;
            if (ri != null)
            {
                // azure container names don't like / or uppercase
                string container = FQSID.ToString().Replace('/', '-').ToLower();
                synchronizer = SyncFactory.Instance.CreateSynchronizer(st, ri, container);
            }
            return new DataFileStream<KeyType, ValType>(FQSID, Op, Ci, synchronizer);
        }

        public IStream createDirStream<KeyType, ValType>(FqStreamID FQSID, StreamFactory.StreamOp Op,
                                                         CallerInfo Ci, RemoteInfo ri, SynchronizerType st)
        {
            ISync synchronizer = null;
            if (ri != null)
            {
                // azure container names don't like / or uppercase
                string container = FQSID.ToString().Replace('/', '-').ToLower();
                synchronizer = SyncFactory.Instance.CreateSynchronizer(st, ri, container);
            } 
            return new DataDirStream<KeyType, ValType>(FQSID, Op, Ci, synchronizer);
        }

        public static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public static string GetString(byte[] bytes)
        {
            if (bytes != null)
            {
                char[] chars = new char[bytes.Length / sizeof(char)];
                System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
                return new string(chars);
            }
            else
            {
                return "";
            }
        }

        public static long Now()
        {
             return Stopwatch.GetTimestamp(); 
        }
    }
}
