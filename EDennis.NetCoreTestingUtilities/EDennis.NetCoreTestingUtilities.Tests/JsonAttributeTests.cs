using EDennis.NetCoreTestingUtilities.Tests.TestJsonTable;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace EDennis.NetCoreTestingUtilities.Tests {

    [Collection("Database collection")]
    public class JsonAttributeTests {


        ITestOutputHelper _output;
        DbFixture _fixture;

        public JsonAttributeTests(ITestOutputHelper output, DbFixture fixture) {
            _output = output;
            _fixture = fixture;
        }

        internal class PartSupplierTestJsonAttribute : TestJsonAttribute {
            public PartSupplierTestJsonAttribute(string methodName)
                : base("Server=(localdb)\\mssqllocaldb;Database=tmp;Trusted_Connection=True;",
                      "dbo", "TestJson", "SomeProject", "SomeClass", methodName) {
                var fixture = new DbFixture();
            }
        }


        [Theory]
        [PartSupplierTestJson("SomeMethod")]
        public void TestTheAttribute(JsonTestCase testCase) {

            foreach (var file in testCase.JsonTestFiles)
                _output.WriteLine(file.TestFile);
        }



    }
}
