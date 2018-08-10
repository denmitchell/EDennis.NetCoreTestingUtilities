using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Sdk;

namespace EDennis.NetCoreTestingUtilities
{
    /// <summary>
    /// This class allows one to enumerate relevant records 
    /// from a TestJson table, using a custom Xunit attribute
    /// </summary>
    public class TestJsonAttribute : DataAttribute {


        public static List<JsonTestCase> TestCases { get; set; }


        private string _connectionString;
        private string _testJsonSchema;
        private string _testJsonTable;
        private string _projectName;
        private string _className;
        private string _methodName;
        private string _testScenario;
        private string _testCase;


        public TestJsonAttribute(string connectionString, string testJsonSchema, string testJsonTable,
            string projectName, string className, string methodName, string testScenario, string testCase) {

            TestCases = new List<JsonTestCase>();

            _connectionString = connectionString;
            _testJsonSchema = testJsonSchema;
            _testJsonTable = testJsonTable;
            _projectName = projectName;
            

            using (var cxn = new SqlConnection(_connectionString)) {
                var testJson = cxn.Query<TestJson>(
                    "select ProjectName, ClassName, MethodName, TestScenario, TestCase, TestFile, Json from "
                    + $"{_testJsonSchema}.{_testJsonTable} where "
                    + "ProjectName = @_projectName",
                    new { _projectName });


                if (testJson.Count() == 0) {
                    throw new ArgumentException(
                        $"No TestJson record found for ProjectName: {_projectName}.");
                }

                //construct a JsonTestCase as the return object
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
                    TestCases.Add(rec);

            }

        }


        public override IEnumerable<object[]> GetData(MethodInfo methodInfo) {

            var qry = TestCases.Where(t => t.ClassName == _className && t.MethodName == _methodName
                && t.TestScenario == _testScenario && t.TestCase == _testCase);

            //return all objects
            foreach (var rec in qry.AsEnumerable())
                yield return new object[] { $"{_testScenario}({_testCase})", rec };

        }
    }


}
