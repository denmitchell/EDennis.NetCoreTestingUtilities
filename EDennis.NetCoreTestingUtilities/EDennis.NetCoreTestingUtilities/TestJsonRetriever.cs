using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace EDennis.NetCoreTestingUtilities {

    /// <summary>
    /// This class allows developers to retrieve
    /// records from a TestJson table.  The method
    /// allows test records to be used as mock
    /// responses.
    /// </summary>
    public class TestJsonRetriever {

        protected internal class TestFileJson {
            public string TestCase { get; set; }
            public string TestFile { get; set; }
            public string Json { get; set; }
        }

        /// <summary>
        /// Configuration data for retrieving records 
        /// </summary>
        public TestJsonConfig TestJsonConfig { get; set; }

        
        /// <summary>
        /// Records retrieved during initialization
        /// </summary>
        protected List<TestFileJson> TestJsonRecords { get; set; }
            = new List<TestFileJson>();

        /// <summary>
        /// Constructs a new TestJsonRetriever with
        /// various optional parameters.  Parameters
        /// are also read in from a TestJsonConfig.json
        /// file, when present.  All of the parameters
        /// must be present in eithe the configuration 
        /// file or specified in this constructor.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="testJsonSchema">The database schema hold the TestJson table</param>
        /// <param name="testJsonTable">The name of the TestJson table</param>
        /// <param name="projectName">The name of the project under test (part of primary key for the test record)</param>
        /// <param name="className">The name of the class under test (part of primary key for the test record)</param>
        /// <param name="methodName">The name of the method under test (part of primary key for the test record)</param>
        /// <param name="testScenario">The name of the test scenario under test (part of primary key for the test record)</param>
        public TestJsonRetriever(string connectionString = null,
            string testJsonSchema = null,
            string testJsonTable = null, 
            string projectName = null,
            string className = null,
            string methodName = null, 
            string testScenario = null) {

            var json = File.ReadAllText("TestJsonConfig.json");
            TestJsonConfig = JToken.Parse(json).ToObject<TestJsonConfig>();
            if (connectionString != null)
                TestJsonConfig.ConnectionString = connectionString;
            if (testJsonSchema != null)
                TestJsonConfig.TestJsonSchema = testJsonSchema;
            if (testJsonTable != null)
                TestJsonConfig.TestJsonTable = testJsonTable;
            if (projectName != null)
                TestJsonConfig.ProjectName = projectName;
            if (className != null)
                TestJsonConfig.ClassName = className;
            if (methodName != null)
                TestJsonConfig.MethodName = methodName;
            if (testScenario != null)
                TestJsonConfig.TestScenario = testScenario;

            Initialize();
        }

        /// <summary>
        /// Populates the TestJsonRecords with one or
        /// more test cases matching the provided 
        /// parameters.
        /// </summary>
        private void Initialize() {

            if (TestJsonConfig.ConnectionString == null)
                throw new ArgumentException("ConnectionString is not optional for TestJsonRetriever");
            if (TestJsonConfig.TestJsonSchema == null)
                throw new ArgumentException("TestJsonSchema is not optional for TestJsonRetriever");
            if (TestJsonConfig.TestJsonTable == null)
                throw new ArgumentException("TestJsonTable is not optional for TestJsonRetriever");
            if (TestJsonConfig.ProjectName == null)
                throw new ArgumentException("ProjectName is not optional for TestJsonRetriever");
            if (TestJsonConfig.ClassName == null)
                throw new ArgumentException("ClassName is not optional for TestJsonRetriever");
            if (TestJsonConfig.MethodName == null)
                throw new ArgumentException("MethodName is not optional for TestJsonRetriever");
            if (TestJsonConfig.TestScenario == null)
                throw new ArgumentException("TestScenario is not optional for TestJsonRetriever");

            using (var cxn = new SqlConnection(TestJsonConfig.ConnectionString)) {
                var sql = $"select TestCase, TestFile, Json from [{TestJsonConfig.TestJsonSchema}].[{TestJsonConfig.TestJsonTable}] " +
                    $"where ProjectName = @ProjectName and " +
                    $"ClassName = @ClassName and " +
                    $"MethodName = @MethodName and " +
                    $"TestScenario = @TestScenario";
                using (var cmd = new SqlCommand(sql, cxn)) {

                    cmd.Parameters.AddWithValue("@ProjectName", TestJsonConfig.ProjectName);
                    cmd.Parameters.AddWithValue("@ClassName", TestJsonConfig.ClassName);
                    cmd.Parameters.AddWithValue("@MethodName", TestJsonConfig.MethodName);
                    cmd.Parameters.AddWithValue("@TestScenario", TestJsonConfig.TestScenario);

                    cxn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows) {
                        while (reader.Read()) {
                            var rec = new TestFileJson {
                                TestCase = reader.GetString(0),
                                TestFile = reader.GetString(1),
                                Json = reader.GetString(2)
                            };
                            TestJsonRecords.Add(rec);
                        }
                    } else {
                        Console.WriteLine("No rows found.");
                    }
                    reader.Close();

                }
            }
        }



        /// <summary>
        /// Retrieves an object stored in the Json column
        /// of the TestJson table.  The target record is
        /// identified by TestJsonConfig properties, as 
        /// well as the two provided parameters.
        /// </summary>
        /// <param name="targetTestFile">The name of the target test file to retrieve (often 'Expected')</param>
        /// <param name="testFileParams">A param array of {key}={value} pairs (e.g., ID=1)</param>
        /// <returns></returns>
        public T GetObject<T>(string targetTestFile,
                params string[] testFileParams) {

            var testCases = TestJsonRecords.Select(x => x.TestCase).Distinct();

            var criteria = testFileParams
                    .Select(x => {
                        var parm = x.Split('=');
                        return
                            JToken.FromObject(
                                new TestFileJson {
                                    TestCase = "",
                                    TestFile = parm[0],
                                    Json = JToken.Parse(parm[1]).ToString(Newtonsoft.Json.Formatting.None)
                                }).ToString();
                    });

            string json = null;

            foreach (var testCase in testCases) {
                var source = TestJsonRecords.Where(x => x.TestCase == testCase)
                    .Select(x =>
                        JToken.FromObject(
                            new TestFileJson{
                                TestCase = "",
                                TestFile = x.TestFile,
                                Json = JToken.Parse(x.Json).ToString(Newtonsoft.Json.Formatting.None)
                            }).ToString());
                if (!criteria.Except(source).Any())
                    json = TestJsonRecords.Where(x=>x.TestCase == testCase 
                        && x.TestFile == targetTestFile).FirstOrDefault().Json;
            }
            if(json == null)
                throw new ArgumentException(
                    $"Cannot find '{targetTestFile}' for {JToken.FromObject(testFileParams).ToString()} " +
                    $"where Project: '{TestJsonConfig.ProjectName}', Class: '{TestJsonConfig.ClassName}', " +
                    $"Method: '{TestJsonConfig.MethodName}', TestScenario: '{TestJsonConfig.TestScenario}'");
            else {
                return GetObject<T>(json);
            }
        }



        private T GetObject<T>(string json) {
            if (json == null && default(T) == null)
                return default(T);

            try {
                if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(TimeSpan) || typeof(T) == typeof(DateTimeOffset) || typeof(T) == typeof(string) || typeof(T) == typeof(Guid))
                    json = "\"" + json + "\"";

                JToken jtoken = JToken.Parse(json);
                T objNew = jtoken.ToObject<T>();
                return objNew;
            } catch (Exception ex) when (ex is ArgumentException || ex is FormatException) {
                throw new ArgumentException(
                    $"Cannot cast '{json}' to {default(T).GetType().Name} " +
                    $"for Project: '{TestJsonConfig.ProjectName}', Class: '{TestJsonConfig.ClassName}', " +
                    $"Method: '{TestJsonConfig.MethodName}', TestScenario: '{TestJsonConfig.TestScenario}' ");
            }
        }


    }

}
