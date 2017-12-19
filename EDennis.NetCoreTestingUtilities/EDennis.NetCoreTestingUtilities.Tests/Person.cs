using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetCoreTestingUtilities.Tests {

    /// <summary>
    /// This is a test model class
    /// </summary>
    public class Person {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public List<Skill> Skills { get; set; }
    }
}
