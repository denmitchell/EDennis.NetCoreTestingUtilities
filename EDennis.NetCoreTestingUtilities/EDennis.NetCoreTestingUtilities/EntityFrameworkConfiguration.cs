using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetCoreTestingUtilities {
    public class EntityFrameworkConfiguration {
        public DatabaseProvider DatabaseProvider { get; set; }
        public string ConnectionString { get; set; }
    }
}
