using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace EDennis.NetCoreTestingUtilities.Tests {
    public class TestJsonTests {

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

public TestJsonTests() {

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
            Assert.Throws<System.ArgumentException>(()=>jcase.GetObject<DateTime>("Input"));
        }


    }
}
