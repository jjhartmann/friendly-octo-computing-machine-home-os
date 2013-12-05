using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.IO;

using HomeOS.Hub.Common.MDServer;

namespace HomeOS.Hub.Common.Bolt.DataStore
{
    /*
    public interface ITrustedService
    {
        bool RegisterPubKey(Principal prin, string key);
        string GetPubKey(Principal prin);

        bool UpdateReaderKey(Principal caller, FQStreamID FQName, ACLEntry entry);
        ACLEntry GetReaderKey(FQStreamID FQName, Principal prin);

        bool AddAccount(FQStreamID FQName, AccountInfo accinfo);
        Dictionary<int, AccountInfo> GetAllAccounts(FQStreamID FQName);

        bool AddMdAccount(FQStreamID FQName, AccountInfo accinfo);
        AccountInfo GetMdAccount(FQStreamID FQName);

        List<Principal> GetAllReaders(FQStreamID FQName);

        void RemoveAllInfo(FQStreamID FQName);
    }
    
    [DataContract]
    public class Principal
    {
        private string _HomeId;
        private string _AppId;
        private Byte[] _Auth;

        [DataMember]
        public string HomeId
        {
            get { return _HomeId; }
            set { _HomeId = value; }
        }

        [DataMember]
        public Byte[] Auth
        {
            get { return _Auth; }
            set { _Auth = value; }
        }

        [DataMember]
        public string AppId
        {
            get { return _AppId; }
            set { _AppId = value; }
        }

        public Principal(string hid, string aid)
        {
            _HomeId = hid;
            _AppId = aid;
        }

        public override string ToString()
        {
            return HomeId + "/" + AppId;
        }
    }

    [DataContract]
    public class FQStreamID
    {
        private string _HomeId;
        private string _AppId;
        private string _StreamId;

        [DataMember]
        public string HomeId
        {
            get { return _HomeId; }
            set { _HomeId = value; }
        }

        [DataMember]
        public string AppId
        {
            get { return _AppId; }
            set { _AppId = value; }
        }

        [DataMember]
        public string StreamId
        {
            get { return _StreamId; }
            set { _StreamId = value; }
        }

        public FQStreamID(string hid, string aid, string sid)
        {
            _HomeId = hid;
            _AppId = aid;
            _StreamId = sid;
        }

        public override string ToString()
        {
            return HomeId + "/" + AppId + "/" + StreamId;
        }
    }

    [DataContract]
    public class AccountInfo
    {
        private int _num;
        private string _accountName;
        private string _accountKey;
        private string _location;

        [DataMember]
        public int num
        {
            get { return _num; }
            set { _num = value; }
        }

        [DataMember]
        public string accountName
        {
            get { return _accountName; }
            set { _accountName = value; }
        }

        [DataMember]
        public string accountKey
        {
            get { return _accountKey; }
            set { _accountKey = value; }
        }

        [DataMember]
        public string location
        {
            get { return _location; }
            set { _location = value; }
        }

        public AccountInfo(string accName, string accKey, string loc)
        {
            _accountName = accName;
            _accountKey = accKey;
            _location = loc;
        }
    }

    [DataContract]
    public class ACLEntry
    {
        private Principal _readerName;
        private byte[] _encKey;
        private byte[] _IV;
        private uint _keyVersion;

        [DataMember]
        public byte[] encKey
        {
            get { return _encKey; }
            set { _encKey = value; }
        }

        [DataMember]
        public byte[] IV
        {
            get { return _IV; }
            set { _IV = value; }
        }

        [DataMember]
        public uint keyVersion
        {
            get { return _keyVersion; }
            set { _keyVersion = value; }
        }

        [DataMember]
        public Principal readerName
        {
            get { return _readerName; }
            set { _readerName = value; }
        }

        public ACLEntry(Principal rName, byte[] key, byte[] iv, uint kversion)
        {
            _readerName = rName;
            _encKey = key;
            _IV = iv;
            _keyVersion = kversion;
        }

        public Principal GetPrincipal()
        {
            return _readerName;
        }
    }

    [DataContract]
    public class StreamInfo
    {
        [DataMember]
        private FQStreamID stream;
        
        [DataMember]
        private Principal owner;

        [DataMember]
        private Dictionary<int, AccountInfo> accounts;

        [DataMember]
        private AccountInfo md_account;

        [DataMember]
        private Dictionary<string, ACLEntry> readers;

        [DataMember]
        private uint latest_keyversion;

        public StreamInfo(FQStreamID sname)
        {
            stream = sname;
            owner = new Principal(sname.HomeId, sname.AppId);
            accounts = new Dictionary<int, AccountInfo>();
            readers = new Dictionary<string, ACLEntry>();
            latest_keyversion = 0;
        }

        public bool UpdateReader(ACLEntry entry)
        {
            if (entry.keyVersion < latest_keyversion)
                return false;
            readers[entry.GetPrincipal().ToString()] = entry;
            return true;
        }

        public ACLEntry GetReader(Principal prin)
        {
            if (readers.ContainsKey(prin.ToString()))
            {
                return readers[prin.ToString()];
            }
            return null;
        }

        public bool AddAccount(AccountInfo account)
        {
            accounts[account.num] = account;
            return true;
        }

        public bool AddMdAccount(AccountInfo account)
        {
            md_account = account;
            return true;
        }

        public Dictionary<int, AccountInfo> GetAllAccounts()
        {
            return accounts;
        }

        public AccountInfo GetMdAccount()
        {
            return md_account;
        }

        public List<Principal> GetAllReaders()
        {
            List<Principal> ret = new List<Principal>();
            foreach (KeyValuePair<string, ACLEntry> entry in readers)
            {
                ret.Add(entry.Value.readerName);
            }
            return ret;
        }

    }
     * */

    [DataContract]
    public class LocalMetaDataServer : IMetaDataService
    {
        private string FQFilename;
        
        [DataMember]
        private Dictionary<string, string> keytable = new Dictionary<string, string>();
        [DataMember]
        private Dictionary<string, StreamInfo> mdtable = new Dictionary<string, StreamInfo>();

        private Logger logger;

        public bool RegisterPubKey(Principal prin, string key)
        {
            if (logger != null) logger.Log("RegisterPubKey request for " + prin.ToString());
            if (keytable.ContainsKey(prin.ToString()))
            {
                return false;
            }
            else
            {
                keytable[prin.ToString()] = key;
            }
            return true;
        }

        public string GetPubKey(Principal prin)
        {
            if (logger != null) logger.Log("GetPubKey request for " + prin.ToString());
            // TODO(trinabh): return should be signed
            if (keytable.ContainsKey(prin.ToString()))
            {
                return keytable[prin.ToString()];
            }
            else
            {
                return null;
            }
        }

        public byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public bool UpdateReaderKey(Principal caller, FQStreamID stream, ACLEntry entry)
        {
            if (logger != null) logger.Log("UpdateReaderKey request from caller " + caller.ToString() + " for stream "
                + stream.ToString() + " and principal " + entry.readerName.ToString()
                + " key version " + entry.keyVersion);
            
            // Authentication is not required for unlisted streams
            /*
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            string callerpubkey = GetPubKey(caller);
            if (callerpubkey == null)
                return false;
            RSA.FromXmlString(callerpubkey);

            Byte[] data = { };
            data = data.Concat(this.GetBytes(caller.HomeId)).ToArray();
            data = data.Concat(this.GetBytes(caller.AppId)).ToArray();
            data = data.Concat(this.GetBytes(stream.HomeId)).ToArray();
            data = data.Concat(this.GetBytes(stream.AppId)).ToArray();
            data = data.Concat(this.GetBytes(stream.StreamId)).ToArray();
            data = data.Concat(this.GetBytes(entry.readerName.HomeId)).ToArray();
            data = data.Concat(this.GetBytes(entry.readerName.AppId)).ToArray();
            data = data.Concat(entry.encKey).ToArray();
            data = data.Concat(entry.IV).ToArray();
            data = data.Concat(this.GetBytes("" + entry.keyVersion)).ToArray();

            if (RSA.VerifyData(data, new SHA256CryptoServiceProvider(), caller.Auth) == false)
            {
                if (logger != null) logger.Log("Verification of request failed");
                return false;
            }
            //
            */

            if (caller.HomeId == stream.HomeId && caller.AppId == stream.AppId)
            {
                if (!mdtable.ContainsKey(stream.ToString()))
                    mdtable[stream.ToString()] = new StreamInfo(stream);
                mdtable[stream.ToString()].UpdateReader(entry);
                return true;
            }
            else
            {
                return false;
            }
        }

        public ACLEntry GetReaderKey(FQStreamID stream, Principal p)
        {
            if (logger != null) logger.Log("GetReaderKey from caller " + p.ToString() + " for stream "
                + stream.ToString());
            // TODO(trinabh): Return should be signed
            if (mdtable.ContainsKey(stream.ToString()))
            {
                return mdtable[stream.ToString()].GetReader(p);
            }
            return null;
        }

        public List<Principal> GetAllReaders(FQStreamID stream)
        {
            if (mdtable.ContainsKey(stream.ToString()))
            {
                return mdtable[stream.ToString()].GetAllReaders();
            }
            return null;
        }

        public bool AddAccount(FQStreamID stream, AccountInfo accinfo)
        {
            if (logger != null) logger.Log("Adding account info for stream "
                + stream.ToString());
            if (!mdtable.ContainsKey(stream.ToString()))
            {
                mdtable[stream.ToString()] = new StreamInfo(stream);
            }
            return mdtable[stream.ToString()].AddAccount(accinfo);
        }

        public Dictionary<int, AccountInfo> GetAllAccounts(FQStreamID stream)
        {
            if (logger != null) logger.Log("Serving account info for stream "
                + stream.ToString());
            if (mdtable.ContainsKey(stream.ToString()))
            {
                return mdtable[stream.ToString()].GetAllAccounts();
            }
            return null;
        }

        public bool AddMdAccount(FQStreamID stream, AccountInfo accinfo)
        {
            if (logger != null) logger.Log("Adding md account info for stream "
                + stream.ToString());
            if (!mdtable.ContainsKey(stream.ToString()))
            {
                mdtable[stream.ToString()] = new StreamInfo(stream);
            }
            return mdtable[stream.ToString()].AddMdAccount(accinfo);
        }

        public AccountInfo GetMdAccount(FQStreamID stream)
        {
            if (logger != null) logger.Log("Serving md account info for stream "
                + stream.ToString());
            if (mdtable.ContainsKey(stream.ToString()))
            {
                return mdtable[stream.ToString()].GetMdAccount();
            }
            return null;
        }

        public void RemoveAllInfo(FQStreamID stream)
        {
            if (logger != null) logger.Log("Removing all info for stream "
                + stream.ToString());
            if (mdtable.ContainsKey(stream.ToString()))
            {
                mdtable.Remove(stream.ToString());
            }
        }

        public LocalMetaDataServer(string filename, Logger log)
        {
            FQFilename = filename;
            logger = log;
            keytable = new Dictionary<string, string>();
            mdtable = new Dictionary<string, StreamInfo>();
        }
        
        public bool LoadMetaDataServer()
        {
            if (!File.Exists(FQFilename))
                return false;
            
            try
            {
                TextReader mdtr = new StreamReader(FQFilename);
                string json = mdtr.ReadToEnd();
                mdtr.Close();

                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(LocalMetaDataServer));
                LocalMetaDataServer ts = (LocalMetaDataServer)ser.ReadObject(ms);
                keytable = ts.keytable;
                mdtable = ts.mdtable;
                return true;
            }
            // catch (Exception e)
            catch
            {
                Console.WriteLine("Failed to load metadata file: " + FQFilename);
                // Console.WriteLine("{0} Exception caught.", e);
                return false;
            }
        }

        public void FlushMetaDataServer()
        {
            TextWriter mdtw = new StreamWriter(FQFilename, false);
            
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(LocalMetaDataServer));
            ser.WriteObject(ms, this);
            byte[] json = ms.ToArray();
            ms.Close();

            mdtw.Write(Encoding.UTF8.GetString(json, 0, json.Length));
            mdtw.Flush();
            mdtw.Close();
        }
    }
}
