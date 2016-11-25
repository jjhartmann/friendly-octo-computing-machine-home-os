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
    }
}
