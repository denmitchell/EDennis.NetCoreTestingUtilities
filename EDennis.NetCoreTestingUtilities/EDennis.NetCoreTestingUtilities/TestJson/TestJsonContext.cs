using Microsoft.EntityFrameworkCore;
using FileContextCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetCoreTestingUtilities {
    public class TestJsonContext : DbContext {

        public DbSet<TestJson> TestJsonRecs { get; set; }
        public DatabaseProvider DatabaseProvider { get; set; }
        public string ConnectionString { get; set; }

        public TestJsonContext() {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            switch (DatabaseProvider) {
                case DatabaseProvider.SqlServer:
                    optionsBuilder.UseSqlServer(ConnectionString);
                    break;
                case DatabaseProvider.InMemory:
                    optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    break;
                case DatabaseProvider.Sqlite:
                    optionsBuilder.UseSqlite(ConnectionString);
                    break;
                case DatabaseProvider.FileContextCore:
                    optionsBuilder.UseFileContextDatabaseConnectionString(ConnectionString);
                    break;
                default:
                    optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    break;
            }
        }

    }
}
