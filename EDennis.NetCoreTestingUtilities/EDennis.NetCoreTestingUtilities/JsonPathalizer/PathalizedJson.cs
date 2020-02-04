using System.Collections.Generic;

namespace EDennis.NetCoreTestingUtilities {
    public class PathalizedJson : SortedDictionary<string,object> {
        public int MaxChildren { get; set; } = 0;
        public string IndexFormat { get; set; }

    }


}
