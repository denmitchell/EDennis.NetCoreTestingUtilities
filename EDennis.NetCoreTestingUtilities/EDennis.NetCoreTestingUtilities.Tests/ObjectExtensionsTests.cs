using EDennis.NetCoreTestingUtilities.Extensions;
using EDennis.NetCoreTestingUtilities.Tests.TestJsonTable;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.NetCoreTestingUtilities.Tests {
    public class ObjectExtensionsTests {

        private readonly ITestOutputHelper _output;

        public ObjectExtensionsTests(ITestOutputHelper output) {
            _output = output;
        }

        [Fact]
        public void ObjExt_Copy() {
            var persons = new List<Person>().FromJsonPath(@"PersonRepo\GetPersons\01.json\persons");
            var persons2 = persons.Copy();

            var json1 = JToken.FromObject(persons).ToString();
            var json2 = JToken.FromObject(persons2).ToString();

            Assert.True(persons.GetHashCode() != persons2.GetHashCode());
            Assert.Equal(json1, json2);
        }


        [Fact]
        public void ObjExt_IsSame() {
            var persons = new List<Person>().FromJsonPath(@"PersonRepo\GetPersons\01.json\persons");
            var persons2 = persons.Copy();
            var persons3 = persons;

            Assert.NotEqual(persons, persons2);
            Assert.False(persons.IsSame(persons2));

            Assert.Equal(persons, persons3);
            Assert.True(persons.IsSame(persons3));
        }


        [Fact]
        public void ObjExt_IsEqual() {
            var persons = new List<Person>().FromJsonPath(@"PersonRepo\GetPersons\01.json\persons");
            var persons2 = persons.Copy();
            var persons3 = persons;

            Assert.NotEqual(persons, persons2);
            Assert.True(persons.IsEqual(persons2));

            Assert.Equal(persons, persons3);
            Assert.True(persons.IsEqual(persons3));

            persons2[0].FirstName = "XXX";

            Assert.False(persons.IsEqual(persons2));
        }


        [Fact]
        public void ObjExt_IsEqualWithFilter() {
            var persons = new List<Person>().FromJsonPath(@"PersonRepo\GetPersons\01.json\persons");
            var persons2 = persons.Copy();

            persons2[0].FirstName = "XXX";
            persons2[1].FirstName = "XXX";

            Assert.False(persons.IsEqual(persons2));
            Assert.True(persons.IsEqual(persons2, new string[] { "FirstName"}));
        }

        [Fact]
        public void ObjExt_IsEqualWithFilter2() {
            var person = new Person().FromJsonPath(@"PersonRepo\GetPersons\01.json\persons[0]");
            var person2 = person.Copy();

            person.FirstName = "XXX";

            Assert.False(person.IsEqual(person2));
            Assert.True(person.IsEqual(person2, new string[] { "FirstName" }));
        }


        [Fact]
        public void ObjExt_ToJsonString() {
            var json = File.ReadAllText(@"PersonRepo\GetPersons\01.json");
            var expectedResult = JToken.FromObject(JToken.Parse(json).SelectToken("persons[0]").ToObject<Person>()).ToString();
            

            var person = new Person() {
                ID = 1,
                FirstName = "Bob",
                LastName = "Jones",
                DateOfBirth = new DateTime(1980, 1, 23),
                Skills = new List<Skill>(new Skill[]{
                        new Skill(){
                            Category = "Application Development",
                            Score = 3
                        },
                        new Skill(){
                            Category = "Project Management",
                            Score = 3
                        }
                    }
                )
            };

            var actualResult = person.ToJsonString();

            Assert.Equal(expectedResult, actualResult);

        }


        [Fact]
        public void ObjExt_FromJsonString() {
            var json = File.ReadAllText(@"PersonRepo\GetPersons\01.json");
            var actualResult = new Person().FromJsonString(JToken.Parse(json).SelectToken("persons[0]").ToString());


            var person = new Person() {
                ID = 1,
                FirstName = "Bob",
                LastName = "Jones",
                DateOfBirth = new DateTime(1980, 1, 23),
                Skills = new List<Skill>(new Skill[]{
                        new Skill(){
                            Category = "Application Development",
                            Score = 3
                        },
                        new Skill(){
                            Category = "Project Management",
                            Score = 3
                        }
                    }
                )
            };

            var expectedResult = person;

            Assert.True(expectedResult.IsEqual(actualResult));

        }



        [Fact]
        public void ObjExt_FromJsonPath1() {

            var actualResult = new Person().FromJsonPath(@"PersonRepo\GetPersons\01.json","persons[0]");

            var person = new Person() {
                ID = 1,
                FirstName = "Bob",
                LastName = "Jones",
                DateOfBirth = new DateTime(1980, 1, 23),
                Skills = new List<Skill>(new Skill[]{
                        new Skill(){
                            Category = "Application Development",
                            Score = 3
                        },
                        new Skill(){
                            Category = "Project Management",
                            Score = 3
                        }
                    }
                )
            };

            var expectedResult = person;

            Assert.True(expectedResult.IsEqual(actualResult));

        }


        [Fact]
        public void ObjExt_FromJsonPath2() {

            var actualResult = new Person().FromJsonPath(@"PersonRepo\GetPersons\01.json\persons[0]");

            var person = new Person() {
                ID = 1,
                FirstName = "Bob",
                LastName = "Jones",
                DateOfBirth = new DateTime(1980, 1, 23),
                Skills = new List<Skill>(new Skill[]{
                        new Skill(){
                            Category = "Application Development",
                            Score = 3
                        },
                        new Skill(){
                            Category = "Project Management",
                            Score = 3
                        }
                    }
                )
            };

            var expectedResult = person;

            Assert.True(expectedResult.IsEqual(actualResult));

        }


        [Fact]
        public void ObjExt_FromJsonPath3() {

            var json = File.ReadAllText(@"PersonRepo\GetPersons\01.json");
            var jtoken = JToken.Parse(json);

            var actualResult = new Person().FromJsonPath(jtoken,@"persons[0]");

            var person = new Person() {
                ID = 1,
                FirstName = "Bob",
                LastName = "Jones",
                DateOfBirth = new DateTime(1980, 1, 23),
                Skills = new List<Skill>(new Skill[]{
                        new Skill(){
                            Category = "Application Development",
                            Score = 3
                        },
                        new Skill(){
                            Category = "Project Management",
                            Score = 3
                        }
                    }
                )
            };

            var expectedResult = person;

            Assert.True(expectedResult.IsEqual(actualResult));

        }



        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void ObjExt_FromTestJsonTable(int partId) {
            var options = new DbContextOptionsBuilder<PartSupplierContext>()
                .UseInMemoryDatabase(databaseName: "FromTestJsonTable")
                .Options;
            using var context = new PartSupplierContext();
            context.ResetValueGenerators();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            PartSupplierFactory.PopulateContext(context);

            Part expected = context.Parts.FirstOrDefault(p => p.PartId == partId);
            var json = expected.ToJsonString(3);

            context.TestJsons.Add(new TestJson {
                ProjectName = "EDennis.NetCoreTestingUtilities.Tests", ClassName = "ObjectExtensions",
                MethodName = "FromTestJsonTable", TestScenario = "Test",
                TestCase = $"{ partId }", TestFile = $"Expected", Json = json
            });

            context.SaveChanges();

            Part actual = new Part().FromTestJsonTable(context, "dbo", "TestJson",
                    "EDennis.NetCoreTestingUtilities.Tests", "ObjectExtensions",
                    "FromTestJsonTable", "Test", $"{ partId }", $"Expected");

            Assert.True(actual.IsEqualOrWrite(expected, 3, _output));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void ObjExt_FromTestJsonTableAsString(int partId) {
            var options = new DbContextOptionsBuilder<PartSupplierContext>()
                .UseInMemoryDatabase(databaseName: "FromTestJsonTable")
                .Options;
            using var context = new PartSupplierContext();
            context.ResetValueGenerators();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            PartSupplierFactory.PopulateContext(context);

            Part expected = context.Parts.FirstOrDefault(p => p.PartId == partId);
            var json = expected.ToJsonString(3);

            context.TestJsons.Add(new TestJson {
                ProjectName = "EDennis.NetCoreTestingUtilities.Tests", ClassName = "ObjectExtensions",
                MethodName = "FromTestJsonTable", TestScenario = "Test",
                TestCase = $"{ partId }", TestFile = $"Expected", Json = json
            });

            context.SaveChanges();

            string actualJson = new string("").FromTestJsonTable(context, "dbo", "TestJson",
                    "EDennis.NetCoreTestingUtilities.Tests", "ObjectExtensions",
                    "FromTestJsonTable", "Test", $"{ partId }", $"Expected");

            Part actual = JToken.Parse(actualJson).ToObject<Part>();

            Assert.True(actual.IsEqualOrWrite(expected, 3, _output));
        }

    }
}
