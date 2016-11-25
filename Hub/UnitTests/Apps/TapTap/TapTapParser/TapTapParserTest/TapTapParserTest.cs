using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HomeOS.Hub.Apps.TapTap;

namespace TapTapParsserTest
{
    [TestClass]
    public class TapTapParserTest
    {
        [TestMethod]
        public void TestXMLLoading()
        {
            TapTapParser parser = new TapTapParser();
            string xml = @"<TEST> 
                                <name>hello</name>
                                <region>canada</region>
                                <number> 234 </number>
                            </TEST>";
            
            parser.ReadRaw(xml.Replace("\r\n", string.Empty));
        }



        class CoolObject
        {
            string mName;
            string mRegion;
            int mNumber = 0;

            public string Name { get { return mName; } set { mName = value; } }

            public string Region { get { return mRegion; } set { mRegion = value; } }

            public int Number { get { return mNumber; } set { mNumber = value; } }

        }

        [TestMethod]
        public void TestXMLObjectGen() {
            TapTapParser parser = new TapTapParser();
            string xml = @"<TEST> 
                                <Name>hello</Name>
                                <Region>canada</Region>
                                <Number> 234 </Number>
                            </TEST>";
            parser.ReadRaw(xml.Replace("\r\n", string.Empty));


            // Generate new object
            CoolObject cobj = parser.GenObject<CoolObject>();
            Assert.AreNotEqual(cobj, null);
        }
    }
}
