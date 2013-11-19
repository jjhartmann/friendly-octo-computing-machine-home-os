using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;

//needed for ArgumentHelper
using HomeOS.Hub.Common;
using HomeOS.Hub.Tools.PackagerHelper;

namespace PlatformPackager
{
    class PlatformPackager
    {
        const string platformBinaryName = "HomeOS.Hub.Platform";

        static void Main(string[] args)
        {
            var argsDict = ProcessArguments(args);

            string platformRootDir = (string)argsDict["PlatformRootDir"];
            string repoDir = (string)argsDict["RepoDir"];

            // check the platform binary is present
            string platformExe = platformRootDir + "\\" + platformBinaryName + ".exe";
            if (!File.Exists(platformExe))
            {
                Console.Error.WriteLine("Platform binary {0} not found. Quitting.", platformExe);
                return;
            }

            BinaryPackagerHelper.Package(platformRootDir, platformBinaryName, true /*singleBin*/, "exe", "platform", repoDir);
        }

        /// <summary>
        /// Processes the command line arguments
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static ArgumentsDictionary ProcessArguments(string[] arguments)
        {
            ArgumentSpec[] argSpecs = new ArgumentSpec[]
            {
                new ArgumentSpec(
                    "Help",
                    '?',
                    false,
                    null,
                    "Display this help message."),
               new ArgumentSpec(
                   "PlatformRootDir",
                   'd',
                   "output\\binaries\\Platform",
                   "directory name",
                   "Platform directory containing the binaries"),
             new ArgumentSpec(
                   "RepoDir",
                   'r',
                   "output\\HomeStore\\repository",
                   "directory name",
                   "Top-level directory where we should create the homestore repository")
            };

            ArgumentsDictionary args = new ArgumentsDictionary(arguments, argSpecs);
            if (args.AppSettingsParseError)
            {
                Console.Error.WriteLine("Error in .config file options: ignoring");
            }

            if (args.CommandLineParseError)
            {
                Console.Error.WriteLine("Error in command line arguments at {0}\n", args.ParseErrorArgument);
                Console.Error.WriteLine(args.GetUsage("PlatformPackager"));
                System.Environment.Exit(1);
            }

            if ((bool)args["Help"])
            {
                Console.Error.WriteLine("Packages platform binaries\n");
                Console.Error.WriteLine(args.GetUsage("PlatformPackager"));
                System.Environment.Exit(0);
            }

            return args;
        }
    }
}
