using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HomeOS.Hub.Common.Bolt.DataStore
{
    /// <summary>
    /// Read and write data to a Windows Azure blob storage account.
    /// </summary>
    public interface IStream
    {
        /* Put */
        /// <summary>
        /// Appends a new value to the specified key.
        /// </summary>
        /// <param name="key">The key to update.</param>
        /// <param name="value">The value.</param>
        /// <example>
        /// <![CDATA[
        /// IStream datastream;
        /// datastream = base.CreateFileStream<StrKey, StrValue>("myStream", false);
        /// StrKey key = new StrKey("myKey");
        /// StrValue val = new StrValue("myVal");
        /// datastream.Append(key, val);]]>
        /// </example>
        void Append(IKey key, IValue value, long timestamp = -1);


        /// <summary>
        /// Appends a list of key value pairs to the stream. Each key value pair is stored with the current timestamp.
        /// </summary>
        /// <param name="listOfKeyValuePairs">List of tuples, where each tuple is a key value pair.</param>
        void Append(List<Tuple<IKey,IValue>> listOfKeyValuePairs);


        /// <summary>
        /// Appends a value to all the keys provided as a list
        /// </summary>
        /// <param name="listOfKeys">List of keys to which the value is to be appended to</param>
        /// <param name="value">Value to append</param>
        void Append(List<IKey> listOfKeys, IValue value);

        /// <summary>
        /// Modifies the newest value for the specified key.
        /// </summary>
        /// <param name="key">The key to update.</param>
        /// <param name="value">The value.</param>
        /// <example>
        /// <![CDATA[
        /// IStream datastream;
        /// datastream = base.CreateFileStream<StrKey, StrValue>("myStream", false);
        /// StrKey key = new StrKey("myKey");
        /// StrValue val = new StrValue("myVal");
        /// datastream.Put(key, val);]]>
        /// </example>
        void Update(IKey key, IValue value);

        /* Get, including Range queries */
        /// <summary>
        /// Gets the newest value from the specified key.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>An IValue containing the results.</returns>
        /// <example>
        /// <![CDATA[
        /// IStream datastream;
        /// datastream = base.CreateFileStream<StrKey, StrValue>("myStream", false);
        /// string result = datastream.Get("myKey");]]>
        /// </example>
        IValue Get(IKey key);

        /// <summary>
        /// Gets the newest [key, value, timestamp] tuple inserted.
        /// </summary>
        /// <returns>The newest tuple (key, value, timestamp) that was inserted.</returns>
        /// <example>
        /// <![CDATA[
        /// IStream datastream;
        /// datastream = base.CreateFileStream<StrKey, StrValue>("myStream", false);
        /// Tuple result = datastream.GetLatest();]]>
        /// </example>
        Tuple<IKey, IValue> GetLatest();

        /// <summary>
        /// Get all the [key, value, ts] tuples corresponding to the specified key.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <returns>An IEnumerable containing the results.</returns>
        /// <example>
        /// <![CDATA[
        /// IStream datastream;
        /// datastream = base.CreateFileStream<StrKey, StrValue>("myStream", false);
        /// IEnumerable result = datastream.GetAll("myKey");]]>
        /// </example>
        IEnumerable<IDataItem> GetAll(IKey key);

        /// <summary>
        /// Get all the [key, value, timestamp] tuples in the given time range corresponding to the specified key.
        /// </summary>
        /// <param name="key">The key to query.</param>
        /// <param name="startTimeStamp">The timestamp at which the range should begin.</param>
        /// <param name="endTimeStamp">The timestamp at which the range should end.</param>
        /// <returns>An IEnumerable containing the results.</returns>
        /// <example>
        /// <![CDATA[
        /// IStream datastream;
        /// datastream = base.CreateFileStream<StrKey, StrValue>("myStream", false);
        /// DateTime fromTime = new DateTime(2001, 01, 01, 0, 0, 0, DateTimeKind.Utc);
        /// DateTime rightNow = DateTime.UtcNow;
        /// IEnumerable result = datastream.GetAll("myKey", fromTime, rightNow);]]>
        /// </example>
        IEnumerable<IDataItem> GetAll(IKey key, long startTimeStamp, long endTimeStamp);



        /// <summary>
        /// Get values for given key at startTimeStamp, startTimeStamp+skip, startTimeStamp+2*skip ..... endTimeStamps
        /// </summary>
        /// <param name="key">The required key</param>
        /// <param name="startTimeStamp">The starting timestamp</param>
        /// <param name="endTimeStamp">The ending or bounding timestamp</param>
        /// <param name="skip">The difference between required consecutive values' timestamps</param>
        /// <returns></returns>
        IEnumerable<IDataItem> GetAllWithSkip(IKey key, long startTimeStamp, long endTimeStamp, long skip);



        /// <summary>
        /// Get a list of all keys in the specified key range.
        /// </summary>
        /// <param name="startKey">The key at which the range should begin.</param>
        /// <param name="endKey">The key at which the range should end.</param>
        /// <returns>A List containing the results.</returns>
        /// <example>
        /// <![CDATA[
        /// IStream datastream;
        /// datastream = base.CreateFileStream<StrKey, StrValue>("myStream", false);
        /// IKey startKey = new IKey("begin");
        /// IKey endKey = new IKey("end");
        /// List result = datastream.GetKeys(startKey, endKey);]]>
        /// </example>
        HashSet<IKey> GetKeys(IKey startKey, IKey endKey);

        /*
        /// <summary>
        /// Deletes the current stream.
        /// </summary>
        void DeleteStream();
        */

        /* ACL calls */
        /// <summary>
        /// Grants read access to the app at the specified AppId.
        /// </summary>
        /// <param name="AppId">The AppId of the app to which read access should be granted.</param>
        /// <returns>A boolean indicating success or failure.</returns>
        bool GrantReadAccess(string AppId);

        /// <summary>
        /// Grants read access to the app at the specified HomeId and AppId.
        /// </summary>
        /// <param name="HomeId">The HomeId of the app to which read access should be granted.</param>
        /// <param name="AppId">The AppId of the app to which read access should be granted.</param>
        /// <returns>A boolean indicating success or failure.</returns>
        bool GrantReadAccess(string HomeId, string AppId);

        /// <summary>
        /// Revokes read access from the app at the specified AppId.
        /// </summary>
        /// <param name="AppId">The AppId of the app from which read access should be revoked.</param>
        /// <returns>A boolean indicating success or failure.</returns>
        bool RevokeReadAccess(string AppId);

        /// <summary>
        /// Revokes read access from the app at the specified HomeId and AppId.
        /// </summary>
        /// <param name="HomeId">The HomeId of the app from which read access should be revoked.</param>
        /// <param name="AppId">The AppId of the app from which read access should be revoked.</param>
        /// <returns>A boolean indicating success or failure.</returns>
        bool RevokeReadAccess(string HomeId, string AppId);

        /// <summary>
        /// Flushes the current stream from memory.
        /// </summary>
        // void Flush();

        /// <summary>
        /// Closes the current stream.
        /// </summary>
        /// <returns>A boolean indicating success or failure.</returns>
        bool Close();

        void DumpLogs(string file);
        void Seal(bool checkMemPressure);
    }
}
