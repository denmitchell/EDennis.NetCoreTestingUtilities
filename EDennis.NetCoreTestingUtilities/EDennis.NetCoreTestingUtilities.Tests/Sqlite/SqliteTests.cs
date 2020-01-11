using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.NetCoreTestingUtilities.Tests.Sqlite {

    public class SqliteTests {

        private readonly ITestOutputHelper _output;
        public SqliteTests(ITestOutputHelper output) {
            _output = output;
        }

        internal class TestJsonA : TestJsonAttribute {
            public TestJsonA(string methodName, string testScenario, string testCase)
                : base("Some Project", "Some Class",
                      methodName, testScenario, testCase, DatabaseProvider.Sqlite, "Data Source=Sqlite\\TestJson.db") {
            }
        }

        private readonly Dictionary<string,int> _input 
            = new Dictionary<string, int> { 
                { "A", 1 }, 
                { "B", 2 } 
            };

        private readonly Dictionary<string, Person> _expected
            = new Dictionary<string, Person> { 
                { "A", new Person { FirstName = "Moe", LastName = "Stooge" } }, 
                { "B", new Person { FirstName = "Larry", LastName = "Stooge" } } 
            };


        [Theory]
        [TestJsonA("Some Method", "", "A")]
        [TestJsonA("Some Method", "", "B")]
        public void Sqlite(string t, JsonTestCase jsonTestCase) {
            _output.WriteLine($"Test case: {t}");

            var input = jsonTestCase.GetObject<int>("Input", _output);
            var missingParam = jsonTestCase.GetObjectOrDefault<string>("Optional", _output);
            var expected = jsonTestCase.GetObject<Person>("Expected", _output);

            Assert.Equal(_input[jsonTestCase.TestCase], input);
            Assert.Equal(_expected[jsonTestCase.TestCase].FirstName, expected.FirstName);

            Assert.Null(missingParam);
        }

    }
}
