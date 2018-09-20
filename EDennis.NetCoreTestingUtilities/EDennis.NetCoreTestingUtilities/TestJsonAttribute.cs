using EDennis.NetCoreTestingUtilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
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
    public class TestJsonAttribute : DataAttribute {

        public static List<JsonTestCase> TestCases { get; set; }

        private static string TestJsonConfigPath { get; set; } = "TestJsonConfig.json";
        private static TestJsonConfig config;


        private static void BuildConfig() {
            config = new TestJsonConfig().FromJsonPath(TestJsonConfigPath);
            config.PopulatedOn = DateTime.Now;
            TestCases = JsonTestCase.GetTestCasesForProject(
            config.ConnectionString,
            config.TestJsonSchema,
            config.TestJsonTable,
            config.ProjectName);
        }

        protected class TestJsonConfig {
            public string ConnectionString { get; set; }
            public string TestJsonSchema { get; set; }
            public string TestJsonTable { get; set; }
            public string ProjectName { get; set; }
            public string ClassName { get; set; }
            public string MethodName { get; set; }
            public string TestScenario { get; set; }
            public string TestCase { get; set; }
            public DateTime PopulatedOn { get; set; }
            public int RebuildConfigAfterMinutes { get; set; } = 5;
        }

        private string _className;
        private string _methodName;
        private string _testScenario;
        private string _testCase;

        public TestJsonAttribute(
                string className = null,
                string methodName = null,
                string testScenario = null,
                string testCase = null,
                string testJsonConfigPath = "TestJsonConfig.json") {

            TestJsonConfigPath = testJsonConfigPath;

            if (config == null)
                BuildConfig();

            var configAge = DateTime.Now - config.PopulatedOn;
            if (configAge.Minutes > config.RebuildConfigAfterMinutes)
                BuildConfig();

            _className = className ?? config.ClassName;
            _methodName = methodName ?? config.MethodName;
            _testScenario = testScenario ?? config.TestScenario;
            _testCase = testCase ?? config.TestCase;
        }


        public override IEnumerable<object[]> GetData(MethodInfo methodInfo) =>
                JsonTestCase.GetDataForXUnit(TestCases, _className, _methodName, _testScenario, _testCase);

    }

}

