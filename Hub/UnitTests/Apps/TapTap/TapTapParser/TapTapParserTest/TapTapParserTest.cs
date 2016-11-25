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
        public void TestXMLObjectGenPostive() {
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


        [TestMethod]
        public void TestXMLObjectGenNegative()
        {
            TapTapParser parser = new TapTapParser();
            string xml = @"<TEST> 
                                <Name>hello</Name>
                                <Lights>canada</Lights>
                                <Staff> 234 </Staff>
                            </TEST>";
            parser.ReadRaw(xml.Replace("\r\n", string.Empty));


            // Generate new object
            CoolObject cobj = parser.GenObject<CoolObject>();
            Assert.AreNotEqual(cobj, null);
        }


        

        [TestMethod]
        public void TestXMLObjectGenConfig()
        {
            TapTapParser parser = new TapTapParser();
            string xml = @"<ConfigTapTap> 
                                <Devices>
                                    <Device><Id>234g45k4k</Id><Name>Jeremy's Phone</Name></Device>
                                    <Device><Id>2352343g5k</Id><Name>Mark's Phone</Name></Device>
                                    <Device><Id>234g2gg344k</Id><Name>Body's Phone</Name></Device>
                                </Devices>
                                <Things>
                                    <Thing><Id>Zwve.Node2</Id><NFC>12384756</NFC></Thing>
                                    <Thing><Id>Zwve.Node1</Id><NFC>56376245</NFC></Thing>
                                    <Thing><Id>Zwve.Node5</Id><NFC>22527216</NFC></Thing>
                                    <Thing><Id>Zwve.Node8</Id><NFC>16134512</NFC></Thing>
                                </Things>
                            </ConfigTapTap>";
            parser.ReadRaw(xml.Replace("\r\n", string.Empty));


            // Generate new object
            TapTapConfig cobj = parser.GenObject<TapTapConfig>();
            Assert.AreNotEqual(cobj, null);
        }
    }
}
