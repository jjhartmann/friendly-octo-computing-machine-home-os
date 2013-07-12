using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using HomeOS.Shared;

namespace HomeOS.Hub.Common.DataStore
{
    public enum SynchronizerType : byte { None = 0, Azure }

    public sealed class SyncFactory
    {
        private static volatile SyncFactory instance;
        private static object syncRoot = new Object();

        private SyncFactory() { }

        public static SyncFactory Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new SyncFactory();
                    }
                }

                return instance;
            }
        }

        public ISync CreateSynchronizer(SynchronizerType st, RemoteInfo ri, string container)
        {
            ISync isync = null;
            switch (st)
            {
                case SynchronizerType.Azure:
                    isync = CreateAzureSynchronizer(ri, container);
                    break;
                default:
                    isync = null;
                    break;
            }
            return isync;
        }

        private ISync CreateAzureSynchronizer(RemoteInfo ri, string container)
        {

            return new AzureSynchronizer(ri, container);
        }
    }
}
