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

        [Fact]
        public void Json_FromObject() {
            var p = new Person {
                FirstName = "Bob",
                LastName = "Barker"
            };

            var jsonObj = new Json().FromObject(p);
            var firstName = jsonObj.JToken.SelectToken("FirstName").ToString();
            var lastName = jsonObj.JToken.SelectToken("LastName").ToString();

            Assert.Equal("Bob", firstName);
            Assert.Equal("Barker", lastName);
        }


        [Fact]
        public void Json_FromString() {

            var json = File.ReadAllText("JsonTests/FromString.json");

            var jsonObj = new Json().FromString(json);
            var firstName = jsonObj.JToken.SelectToken("FirstName").ToString();
            var lastName = jsonObj.JToken.SelectToken("LastName").ToString();

            Assert.Equal("Bob", firstName);
            Assert.Equal("Barker", lastName);
        }

        [Fact]
        public void Json_FromPath1() {

            var json = File.ReadAllText("JsonTests/FromString.json");

            var firstName = new Json().FromPath("JsonTests/FromPath.json", "FirstName").ToString();
            var city = new Json().FromPath("JsonTests/FromPath.json", "Address/City").ToString();

            Assert.Equal("Bob", firstName);
            Assert.Equal("Hartford", city);
        }

        [Fact]
        public void Json_FromPath2() {

            var firstName = new Json().FromPath("JsonTests/FromPath.json/FirstName").ToString();
            var city = new Json().FromPath("JsonTests/FromPath.json/Address/City").ToString();

            Assert.Equal("Bob", firstName);
            Assert.Equal("Hartford", city);
        }


        [Fact]
        public void Json_FromPath3() {

            var p = new Person {
                FirstName = "Bob",
                LastName = "Barker"
            };

            var jtoken = new Json().FromObject(p).JToken;

            var firstName = new Json().FromPath(jtoken, "FirstName").ToString();
            var lastName = new Json().FromPath(jtoken, "LastName").ToString();

            Assert.Equal("Bob", firstName);
            Assert.Equal("Barker", lastName);
        }

        [Fact]
        public void Json_FromSql1() {
            using (var context = new JsonResultContext()) {
                var expectedJson = new Json()
                        .FromPath(@"PersonRepo\GetPersons\01.json\persons");
                var actualJson = new Json()
                        .FromSql(@"PersonRepo\GetPersons\01.sql", context);

                Assert.Equal(expectedJson, actualJson);
            }
        }

        [Fact]
        public void Json_FromSql2() {
            using (var context = new JsonResultContext()) {
                var expectedJson = new Json()
                        .FromPath(@"PersonRepo\GetPersons\01.json\persons");
                var actualJson = new Json()
                        .FromSql(@"PersonRepo\GetPersons\01.sql", "Server=(localdb)\\mssqllocaldb;Database=tempdb;Trusted_Connection=True;");

                Assert.Equal(expectedJson, actualJson);
            }

        }

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



        [Theory]
        [InlineData("categoryFilter", "bookstore", "category")]
        [InlineData("bookPriceOver10Filter", "bookstore", "$.store.book[?(@.price > 10)]")]
        [InlineData("noBob", "persons", "$..[?(@.FirstName==\"Bob\")]")]
        [InlineData("isbnAndBicycleFilter", "bookstore", "isbn", "bicycle")]
        public void Filter(string expectedFile, string inputFile, params string[] filters) {
            using (var context = new JsonResultContext()) {
                var expectedJson = new Json()
                        .FromPath($"JsonTests\\FilterTests\\{expectedFile}.json");
                var actualJson = new Json()
                        .FromPath($"JsonTests\\FilterTests\\{inputFile}.json");

                foreach(string filter in filters)
                    actualJson = actualJson.Filter(filter);

                Assert.Equal(expectedJson.ToString(), actualJson.ToString());
            }

        }

        [Fact]
        public void ToObject() {

            var expectedObject = new Person {
                FirstName = "Bob",
                LastName = "Barker"
            };

            var json = "{\"FirstName\":\"Bob\",\"LastName\":\"Barker\"}";
            var j = new Json().FromString(json);
            var actualObject = j.ToObject<Person>();

            Assert.Equal(expectedObject.FirstName, actualObject.FirstName);
            Assert.Equal(expectedObject.LastName, actualObject.LastName);

        }


        [Fact]
        public void ToStringTest() {

            var expectedObject = new Person {
                FirstName = "Bob",
                LastName = "Barker"
            };

            var json = "{\"FirstName\":\"Bob\",\"LastName\":\"Barker\"}";
            var j = new Json().FromString(json);
            var actualObject = j.ToObject<Person>();

            Assert.Equal(j.ToString(), JToken.Parse(json).ToString());

        }

        [Fact]
        public void EqualsTest() {

            var person1 = new Person {
                FirstName = "Bob",
                LastName = "Barker"
            };

            var person2 = new Person {
                FirstName = "Bob",
                LastName = "Barker"
            };

            var j1 = new Json().FromObject(person1);
            var j2 = new Json().FromObject(person2);

            Assert.Equal(j1,j2);

        }

    }
}
