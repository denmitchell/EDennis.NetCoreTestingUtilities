using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDennis.NetCoreTestingUtilities.Tests.InMemory{
    public class TestJsonFixture : IDisposable {

        public TestJsonFixture() {
            using var context = new TestJsonContext { DatabaseProvider = DatabaseProvider.InMemory, ConnectionString = "Some Database" };
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var range = context.TestJsonRecs.Where(r => r.ProjectName == "Some Project");
            context.TestJsonRecs.RemoveRange(range);
            context.SaveChanges();

            context.TestJsonRecs.Add(new TestJson {
                ProjectName = "Some Project",
                ClassName = "Some Class", MethodName = "Some Method", TestScenario = "",
                TestCase = "A", TestFile = "Input", Json = "1"
            });
            context.TestJsonRecs.Add(new TestJson {
                ProjectName = "Some Project",
                ClassName = "Some Class", MethodName = "Some Method", TestScenario = "",
                TestCase = "A", TestFile = "Expected", Json = "{\"FirstName\":\"Moe\",\"LastName\":\"Stooge\"\n}"
            });
            context.TestJsonRecs.Add(new TestJson {
                ProjectName = "Some Project",
                ClassName = "Some Class", MethodName = "Some Method", TestScenario = "",
                TestCase = "B", TestFile = "Input", Json = "2"
            });
            context.TestJsonRecs.Add(new TestJson {
                ProjectName = "Some Project",
                ClassName = "Some Class", MethodName = "Some Method", TestScenario = "",
                TestCase = "B", TestFile = "Expected", Json = "{\"FirstName\":\"Larry\",\"LastName\":\"Stooge\"\n}"
            });

            context.SaveChanges();
        }

            public void Dispose() {
            // ... clean up test data from the database ...
        }

    }
}
