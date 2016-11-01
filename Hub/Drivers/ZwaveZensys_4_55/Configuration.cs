using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace HomeOS.Hub.Drivers.ZwaveZensys.Configuration
{

    public static class ConfigurationHelper
    {
        public static DeviceSettingsSection GetCurrentComponentConfiguration()
        {
            return GetCurrentConfiguration().Sections["deviceSettings"] as DeviceSettingsSection;
        }

        public static System.Configuration.Configuration GetCurrentConfiguration()
        {
            return ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
        }
    }

    public class DeviceSettingsSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public DeviceCollection Devices
        {
            get { return (DeviceCollection)base[""]; }
        }
    }

    public class DeviceCollection : ConfigurationElementCollection
    {
        public DeviceCollection()
        {
            DeviceElement details = (DeviceElement)CreateNewElement();
            if (details.Name != "")
            {
                Add(details);
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new DeviceElement();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            return ((DeviceElement)element).Name;
        }

        public DeviceElement this[int index]
        {
            get
            {
                return (DeviceElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        new public DeviceElement this[string name]
        {
            get
            {
                return (DeviceElement)BaseGet(name);
            }
        }

        public int IndexOf(DeviceElement details)
        {
            return BaseIndexOf(details);
        }

        public void Add(DeviceElement details)
        {
            BaseAdd(details);
        }
        protected override void BaseAdd(ConfigurationElement element)
        {
            BaseAdd(element, false);
        }

        public void Remove(DeviceElement details)
        {
            if (BaseIndexOf(details) >= 0)
                BaseRemove(details.Name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override string ElementName
        {
            get { return "device"; }
        }
    }

    public class DeviceElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("settings", IsDefaultCollection = false)]
        public DeviceSettingCollection DeviceSettings
        {
            get { return (DeviceSettingCollection)base["settings"]; }

        }

        [ConfigurationProperty("roles", IsDefaultCollection = false)]
        public RoleCollection RoleList
        {
            get { return (RoleCollection)base["roles"]; }

        }
    }

    public class DeviceSettingCollection : ConfigurationElementCollection
    {

        public new DeviceSettingElement this[string name]
        {
            get
            {
                if (IndexOf(name) < 0) return null;

                return (DeviceSettingElement)BaseGet(name);
            }
        }

        public DeviceSettingElement this[int index]
        {
            get { return (DeviceSettingElement)BaseGet(index); }
        }

        public int IndexOf(string name)
        {
            name = name.ToLower();

            for (int idx = 0; idx < base.Count; idx++)
            {
                if (this[idx].Name.ToLower() == name)
                    return idx;
            }
            return -1;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new DeviceSettingElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DeviceSettingElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "setting"; }
        }

    }

    public class DeviceSettingElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public String Name
        {
            get { return (String)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("paramNum", IsRequired = true, IsKey = true)]
        public byte ParamNum
        {
            get { return (byte)this["paramNum"]; }
            set { this["paramNum"] = value; }
        }

        [ConfigurationProperty("level", IsRequired = true)]
        public byte Level
        {
            get { return (byte)this["level"]; }
            set { this["level"] = value; }
        }

        [ConfigurationProperty("value", IsRequired = true)]
        public string Value
        {
            get { return (string)this["value"]; }
            set { this["value"] = value; }
        }

        public override string ToString()
        {
            return String.Format("Name=\"{0}\"; ParamNumber=\"{1}\"; Level=\"{2}\"; Value=\"{3}\";", Name, ParamNum, Level, Value);
        }
    }

    public class RoleCollection : ConfigurationElementCollection
    {

        public new RoleElement this[string name]
        {
            get
            {
                if (IndexOf(name) < 0) return null;

                return (RoleElement)BaseGet(name);
            }
        }

        public RoleElement this[int index]
        {
            get { return (RoleElement)BaseGet(index); }
        }

        public int IndexOf(string name)
        {
            name = name.ToLower();

            for (int idx = 0; idx < base.Count; idx++)
            {
                if (this[idx].Name.ToLower() == name)
                    return idx;
            }
            return -1;
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new RoleElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RoleElement)element).Name;
        }

        protected override string ElementName
        {
            get { return "role"; }
        }
    }

    public class RoleElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        public String Name
        {
            get { return (String)this["name"]; }
            set { this["name"] = value; }
        }

        public override string ToString()
        {
            return String.Format("Name=\"{0}\";", Name);
        }
    }

}
