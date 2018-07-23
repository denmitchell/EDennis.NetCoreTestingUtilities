using EDennis.NetCoreTestingUtilities.Tests.TestJsonTable;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.NetCoreTestingUtilities.Tests {
    public class JsonAttributeTests {


        private readonly ITestOutputHelper _output;

        public JsonAttributeTests(ITestOutputHelper output) {
            _output = output;

            /*
            var options = new DbContextOptionsBuilder<PartSupplierContext>()
                .UseInMemoryDatabase(databaseName: "FromTestJsonTable")
                .Options;
            using (var context = new PartSupplierContext()) {

                context.ResetValueGenerators();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                PartSupplierFactory.PopulateContext(context);

                context.TestJsons.Add(new TestJson {
                    Project = "P1", Class = "C1", Method = "M1", FileName = "123", Json = "J1"
                });

                context.TestJsons.Add(new TestJson {
                    Project = "P1", Class = "C1", Method = "M1B", FileName = "456", Json = "J2"
                });

                context.TestJsons.Add(new TestJson {
                    Project = "P1", Class = "C2", Method = "M1", FileName = "789", Json = "J3"
                });

                context.SaveChanges();
                
            }*/
        }

        [Theory]
        [TestJson("Server=(localdb)\\mssqllocaldb;Database=Tmp;Trusted_Connection=True;",
            "_maintenance","TestJson","P1","C1","Method LIKE 'M1%'")]
        public void TestTheAttribute(string projectName, string className, string methodName, string fileName, string json) {
            _output.WriteLine($"Project: {projectName}, Class: {className}, Method: {methodName}, FileName: {fileName}, Json: {json}");
        }



    }
}
