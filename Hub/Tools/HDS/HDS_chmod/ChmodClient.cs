using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HomeOS.Hub.Common.DataStore;

namespace HomeOS.Hub.Tools.HDS.HDS_Client
{
    class ChmodClient
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                System.Console.WriteLine("Usage: chmod Op Stream AppName");
                return;
            }

            try
            {
                Chmod c = new Chmod();
                string op = args[0];
                string path = args[1];
                string appName = args[2];

                //TODO(trinabh): What if AppName is a wierd string that messes with Json

                path = Path.GetDirectoryName(path);

                Console.WriteLine("Path is " + path);

                if (!File.Exists(path + "./md"))
                {
                    Console.WriteLine("Invalid path. Metadata file does not exist");
                    return;
                }

                switch (op)
                {
                    case "+r":
                        c.AddRead(path, appName);
                        break;
                    case "+w":
                        c.AddWrite(path, appName);
                        break;
                    case "+rw":
                        c.AddRead(path, appName);
                        c.AddWrite(path, appName);
                        break;
                    case "-r":
                        c.RemoveRead(path, appName);
                        break;
                    case "-w":
                        c.RemoveWrite(path, appName);
                        break;
                    case "-rw":
                        c.RemoveRead(path, appName);
                        c.RemoveWrite(path, appName);
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
