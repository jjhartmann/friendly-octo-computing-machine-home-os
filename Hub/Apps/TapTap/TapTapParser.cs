using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HomeOS.Hub.Apps.TapTap
{
    public class TapTapParser
    {

        XmlDocument xmlDoc = new XmlDocument();
        XmlReaderSettings xmlsettings = new XmlReaderSettings();
        XmlReader xmlReader;
        string mFile;
        bool mIsValid = false;

        public TapTapParser()
        {

        }

        public TapTapParser(string in_dir, string in_file)
        {
            Read(in_dir, in_file); 
        }

        public void Read(string in_dir, string in_file)
        {
            // Read configuration file
            Directory.CreateDirectory(in_dir);

            string mFile = in_dir + "\\" + in_file;
            if (!File.Exists(mFile))
            {
                File.Create(mFile).Close();
            }

            xmlReader = XmlReader.Create(mFile, xmlsettings);


            try
            {
                xmlDoc.Load(xmlReader);
                mIsValid = true;
            }
            catch (Exception e)
            {
                // Create file 
                xmlReader.Close();
                XmlElement root = xmlDoc.CreateElement("TapTapConfig");
                xmlDoc.AppendChild(root);

                SaferSave();
            }

        }


        public void ReadRaw(string xml)
        {
            try
            {
                xmlDoc.LoadXml(xml);
                mIsValid = true;
            }
            catch (Exception e)
            {
                // Create file 
                Console.WriteLine("Error has occurred in xml parsing");
            }

        }


        // Determine of XML is in a valid state
        public bool IsValid()
        {
            return mIsValid;
        }
        

        // Genearte object form XML
        // OUT: Object of type T (NOT OWNED)
        public T GenObject<T>() where T : new()
        {
            T obj = new T();


            foreach(PropertyInfo propInfo in obj.GetType().GetProperties())
            {
                if (propInfo.CanRead)
                {

                }
            }

            return (T) obj;
        }


        
        // Get attribute from XMLDoc
        public object GetAttribute(string in_attr)
        {
            XmlNode node = xmlDoc.DocumentElement;
            return node[in_attr].Value;
        }

        //we use this method to save xmlDoc to minimize the chances that bad configs will be left on disk
        public void SaferSave()
        {
            string tmpFile = mFile + ".tmp";

            xmlDoc.Save(tmpFile);

            if (System.IO.File.Exists(mFile))
                System.IO.File.Delete(mFile);

            System.IO.File.Move(tmpFile, mFile);
        }



        /// TESTING ////
        public static void Main()
        {

        }


    }


}
