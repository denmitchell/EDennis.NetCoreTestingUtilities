using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDennis.NetCoreTestingUtilities.Tests.TestJsonTable {
    public class PartSupplierFactory {

        public static void PopulateContext(PartSupplierContext context) {

            context.Parts.Add(new Part { PartId = 1, PartName = "Nut", Material = "Brass", Weight = 12.50, City = "London" });
            context.Parts.Add(new Part { PartId = 2, PartName = "Bolt", Material = "Brass", Weight = 27.00, City = "Paris" });
            context.Parts.Add(new Part { PartId = 3, PartName = "Screw", Material = "Stainless", Weight = 7.25, City = "Rome" });
            context.Parts.Add(new Part { PartId = 4, PartName = "Screw", Material = "Carbon", Weight = 6.75, City = "London" });
            context.Parts.Add(new Part { PartId = 5, PartName = "Washer", Material = "Chrome", Weight = 5.10, City = "Paris" });
            context.Parts.Add(new Part { PartId = 6, PartName = "Bolt", Material = "Stainless", Weight = 25.50, City = "London" });

            context.Suppliers.Add(new Supplier { SupplierId = 1, SupplierName = "Smith", Since = DateTime.Parse("2000-05-05"), City = "London" });
            context.Suppliers.Add(new Supplier { SupplierId = 2, SupplierName = "Jones", Since = DateTime.Parse("2012-10-12"), City = "Paris" });
            context.Suppliers.Add(new Supplier { SupplierId = 3, SupplierName = "Blake", Since = DateTime.Parse("1995-07-23"), City = "Paris" });
            context.Suppliers.Add(new Supplier { SupplierId = 4, SupplierName = "Clark", Since = DateTime.Parse("2006-11-03"), City = "London" });
            context.Suppliers.Add(new Supplier { SupplierId = 5, SupplierName = "Adams", Since = DateTime.Parse("1985-12-23"), City = "Athens" });

            context.SaveChanges();

            context.PartSuppliers.Add(new PartSupplier { SupplierId = 1, PartId = 1, Quantity = 300,
                Part = context.Parts.Find(1), Supplier = context.Suppliers.Find(1)
            });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 2, PartId = 1, Quantity = 300,
                Part = context.Parts.Find(1), Supplier = context.Suppliers.Find(2)
            });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 1, PartId = 2, Quantity = 200,
                Part = context.Parts.Find(2), Supplier = context.Suppliers.Find(1)
            });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 2, PartId = 2, Quantity = 400,
                Part = context.Parts.Find(2), Supplier = context.Suppliers.Find(2)
            });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 3, PartId = 2, Quantity = 200,
                Part = context.Parts.Find(2), Supplier = context.Suppliers.Find(3)
            });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 4, PartId = 2, Quantity = 200,
                Part = context.Parts.Find(2), Supplier = context.Suppliers.Find(4)
            });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 1, PartId = 3, Quantity = 400,
                Part = context.Parts.Find(3), Supplier = context.Suppliers.Find(1)
            });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 1, PartId = 4, Quantity = 200,
                Part = context.Parts.Find(4), Supplier = context.Suppliers.Find(1)
            });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 4, PartId = 4, Quantity = 300,
                Part = context.Parts.Find(4), Supplier = context.Suppliers.Find(4)
            });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 1, PartId = 5, Quantity = 100,
                Part = context.Parts.Find(5), Supplier = context.Suppliers.Find(1)
            });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 4, PartId = 5, Quantity = 400,
                Part = context.Parts.Find(5), Supplier = context.Suppliers.Find(4)
            });
            context.PartSuppliers.Add(new PartSupplier { SupplierId = 1, PartId = 6, Quantity = 100,
                Part = context.Parts.Find(6), Supplier = context.Suppliers.Find(1)
            });

            var PartSuppliers_PartIds = context.PartSuppliers.Select(p => p.PartId).ToList();
            var Parts_PartIds = context.Parts.Select(p => p.PartId).ToList();


            context.SaveChanges();

        }

    }
}
