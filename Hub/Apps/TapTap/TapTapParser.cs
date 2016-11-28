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
        string mFile;
        bool mIsValid = false;


        ~TapTapParser()
        {
            if (xmlDoc != null)
            {
                xmlDoc = null;
            }
        }

        public TapTapParser()
        {

        }

        public TapTapParser(string in_dir, string in_file, string in_name)
        {
            Read(in_dir, in_file, in_name); 
        }

        public void Read(string in_dir, string in_file, string in_name)
        {
            // Read configuration file
            Directory.CreateDirectory(in_dir);

            mFile = in_dir + "\\" + in_file;
            if (!File.Exists(mFile))
            {
                File.Create(mFile).Close();
            }

            XmlReader xmlReader = XmlReader.Create(mFile, xmlsettings);


            try
            {
                xmlDoc.Load(xmlReader);
                xmlReader.Close();
                xmlReader.Dispose();
                mIsValid = true;
            }
            catch (Exception e)
            {
                // Create file 
                xmlReader.Close();
                xmlReader.Dispose();
                XmlElement root = xmlDoc.CreateElement(in_name);
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

                    // TODO: Check for child nodes.
                    XmlNode node = xmlDoc.DocumentElement;
                    string name = propInfo.Name;
                    Type propType = propInfo.PropertyType;
                    XmlNode val;

                    try
                    {
                        val = node[propInfo.Name];
                    }
                    catch (Exception e)
                    {
                        val = null;
                    }

                    switch (Type.GetTypeCode(propType))
                    {
                        case TypeCode.Byte:
                        case TypeCode.SByte:
                        case TypeCode.UInt16:
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Decimal:
                        case TypeCode.Double:
                        case TypeCode.Single:
                            int intVal = val ==  null ? -1 : Int32.Parse(val.InnerText);
                            propInfo.SetValue(obj, intVal);
                            break;
                        case TypeCode.String:
                            string valstr = val != null ? val.InnerText : "NULL";
                            propInfo.SetValue(obj, valstr);
                            break;
                        case TypeCode.Object:

                            bool isDict = propType.Name.Equals("Dictionary`2") ? true : false;

                            if (isDict && val != null && val.HasChildNodes)
                            {
                                Type[] args = propType.GenericTypeArguments;
                                Type TKey = args[0];
                                Type TVal = args[1];

                                // For String string
                                if (Type.GetTypeCode(TKey) == TypeCode.String && Type.GetTypeCode(TVal) == TypeCode.String)
                                {

                                    Dictionary<string, string> dictProp = new Dictionary<string, string>();

                                    XmlNodeList nodeList = val.ChildNodes;
                                    foreach (XmlNode i in nodeList)
                                    {
                                        string textOne = i.ChildNodes[0].InnerText;
                                        string textTwo = i.ChildNodes[1].InnerText;

                                        dictProp[textOne] = textTwo;
                                    }


                                    propInfo.SetValue(obj, dictProp);
                                }
                                else if (Type.GetTypeCode(TKey) == TypeCode.String && Type.GetTypeCode(TVal) == TypeCode.Object)
                                {

                                    Dictionary<string, List<string>> dictProp = new Dictionary<string, List<string>>();

                                    XmlNodeList nodeList = val.ChildNodes;
                                    foreach (XmlNode i in nodeList)
                                    {
                                        string textOne = i.ChildNodes[0].InnerText;

                                        XmlNodeList innerList = i.ChildNodes[1].ChildNodes;
                                        List<string> thinglist = new List<string>();
                                        foreach (XmlNode j in innerList) {
                                            thinglist.Add(j.InnerText);
                                        }
                                        dictProp[textOne] = thinglist;
                                    }


                                    propInfo.SetValue(obj, dictProp);
                                }


                            }
                            break;
                        default:
                            break;
                    }
                    
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




        public bool CreateXml(IXMLParsable obj)
        {
            string xml = obj.GetXMLString();
            
            try
            {
                xmlDoc.LoadXml(xml);
                SaferSave();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: Unable to save file");
            }

            return false;
        }

        //we use this method to save xmlDoc to minimize the chances that bad configs will be left on disk
        public void SaferSave()
        {
            string tmpFile = mFile + ".tmp";

            xmlDoc.Save(mFile);

            //if (System.IO.File.Exists(mFile))
            //    System.IO.File.Delete(mFile);

            //System.IO.File.Move(tmpFile, mFile);
        }



        /// TESTING ////
        public static void Main()
        {

        }


    }


}
