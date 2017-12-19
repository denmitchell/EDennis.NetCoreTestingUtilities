using System.IO;
using Xunit;

namespace EDennis.NetCoreTestingUtilities.Json {

    public class Person {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }


    public class JsonTests{

        [Fact]
        public void FromObject() {
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
        public void FromString() {

            var json = File.ReadAllText("JsonTests/FromString.json");

            var jsonObj = new Json().FromString(json);
            var firstName = jsonObj.JToken.SelectToken("FirstName").ToString();
            var lastName = jsonObj.JToken.SelectToken("LastName").ToString();

            Assert.Equal("Bob", firstName);
            Assert.Equal("Barker", lastName);
        }

        [Fact]
        public void FromPath1() {

            var json = File.ReadAllText("JsonTests/FromString.json");

            var firstName = new Json().FromPath("JsonTests/FromPath.json","FirstName").ToString();
            var city = new Json().FromPath("JsonTests/FromPath.json", "Address/City").ToString();

            Assert.Equal("Bob", firstName);
            Assert.Equal("Hartford", city);
        }

        [Fact]
        public void FromPath2() {

            var firstName = new Json().FromPath("JsonTests/FromPath.json/FirstName").ToString();
            var city = new Json().FromPath("JsonTests/FromPath.json/Address/City").ToString();

            Assert.Equal("Bob", firstName);
            Assert.Equal("Hartford", city);
        }


        [Fact]
        public void FromPath3() {

            var p = new Person {
                FirstName = "Bob",
                LastName = "Barker"
            };

            var jtoken = new Json().FromObject(p).JToken;

            var firstName = new Json().FromPath(jtoken,"FirstName").ToString();
            var lastName = new Json().FromPath(jtoken, "LastName").ToString();

            Assert.Equal("Bob", firstName);
            Assert.Equal("Barker", lastName);
        }

        [Fact]
        public void FromSql1() {
            using (var context = new JsonResultContext()) {
                var expectedJson = new Json()
                        .FromPath(@"PersonRepo\GetPersons\01.json\persons");
                var actualJson = new Json()
                        .FromSql(@"PersonRepo\GetPersons\01.sql", context);

                Assert.Equal(expectedJson, actualJson);
            }
        }

        [Fact]
        public void FromSql2() {
            using (var context = new JsonResultContext()) {
                var expectedJson = new Json()
                        .FromPath(@"PersonRepo\GetPersons\01.json\persons");
                var actualJson = new Json()
                        .FromSql(@"PersonRepo\GetPersons\01.sql", "Server=(localdb)\\mssqllocaldb;Database=tempdb;Trusted_Connection=True;");

                Assert.Equal(expectedJson, actualJson);
            }

        }


        [Fact]
        public void Filter1() {
            using (var context = new JsonResultContext()) {
                var expectedJson = new Json()
                        .FromPath(@"JsonTests\FilterTests\categoryFilter.json");
                var actualJson = new Json()
                        .FromPath(@"JsonTests\FilterTests\bookstore.json")
                        .Filter(@"category");

                Assert.Equal(expectedJson.ToString(), actualJson.ToString());
            }

        }


    }
}
