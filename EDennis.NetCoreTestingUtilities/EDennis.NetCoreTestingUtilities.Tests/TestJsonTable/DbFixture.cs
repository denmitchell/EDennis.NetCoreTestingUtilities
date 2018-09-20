using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDennis.NetCoreTestingUtilities.Tests.TestJsonTable {
    public class DbFixture : IDisposable {

        public DbFixture() {
            using (var context = new PartSupplierContext()) {

                //context.Database.EnsureDeleted();
                //context.Database.EnsureCreated();

                var range = context.TestJsons.Where(r => r.ProjectName == "SomeProject");
                context.TestJsons.RemoveRange(range);
                context.SaveChanges();

                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "1",
                    TestCase = "A", TestFile = "Input", Json = "[1]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "1",
                    TestCase = "A", TestFile = "Expected", Json = "[2]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "1",
                    TestCase = "B", TestFile = "Input", Json = "[3]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "1",
                    TestCase = "B", TestFile = "Expected", Json = "[4]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "2",
                    TestCase = "A", TestFile = "Input", Json = "[5]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "2",
                    TestCase = "A", TestFile = "Expected", Json = "[6]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "2",
                    TestCase = "B", TestFile = "Input", Json = "[7]"
                });
                context.TestJsons.Add(new TestJson {
                    ProjectName = "SomeProject",
                    ClassName = "SomeClass", MethodName = "SomeMethod", TestScenario = "2",
                    TestCase = "B", TestFile = "Expected", Json = "[8]"
                });


            }
        }

            public void Dispose() {
            // ... clean up test data from the database ...
        }

    }
}
