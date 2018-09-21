using EDennis.NetCoreTestingUtilities.Tests.TestJsonTable;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace EDennis.NetCoreTestingUtilities.Tests {
    public class TestJsonTests : IClassFixture<TestJsonFixture>{

        private JsonTestCase jcase = new JsonTestCase() {
            ProjectName = "MyProject",
            MethodName = "MyMethod",
            ClassName = "MyClass",
            TestScenario = "MyScenario",
            TestCase = "MyCase",
            JsonTestFiles = new List<JsonTestFile> {
                    new JsonTestFile() {
                        TestFile = "Input",
                        Json = "123"
                    },
                    new JsonTestFile() {
                        TestFile = "Expected",
                        Json = "{\"firstName\":\"Bob\",\"lastName\":\"Barker\"}"
                    },
                    new JsonTestFile() {
                        TestFile = "NullRec",
                        Json = null
                    }
            }
        };

        static TestJsonTests() {
            /*
            var options = new DbContextOptionsBuilder<PartSupplierContext>()
                .UseInMemoryDatabase(databaseName: "FromTestJsonTable")
                .Options;
            using (var context = new PartSupplierContext()) {

                context.ResetValueGenerators();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.TestJsons.Add(new TestJson {
                    ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                    ClassName = "ClassA",
                    MethodName = "MethodA",
                    TestScenario = "TestScenarioA",
                    TestCase = "TestCaseA",
                    TestFile = "Input",
                    Json = "123"
                });

                context.TestJsons.Add(new TestJson {
                    ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                    ClassName = "ClassA",
                    MethodName = "MethodA",
                    TestScenario = "TestScenarioA",
                    TestCase = "TestCaseA",
                    TestFile = "Expected",
                    Json = "[\"A\",\"B\",\"C\"]"
                });

                context.TestJsons.Add(new TestJson {
                    ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                    ClassName = "ClassA",
                    MethodName = "MethodA",
                    TestScenario = "TestScenarioA",
                    TestCase = "TestCaseB",
                    TestFile = "Input",
                    Json = "456"
                });

                context.TestJsons.Add(new TestJson {
                    ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                    ClassName = "ClassA",
                    MethodName = "MethodA",
                    TestScenario = "TestScenarioA",
                    TestCase = "TestCaseB",
                    TestFile = "Expected",
                    Json = "[\"D\",\"E\",\"F\"]"
                });

                context.TestJsons.Add(new TestJson {
                    ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                    ClassName = "ClassA",
                    MethodName = "MethodA",
                    TestScenario = "TestScenarioB",
                    TestCase = "TestCaseA",
                    TestFile = "Input",
                    Json = "789"
                });

                context.TestJsons.Add(new TestJson {
                    ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                    ClassName = "ClassA",
                    MethodName = "MethodA",
                    TestScenario = "TestScenarioB",
                    TestCase = "TestCaseA",
                    TestFile = "Expected",
                    Json = "[\"G\",\"H\",\"I\"]"
                });

                context.TestJsons.Add(new TestJson {
                    ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                    ClassName = "ClassA",
                    MethodName = "MethodA",
                    TestScenario = "TestScenarioB",
                    TestCase = "TestCaseB",
                    TestFile = "Input",
                    Json = "123"
                });

                context.TestJsons.Add(new TestJson {
                    ProjectName = "EDennis.NetCoreTestingUtilities.Tests",
                    ClassName = "ClassA",
                    MethodName = "MethodA",
                    TestScenario = "TestScenarioB",
                    TestCase = "TestCaseB",
                    TestFile = "Expected",
                    Json = "[\"J\",\"K\",\"L\"]"
                });

                context.SaveChanges();
*/
            

        }

        [Theory]
        [TestJson("ClassA", "MethodA", "TestScenarioA", "TestCaseA")]
        [TestJson("ClassA", "MethodA", "TestScenarioA", "TestCaseB")]
        [TestJson("ClassA", "MethodA", "TestScenarioB", "TestCaseA")]
        [TestJson("ClassA", "MethodA", "TestScenarioB", "TestCaseB")]
        public void TestJson_GetData(string t, JsonTestCase jsonTestCase) {
            if (jsonTestCase.TestScenario.EndsWith("A")
                && jsonTestCase.TestCase.EndsWith("A")) {
                Assert.Equal("123", jsonTestCase.JsonTestFiles.Find(f => f.TestFile == "Input").Json);
                Assert.Equal("[\"A\",\"B\",\"C\"]", jsonTestCase.JsonTestFiles.Find(f => f.TestFile == "Expected").Json);
            } else if (jsonTestCase.TestScenario.EndsWith("A")
                && jsonTestCase.TestCase.EndsWith("B")) {
                Assert.Equal("2018-01-01", jsonTestCase.JsonTestFiles.Find(f => f.TestFile == "Input").Json);
                Assert.Equal("[\"D\",\"E\",\"F\"]", jsonTestCase.JsonTestFiles.Find(f => f.TestFile == "Expected").Json);
            } else if (jsonTestCase.TestScenario.EndsWith("B")
                && jsonTestCase.TestCase.EndsWith("A")) {
                Assert.Equal("789", jsonTestCase.JsonTestFiles.Find(f => f.TestFile == "Input").Json);
                Assert.Equal("[\"G\",\"H\",\"I\"]", jsonTestCase.JsonTestFiles.Find(f => f.TestFile == "Expected").Json);
            } else if (jsonTestCase.TestScenario.EndsWith("B")
                && jsonTestCase.TestCase.EndsWith("B")) {
                Assert.Equal("123", jsonTestCase.JsonTestFiles.Find(f => f.TestFile == "Input").Json);
                Assert.Equal("[\"J\",\"K\",\"L\"]", jsonTestCase.JsonTestFiles.Find(f => f.TestFile == "Expected").Json);
            }
        }


        [Fact]
        public void ToObjectPerson() {
            Person person = jcase.GetObject<Person>("Expected");
            Assert.Equal("Bob", person?.FirstName);
        }

        [Fact]
        public void ToObjectPersonNull() {
            Person person = jcase.GetObject<Person>("NullRec");
            Assert.Null(person);
        }

        [Fact]
        public void ToObjectInt() {
            int value = jcase.GetObject<int>("Input");
            Assert.Equal(123, value);
        }


        [Fact]
        public void ToObjectBadCast() {
            Assert.Throws<System.ArgumentException>(() => jcase.GetObject<DateTime>("Input"));
        }


    }
}
