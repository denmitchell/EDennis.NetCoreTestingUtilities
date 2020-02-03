using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xunit;
using Xunit.Abstractions;
using EDennis.NetCoreTestingUtilities.Extensions;

namespace EDennis.NetCoreTestingUtilities.Tests {
    public class JsonPathalizerTests {

        private readonly ITestOutputHelper _output;


        public JsonPathalizerTests(ITestOutputHelper output) {
            _output = output;
        }

        [Theory]
        [InlineData(1, 2, false)]
        [InlineData(1, 3, true)]
        public void TestPathalizer(int file1, int file2, bool expectedResult) {
            var json1 = File.ReadAllText($"JsonPathalizer\\persons{file1}.json");
            var json2 = File.ReadAllText($"JsonPathalizer\\persons{file2}.json");
            var actualResult = ObjectExtensions.IsEqualAndWrite(json1, json2, _output, null, true, true);
            Assert.Equal(expectedResult, actualResult);
        }




    }
}
