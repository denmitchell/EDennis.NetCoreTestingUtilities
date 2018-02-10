using EDennis.NetCoreTestingUtilities.Extensions;
using System.Collections.Generic;
using Xunit;

namespace EDennis.NetCoreTestingUtilities.Tests {
    public class PersonRepoTests {
        [Theory]
        [Folder("PersonRepo\\GetPersons", "json")]
        public void _01(string f) {
            var persons = new List<Person>().FromJsonPath($"{f}\\persons");
            JsonAssert.ExecuteTest(new PersonRepo(persons), "GetPersons", f);
        }

    }
}
