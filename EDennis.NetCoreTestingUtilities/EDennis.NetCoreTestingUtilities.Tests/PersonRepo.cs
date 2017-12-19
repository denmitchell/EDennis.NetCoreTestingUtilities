using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDennis.NetCoreTestingUtilities.Tests {

    /// <summary>
    /// This is a test repository
    /// </summary>
    public class PersonRepo {

        private List<Person> persons;

        public PersonRepo() {
            persons = new List<Person>();
        }

        public PersonRepo(List<Person> persons) {
            this.persons = persons;
        }

        public List<Person> GetPersons(int minProjectManagementScore) {
            return persons
                .Where(p => p.Skills
                    .Any(s => s.Category == "Project Management" 
                        && s.Score >= minProjectManagementScore))
                .ToList();
        }

    }
}
