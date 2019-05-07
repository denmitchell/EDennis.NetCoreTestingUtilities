using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;
using System.Linq;
using System.Collections;
using EDennis.NetCoreTestingUtilities.Extensions;

namespace EDennis.NetCoreTestingUtilities.Tests {
    public class JsonSorterTests {

        private readonly ITestOutputHelper _output;

        public JsonSorterTests(ITestOutputHelper output) {
            _output = output;
        }

        internal class Person {
            public int Id { get; set; }
            public string FirstName { get; set; }
        }


        [Fact]
        public void SortEnumerable() {
            IEnumerable<Person> persons = GetPersons();
            var sorted = persons.OrderBy(x => x.Id).ThenBy(x => x.FirstName);
            Assert.True(sorted.IsEqual(sorted));
        }

        [Fact]
        public void SortedDictionary() {
            Dictionary<string,Person> persons = new Dictionary<string, Person>();
            persons.Add("Bob", new Person { Id = 1, FirstName = "Bob" });
            persons.Add("Jane", new Person { Id = 2, FirstName = "Jane" });
            var sorted = persons.OrderBy(x => x.Key).ThenBy(x => x.Value.FirstName);
            Assert.True(sorted.IsEqual(sorted));
        }


        [Fact]
        public void SortedList() {
            List<Person> persons = new List<Person>();
            persons.Add(new Person { Id = 1, FirstName = "Bob" });
            persons.Add(new Person { Id = 2, FirstName = "Jane" });
            //var sorted = persons.OrderBy(x => x.Key).ThenBy(x => x.Value.FirstName);
            Assert.True(persons.IsEqual(persons));
        }



        private IEnumerable<Person> GetPersons() {
            for (int i=0; i<1000; i++)
                yield return new Person { Id = i, FirstName = "Bob" + i.ToString() };
        }



        [Theory]
        [InlineData("A")]
        [InlineData("B")]
        [InlineData("C")]
        public void Sort(string testCase) {
            var expected = File.ReadAllText($"Sort\\expected{testCase}.json");
            var input = File.ReadAllText($"Sort\\input{testCase}.json");
            var actual = JsonSorter.Sort(input);

            _output.WriteLine(actual);

            var expectedJToken = JToken.Parse(expected);
            var actualJToken = JToken.Parse(actual);

            Assert.Equal(expectedJToken.ToString(), actualJToken.ToString());

        }


    }
}
