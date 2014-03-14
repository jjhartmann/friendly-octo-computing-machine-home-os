using HomeOS.Hub.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using HomeOS.Hub.Tools.PackagerHelper;

namespace HomeOS.Hub.Tools
{
    /// <summary>
    /// This tool package the Scouts on your local disk in a manner that they can be uploaded to the homestore repository
    /// </summary>
    class ScoutPackager
    {
        static void Main(string[] args)
        {
            var argsDict = ProcessArguments(args);

            string ScoutsRootDir = (string)argsDict["ScoutsRootDir"];
            string scoutName = (string)argsDict["ScoutName"];
            string repoDir = (string)argsDict["RepoDir"];

            // file/directory status
            string currentDir = Directory.GetCurrentDirectory();
            Console.WriteLine("Current Directory is {0}", currentDir);
            Console.WriteLine("ScoutsRootDir is {0}", Path.GetFullPath(ScoutsRootDir));
            Console.WriteLine("RepoDir is {0}", Path.GetFullPath(repoDir));

            if (!Directory.Exists(ScoutsRootDir))
            {
                Console.Error.WriteLine("ScoutsRootDir directory {0} does not exist!", ScoutsRootDir);
                System.Environment.Exit(1);
            }

            //get the scouts
            List<string> scoutsList = GetScouts(ScoutsRootDir, scoutName);

            bool packagedSomething = false;

            foreach (string scout in scoutsList)
            {
                if (string.IsNullOrWhiteSpace(scoutName) ||
                    scout.Equals(scoutName))
                {
                    string[] filePaths = new string[0];
                    BinaryPackagerHelper.Package(ScoutsRootDir, scout, false /* singleBin */, "dll", "scout", repoDir, ref filePaths);
                    packagedSomething = true;
                }
            }   

            if (!packagedSomething) 
            {
                Console.Error.WriteLine("I did not package anything. Did you supply the correct ScoutsRootDir ({0})?", ScoutsRootDir);
                if (!string.IsNullOrWhiteSpace(scoutName))
                    Console.Error.WriteLine("Is there a views dll in the output directory of {0}", scoutName);
            }
        }

        private static List<string> GetScouts(string ScoutsRootDir,  string scoutName)
        {

            // Search for add-ins of type VModule
            List<string> scoutsFullPath = Directory.GetDirectories(ScoutsRootDir).ToList();
            List<string> scoutNames = new List<string>();
            foreach (string scoutPath in scoutsFullPath)
            {
                scoutNames.Add(Path.GetFileName(scoutPath));
            }

            return scoutNames;
        }

        /// <summary>
        /// Processes the command line arguments
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        private static ArgumentsDictionary ProcessArguments(string[] arguments)
        {

            //the default values are under the assumption that you run this tool from the directory where Hub.sln sits.

            ArgumentSpec[] argSpecs = new ArgumentSpec[]
            {
                new ArgumentSpec(
                    "Help",
                    '?',
                    false,
                    null,
                    "Display this help message."),
               new ArgumentSpec(
                   "ScoutsRootDir",
                   'd',
                   "output\\binaries\\Scouts",
                   "directory name",
                   "Root directory for all Scouts"),
               new ArgumentSpec(
                   "ScoutName",
                   'n',
                   "",
                   "scout name",
                   "Name of the scout. Leave empty if you want to package all scouts"),
             new ArgumentSpec(
                   "RepoDir",
                   'r',
                   "output\\HomeStore\\repository",
                   "directory name",
                   "Top-level directory where we should create the homestore repository"),
            };

            ArgumentsDictionary args = new ArgumentsDictionary(arguments, argSpecs);
            if (args.AppSettingsParseError)
            {
                Console.Error.WriteLine("Error in .config file options: ignoring");
            }

            if (args.CommandLineParseError)
            {
                Console.Error.WriteLine("Error in command line arguments at {0}\n", args.ParseErrorArgument);
                Console.Error.WriteLine(args.GetUsage("ScoutPackager"));
                System.Environment.Exit(1);
            }

            if ((bool)args["Help"])
            {
                Console.Error.WriteLine("Packages platform binaries\n");
                Console.Error.WriteLine(args.GetUsage("ScoutPackager"));
                System.Environment.Exit(0);
            }

            return args;
        }
    }
}
