using EDennis.NetCoreTestingUtilities.Extensions;
using System;
using System.Collections.Generic;
using Xunit;

namespace EDennis.NetCoreTestingUtilities.Tests {
    public class PersonRepoTests { 

        [Theory]
        [InlineData("02", 1, true)]
        [InlineData("02", 2, false)]
        public void MaxDepthException1(string testCase, int maxDepth, bool shouldThrowException) {
            var persons = new List<Person>().FromJsonPath($"PersonRepo\\MaxDepthException\\{testCase}.json");
            if(shouldThrowException)
                Assert.Throws<ArgumentException>(() => persons.ToJsonString(maxDepth));
        }

        [Theory]
        [InlineData("03", 1, false)]
        public void MaxDepthException2(string testCase, int maxDepth, bool shouldThrowException) {
            var person = new Person().FromJsonPath($"PersonRepo\\MaxDepthException\\{testCase}.json");
            if (shouldThrowException)
                Assert.Throws<ArgumentException>(() => person.ToJsonString(maxDepth));
        }

    }
}
