using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using HomeOS.Hub.Platform.Views;
using System.Management;
using System.IO;
using System.Security.Cryptography;

namespace HomeOS.Hub.Common
{
    /// <summary>
    /// This class contains a bunch of static utilities that both platform and modules can access
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// cached hardware id because the actual call has some overhead
        /// </summary>
        private static string hwId = null;

        /// NB: If you change how hardware id is computed, make sure that you change it in Watchdog as well

        /// <summary>
        /// Returns a unique hardware id for a computer, by combining the cpu and hdd ids.        
        /// </summary>
        /// <returns></returns>
        public static string HardwareId
        {
            get
            {
                if (hwId == null)
                    hwId = String.Format("cpu:{0} hdd:{1}", FirstCpuId(), CVolumeSerial());

                return hwId;
            }
        }

        /// <summary>
        /// Returns a valid mac address for the fastest interface
        /// </summary>
        /// <param name="logger"></param>
        /// <returns></returns>
        private static string MacAddress()
        {
            const int MIN_MAC_ADDR_LENGTH = 12;
            string macAddress = string.Empty;
            long maxSpeed = -1;

            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                string tempMac = nic.GetPhysicalAddress().ToString();
                if (nic.Speed > maxSpeed &&
                    !string.IsNullOrEmpty(tempMac) &&
                    tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                {
                    maxSpeed = nic.Speed;
                    macAddress = tempMac;
                }
            }

            return macAddress;
        }

        /// <summary>
        /// Returns the HDD volume of c:
        /// </summary>
        /// <returns></returns>
        private static string CVolumeSerial()
        {
            var disk = new ManagementObject(@"win32_logicaldisk.deviceid=""c:""");
            disk.Get();

            string volumeSerial = disk["VolumeSerialNumber"].ToString();
            disk.Dispose();

            return volumeSerial;
        }

        /// <summary>
        /// Returns the id of the first CPU listed by WMI
        /// </summary>
        /// <returns></returns>
        private static string FirstCpuId()
        {
            var mClass = new ManagementClass("win32_processor");

            foreach (var obj in mClass.GetInstances())
            {
                return obj.Properties["processorID"].Value.ToString();
            }
            return "";
        }


        public static void RunCommandTillEnd(string filename, string arguments, List<string> stdout, List<string> stderr, Logger logger)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();

            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = arguments;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.OutputDataReceived += (sender, eventArgs) => CommandOutputReceived(sender, eventArgs, stdout);
            process.ErrorDataReceived += (sender, eventArgs) => CommandOutputReceived(sender, eventArgs, stderr);

            logger.Log("Abbout to start: {0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception e)
            {
                logger.Log("Got an exception while starting the process");
                logger.Log(e.ToString());

                return;
            }

            process.WaitForExit();
        }

        private static void CommandOutputReceived(object sender, System.Diagnostics.DataReceivedEventArgs e, List<string> output)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                output.Add(e.Data);
            }
        }

        /// <summary>
        /// Determines whether or not the given prospective HomeId is valid.
        /// </summary>
        /// <param name="input">The prospective HomeId.</param>
        /// <returns>
        /// True if the prospective HomeId is valid, false otherwise.
        /// </returns>
        public static bool IsValidHomeId(string homeId)
        {
            return IsValidHomeId(Encoding.ASCII.GetBytes(homeId));
        }

        /// <summary>
        /// Determines whether or not the given prospective HomeId is valid.
        /// </summary>
        /// <param name="input">The prospective HomeId.</param>
        /// <returns>
        /// True if the prospective HomeId is valid, false otherwise.
        /// </returns>
        private static bool IsValidHomeId(byte[] input)
        {
            ArraySegment<byte> test = new ArraySegment<byte>(input);
            return IsValidHomeId(test);
        }

        /// <summary>
        /// Determines whether or not a prospective HomeId is valid.
        /// </summary>
        /// <remarks>
        /// We only allow ASCII alpha-numeric characters in HomeIds.
        /// </remarks>
        /// <param name="input">
        /// A byte array segment containing the prospective HomeId.
        /// </param>
        /// <returns>
        /// True if the prospecive HomeId is valid, false otherwise.
        /// </returns>
        private static bool IsValidHomeId(ArraySegment<byte> input)
        {
            for (int index = input.Offset;
                index < input.Offset + input.Count;
                index++)
            {
                byte test = input.Array[index];
                if ((test < 48) ||
                    (test > 122) ||
                    ((test > 57) && (test < 65)) ||
                    ((test > 90) && (test < 97)))
                {
                    return false;
                }
            }

            return true;
        }

        #region file and directory operations 
        public static void CleanDirectory(VLogger logger, string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                    return;

                DirectoryInfo dir = new DirectoryInfo(directory);
                foreach (FileInfo fi in dir.GetFiles())
                {
                    fi.Delete();
                }

                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    Utils.CleanDirectory(logger,di.FullName);
                    di.Delete();
                }
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger,"E", e.Message + ". CleanDirectory, directory:" + directory);
            }
        }

        public static void DeleteDirectory(VLogger logger, string directory)
        {
            try
            {
                if (Directory.Exists(directory))
                {
                    Utils.CleanDirectory(logger,directory);
                    Directory.Delete(directory, true);
                }
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger,"E", e.Message + ". DeleteDirectory, directory :" + directory);
            }

        }

        public static string CreateDirectory(VLogger logger, String completePath)
        {
            try
            {
                if (Directory.Exists(completePath))
                {
                    Utils.CleanDirectory(logger, completePath);
                    Directory.Delete(completePath, true);
                }

                Directory.CreateDirectory(completePath);
                DirectoryInfo info = new DirectoryInfo(completePath);
                System.Security.AccessControl.DirectorySecurity security = info.GetAccessControl();
                security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(Environment.UserName, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.InheritanceFlags.ContainerInherit, System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow));
                security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(Environment.UserName, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.InheritanceFlags.ContainerInherit, System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow));
                info.SetAccessControl(security);
                return completePath;
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger,"E", e.Message + ". CreateDirectory, completePath:" + completePath);
            }

            return null;
        }

        public static void DeleteFile(VLogger logger, string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger,"E", e.Message + ". DeleteFile, filePath:" + filePath);
            }
        }

        public static List<string> ListFiles(VLogger logger, string directory)
        {
            List<string> retVal = new List<string>();
            try
            {
                foreach (string f in Directory.GetFiles(directory))
                    retVal.Add(Path.GetFileName(f));
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger,"E", e.Message + ". ListFiles, directory: " + directory);
            }
            return retVal;
        }

        public static void CopyDirectory(VLogger logger, string sourceDir, string destDir)
        {
            try
            {
                if (!Directory.Exists(destDir))
                    Directory.CreateDirectory(destDir);
                string[] files = Directory.GetFiles(sourceDir);
                foreach (string file in files)
                {
                    string name = Path.GetFileName(file);
                    string dest = Path.Combine(destDir, name);
                    File.Copy(file, dest, true);
                }
                string[] folders = Directory.GetDirectories(sourceDir);
                foreach (string folder in folders)
                {
                    string name = Path.GetFileName(folder);
                    string dest = Path.Combine(destDir, name);
                    Utils.CopyDirectory(logger, folder, dest);
                }
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger,"E", e.Message + ". CopyDirectory, sourceDir: " + sourceDir + ", destDir:" + destDir);
            }
        }

        public static bool UnpackZip(VLogger logger, String zipPath, string extractPath)
        {
            try
            {
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);
                return true;
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger,"E", e.Message + ". UnpackZip, zipPath: " + zipPath + ", extractPath:" + extractPath);
                return false;
            }

        }

        public static bool PackZip(VLogger logger, string startPath, String zipPath)
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
                Utils.structuredLog(logger,"E", e.Message + ". PackZip, startPath: " + startPath + ", zipPath:" + zipPath);
                return false;
            }

        }

        public static string GetMD5HashOfFile(VLogger logger, string filePath)
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
                Utils.structuredLog(logger,"E", e.Message + ". GetMD5HashOfFile(), file" + filePath);
                return "";
            }
        }

        public static string ReadFile(VLogger logger, string filePath)
        {
            try
            {

                System.IO.StreamReader myFile = new System.IO.StreamReader(filePath);
                string myString = myFile.ReadToEnd();
                myFile.Close();
                return myString;
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger, "E", e.Message + ". GetMD5HashOfFile(), file" + filePath);
                return "";
            }
        }

        public static string WriteToFile(VLogger logger, string filePath, string text)
        {
            try
            {
                System.IO.StreamReader myFile = new System.IO.StreamReader(filePath);
                string myString = myFile.ReadToEnd();
                myFile.Close();
                return myString;
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger, "E", e.Message + ". WriteToFile "+filePath+" "+ text);
                return "";
            }

        }

        public static bool CopyFile(VLogger logger, string filePath1, string filePath2)
        {
            if (File.Exists(filePath2))
                Utils.DeleteFile(logger, filePath2);
            try
            {
                File.Copy(filePath1, filePath2);
                return true;
            }
            catch (Exception e)
            {
                Utils.structuredLog(logger, "E", e.Message + ". CopyFile(), file" + filePath1 + " to "+filePath2);
                return false; ;
            }
        }
        #endregion


        public static void structuredLog(VLogger logger, string type, params string[] messages)
        {
            if (type == "ER") type = "ERROR";
            else if (type == "I") type = "INFO";
            else if (type == "E") type = "EXCEPTION";
            else if (type == "W") type = "WARNING";

            StringBuilder s = new StringBuilder();
            s.Append("[ConfigUpdater]" + "[" + type + "]");
            foreach (string message in messages)
                s.Append("[" + message + "]");
            logger.Log(s.ToString());
        }
    }
}
