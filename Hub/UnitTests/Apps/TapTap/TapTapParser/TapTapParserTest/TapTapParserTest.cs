using HomeOS.Hub.Apps.TapTap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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
            string xml =
                            @"<TapTapConfig>
                              <Devices>
                                <Device>
                                  <Id>11131yweryeh5112</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                                <Device>
                                  <Id>24whshwey5622</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                                <Device>
                                  <Id>2324562525uy4uyt423</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                                <Device>
                                  <Id>23252562456</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                                <Device>
                                  <Id>562625622yehw</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                                <Device>
                                  <Id>2426525622252</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                                <Device>
                                  <Id>11126262315112</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                                <Device>
                                  <Id>111325625615112</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                                <Device>
                                  <Id>werter33y</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                                <Device>
                                  <Id>fweryy</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                                <Device>
                                  <Id>dfgqerqyqy</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                                <Device>
                                  <Id>1113256215112</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                                <Device>
                                  <Id>111256252315112</Id>
                                  <Name>Hello Phone</Name>
                                </Device>
                              </Devices>
                              <Things>
                                <Thing>
                                  <Id>sdfgsfg</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>hwwerh</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>sdfgs</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>zwave4sdfgsf35</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>zwave453</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>zw5avsdfgsdfse43</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>zw5a6fgsdfgsdgsve43</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>asdfasdfasdfasdfas</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>zwa6ve43</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>zw7sdfgsdg5ave43</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>zwa7dgsdfgsd252ve43</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>zwa2gqw3267ve43</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>zwav252525e43</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                                <Thing>
                                  <Id>zwa223452ve43</Id>
                                  <NFCTag>2314123123232</NFCTag>
                                </Thing>
                              </Things>
                              <DeviceAuth>
                                <Auth>
                                  <DeviceID>2324562525uy4uyt423</DeviceID>
                                  <ThingsList>
                                    <Thing>zwave4sdfgsf35</Thing>
                                    <Thing>adfasdf</Thing>
                                    <Thing>zwavsdfae4sdfgsf35</Thing>
                                    <Thing>zwaveasdfasdfasd4sdfgsf35</Thing>
                                    <Thing>sdfasasdfasddf</Thing>
                                    <Thing>zwave4sdfgsf35</Thing>
                                    <Thing>zwave4sasdfadfgsf35</Thing>
                                    <Thing>zwavefasdfasd4sdfgsf35</Thing>
                                    <Thing>zwaveasdfafadsf4sdfgsf35</Thing>
                                    <Thing>zwaveadsfasdfas4sdfgsf35</Thing>
                                  </ThingsList>
                                </Auth>
                                <Auth>
                                  <DeviceID>dsfasdfadsf</DeviceID>
                                  <ThingsList>
                                    <Thing>zwave4sdfgsf35</Thing>
                                    <Thing>adfasdf</Thing>
                                    <Thing>zwavsdfae4sdfgsf35</Thing>
                                    <Thing>zwaveasdfasdfasd4sdfgsf35</Thing>
                                    <Thing>sdfasasdfasddf</Thing>
                                    <Thing>zwave4sdfgsf35</Thing>
                                    <Thing>zwave4sasdfadfgsf35</Thing>
                                    <Thing>zwavefasdfasd4sdfgsf35</Thing>
                                    <Thing>zwaveasdfafadsf4sdfgsf35</Thing>
                                    <Thing>zwaveadsfasdfas4sdfgsf35</Thing>
                                  </ThingsList>
                                </Auth>
                                <Auth>
                                  <DeviceID>asdfasdfasdf</DeviceID>
                                  <ThingsList>
                                    <Thing>zwave4sdfgsf35</Thing>
                                    <Thing>adfasdf</Thing>
                                    <Thing>zwavsdfae4sdfgsf35</Thing>
                                    <Thing>zwaveasdfasdfasd4sdfgsf35</Thing>
                                    <Thing>sdfasasdfasddf</Thing>
                                    <Thing>zwave4sdfgsf35</Thing>
                                    <Thing>zwave4sasdfadfgsf35</Thing>
                                    <Thing>zwavefasdfasd4sdfgsf35</Thing>
                                    <Thing>zwaveasdfafadsf4sdfgsf35</Thing>
                                    <Thing>zwaveadsfasdfas4sdfgsf35</Thing>
                                  </ThingsList>
                                </Auth>
                              </DeviceAuth>
                            </TapTapConfig>";
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

            HashSet<string> d1 = new HashSet<string>
            {
                "zwave4sdfgsf35",
                "adfasdf",
                "zwavsdfae4sdfgsf35",
                "zwaveasdfasdfasd4sdfgsf35",
                "sdfasasdfasddf",
                "zwave4sdfgsf35",
                "zwave4sasdfadfgsf35",
                "zwavefasdfasd4sdfgsf35",
                "zwaveasdfafadsf4sdfgsf35",
                "zwaveadsfasdfas4sdfgsf35"
            };
            config.DeviceAuth["2324562525uy4uyt423"] = d1;
            config.DeviceAuth["dsfasdfadsf"] = d1;
            config.DeviceAuth["asdfasdfasdf"] = d1;
            config.DeviceAuth["asdfasdfasdf"] = d1;
            config.DeviceAuth["asdfasdfasdf"] = d1;

            TapTapParser parser = new TapTapParser(Directory.GetCurrentDirectory().ToString(), "Test.xml", "Test");
            parser.CreateXml(config);
        }
    }
}