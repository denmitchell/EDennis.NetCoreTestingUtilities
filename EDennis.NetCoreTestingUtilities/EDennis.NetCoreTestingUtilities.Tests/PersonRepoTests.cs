using EDennis.NetCoreTestingUtilities.Extensions;
using System.Collections.Generic;
using Xunit;

namespace EDennis.NetCoreTestingUtilities.Tests {
    public class PersonRepoTests {
        [Theory]
        [InlineData("01")]
        public void GetPersons(string file) {
            var persons = new List<Person>().FromJsonPath(@"PersonRepo\GetPersons\01.json\persons");
            JsonAssert.ExecuteTest(new PersonRepo(persons), file);
        }
    }
}
