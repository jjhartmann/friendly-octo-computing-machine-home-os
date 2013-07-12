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
    public interface IKey : IJsonSerializer, IEquatable<IKey>, IComparable<IKey>
    {
        /* returns true if startKey >= key <= endKey */
        bool Between(IKey startKey, IKey endKey);
    }

    [DataContract]
    public class StrKey : JsonSerializerBase<StrKey>, IKey
    {
        [DataMember(Name = "key")]
        public string key { get; set; }

        public StrKey(string k) 
        {
            key = k;
        }

        public bool Equals(IKey other)
        {
            if (other == null)
                return false;

            StrKey sk = other as StrKey;
            if (this.key == sk.key)
                return true;
            else
                return false;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            StrKey sk = obj as StrKey;
            if (sk == null)
                return false;
            else
                return Equals(sk);
        }

        public override int GetHashCode()
        {
            return key.GetHashCode();
        }

        public int CompareTo(IKey other)
        {
            // If other is not a valid object reference, this instance is greater. 
            if (other == null) return 1;

            StrKey sk = other as StrKey;
            return key.CompareTo(sk.key);
        }

        public bool Between(IKey startKey, IKey endKey)
        {
            if ((startKey == null) || (endKey == null)) return false;

            StrKey sk = startKey as StrKey;
            StrKey ek = endKey as StrKey;

            return ((key.CompareTo(sk.key) >= 0) && (key.CompareTo(ek.key) <= 0)) ? true : false;
        }
        
        public override string ToString()
        {
            return key;
        }
    }
}
