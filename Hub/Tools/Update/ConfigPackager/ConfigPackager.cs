using System;
using System.Collections.Generic;
using System.Linq;
using HomeOS.Hub.Common;
using System.IO;
using HomeOS.Hub.Tools.UpdateHelper;
using HomeOS.Hub.Tools.PackagerHelper;

namespace ConfigPackager
{
    class ConfigPackager
    {
        //commands supported
        private const string getactual = "getactual";
        private const string setdesired = "setdesired";

        static void Main(string[] args)
        {
            var argsDict = ProcessArguments(args);

            // if the function is missing
            if(string.IsNullOrEmpty((string)argsDict["Function"]))
                Utils.die(Utils.missingArgumentMessage("Function"));

            // if the key file is missing
            if (string.IsNullOrEmpty((string)argsDict["AccountKeyFile"]) && string.IsNullOrEmpty((string)argsDict["AccountKey"]))
                 Utils.die(Utils.missingArgumentMessage("Key")); 

            // if the function is neither of the two the tool supports
            if (!getactual.Equals((string)argsDict["Function"], StringComparison.CurrentCultureIgnoreCase) &&
                !setdesired.Equals((string)argsDict["Function"], StringComparison.CurrentCultureIgnoreCase) )
                Utils.die("unknown function provided. chose " + getactual + " or " +setdesired); 

            // if the ActualConfigDir is missing 
            if (string.IsNullOrEmpty((string)argsDict["ActualConfigDir"]))
                Utils.die(Utils.missingArgumentMessage("ActualConfigDir"));

            // if the function is desired. check further arguments
            if (setdesired.Equals((string)argsDict["Function"], StringComparison.CurrentCultureIgnoreCase) &&
                string.IsNullOrEmpty((string)argsDict["DesiredConfigDir"]) )
                Utils.die(Utils.missingArgumentMessage("DesiredConfigDir"));

            if(getactual.Equals((string)argsDict["Function"]))
            {
                Console.WriteLine("\nPerforming GetActualConfig with following arguments:");
                Utils.printArgumentsDictionary(argsDict);
                getActualConfig(argsDict);
            }

            if(setdesired.Equals((string)argsDict["Function"]))
            {
                Console.WriteLine("\nPerforming SetDesiredConfig with following arguments:");
                Utils.printArgumentsDictionary(argsDict);
                setDesiredConfig(argsDict);
            }
            
        }

        #region methods for function: setdesiredconfig
        
        private static void setDesiredConfig(ArgumentsDictionary argsDict)
        {
            string orgId = (string)argsDict["OrgID"];
            string studyId = (string)argsDict["StudyID"];
            string homeIDs = (string)argsDict["HomeIDs"];
            string container = (string)argsDict["Container"];
            string actualConfigDir = (string)argsDict["ActualConfigDir"];
            string desiredConfigDir = (string)argsDict["DesiredConfigDir"];
            string accountName = (string)argsDict["AccountName"];


            string accountKey = null;
            if (!String.IsNullOrEmpty((string)argsDict["AccountKey"]))
                accountKey = (string)argsDict["AccountKey"];
            else
                accountKey = PackagerHelper.ReadFile((string)argsDict["AccountKeyFile"]);

            if (string.IsNullOrEmpty(accountKey))
                Utils.die("Could not obtain AccountKey");


            if(!Directory.Exists(desiredConfigDir+"/"+orgId))
                Utils.die("ERROR! Directory "+desiredConfigDir+"/"+orgId+" corresponding to given orgID: "+orgId+" does not exist.");

            if (!Directory.Exists(desiredConfigDir + "/" + orgId + "/" + studyId))
                Utils.die("ERROR! Directory " + desiredConfigDir + "/" + orgId + "/" + studyId + " corresponding to given studyID: " + studyId + " does not exist.");

            string[] homeID = null;
            if (!homeIDs.Equals("*"))
            {
                homeID = homeIDs.Split(',');
            }
            else
            {
                Console.WriteLine("\nFetching list of homeIDs from "+desiredConfigDir);
                homeID = PackagerHelper.ListDirectories(desiredConfigDir + "/" + orgId + "/" + studyId+"/").ToArray();
            }

            Console.WriteLine("HomeID list: ");
            for (int i = 0; i < homeID.Count(); i++)
            {
                homeID[i] = homeID[i].TrimEnd('/');
                Console.WriteLine("{0}", homeID[i]);
            }

            foreach (string h in homeID)
            {
                Console.WriteLine("\nSetting desired config for homeID:" + h);
                writeDesiredConfigToAzure(accountName, accountKey, container, orgId, studyId, h, actualConfigDir, desiredConfigDir);
            }


        }

        private static void writeDesiredConfigToAzure(string accountName, string accountKey, string container, string orgId, string studyId, string homeID, string actualConfigDir, string desiredConfigDir)
        {
            string zipPath_desired = desiredConfigDir + "/" + orgId + "/" + studyId + "/" + homeID + "/";
            string zipPath_actual = actualConfigDir + "/" + orgId + "/" + studyId + "/" + homeID + "/";

            if (!Directory.Exists(zipPath_desired))
            {
                Console.WriteLine("ERROR! desired config  for homeID: "+homeID +" "+zipPath_desired+ " does not exist" );
                return;
            }

            if (!Directory.Exists(zipPath_actual))
            {
                Console.WriteLine("ERROR! current/actual config for homeID: " + homeID + " " + zipPath_actual + " does not exist");
                return;
            }

            // copy current version from actual onto that of desired

            if (File.Exists(zipPath_actual + ConfigPackagerHelper.CurrentVersionFileName))
            {
                Console.WriteLine("Copying {0} to {1} ", zipPath_actual + ConfigPackagerHelper.CurrentVersionFileName, zipPath_desired + ConfigPackagerHelper.ParentVersionFileName);
                PackagerHelper.CopyFile(zipPath_actual + ConfigPackagerHelper.CurrentVersionFileName, zipPath_desired + ConfigPackagerHelper.ParentVersionFileName);
            }
            else
            {
                Console.WriteLine("Writing version of config in {0} to {1} ", zipPath_actual, zipPath_desired + ConfigPackagerHelper.ParentVersionFileName);
                ConfigPackagerHelper.UpdateVersionFile(ConfigPackagerHelper.GetConfigVersion(zipPath_actual), zipPath_desired + ConfigPackagerHelper.ParentVersionFileName);
            }

            Console.WriteLine("Writing version of config in {0} to {1} ", zipPath_desired, zipPath_desired + ConfigPackagerHelper.CurrentVersionFileName);
            Dictionary<string, string> currentVersion_desired = ConfigPackagerHelper.GetConfigVersion(zipPath_desired);
            ConfigPackagerHelper.UpdateVersionFile(currentVersion_desired, zipPath_desired + ConfigPackagerHelper.CurrentVersionFileName);

            File.Delete(desiredConfigDir + "/" + ConfigPackagerHelper.desiredConfigFileName);
            PackagerHelper.PackZip(zipPath_desired, desiredConfigDir + "/" + ConfigPackagerHelper.desiredConfigFileName);
            PackagerHelper.MoveFile(desiredConfigDir + "/" + ConfigPackagerHelper.desiredConfigFileName, zipPath_desired + ConfigPackagerHelper.desiredConfigFileName);

            Console.WriteLine("Uploading desired config for homeID {0} ", homeID);
            if (!AzureBlobConfigUpdate.UploadConfig(zipPath_desired + ConfigPackagerHelper.desiredConfigFileName, accountName, accountKey, orgId, studyId, homeID, container, ConfigPackagerHelper.desiredConfigFileName))
            {
                Console.WriteLine("WARNING! unable to upload config for homeID: " + homeID);
                
            }
            PackagerHelper.DeleteFile(zipPath_desired + ConfigPackagerHelper.desiredConfigFileName);
        }

        #endregion


        #region methods for function: getactualconfig

        private static void getActualConfig(ArgumentsDictionary argsDict)
        {
            string orgId = (string)argsDict["OrgID"];
            string studyId = (string)argsDict["StudyID"];
            string homeIDs = (string)argsDict["HomeIDs"];
            string container = (string)argsDict["Container"];
            string actualConfigDir = (string)argsDict["ActualConfigDir"];
            string accountName = (string)argsDict["AccountName"];


            string accountKey= null ;
            if (!String.IsNullOrEmpty((string)argsDict["AccountKey"]))
                accountKey = (string)argsDict["AccountKey"]; 
            else 
                accountKey = PackagerHelper.ReadFile((string)argsDict["AccountKeyFile"]);

            if (string.IsNullOrEmpty(accountKey))
                Utils.die("Could not obtain AccountKey");

            string[] homeID=null;
            if (!homeIDs.Equals("*"))
            {
                homeID = homeIDs.Split(',');
            }
            else
            {

                Console.WriteLine("\nFetching list of homeIDs from container: "+container);
                Tuple<bool, List<string>> fetchHubList = AzureBlobConfigUpdate.listHubs(accountName, accountKey, container, orgId, studyId );
                if (fetchHubList.Item1)
                    homeID = fetchHubList.Item2.ToArray();
                else
                    Utils.die("ERROR! Cannot obtain hubs list! Exception : "+ fetchHubList.Item2[0]);
            }

            Console.WriteLine("HomeID list: ");
            for (int i=0; i < homeID.Count() ; i++)
            {
                homeID[i] = homeID[i].TrimEnd('/'); 
                Console.WriteLine("{0}", homeID[i]);
            }

            foreach (string h in homeID)
            {
                Console.WriteLine("\nFetching config for homeID:"+h );
                writeActualConfigToDisk(accountName, accountKey, container, orgId, studyId, h, actualConfigDir);
            }
        }

        private static void writeActualConfigToDisk(string accountName, string accountKey, string container, string orgId, string studyId, string homeID, string actualConfigDir)
        {

            string zipPath = actualConfigDir + "/" + orgId + "/" + studyId + "/" + homeID +"/";
            
            Console.WriteLine("Creating directory: "+zipPath);
            PackagerHelper.CreateDirectory(zipPath);

            Console.WriteLine("Downloading config blob...");
            if (!AzureBlobConfigUpdate.DownloadConfig(zipPath + ConfigPackagerHelper.actualConfigFileName, accountName, accountKey, orgId, studyId, homeID, container, ConfigPackagerHelper.actualConfigFileName))
            {
                Console.WriteLine("WARNING! unable to download config for homeID: " + homeID);
                return;
            }

            Console.WriteLine("Unpacking config blob... ");
            if (!PackagerHelper.UnpackZip(zipPath + ConfigPackagerHelper.actualConfigFileName, zipPath))
                Console.WriteLine("WARNING! unable to unpack zip for homeID: " + homeID);
            else
            {
                Console.WriteLine("Current/actual config for homeID: "+homeID +"  is now in {0}", zipPath );
                PackagerHelper.DeleteFile(zipPath + ConfigPackagerHelper.actualConfigFileName);
            }
        }

        #endregion 
              

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
                   "AccountKey",
                   'k',
                   "",
                   "Azure account key",
                   "Azure account key"),

                    new ArgumentSpec(
                   "AccountKeyFile",
                   '\0',
                   "",
                   "file containing Azure account key",
                   "file containing Azure account key"),

                   new ArgumentSpec(
                   "AccountName",
                   'n',
                   "homelab",
                   "Azure account name",
                   "Azure account name"),

                   new ArgumentSpec(
                   "Container",
                   'c',
                   "configs",
                   "Config storage container name",
                   "Config storage container name"),

                   new ArgumentSpec(
                   "OrgID",
                   'o',
                   "Default",
                   "Organization ID or OrgID",
                   "Organization ID or OrgID"),

                   new ArgumentSpec(
                   "StudyID",
                   's',
                   "Default",
                   "StudyID",
                   "StudyID"),

                   new ArgumentSpec(
                   "HomeIDs",
                   'h',
                   "*",
                   "HomeIDs",
                   "single HomeID or comma-separated multiple HomeIDs"),

                   new ArgumentSpec(
                   "ActualConfigDir",
                   'a',
                   "",
                   "Actual Configs Directory",
                   "Directory for reading or writing actual/current Hub configs"),

                   new ArgumentSpec(
                   "DesiredConfigDir",
                   'd',
                   "",
                   "Desired Configs Directory",
                   "Directory for reading desired Hub configs"),

                    new ArgumentSpec(
                   "Function",
                   'f',
                   "",
                   "function to get performed",
                   "function to get performed"),

            };

            ArgumentsDictionary args = new ArgumentsDictionary(arguments, argSpecs);
            if (args.AppSettingsParseError)
            {
                Console.Error.WriteLine("Error in .config file options: ignoring");
            }

            if (args.CommandLineParseError)
            {
                Console.Error.WriteLine("Error in command line arguments at {0}\n", args.ParseErrorArgument);
                Console.Error.WriteLine(args.GetUsage("ConfigPackager"));
                System.Environment.Exit(1);
            }

            if ((bool)args["Help"])
            {
                Console.Error.WriteLine("Downloads current Hub configs, and uploads desired configs for deployment on hubs\n");
                Console.Error.WriteLine(args.GetUsage("ConfigPackager"));
                System.Environment.Exit(0);
            }

            return args;

        }
    }
}
