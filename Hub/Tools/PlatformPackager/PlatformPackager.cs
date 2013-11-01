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

namespace PlatformPackager
{
    class PlatformPackager
    {
        const string platformBinaryName = "HomeOS.Hub.Platform";
        const string DefaultHomeOSUpdateVersionValue = "0.0.0.0";
        const string ConfigAppSettingKeyHomeOSUpdateVersion = "HomeOSUpdateVersion";

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

            Package(platformRootDir, platformBinaryName, repoDir);

        }

        private static void Package(string platformDir, string platformBinaryName, string repoDir)
        {

            if (!Directory.Exists(platformDir))
            {
                Console.Error.WriteLine("Platform directory {0} does not exist?", platformDir);
                return;
            }

            //get the zip dir
            string zipDir = repoDir;

            string[] parts = platformBinaryName.Split('.');

            foreach (var part in parts)
                zipDir += "\\" + part;

            // Use HomeOSUpdateVersion from App.Config

            string file = platformDir + "\\" + platformBinaryName + ".exe.config";
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
                Console.WriteLine("Warning didn't find platform version in {0}, defaulting to {1}", file, homeosUpdateVersion);
            }

            zipDir += "\\" + homeosUpdateVersion;
            Directory.CreateDirectory(zipDir);

            //get the name of the zip file and pack it
            string zipFile = zipDir + "\\" + platformBinaryName + ".zip";
            string hashFile = zipDir + "\\" + platformBinaryName + ".md5";

            bool result = PackZip(platformDir, zipFile);

            if (!result)
            {
                Console.Error.WriteLine("Failed to pack zip for {0}. Quitting", platformBinaryName);
                return;
            }

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
                Console.Out.WriteLine("Failed to write hash file {0}, Exception:{1}. Quitting", hashFile, e.ToString());
                return;
            }

            Console.Out.WriteLine("Prepared platform package: {0}.\n Hash file: {1}", zipFile, hashFile);
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
                   'b',
                   "..\\..\\..\\..\\output\\binaries",
                   "directory name",
                   "The parent directory of the platform directory containing the binaries"),
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

        private static bool PackZip(string startPath, String zipPath)
        {
            try
            {
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
                System.IO.Compression.ZipFile.CreateFromDirectory(startPath, zipPath, System.IO.Compression.CompressionLevel.Optimal, true);
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
