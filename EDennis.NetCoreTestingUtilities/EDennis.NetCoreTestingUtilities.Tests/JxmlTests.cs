using EDennis.NetCoreTestingUtilities.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Xunit;

namespace EDennis.NetCoreTestingUtilities.Tests {
    public class JxmlTests {

        [Theory]
        [InlineData("persons")]
        [InlineData("bookstore")]
        [InlineData("colors")]
        [InlineData("colors2")]
        [InlineData("colors3")]
        public void TestJxml(string file) {

            var json = File.ReadAllText($"JsonTests\\FilterTests\\{file}.json");
            var jtoken = JToken.Parse(json);
            json = jtoken.ToString();
            XmlDocument doc = Jxml.ToJxml(jtoken);

            var jtoken2 = Jxml.ToJson(doc);
            var json2 = jtoken2.ToString();

            Assert.Equal(json, json2);
        }





    }
}
