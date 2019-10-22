using Newtonsoft.Json.Linq;
using System.IO;
using System.Xml;
using Xunit;

namespace EDennis.NetCoreTestingUtilities.Json {

    public class Person {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }


    public class JsonTests {


        [Theory]
        [InlineData("persons")]
        [InlineData("bookstore")]
        [InlineData("colors")]
        [InlineData("colors2")]
        [InlineData("colors3")]
        public void JsonToJxml(string file){

            var j = new JsonToJxml();
            var json = File.ReadAllText($"JsonTests\\FilterTests\\{file}.json");
            var jtoken = JToken.Parse(json);
            json = jtoken.ToString();
            XmlDocument doc = j.ConvertToJxml(jtoken);

            var x = new JxmlToJson();
            var jtoken2 = x.ConvertToJson(doc);
            var json2 = jtoken2.ToString();

            Assert.Equal(json, json2);

        }





    }
}
