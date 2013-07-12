using HomeOS.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;

namespace HomeOS.Hub.Common.DataStore
{   
    public interface IValue : IJsonSerializer, IBinarySerializer
    {
    }

    [DataContract]
    [Serializable]
    public class ByteValue : ValueSerializerBase<ByteValue>, IValue
    {
        [DataMember(Name = "val")]
        public byte[] val { get; set; }

        public ByteValue(byte[] v) 
        {
            val = v;
        }

        public override string ToString()
        {
            return  StreamFactory.GetString(val);
        }
    }

    [DataContract]
    [Serializable]
    public class StrValue : ValueSerializerBase<StrValue>, IValue
    {
        [DataMember(Name = "val")]
        public string val { get; set; }

        public StrValue(string v)
        {
            val = v;
        }

        public override string ToString()
        {
            return val;
        }
    }
}
