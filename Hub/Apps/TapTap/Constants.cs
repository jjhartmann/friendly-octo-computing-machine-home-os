using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeOS.Hub.Apps.TapTap
{
    static class TapTapConstants
    {
        public const bool DEBUG_CLIENT_SERVER = false;
        public const string STR_NULL = "NULL";
        public const string CERT_DIR_FILE = "\\Cert\\certificate.cer";
        public const string CONFIG_DIR = "\\Config";
        public const string CONFIG_FILE = "taptapconfig.xml";

    }

    static class ENUM_MESSAGE_TYPE
    {
        public const string ACTION_BINARY_SWITCH = "binarySwitch";
        public const string ACTION_ADD_DEVICE_REQUEST = "addDeviceRequest";
    }
}
