



using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

namespace EDennis.NetCoreTestingUtilities {


    /// <summary>
    /// Builds an in-memory list of records from a TestJson table and
    /// allows Xunit tests decorated with [TestJson(...)] to pull a test
    /// case from the in-memory list. 
    /// The class requires a TestJsonConfig.json file, which
    /// should be placed in the top-level directory of the test project,
    /// unless the full path to the config file is provided in the attribute
    /// </summary>
    public partial class TestJsonAttribute : DataAttribute {

        public static Dictionary<string, List<JsonTestCase>> TestCases { get; set; }
            = new Dictionary<string, List<JsonTestCase>>();

        private readonly TestJsonConfig _config;
        private readonly string _configKey;


        public TestJsonAttribute(
                string projectName,
                string className,
                string methodName,
                string testScenario,
                string testCase,
                DatabaseProvider databaseProvider,
                string connectionString
        ) {

            _config = new TestJsonConfig {
                DatabaseName = "(from connection string)",
                ServerName = "(from connection string)",
                ConnectionString = connectionString,
                TestJsonSchema = "",
                TestJsonTable = "TestJson",
                ProjectName = projectName,
                ClassName = className,
                MethodName = methodName,
                TestScenario = testScenario,
                TestCase = testCase
            };


            _configKey = new TestJsonConfig {
                ConnectionString = _config.ConnectionString,
                TestJsonSchema = _config.TestJsonSchema,
                TestJsonTable = _config.TestJsonTable,
                ProjectName = _config.ProjectName
            }.ToString();


            if (!TestCases.ContainsKey(_configKey)) {
                if(databaseProvider == DatabaseProvider.Excel)
                    TestCases.Add(_configKey, JsonTestCase.GetTestCasesForProjectExcel(
                        _config.ConnectionString, projectName));
                else
                    TestCases.Add(_configKey, JsonTestCase.GetTestCasesForProject(
                        databaseProvider, connectionString, projectName));

            }

        }


        public TestJsonAttribute(
                string databaseName,
                string projectName,
                string className,
                string methodName,
                string testScenario,
                string testCase,
                string serverName = "(LocalDb)\\MSSQLLocalDb",
                string testJsonSchema = "_",
                string testJsonTable = "TestJson"
            ) {

            _config = new TestJsonConfig {
                DatabaseName = databaseName,
                ServerName = serverName,
                ConnectionString = $"Server={serverName};Database={databaseName};Trusted_Connection=True;MultipleActiveResultSets=true",
                TestJsonSchema = testJsonSchema,
                TestJsonTable = testJsonTable,
                ProjectName = projectName,
                ClassName = className,
                MethodName = methodName,
                TestScenario = testScenario,
                TestCase = testCase
            };


            _configKey = new TestJsonConfig {
                ConnectionString = _config.ConnectionString,
                TestJsonSchema = _config.TestJsonSchema,
                TestJsonTable = _config.TestJsonTable,
                ProjectName = _config.ProjectName
            }.ToString();


            if (!TestCases.ContainsKey(_configKey))
                TestCases.Add(_configKey, JsonTestCase.GetTestCasesForProject(
                    _config.ConnectionString,
                    _config.TestJsonSchema,
                    _config.TestJsonTable,
                    _config.ProjectName));

        }


        public override IEnumerable<object[]> GetData(MethodInfo methodInfo) =>
                JsonTestCase.GetDataForXUnit(TestCases[_configKey], _config);

    }

}



