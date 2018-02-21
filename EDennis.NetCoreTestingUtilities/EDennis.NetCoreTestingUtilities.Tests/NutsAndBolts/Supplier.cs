using System;
using System.Collections.Generic;

namespace NutsAndBolts {
    public class Supplier {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public DateTime? Since { get; set; }
        public string City { get; set; }
        public List<PartSupplier> PartSuppliers { get; set; } = new List<PartSupplier>();
    }
}
