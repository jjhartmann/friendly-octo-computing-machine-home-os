using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace HomeOS.Hub.Common.DataStore
{
    public class Chmod
    {

        public Chmod()
        {
        }

        public void AddRead(string Path, string AppName)
        {
            //TODO(trinabh): Doesn't respect locking
            MetaData md = new MetaData(Path, ".md");
            if (!md.load)
            {
                throw new IOException("Metadata file " + Path + " does not exist.");
            }

            md.SetReadAccess(AppName);
            md.FlushMetaData();
        }

        public void AddWrite(string Path, string AppName)
        {
            //TODO(trinabh): Doesn't respect locking
            MetaData md = new MetaData(Path, ".md");
            if (!md.load)
            {
                throw new IOException("Metadata file " + Path + " does not exist.");
            }
            md.SetWriteAccess(AppName);
            md.FlushMetaData();
        }
        
        public void RemoveRead(string Path, string AppName)
        {
            //TODO(trinabh): Doesn't respect locking
            MetaData md = new MetaData(Path, ".md");
            if (!md.load)
            {
                throw new IOException("Metadata file " + Path + " does not exist.");
            }
            md.RemoveReadAccess(AppName);
            md.FlushMetaData();
        }

        public void RemoveWrite(string Path, string AppName)
        {
            //TODO(trinabh): Doesn't respect locking
            MetaData md = new MetaData(Path, ".md");
            if (!md.load)
            {
                throw new IOException("Metadata file " + Path + " does not exist.");
            }
            
            md.RemoveWriteAccess(AppName);
            md.FlushMetaData();
        }
    }
}
