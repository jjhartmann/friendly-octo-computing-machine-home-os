using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Linq;
using System.Xml.Linq;
using HomeOS.Hub.Common;

namespace HomeOS.Hub.Tools.PackagerHelper
{
    /// <summary>
    /// Shared Helper code used by the different packagers: ConfigPackager, ModulePackager, PlatformPackager and
    /// ScoutPackager, and the UpdateManager
    /// </summary>
    public class BinaryPackagerHelper
    {
        #region constants
        public const string DefaultHomeOSUpdateVersionValue = "0.0.0.0";
        public const string ConfigAppSettingKeyHomeOSUpdateVersion = "HomeOSUpdateVersion";
        #endregion

        #region main package function
        public static void Package(string binRootDir, string binName, bool singleBin, string binType, string packType, string repoDir)
        {
            //get the binary directory
            string binDir = singleBin ? binRootDir : binRootDir + "\\" + binName;

            if (!Directory.Exists(binDir))
            {
                Console.Error.WriteLine("{0} directory {1} does not exist. Is there a mismatch in {0} name and its location?", packType, binDir);
                return;
            }

            //get the zip dir
            string zipDir = repoDir;

            string[] parts = binName.Split('.');

            foreach (var part in parts)
                zipDir += "\\" + part;

            // Use HomeOSUpdateVersion from App.Config

            string file = binDir + "\\" + binName + "." + binType + ".config";
            string homeosUpdateVersion = GetHomeOSUpdateVersion(file);
            if (homeosUpdateVersion == DefaultHomeOSUpdateVersionValue)
            {
                Console.WriteLine("Warning didn't find {0} version in {1}, defaulting to {2}", packType, file, homeosUpdateVersion);
            }

            zipDir += "\\" + homeosUpdateVersion;
            Directory.CreateDirectory(zipDir);

            //get the name of the zip file and pack it
            string zipFile = zipDir + "\\" + binName + ".zip";
            string hashFile = zipDir + "\\" + binName + ".md5";

            bool result = PackagerHelper.PackZip(binDir, zipFile);

            if (!result)
            {
                Console.Error.WriteLine("Failed to pack zip for {0}. Quitting", binName);
            }

            string md5hash = PackagerHelper.GetMD5HashOfFile(zipFile);

            if (string.IsNullOrWhiteSpace(md5hash))
            {
                return;
            }

            try
            {
                File.WriteAllText(hashFile, md5hash);
            }
            catch (Exception)
            {
                Console.Out.WriteLine("Failed to write hash file {0}. Quitting", hashFile);
                return;
            }

            Console.Out.WriteLine("Prepared {0} package: {1}.\n", packType, zipFile);
        }
        #endregion

        #region version related
        public static string GetHomeOSUpdateVersion(string configFile)
        {
            string homeosUpdateVersion = DefaultHomeOSUpdateVersionValue;
            try
            {
                XElement xmlTree = XElement.Load(configFile);
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
                Console.Error.WriteLine("Failed to parse {0}, exception: {1}", configFile, e.ToString());
            }
            return homeosUpdateVersion;
        }
        #endregion

    }
}
