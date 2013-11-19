using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;
using HomeOS.Hub.Tools;
using System;
using System.AddIn.Hosting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using HomeOS.Hub.Tools.PackagerHelper;

namespace HomeOS.Hub.Tools
{
    /// <summary>
    /// This tool package the modules on your local disk in a manner that they can be uploaded to the homestore repository
    /// </summary>
    class ModulePackager
    {
        static void Main(string[] args)
        {
            var argsDict = ProcessArguments(args);

            string addInRoot = (string)argsDict["AddInRoot"];
            string moduleName = (string)argsDict["ModuleName"];
            string repoDir = (string)argsDict["RepoDir"];

            if (!Directory.Exists(addInRoot))
            {
                Console.Error.WriteLine("AddInRoot directory {0} does not exist!", addInRoot);
                System.Environment.Exit(1);
            }

            //get the tokens
            Collection<AddInToken> tokens = GetAddInTokens(addInRoot, moduleName);

            bool packagedSomething = false;

            foreach (AddInToken token in tokens)
            {
                if (string.IsNullOrWhiteSpace(moduleName) ||
                    token.Name.Equals(moduleName))
                {
                    BinaryPackagerHelper.Package(addInRoot + "\\AddIns", token.Name, false /*singleBin*/, "dll", "module", repoDir);
                    packagedSomething = true;
                }
            }   

            if (!packagedSomething) 
            {
                Console.Error.WriteLine("I did not package anything. Did you supply the correct AddInRoot ({0})?", addInRoot);
                if (!string.IsNullOrWhiteSpace(moduleName))
                    Console.Error.WriteLine("Is there a views dll in the output directory of {0}", moduleName);
            }
        }

        private static Collection<AddInToken> GetAddInTokens(string addInRoot,  string moduleName)
        {
            // rebuild the cache files of the pipeline segments and add-ins.
            string[] warnings = AddInStore.Rebuild(addInRoot);

            foreach (string warning in warnings)
                Console.WriteLine(warning);

            // Search for add-ins of type VModule
            Collection<AddInToken> tokens = AddInStore.FindAddIns(typeof(VModule), addInRoot);

            return tokens;
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
                   "AddInRoot",
                   'd',
                   "output\\binaries\\Pipeline",
                   "directory name",
                   "Root directory for AddIns (i.e., the parent directory of AddIns, under which modules are present)"),
               new ArgumentSpec(
                   "ModuleName",
                   'n',
                   "",
                   "module name",
                   "Name of the module. Leave empty if you want to package all modules"),
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
                Console.Error.WriteLine(args.GetUsage("ModulePackager"));
                System.Environment.Exit(1);
            }

            if ((bool)args["Help"])
            {
                Console.Error.WriteLine("Packages platform binaries\n");
                Console.Error.WriteLine(args.GetUsage("ModulePackager"));
                System.Environment.Exit(0);
            }

            //if (string.IsNullOrWhiteSpace((string)args["AddInRoot"]))
            //{
            //    Console.Error.WriteLine("AddInRoot not supplied\n");
            //    Console.Error.WriteLine(args.GetUsage("ModulePackager"));
            //    System.Environment.Exit(1);
            //}

            //if (string.IsNullOrWhiteSpace((string) args["ModuleName"]))
            //{
            //    Console.Error.WriteLine("Module name not supplied\n");
            //    Console.Error.WriteLine(args.GetUsage("ModulePackager"));
            //    System.Environment.Exit(1);
            //}

            //if (string.IsNullOrWhiteSpace((string)args["RepoDir"]))
            //{
            //    Console.Error.WriteLine("Repository directory name not supplied\n");
            //    Console.Error.WriteLine(args.GetUsage("ModulePackager"));
            //    System.Environment.Exit(1);
            //}

            return args;
        }

    }
}
