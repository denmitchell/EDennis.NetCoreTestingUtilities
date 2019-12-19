using Microsoft.EntityFrameworkCore;
using FileContextCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using OfficeOpenXml;

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
                    case DatabaseProvider.Excel:
                        optionsBuilder.UseInMemoryDatabase(ConnectionString);
                        //optionsBuilder.UseFileContextDatabase(serializer: "excel", databaseName: "TestJson", location: ConnectionString );
                        //optionsBuilder.UseFileContextDatabase(serializer: "csv", databaseName: "TestJson", location: ConnectionString);
                        //optionsBuilder.UseFileContextDatabaseConnectionString(ConnectionString);
                        break;
                    default:
                        optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
                        break;
                }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<TestJson>()
                .HasKey(e => new { e.ProjectName, e.ClassName, e.MethodName, e.TestScenario, e.TestCase, e.TestFile });

            if(DatabaseProvider == DatabaseProvider.Excel)
                modelBuilder.Entity<TestJson>()
                    .HasData(LoadDataFromExcel(ConnectionString));
        }

        public const int PROJECT_NAME_COL = 1;
        public const int CLASS_NAME_COL = 2;
        public const int METHOD_NAME_COL = 3;
        public const int TEST_SCENARIO_COL = 4;
        public const int TEST_CASE_COL = 5;
        public const int TEST_FILE_COL = 6;
        public const int JSON_COL = 7;


        public IEnumerable<object> LoadDataFromExcel(string filePath) {

            var existingFile = new FileInfo(filePath);
            using ExcelPackage package = new ExcelPackage(existingFile);
            // get the first worksheet in the workbook
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];

            int row = 1;
            if (worksheet.Cells[1, PROJECT_NAME_COL].Text == "ProjectName")
                row++;

            var recs = new List<TestJson>();
            while (true) {
                if (string.IsNullOrEmpty(worksheet.Cells[row, PROJECT_NAME_COL].Text))
                    break;
                var testJson = new TestJson {
                    ProjectName = worksheet.Cells[row, PROJECT_NAME_COL].Text,
                    ClassName = worksheet.Cells[row, CLASS_NAME_COL].Text,
                    MethodName = worksheet.Cells[row, METHOD_NAME_COL].Text,
                    TestScenario = worksheet.Cells[row, TEST_SCENARIO_COL].Text,
                    TestCase = worksheet.Cells[row, TEST_CASE_COL].Text,
                    TestFile = worksheet.Cells[row, TEST_FILE_COL].Text,
                    Json = worksheet.Cells[row, JSON_COL].Text,
                };
                recs.Add(testJson);
                row++;
            }
            return recs;

        }


    }
}
