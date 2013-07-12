using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HomeOS.Hub.Common.DataStore
{
    public interface IStream
    {
        /* Put */
        /// <summary>
        /// Updates the latest value corresponding to a key.
        /// </summary>
        /// <param name="key">The key for the pair to update.</param>
        /// <param name="value">The new value.</param>
        void Update(IKey key, IValue value);

        /// <summary>
        /// Append a new value corresponding to a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Append(IKey key, IValue value);

        /* Get, including Range queries */
        /// <summary>
        /// Get the latest value corresponding to a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IValue Get(IKey key);

        /// <summary>
        /// Get the latest [key, value, ts] tuple inserted
        /// </summary>
        /// <returns></returns>
        Tuple<IKey, IValue> GetLatest();

        /// <summary>
        /// Get all the [key, value, ts] tuples corresponding to a key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<IDataItem> GetAll(IKey key);

        /// <summary>
        /// Get all the [key, value, ts] tuples in the given time range corresponding to a key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="startTimeStamp"></param>
        /// <param name="endTimeStamp"></param>
        /// <returns></returns>
        IEnumerable<IDataItem> GetAll(IKey key, long startTimeStamp, long endTimeStamp);

        /// <summary>
        /// Get all the keys in the specified key-range
        /// </summary>
        /// <param name="startKey"></param>
        /// <param name="endKey"></param>
        /// <returns></returns>
        List<IKey> GetKeys(IKey startKey, IKey endKey);

        /// <summary>
        /// 
        /// </summary>
        void DeleteStream();

        /* ACL calls */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        bool GrantReadAccess(string AppId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        bool RevokeReadAccess(string AppId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        bool GrantWriteAccess(string AppId);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        bool RevokeWriteAccess(string AppId);
        
        /// <summary>
        /// 
        /// </summary>
        void Flush();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool Close();
    }
}
