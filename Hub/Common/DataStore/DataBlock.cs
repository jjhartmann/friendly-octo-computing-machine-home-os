using HomeOS.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HomeOS.Hub.Common.DataStore
{
    [DataContract]
    [KnownType("GetKnownTypes")]
    class DataBlock<KeyType, ValType> : JsonSerializerBase<DataBlock<KeyType, ValType>>
    {
        public DataBlock()
        {
            if (!typeof(IKey).IsAssignableFrom(typeof(KeyType)))
            {
                throw new InvalidDataException("KeyType must implement IKey");
            }
            if (!typeof(IValue).IsAssignableFrom(typeof(ValType)))
            {
                throw new InvalidDataException("ValType must implement IValue");
            }
        }

        public static Type[] knownTypes = new Type[] {typeof(KeyType), typeof(ValType)};
        public static Type[] GetKnownTypes()
        {
            return knownTypes;
        }
        [DataMember(Name = "op")]
        public byte op { get; set; }
        [DataMember(Name = "timestamp")]
        public long timestamp { get; set; }
        [DataMember(Name = "key")]
        public IKey key { get; set; }
        [DataMember(Name = "value")]
        public IValue value { get; set; }
    }
}
