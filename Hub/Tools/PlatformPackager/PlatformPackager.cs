using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

//needed for ArgumentHelper
using HomeOS.Hub.Common;

namespace PlatformPackager
{
    class PlatformPackager
    {
        const string PLATFORM_DIRECTORY_NAME = "Platform";
        const string PLATFORM_CONFIG_FILE_NAME = "HomeOS.Hub.Platform.exe.config";

        static void Main(string[] args)
        {
            var argsDict = ProcessArguments(args);

            string sourceBinaryDir = (string)argsDict["BinaryDir"];

            if (!Directory.Exists(sourceBinaryDir))
            {
                Console.Error.WriteLine("Binary directory {0} does not exist!", sourceBinaryDir);
                System.Environment.Exit(1);
            }

            //1. create a temporary directory and copy there the contents of the binary directory
            var random = new Random();
            string tmpDirectory = "tmp." + random.Next();
            string tmpPlatformDir = tmpDirectory  + "\\" + PLATFORM_DIRECTORY_NAME;

            Directory.CreateDirectory(tmpDirectory);
            CopyFolder(sourceBinaryDir, tmpPlatformDir) ;

            //2. pack the zip
            string zipFile = tmpPlatformDir + ".zip";
            bool result = PackZip(tmpPlatformDir, zipFile);

            if (!result)
            {
                Console.Error.WriteLine("Failed to pack zip. Quitting");
                Directory.Delete(tmpDirectory);
                System.Environment.Exit(1);
            }

            //3. compute the md5 hash
            string md5hash = GetMD5HashOfFile(zipFile);

            //4. read the app settings
            var appSettings = GetAppSettings(tmpPlatformDir + "\\" + PLATFORM_CONFIG_FILE_NAME);

            if (appSettings.ContainsKey("md5hash"))
                appSettings["md5hash"] = md5hash;
            else
                appSettings.Add("md5hash", md5hash);

            string configFileToUpload = tmpDirectory + "\\" + PLATFORM_CONFIG_FILE_NAME;
            WriteAppSettings(appSettings, configFileToUpload);

            Console.Out.WriteLine("Your package is ready in {0}.\n  - Upload {1} and {2}.zip at the right place. \n  - Then, you can delete {3}", tmpDirectory, PLATFORM_CONFIG_FILE_NAME, PLATFORM_DIRECTORY_NAME, tmpDirectory);
        }

        private static void WriteAppSettings(Dictionary<string, string> settings, string fileToWrite) 
        {
            var fileWriter = new StreamWriter(fileToWrite);

            fileWriter.WriteLine("<configuration>");
            fileWriter.WriteLine("<appSettings>");
            foreach (string key in settings.Keys)
            {
                fileWriter.WriteLine("<add key=\"{0}\" value=\"{1}\"/>", key, settings[key]);
            }
            fileWriter.WriteLine("</appSettings>");
            fileWriter.WriteLine("</configuration>");

            fileWriter.Close();
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
                   "BinaryDir",
                   'b',
                   "..\\..\\..\\..\\output\\binaries\\Platform",
                   "directory name",
                   "The name of the directory where binaries are"),
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

        private static Dictionary<string, string> GetAppSettings(string fileUri)
        {
            Dictionary<string, string> retDict = new Dictionary<string, string>();

            XmlDocument xmlDoc = new XmlDocument();
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.IgnoreComments = true;
            xmlReaderSettings.DtdProcessing = DtdProcessing.Parse;

            XmlReader xmlReader = XmlReader.Create(fileUri, xmlReaderSettings);
            xmlDoc.Load(xmlReader);

            foreach (var child in xmlDoc.ChildNodes)
            {
                XmlElement root = child as XmlElement;

                if (root == null || !root.Name.Equals("configuration"))
                    continue;

                foreach (XmlElement xmlDevice in root.ChildNodes)
                {
                    if (!String.Equals(xmlDevice.Name, "appSettings", StringComparison.OrdinalIgnoreCase))
                        continue;

                    foreach (XmlElement xmlKey in xmlDevice.ChildNodes)
                    {
                        if (!String.Equals(xmlKey.Name, "add", StringComparison.OrdinalIgnoreCase))
                            continue;

                        string key = xmlKey.GetAttribute("key");
                        string value = xmlKey.GetAttribute("value");

                        //to make our life easiers, let us not store empty/null values
                        if (!String.IsNullOrEmpty(value))
                            retDict.Add(key, value);
                    }
                }
            }

            xmlReader.Close();

            return retDict;
        }


    }
}
