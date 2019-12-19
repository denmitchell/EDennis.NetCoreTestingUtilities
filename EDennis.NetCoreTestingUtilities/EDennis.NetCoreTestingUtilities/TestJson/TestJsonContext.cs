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
            if(!optionsBuilder.IsConfigured)
                switch (DatabaseProvider) {
                    case DatabaseProvider.SqlServer:
                        optionsBuilder.UseSqlServer(ConnectionString);
                        break;
                    case DatabaseProvider.InMemory:
                        optionsBuilder.UseInMemoryDatabase(ConnectionString);
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


        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<TestJson>()
                .HasKey(e => new { e.ProjectName, e.ClassName, e.MethodName, e.TestScenario, e.TestCase, e.TestFile });
        }        

    }
}
