using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HomeOS.Hub.Common.DataStore
{
    public class Lsof
    {
        public Lsof()
        {
        }

        public void PrintReaders(string Path)
        {
            MetaData md = new MetaData(Path, ".md");
            if (!md.load)
            {
                throw new  IOException("Metadata file " + Path + " does not exist.");
            }

            Console.WriteLine("Active Readers:");
            foreach (string reader in md.GetReaders())
            {
                Console.WriteLine(reader);
            }
        }

        public void PrintWriters(string Path)
        {
            MetaData md = new MetaData(Path, ".md");
            if (!md.load)
            {
                throw new  IOException("Metadata file " + Path + " does not exist.");
            }

            Console.WriteLine("Active writers:");
            foreach (string reader in md.GetWriters())
            {
                Console.WriteLine(reader);
            }
        }

        public void ClearWriters(string Path)
        {
            //TODO: Respect locking
            MetaData md = new MetaData(Path, ".md");
            if (!md.load)
            {
                throw new IOException("Metadata file " + Path + " does not exist.");
            }

            md.ClearWriters();
            md.FlushMetaData();
        }

        public void ClearReaders(string Path)
        {
            //TODO: Respect locking
            MetaData md = new MetaData(Path, ".md");
            if (!md.load)
            {
                throw new IOException("Metadata file " + Path + " does not exist.");
            }

            md.ClearReaders();
            md.FlushMetaData();
        }
    }
}
