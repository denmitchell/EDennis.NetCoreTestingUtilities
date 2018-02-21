using EDennis.NetCoreTestingUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace NutsAndBolts.Tests {
    public class PartRepoTests {

        private string[] ignoreSupplierToParts = new string[] { "PartSuppliers[*]\\Part", "PartSuppliers[*]\\Supplier\\PartSuppliers[*]" };
        private readonly ITestOutputHelper output;

        public PartRepoTests(ITestOutputHelper output) {
            this.output = output;
        }

        [Fact]
        public void PersonRepoGetById() {
            var context = TestContext.GetContext();
            var repo = new PartRepo(context);

            var part = repo.GetById(1);

            output.WriteLine(part.ToJsonString(ignoreSupplierToParts));

            Assert.True(part.IsEqual(context.Parts[0], ignoreSupplierToParts));

            Assert.Equal(1,part.PartId);
            Assert.Equal("Nut", part.PartName);
            Assert.Equal("Brass", part.Material);

        }


    }
}
