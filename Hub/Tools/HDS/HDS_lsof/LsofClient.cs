using HomeOS.Hub.Common.DataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HomeOS.Hub.Tools.HDS.HDS_Client
{
    class LsofClient
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                System.Console.WriteLine("Usage: lsof Op(print/clear) Stream");
                return;
            }

            try
            {
                Lsof lsof = new Lsof();
                string op = args[0];
                string path = args[1];

                
                path = Path.GetDirectoryName(path);

                Console.WriteLine("Path is " + path);

                if (!File.Exists(path + "/.md"))
                {
                    Console.WriteLine("Invalid path. Metadata file does not exist");
                    return;
                }

                switch (op)
                {
                    case "print":
                        lsof.PrintReaders(path);
                        lsof.PrintWriters(path);
                        break;
                    case "clear":
                        lsof.ClearReaders(path);
                        lsof.ClearWriters(path);
                        break;
                    default:
                        Console.WriteLine("Invalid operation");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} Exception caught.", e);
            }
        }
    }
}
