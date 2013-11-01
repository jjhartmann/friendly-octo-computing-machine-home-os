using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;

using System;
using System.AddIn.Hosting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

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
                    Package(addInRoot, token, repoDir);
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

        const string DefaultHomeOSUpdateVersionValue = "0.0.0.0";
        const string ConfigAppSettingKeyHomeOSUpdateVersion = "HomeOSUpdateVersion";

        private static void Package(string addInRoot, AddInToken token, string repoDir)
        {
            //get the module directory
            string moduleDir = addInRoot + "\\AddIns\\" + token.Name;

            if (!Directory.Exists(moduleDir))
            {
                Console.Error.WriteLine("Module directory {0} does not exist. Is there a mismatch in moduleName and its location?", moduleDir);
                return;
            }

            //get the zip dir
            string zipDir = repoDir;

            string[] parts = token.Name.Split('.');

            foreach (var part in parts)
                zipDir += "\\" + part;

            // Use HomeOSUpdateVersion from App.Config

            string file = moduleDir + "\\" + token.Name + ".dll.config";
            string homeosUpdateVersion = DefaultHomeOSUpdateVersionValue;
            try
            {
                XElement xmlTree = XElement.Load(file);
                IEnumerable<XElement> das =
                    from el in xmlTree.DescendantsAndSelf()
                    where el.Name == "add" && el.Parent.Name == "appSettings" && el.Attribute("key").Value == ConfigAppSettingKeyHomeOSUpdateVersion
                    select el;
                if (das.Count() > 0)
                {
                    homeosUpdateVersion = das.First().Attribute("value").Value;
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Failed to parse {0}, exception: {1}", file, e.ToString());
            }

            if (homeosUpdateVersion == DefaultHomeOSUpdateVersionValue)
            {
                Console.WriteLine("Warning didn't find module version in {0}, defaulting to {1}", file, homeosUpdateVersion);
            }

            zipDir += "\\" + homeosUpdateVersion;
            Directory.CreateDirectory(zipDir);

            //get the name of the zip file and pack it
            string zipFile = zipDir + "\\" + token.Name + ".zip";
            string hashFile = zipDir + "\\" + token.Name + ".md5";

            bool result = PackZip(moduleDir, zipFile);

            if (!result)
                Console.Error.WriteLine("Failed to pack zip for {0}. Quitting", token.Name);

            string md5hash = GetMD5HashOfFile(zipFile);

            if (string.IsNullOrWhiteSpace(md5hash))
            {
                return;
            }

            try
            {
                File.WriteAllText(hashFile, md5hash);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine("Failed to write hash file {0}. Quitting", hashFile);
                return;
            }

            Console.Out.WriteLine("Prepared module package: {0}.\n", zipFile);
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
                   'a',
                   "output\\binaries\\Pipeline",
                   "directory name",
                   "Root directory for AddIns (i.e., the parent directory of AddIns, under which modules are present)"),
               new ArgumentSpec(
                   "ModuleName",
                   'm',
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

        private static void CopyFolder(string sourceFolder, string destFolder)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);
            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);

                int numTries = 3;
                while (numTries > 0)
                {
                    try
                    {
                        numTries--;

                        File.Copy(file, dest, true);

                        break;
                    }
                    catch (Exception e)
                    {
                        if (numTries > 0)
                            System.Threading.Thread.Sleep(5 * 1000);
                        else
                            Console.WriteLine("Failed to copy " + file + "\n" + e.ToString(), true);
                    }
                }
            }
            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                try
                {
                    CopyFolder(folder, dest);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e.ToString(), true);
                }
            }
        }

        private static bool PackZip(string startPath, String zipPath)
        {
            try
            {
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
                System.IO.Compression.ZipFile.CreateFromDirectory(startPath, zipPath);

                return true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("PackZipError. Ran with {0}, {1}. Error = {2}", startPath, zipPath, e.Message);
                return false;
            }
        }

        private static string GetMD5HashOfFile(string filePath)
        {
            try
            {
                FileStream file = new FileStream(filePath, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("E", e.Message + ". GetMD5HashOfFile(), file" + filePath);
                return "";
            }
        }

    }
}
