using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetCoreTestingUtilities.Tests.TestJsonTable {

    /// <summary>
    /// This class is the DbContext class for the SQL Server 
    /// FOR JSON query functionality.
    /// </summary>
    public class TestJsonContext : DbContext{


        public TestJsonContext() { }

        public TestJsonContext(DbContextOptions<TestJsonContext> options) :
            base(options) { }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=TestJson;Trusted_Connection=True;ConnectRetryCount=0");
            }
        }

        public DbSet<TestJson> TestJsons { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            modelBuilder.Entity<TestJson>()
                .ToTable("TestJson","_")
                .HasKey(s => new { s.ProjectName, s.ClassName, s.MethodName, s.TestScenario, s.TestCase, s.TestFile });


        }
    }
}
