using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeOS.Hub.Common.DataStore
{
    public interface IDataItem
    {
        long GetTimestamp();
        IKey GetKey();
        IValue GetVal();
    }

    public class DataItems<KeyType, ValType> : IEnumerable<IDataItem>
    {
        private List<IDataItem> offsets;

        public DataItems(List<TS_Offset> tso_list, DataFileStream<KeyType, ValType> dfs, IKey k)
        {
            offsets = new List<IDataItem>();
            foreach (TS_Offset tso in tso_list)
            {
                offsets.Add((IDataItem) new DataItem<KeyType, ValType>(tso.offset, dfs, k));
            }
        }

        public DataItems(List<TS_Offset> tso_list, DataDirStream<KeyType, ValType> dds, IKey k)
        {
            offsets = new List<IDataItem>();
            foreach (TS_Offset tso in tso_list)
            {
                offsets.Add((IDataItem)new DataDirItem<KeyType, ValType>(tso.offset, dds, k));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public IEnumerator<IDataItem> GetEnumerator()
        {
            return (offsets as IEnumerable<IDataItem>).GetEnumerator();
        }
    }

    public class DataItem<KeyType, ValType> : IDataItem
    {
        private long offset;
        private DataFileStream<KeyType, ValType> stream;
        private IKey originalKey;
        private bool loaded;

        private long ts;
        private IValue val;
        private IKey key;

        public DataItem(long o, DataFileStream<KeyType, ValType> dfs, IKey k)
        {
            offset = o;
            stream = dfs;
            originalKey = k;
            loaded = false;
        }

        internal void LoadData()
        {
            DataBlock<KeyType, ValType> db = stream.ReadDataBlock(offset);
            key = db.key;
            val = db.value;
            ts = db.timestamp;

            if (originalKey.CompareTo(key) != 0)
            {
                // collision?
                // TODO: need to handle collision?
                throw new InvalidDataException("Key mismatch (unhandled key collision)");
            }

            loaded = true;
        }
        
        public long GetTimestamp()
        {
            if (!loaded)
            {
                LoadData();
            }
            return ts;
        }

        public IKey GetKey()
        {
            if (!loaded)
            {
                LoadData();
            }
            return key;
        }
        
        public IValue GetVal()
        {
            if (!loaded)
            {
                LoadData();
            }
            return val;
        }
    }

    public class DataDirItem<KeyType, ValType> : DataItem<KeyType, StrValue>
    {
        private DataDirStream<KeyType, ValType> DirStream;

        public DataDirItem(long o, DataDirStream<KeyType, ValType> dds, IKey k)
            : base(o, (DataFileStream<KeyType, StrValue>)dds, k)
        {
            DirStream = dds;
        }

        public new IValue GetVal()
        {
            IValue valuePath = base.GetVal();
            return DirStream.ReadData(valuePath); 
        }
    }
}
