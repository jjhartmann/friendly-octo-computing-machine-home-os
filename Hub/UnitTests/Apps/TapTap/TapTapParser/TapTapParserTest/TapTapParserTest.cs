using HomeOS.Hub.Apps.TapTap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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

        private class CoolObject
        {
            private string mName;
            private string mRegion;
            private int mNumber = 0;

            public string Name { get { return mName; } set { mName = value; } }

            public string Region { get { return mRegion; } set { mRegion = value; } }

            public int Number { get { return mNumber; } set { mNumber = value; } }
        }

        [TestMethod]
        public void TestXMLObjectGenPostive()
        {
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

        [TestMethod]
        public void TestXMLObjectGenEmpty()
        {
            TapTapParser parser = new TapTapParser();
            string xml = @"<ConfigTapTap>
                           </ConfigTapTap>";
            parser.ReadRaw(xml.Replace("\r\n", string.Empty));

            // Generate new object
            TapTapConfig cobj = parser.GenObject<TapTapConfig>();
            Assert.AreNotEqual(cobj, null);
        }

        [TestMethod]
        public void TestXMLFileCreation()
        {
            TapTapConfig config = new TapTapConfig();

            config.Devices["11131yweryeh5112"] = "Hello Phone";
            config.Devices["24whshwey5622"] = "Hello Phone";
            config.Devices["2324562525uy4uyt423"] = "Hello Phone";
            config.Devices["23252562456"] = "Hello Phone";
            config.Devices["562625622yehw"] = "Hello Phone";
            config.Devices["2426525622252"] = "Hello Phone";
            config.Devices["11126262315112"] = "Hello Phone";
            config.Devices["111325625615112"] = "Hello Phone";
            config.Devices["werter33y"] = "Hello Phone";
            config.Devices["fweryy"] = "Hello Phone";
            config.Devices["dfgqerqyqy"] = "Hello Phone";
            config.Devices["1113256215112"] = "Hello Phone";
            config.Devices["111256252315112"] = "Hello Phone";

            config.Things["sdfgsfg"] = "2314123123232";
            config.Things["hwwerh"] = "2314123123232";
            config.Things["sdfgs"] = "2314123123232";
            config.Things["zwave4sdfgsf35"] = "2314123123232";
            config.Things["zwave453"] = "2314123123232";
            config.Things["zw5avsdfgsdfse43"] = "2314123123232";
            config.Things["zw5a6fgsdfgsdgsve43"] = "2314123123232";
            config.Things["asdfasdfasdfasdfas"] = "2314123123232";
            config.Things["zwa6ve43"] = "2314123123232";
            config.Things["zw7sdfgsdg5ave43"] = "2314123123232";
            config.Things["zwa7dgsdfgsd252ve43"] = "2314123123232";
            config.Things["zwa2gqw3267ve43"] = "2314123123232";
            config.Things["zwav252525e43"] = "2314123123232";
            config.Things["zwa223452ve43"] = "2314123123232";

            TapTapParser parser = new TapTapParser(Directory.GetCurrentDirectory().ToString(), "Test.xml", "Test");
            parser.CreateXml(config);
        }
    }
}