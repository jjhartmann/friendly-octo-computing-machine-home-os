using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
using System.ServiceProcess;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using System.Net;
using System.Linq;

namespace HomeOS.Hub.Watchdog
{
    public partial class WatchdogService : ServiceBase
    {
        public const bool SendHeartBeats = false;
        public const bool CheckForUpdates = true;
        public const bool CheckForProcessLiveness = true;


        public const bool EnforceChecksumMatch = false;
        public const bool CheckEmbeddedVersionMatch = true;

#if DEBUG
        public const int SLEEP_TIME_SECONDS = 1;
        public const double RESTART_TIME_SECONDS = (10.0);
        public const double UPDATE_CHECK_SECONDS = (60.0);
#else
        public const int SLEEP_TIME_SECONDS = 30;
        public const double RESTART_TIME_SECONDS = (15.0);
        public const double UPDATE_CHECK_SECONDS = (60.0);
#endif
        // Stores Array of programs to monitor
        public class ProgramDetails
        {
            //the following fields are read from Watchdog.txt
            public string ProcessName;
            public string ExeDir;       //stored as absolute path, but should be relative watchdog.exe in watchdog.txt
            public string ExeName;     
            public int nSecondsRunDelay;
            public string UpdateUri;
            public string Args;

            //the following fields are dynamically maintained state
            public bool fRunningAtLastCheck;
            public DateTime dtLastRun;
            public DateTime lastUpdateCheck;
        };

        public class MessageQueue
        {
            Queue<string> queue;
            int capacity;
            int messageNumber = 0;

            public MessageQueue(int capacity = 20)
            {
                queue = new Queue<string>(capacity);
                this.capacity = capacity;
            }

            public string AddMessage(string text)
            {
                while (queue.Count >= capacity)
                    queue.Dequeue();

                queue.Enqueue(++messageNumber + ": " + text + "\n");

                return this.ToString();
            }

            public override string ToString()
            {
                string ret = "";

                for (int index = 0; index < queue.Count; index++)
                {
                    ret += queue.ElementAt(index);
                }

                return ret;
            }
        }

        private List<ProgramDetails> aPrograms = new List<ProgramDetails>();
        private string inputDir;
        private System.Timers.Timer watchDogTimer;
        private MessageQueue errorMessages;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            WatchdogService service = new WatchdogService();

            if (Environment.UserInteractive)
            {
                service.OnStart(args);
                Application.Run();
            }
            else
            {
                ServiceBase.Run(service);
            }
        }

        public WatchdogService()
        {
            InitializeComponent();
        }
 
        protected override void OnStart(string[] args)
        {
            StartWatching();
        }
 
        protected override void OnStop()
        {
            watchDogTimer.Stop();
        }

        public void StartWatching()
        {
            // Make sure only one copy of Watchdog is running...
            Process p = Process.GetCurrentProcess();

            Process[] processes = Process.GetProcessesByName(p.ProcessName);

            if (processes.Length > 1)
            {
                foreach (Process rp in processes)
                {
                    if (rp.Id != p.Id && rp.SessionId == p.SessionId)
                    {
#if DEBUG
                        Console.WriteLine("Watchdog Monitor is already running " + rp.SessionId.ToString() + " is equal to " + p.SessionId.ToString());
#endif
                        return;
                    }
                }
            }

            // Find input file
            inputDir = p.MainModule.FileName;
            inputDir = inputDir.Remove(inputDir.LastIndexOf('\\'));
            Directory.SetCurrentDirectory(inputDir);
            string inputFile = inputDir + "\\Watchdog.txt";

            errorMessages = new MessageQueue();

            ReadInputFile(inputFile);

#if DEBUG
            Win32Imports.AllocConsole();
#endif
            
            watchDogTimer = new System.Timers.Timer();

            //we do not autoreset because we want the count to restart after each time watchdogmonitor (which can take a lot of time) finishes 
            watchDogTimer.AutoReset = false;

            watchDogTimer.Interval = (SLEEP_TIME_SECONDS * 1000);
            watchDogTimer.Elapsed += new ElapsedEventHandler(WatchdogMonitorTick);
            watchDogTimer.Start();

            //Application.Run();

        }

        private void ReadInputFile(string inputFile)
        {
            using (StreamReader sr = new StreamReader(inputFile))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    //ignore lines starting with a hash (comments) and empty lines
                    if (line.StartsWith("#") || string.IsNullOrWhiteSpace(line))
                        continue;

                    try
                    {
                        ProgramDetails pd = new ProgramDetails();
                        string[] data = line.Split(';');
                        if (data.Length > 1)
                        {
                            pd.ProcessName = data[0];
                            pd.ExeDir = inputDir + "\\" + data[1];
                            pd.ExeName = data[2];
                            pd.nSecondsRunDelay = Convert.ToInt32(data[3]);
                            pd.UpdateUri = data[4];
                            // add in args if any
                            if (data.Length > 5)
                                pd.Args = data[5];

                            pd.fRunningAtLastCheck = false;
                            pd.dtLastRun = DateTime.Now.AddHours(-1.0);
                            pd.lastUpdateCheck = DateTime.Now.AddHours(-1.0);

                            aPrograms.Add(pd);
                        }
                    }
                    catch (Exception e)
                    {
                        LogInfo(e.ToString(), true);
                    }
                }
            }
        }

        private void LogInfo(string message, bool isError=false)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
            if (isError)
                errorMessages.AddMessage(message);
        }

        public void WatchdogMonitorTick(object sender, EventArgs Args)
        {
            try
            {
                //1. deposit heart beat information
                if (SendHeartBeats)
                {
                    // TODO
                }

                //2. check if there is a new version of the platform
                if (CheckForUpdates)
                {
                    CheckProcessUpdatedness();
                }

                //3. check if the process is running
                if (CheckForProcessLiveness)
                {
                    CheckProcessLiveness();
                }
            }
            catch (Exception e)
            {
                LogInfo(e.ToString(), true);
            }

            //let's start count again
            watchDogTimer.Start();
        }

        #region code relater to updating platform
        private  void CheckProcessUpdatedness()
        {
            foreach (ProgramDetails pd in aPrograms)
            {
                //did enough time elapse since the last update
                if (pd.lastUpdateCheck.AddSeconds(UPDATE_CHECK_SECONDS) > DateTime.Now)
                    continue;

                string configFileUrl = pd.UpdateUri + "/" + pd.ExeName + ".config";
                LogInfo("Checking for updates at URI: " + configFileUrl);

                string tmpZipFile = null;
                string tmpBinFolder = null;

                try
                {
                    var desiredSettings = GetAppSettings(configFileUrl);

                    if (!desiredSettings.ContainsKey("version") ||
                        !desiredSettings.ContainsKey("md5hash"))
                        throw new Exception("Missing key in " + pd.UpdateUri);

                    var currentSettings = GetAppSettings(pd.ExeDir + "\\" + pd.ExeName + ".config");

                    var currentVersion = currentSettings.ContainsKey("version") ? new Version(currentSettings["version"]) : new Version("0.0.0.0");

                    LogInfo(String.Format("current version = {0}, desired version = {1}", currentVersion, desiredSettings["version"]));

                    //do we need an update
                    if (new Version(desiredSettings["version"]).CompareTo(currentVersion) > 0)
                    {
                        //1. download the new zip
                        tmpZipFile = String.Format("{0}\\{1}.{2}.zip", inputDir, pd.ExeName, desiredSettings["version"]);
                        string zipFileUrl = pd.UpdateUri + "/" + new DirectoryInfo(pd.ExeDir).Name + ".zip";
                        GetZip(zipFileUrl, desiredSettings["md5hash"], tmpZipFile);

                        //2. unpack the zip
                        tmpBinFolder = String.Format("{0}\\{1}.{2}", inputDir, pd.ExeName, desiredSettings["version"]);
                        CreateFolder(tmpBinFolder);
                        System.IO.Compression.ZipFile.ExtractToDirectory(tmpZipFile, tmpBinFolder);

                        //3. check what we got back after unzipping. 
                        //3a. we should have exactly one top-level folder and zero files
                        string[] files = Directory.GetFiles(tmpBinFolder);
                        string[] folders = Directory.GetDirectories(tmpBinFolder);
                        if (files.Length != 0 || folders.Length != 1)
                            //this exception will get logged as part of catch handling
                            throw new Exception(String.Format("Unexpected outcome from unzipping. #files = {0} #folders = {1}", files.Length, folders.Length));

                        //4. things seem in order lets kill the process and then copy files
                        KillProcess(pd);

                        //5. copy over the folder
                        string curBinFolder = pd.ExeDir;
                        CopyFolder(folders[0], curBinFolder);

                        //3b. now check if we got the right version
                        if (CheckEmbeddedVersionMatch)
                        {
                            var inZipSettings = GetAppSettings(pd.ExeDir + "\\" + pd.ExeName + ".config");

                            //var inZipSettings = GetAppSettings(folders[0] + "\\" + pd.ExeName + ".config");

                            if (!inZipSettings.ContainsKey("version") || !desiredSettings["version"].Equals(inZipSettings["version"]))
                                LogInfo(String.Format("Version embedded in zip does not match desired version. Desired = {0} Embedded = {1}.",
                                      desiredSettings["version"], inZipSettings["version"]), true);

                                //throw new Exception(String.Format("Version embedded in zip does not match desired version. Desired = {0} Embedded = {1}.",
                                //    desiredSettings["version"], inZipSettings["version"]));
                        }

                    }

                }
                catch (Exception e)
                {
                    LogInfo(e.ToString(), true);
                }
                finally
                {
                    if (tmpZipFile != null)
                        File.Delete(tmpZipFile);

                    if (tmpBinFolder != null)
                        Directory.Delete(tmpBinFolder, true);
                }

                pd.lastUpdateCheck = DateTime.Now;
            }
        }

        public void CopyFolder(string sourceFolder, string destFolder)
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
                           LogInfo("Failed to copy " + file + "\n" + e.ToString(), true);
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
                    LogInfo(e.ToString(), true);
                }
            }
        }

        private Dictionary<string, string> GetAppSettings(string fileUri)
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


        private void KillProcess(ProgramDetails pd)
        {
            Process ThisProcess = Process.GetCurrentProcess();
            Process[] processes = System.Diagnostics.Process.GetProcesses();

            // Is it running right now?
            Process process = GetProcessIfRunning(ThisProcess.SessionId, pd, processes);

            if (process != null)
            {
                //todo: need a way to gracefully shut down the process

                //bool result = process.CloseMainWindow();

                //LogInfo(String.Format("Result of CloseMainWindow on {0} is {1}", pd.ProcessName, result));

                process.Kill();

                process.Close();
            }
            else
            {
                LogInfo(String.Format("Tried to kill {0} but it is not running", pd.ProcessName));
            }
        }


        private void CreateFolder(String folder)
        {
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);

            Directory.CreateDirectory(folder);

            //DirectoryInfo info = new DirectoryInfo(completePath);
            //System.Security.AccessControl.DirectorySecurity security = info.GetAccessControl();
            //security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(Environment.UserName, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.InheritanceFlags.ContainerInherit, System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow));
            //security.AddAccessRule(new System.Security.AccessControl.FileSystemAccessRule(Environment.UserName, System.Security.AccessControl.FileSystemRights.FullControl, System.Security.AccessControl.InheritanceFlags.ContainerInherit, System.Security.AccessControl.PropagationFlags.None, System.Security.AccessControl.AccessControlType.Allow));
            //info.SetAccessControl(security);
            //return completePath;

            //return null;
        }

        private void DeleteFolder(string folder)
        {
            Exception exception = null;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Directory.Delete(folder);

                    return;
                }
                catch (Exception e)
                {
                    exception = e;
                }

                //lets wait 5 seconds and then we try to delete again
                System.Threading.Thread.Sleep(5 * 1000);
            }

            throw exception;
        }

        private string GetZip(String url, string checksumvalue, String zipFile)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFile(url, zipFile);
            webClient.Dispose();

            string localCheckSum = GetMD5HashFromFile(zipFile);

            if (localCheckSum.Equals(checksumvalue))
                return zipFile;

            //...if we come here, checksums do not match
            if (EnforceChecksumMatch)
            {
                File.Delete(zipFile);
                throw new Exception(String.Format("Checksums do not match. Expected {0}. Got {1}", checksumvalue, localCheckSum));
            }
            else
            {
                //we are not forcing the checksums to match; log this problems and move on.
                LogInfo("Checksums did not match", true);
                return zipFile;
            }

        }

        private string GetMD5HashFromFile(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }


        #endregion

        private void CheckProcessLiveness()
        {
            Process ThisProcess = Process.GetCurrentProcess();
            Process[] processes = System.Diagnostics.Process.GetProcesses();
            foreach (ProgramDetails pd in aPrograms)
            {
                // Should this be running?
                if (pd.dtLastRun.AddSeconds(pd.nSecondsRunDelay) > DateTime.Now)
                    continue;

                LogInfo("Checking liveness for ..." + pd.ProcessName);

                // Is it running right now?
                Process process = GetProcessIfRunning(ThisProcess.SessionId, pd, processes);

                if (process != null)
                {
                    pd.fRunningAtLastCheck = true;
                }
                else
                {
                    DateTime curTime = DateTime.Now;

                    // Hey look, it isn't running...
                    if (pd.fRunningAtLastCheck)
                    {
                        
                        LogInfo(String.Format("{0} appears to have died...", pd.ProcessName));

                        pd.fRunningAtLastCheck = false;
                        pd.dtLastRun = curTime;
                    }

                    // It is _still_ not running...
                    else
                    {
                        LogInfo(String.Format("{0} still not running...", pd.ProcessName));

                        pd.fRunningAtLastCheck = false;
                        TimeSpan ts = curTime - pd.dtLastRun;
                        if (ts.TotalSeconds > RESTART_TIME_SECONDS)
                        {
                            LogInfo(String.Format("Re-starting {0}...", pd.ProcessName));

                            try
                            {
                                ProcessStartInfo startInfo = new ProcessStartInfo(pd.ExeDir + '\\' + pd.ExeName, pd.Args);
                                startInfo.WorkingDirectory = pd.ExeDir;
                                Process.Start(startInfo);
                            }
                            catch (Exception e)
                            {
                                LogInfo(e.ToString(), true);
                            }

                            pd.fRunningAtLastCheck = true;
                        }
                    }
                }
            }
        }

        private Process GetProcessIfRunning(int SessionId, ProgramDetails pd, Process[] processes)
        {
            for (int i = 0; i < processes.Length; i++)
            {
                if (processes[i].SessionId != SessionId)
                    continue;

                String name = processes[i].ProcessName;
                // Console.WriteLine("Found a process: {0}", name);

                if (name.ToLower().Equals(pd.ProcessName.ToLower()))
                {
                    LogInfo(String.Format("Hooray!  {0} is running...", pd.ProcessName));

                    return processes[i];
                }
            }
            return null;
        }

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        //#region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            this.ServiceName = "HomeOS Hub Watchdog";
        }

        //#endregion

    }
}