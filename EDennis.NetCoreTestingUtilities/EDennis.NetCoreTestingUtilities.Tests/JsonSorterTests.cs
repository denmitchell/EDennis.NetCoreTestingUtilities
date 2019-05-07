using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.NetCoreTestingUtilities.Tests {
    public class JsonSorterTests {

        private readonly ITestOutputHelper _output;

        public JsonSorterTests(ITestOutputHelper output) {
            _output = output;
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
