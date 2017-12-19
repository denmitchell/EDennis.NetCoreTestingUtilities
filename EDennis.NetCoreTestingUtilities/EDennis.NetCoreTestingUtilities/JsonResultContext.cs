using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EDennis.NetCoreTestingUtilities.Json {

    /// <summary>
    /// This class is the DbContext class for the SQL Server 
    /// FOR JSON query functionality.
    /// </summary>
    public class JsonResultContext : DbContext{

        public JsonResultContext(string connectionString) :
            base(new DbContextOptionsBuilder()
                .UseSqlServer(connectionString)
                .Options) { }

        public JsonResultContext() :
            base(new DbContextOptionsBuilder()
                .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=tempdb;Trusted_Connection=True;")
                .Options) {}

        public DbSet<JsonResult> JsonResults {get; set;}
    }
}
