

using Dapper;
using EDennis.NetCoreTestingUtilities.Extensions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Xunit.Abstractions;

namespace EDennis.NetCoreTestingUtilities {

    /// <summary>
    /// Flat TestJson record, which reflects the 
    /// table in the database.
    /// </summary>
    public class TestJson {
        public string ProjectName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public string TestScenario { get; set; }
        public string TestCase { get; set; }
        public string TestFile { get; set; }
        public string Json { get; set; }

    }

    /// <summary>
    /// Provides TestJson records in a hierarchical result
    /// grouped by a particular TestCase
    /// </summary>
    public class JsonTestCase : IXunitSerializable {
        public string ProjectName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public string TestScenario { get; set; }
        public string TestCase { get; set; }

        public List<JsonTestFile> JsonTestFiles { get; set; }

        public void Deserialize(IXunitSerializationInfo info) {
            var obj = JsonConvert.DeserializeObject<JsonTestCase>(info.GetValue<string>("obj"));
            ProjectName = obj.ProjectName;
            ClassName = obj.ClassName;
            MethodName = obj.MethodName;
            TestScenario = obj.TestScenario;
            TestCase = obj.TestCase;
            JsonTestFiles = obj.JsonTestFiles;
        }

        public void Serialize(IXunitSerializationInfo info) {
            var json = JsonConvert.SerializeObject(this);
            info.AddValue("obj", json);
        }

        public string GetJson(string testFile) {
            var rec = JsonTestFiles.FirstOrDefault(f => f.TestFile == testFile);
            if (rec == null)
                throw new MissingRecordException($"Cannot find testFile '{testFile}' " +
                    $"for Project '{ProjectName}', Class '{ClassName}', " +
                    $"Method '{MethodName}', TestScenario '{TestScenario}', " +
                    $"TestCase '{TestCase}'");
            return rec.Json;
        }


        /// <summary>
        /// Gets the object stored in the Json column of the
        /// TestJson table
        /// </summary>
        /// <typeparam name="T">Type of object stored</typeparam>
        /// <param name="testFile">Name of test file</param>
        /// <returns>object of type T (can be a boxed primitive)</returns>
        public T GetObject<T>(string testFile) {
            var json = GetJson(testFile);
            if (json == null && default(T) == null)
                return default;
            else if (typeof(T) == typeof(string))
                return (dynamic)json;
            try {
                if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(TimeSpan) || typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(string) || typeof(T) == typeof(Guid))
                    json = "\"" + json + "\"";

                T objNew = default;
                try {
                    var reader = new JsonTextReader(new StringReader(json)) {
                        FloatParseHandling = FloatParseHandling.Decimal
                    };
                    JObject jtoken = JObject.Load(reader);
                    objNew = jtoken.ToObject<T>();
                } catch {
                    JToken jtoken = JToken.Parse(json);
                    objNew = jtoken.ToObject<T>();
                }
                return objNew;
            } catch (Exception ex) when (ex is ArgumentException || ex is FormatException) {
                throw new ArgumentException(
                    $"Cannot cast '{json}' to {default(T).GetType().Name} " +
                    $"for Project: '{ProjectName}', Class: '{ClassName}', " +
                    $"Method: '{MethodName}', TestScenario: '{TestScenario}', " +
                    $"TestCase: '{TestCase}', TestFile: '{testFile}' ");
            }
        }


        public static List<JsonTestCase> GetTestCasesForProject(string connectionString, string testJsonSchema, string testJsonTable, string projectName) {
            var sql = 
@$"select ProjectName, ClassName, MethodName, TestScenario, 
    TestCase, TestFile, Json 
    from {testJsonSchema}.{testJsonTable} 
    where ProjectName = @projectName";

            using var cxn = new SqlConnection(connectionString);

            //get data from SQL Server
            var testJson = cxn.Query<TestJson>(sql, new { projectName });

            //add {ANY} files to test case
            testJson = AddAny(testJson);

            //ensure that there is at least one valid test case
            CheckCount(testJson, projectName);

            //group the test files
            var testCases = GroupTestFiles(testJson);

            //return the test cases
            return testCases;

        }


        public static List<JsonTestCase> GetTestCasesForProject(DatabaseProvider databaseProvider, string connectionString, string projectName) {

            IEnumerable<TestJson> testJson = new List<TestJson>();

            //get data from a valid TestJsonContext (extends DbContext)
            using (var ctx = new TestJsonContext {
                DatabaseProvider = databaseProvider,
                ConnectionString = connectionString
            }) {
                testJson = ctx.TestJsonRecs
                    .Where(t => t.ProjectName == projectName)
                    .ToList();
            }


            //add {ANY} files to test case
            testJson = AddAny(testJson);

            //ensure that there is at least one valid test case
            CheckCount(testJson, projectName);

            //group the test files
            var testCases = GroupTestFiles(testJson);

            //return the test cases
            return testCases;

        }

        public static List<JsonTestCase> GetTestCasesForProjectExcel(string filePath, string projectName) {

            using var ctx = new TestJsonContext();

            //load data from Excel
            IEnumerable<TestJson> testJson = ctx.LoadDataFromExcel(filePath).ToList();

            //add {ANY} files to test case
            testJson = AddAny(testJson);

            //ensure that there is at least one valid test case
            CheckCount(testJson, projectName);

            //group the test files
            var testCases = GroupTestFiles(testJson);

            //return the test cases
            return testCases;

        }

        private static void CheckCount(IEnumerable<TestJson> testJson, string projectName) {
            if (testJson.Count() == 0) {
                throw new ArgumentException(
                    $"No TestJson record found for ProjectName: {projectName}.");
            }
        }

        private static IEnumerable<TestJson> AddAny(IEnumerable<TestJson> testJson) {
            var anyValue = TestJsonConfig.ANY_VALUE;

            var any = testJson.Where(
                    t => t.ClassName == anyValue 
                    || t.MethodName == anyValue
                    || t.TestScenario == anyValue
                    || t.TestCase == anyValue)
                .ToList();

            var testJsonAugm = new List<TestJson>(testJson);
            var distinct = testJson
                .Except(any)
                .Select(d => new { d.ProjectName, d.ClassName, d.MethodName, d.TestScenario, d.TestCase })
                .Distinct();

            foreach (var a in any)
                foreach (var t in distinct) {
                    if((a.ClassName == anyValue || a.ClassName == t.ClassName)
                        && (a.MethodName == anyValue || a.MethodName == t.MethodName)
                        && (a.TestScenario == anyValue || a.TestScenario == t.TestScenario)
                        && (a.TestCase == anyValue || a.TestCase == t.TestCase))
                        testJsonAugm.Add(new TestJson {
                            ProjectName = t.ProjectName,
                            ClassName = t.ClassName,
                            MethodName = t.MethodName,
                            TestScenario = t.TestScenario,
                            TestCase = t.TestCase,
                            TestFile = a.TestFile,
                            Json = a.Json
                    });
                }
            return testJsonAugm.Except(any);

        }

        private static List<JsonTestCase> GroupTestFiles(IEnumerable<TestJson> testJson) {
            var testCases = new List<JsonTestCase>();
            var qry = testJson.GroupBy(
                    r => new { r.ProjectName, r.ClassName, r.MethodName, r.TestScenario, r.TestCase },
                    r => new JsonTestFile { TestFile = r.TestFile, Json = r.Json },
                    (key, g) =>
                        new JsonTestCase {
                            ProjectName = key.ProjectName,
                            ClassName = key.ClassName,
                            MethodName = key.MethodName,
                            TestScenario = key.TestScenario,
                            TestCase = key.TestCase,
                            JsonTestFiles = g.ToList()
                        });

            //return all objects
            foreach (var rec in qry)
                testCases.Add(rec);

            return testCases;

        }



        public static IEnumerable<object[]> GetDataForXUnit(List<JsonTestCase> TestCases, TestJsonConfig config) {

            if (TestCases == null) {
                throw new ArgumentException(
                    $"No TestJson records found for ConnectionString: {config.ConnectionString}, TestJsonSchema: {config.TestJsonSchema}, TestJsonTable: {config.TestJsonTable}, ProjectName: {config.ProjectName}.  Check your configuration file for possible errors.");
            }

            var qry = TestCases.Where(
                        t => t.ClassName == config.ClassName && t.MethodName == config.MethodName
                          && t.TestScenario == config.TestScenario && t.TestCase == config.TestCase);
                 
            if (qry == null || qry.Count() == 0) {
                throw new ArgumentException(
                    $"No TestJson record found for ProjectName: {TestCases[0].ProjectName}, ClassName: {config.ClassName}, MethodName: {config.MethodName}, TestScenario: {config.TestScenario}, TestCase {config.TestCase}");
            }

            //return all objects
            foreach (var rec in qry.AsEnumerable())
                yield return new object[] { $"{config.TestScenario}({config.TestCase})", rec };
        }


    }

    /// <summary>
    /// Provides a single test file and JSON
    /// for a test case
    /// </summary>
    public class JsonTestFile {
        public string TestFile { get; set; }
        public string Json { get; set; }

    }



}
