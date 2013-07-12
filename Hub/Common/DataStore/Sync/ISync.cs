using System;
using System.IO;

namespace HomeOS.Hub.Common.DataStore
{
    public interface ISync
    {
        void SetLocalSource(string FqDirName);

        /* Sync Data */
        bool Sync();

        void Dispose();
    }
}
