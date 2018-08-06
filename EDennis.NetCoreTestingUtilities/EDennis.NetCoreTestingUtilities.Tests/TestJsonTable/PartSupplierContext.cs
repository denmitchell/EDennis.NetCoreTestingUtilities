using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetCoreTestingUtilities.Tests.TestJsonTable {

    /// <summary>
    /// This class is the DbContext class for the SQL Server 
    /// FOR JSON query functionality.
    /// </summary>
    public class PartSupplierContext : DbContext{


        public PartSupplierContext() { }

        public PartSupplierContext(DbContextOptions<PartSupplierContext> options) :
            base(options) { }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            if (!optionsBuilder.IsConfigured) {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=Tmp;Trusted_Connection=True;ConnectRetryCount=0");
            }
        }

        public DbSet<Part> Parts {get; set;}
        public DbSet<Supplier> Suppliers{ get; set; }
        public DbSet<PartSupplier> PartSuppliers{ get; set; }
        public DbSet<TestJson> TestJsons { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder) {

            modelBuilder.HasSequence<int>("seqPart").StartsAt(1);
            modelBuilder.HasSequence<int>("seqSupplier").StartsAt(1);

            modelBuilder.Entity<Part>().ToTable("Part")
                .HasKey(p => p.PartId);

            modelBuilder.Entity<Part>().Property(p => p.PartId)
                .HasDefaultValueSql("next value for seqPart");

            modelBuilder.Entity<Supplier>().ToTable("Supplier")
                .HasKey(s => s.SupplierId);

            modelBuilder.Entity<Supplier>().Property(s => s.SupplierId)
                .HasDefaultValueSql("next value for seqSupplier");

            modelBuilder.Entity<PartSupplier>().ToTable("PartSupplier")
                .HasKey(s => new { s.PartId, s.SupplierId });

            modelBuilder.Entity<PartSupplier>()
                .HasOne(ps => ps.Part)
                .WithMany(p => p.PartSuppliers)
                .HasForeignKey(ps => ps.PartId)
                .HasConstraintName("FK_PartSupplier_Part")
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PartSupplier>()
                .HasOne(ps => ps.Supplier)
                .WithMany(s => s.PartSuppliers)
                .HasForeignKey(ps => ps.SupplierId)
                .HasConstraintName("FK_PartSupplier_Supplier")
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<TestJson>().ToTable("TestJson")
                .HasKey(s => new { s.ProjectName, s.ClassName, s.MethodName, s.TestScenario, s.TestCase, s.TestFile });


        }
    }
}
