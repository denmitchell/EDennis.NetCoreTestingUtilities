using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace EDennis.NetCoreTestingUtilities.Tests.TestJsonTable
{
    [CollectionDefinition("Database collection")]
    public class DbCollection : ICollectionFixture<DbFixture>
    {
    }
}
