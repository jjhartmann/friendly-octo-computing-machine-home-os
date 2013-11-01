using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HomeOS.Hub.Common;
using HomeOS.Hub.Platform.Views;

namespace HomeOS.Hub.Platform.DeviceScout
{
    public class ScoutInfo
    {
        public string Name { get; private set; }
        public string DllName { get; private set; }
        public string Version { get; private set; }


        public ScoutInfo(string name, string dllName)
        {
            Name = name;
            DllName = dllName;
            Version = null;
        }

        public ScoutInfo(HomeStoreScout hsScout) : this(hsScout.Name, hsScout.DllName)
        {
            if (!string.IsNullOrEmpty(hsScout.Version))
                SetVersion(hsScout.Version);
        }

        public void SetVersion(string Version)
        {
            this.Version = Version;
        }
    }

    public interface ScoutViewOfPlatform : SafeServicePolicyDecider
    {
        void ProcessNewDiscoveryResults(List<Device> deviceList);
        void SetDeviceDriverParams(Device device, List<string> paramList);
        List<string> GetDeviceDriverParams(Device device);
        string GetConfSetting(string paramName);
        string GetPrivateConfSetting(string paramName);
    }

    public interface IScout
    {
        void Init(string baseUrl, string baseDir, ScoutViewOfPlatform platform, VLogger logger);
        List<HomeOS.Hub.Common.Device> GetDevices();

        void Dispose();
    }
}
