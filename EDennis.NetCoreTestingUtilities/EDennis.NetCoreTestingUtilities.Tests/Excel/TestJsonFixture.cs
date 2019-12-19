using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDennis.NetCoreTestingUtilities.Tests.Excel{
    public class TestJsonFixture : IDisposable {

        public TestJsonContext TestJsonContext { get; private set; }

        public TestJsonFixture() {
            TestJsonContext = new TestJsonContext { DatabaseProvider = DatabaseProvider.Excel, ConnectionString = "Excel\\TestJson.xlsx" };
            TestJsonContext.Database.EnsureCreated();
        }

        private void CreateDatabase() {
            TestJsonContext.Database.EnsureDeleted();

            var range = TestJsonContext.TestJsonRecs.Where(r => r.ProjectName == "Some Project");
            TestJsonContext.TestJsonRecs.RemoveRange(range);
            TestJsonContext.SaveChanges();

            TestJsonContext.TestJsonRecs.Add(new TestJson {
                ProjectName = "Some Project",
                ClassName = "Some Class", MethodName = "Some Method", TestScenario = "",
                TestCase = "A", TestFile = "Input", Json = "1"
            });
            TestJsonContext.TestJsonRecs.Add(new TestJson {
                ProjectName = "Some Project",
                ClassName = "Some Class", MethodName = "Some Method", TestScenario = "",
                TestCase = "A", TestFile = "Expected", Json = "{\"FirstName\":\"Moe\",\"LastName\":\"Stooge\"\n}"
            });
            TestJsonContext.TestJsonRecs.Add(new TestJson {
                ProjectName = "Some Project",
                ClassName = "Some Class", MethodName = "Some Method", TestScenario = "",
                TestCase = "B", TestFile = "Input", Json = "2"
            });
            TestJsonContext.TestJsonRecs.Add(new TestJson {
                ProjectName = "Some Project",
                ClassName = "Some Class", MethodName = "Some Method", TestScenario = "",
                TestCase = "B", TestFile = "Expected", Json = "{\"FirstName\":\"Larry\",\"LastName\":\"Stooge\"\n}"
            });

            TestJsonContext.SaveChanges();

        }

        public void Dispose() {
            TestJsonContext.Database.EnsureDeleted();
        }

    }
}
