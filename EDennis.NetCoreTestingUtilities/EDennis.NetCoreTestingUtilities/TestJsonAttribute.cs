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

        private string _connectionString;
        private string _testJsonSchema;
        private string _testJsonTable;
        private string _projectName;
        private string _className;
        private string _methodName;

        /// <summary>
        /// Constructs a new TestJson attribute with the given parameters
        /// for the database and query criteria
        /// </summary>
        /// <param name="connectionString">SQL Server Connection String</param>
        /// <param name="testJsonSchema">Schema name (e.g., _maintenance)</param>
        /// <param name="testJsonTable">Table name</param>
        /// <param name="projectName">Project name</param>
        /// <param name="className">Class Name</param>
        /// <param name="additionalCriteria">any additional criteria formatted as a SQL expression </param>
        public TestJsonAttribute(string connectionString, string testJsonSchema, string testJsonTable,
            string projectName, string className, string methodName) {

            _connectionString = connectionString;
            _testJsonSchema = testJsonSchema;
            _testJsonTable = testJsonTable;
            _projectName = projectName;
            _className = className;
            _methodName = methodName;
    }




        /// <summary>
        /// Returns an IEnumerable of JsonTestCase records, each of which
        /// has been cast as an object[].  In the XUnit test, use a Theory
        /// attribute and a TestJson attribute such as the following:
        ///
        /// [Theory]
        /// [TestJson("Server=(localdb)\\mssqllocaldb;Database=tmp;Trusted_Connection=True;",
        ///     "_maintenance", "TestJson", "P1", "C1", "M")]
        /// 
        /// Then, specify "JsonTestCase testCase" as the single parameter 
        /// for the test method
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public override IEnumerable<object[]> GetData(MethodInfo methodInfo) {

            using (var cxn = new SqlConnection(_connectionString)) {
                var testJson = cxn.Query<TestJson>(
                    "select ProjectName, ClassName, MethodName, TestScenario, TestCase, TestFile, Json from "
                    + $"{_testJsonSchema}.{_testJsonTable} where "
                    + "ProjectName = @_projectName and ClassName = @_className"
                    + $" and MethodName = @_methodName ",
                    new { _projectName, _className,  _methodName });


                if (testJson.Count() == 0) {
                    throw new ArgumentException(
                        $"No TestJson record found for ProjectName: {_projectName}, " +
                        $"ClassName: {_className}, MethodName: {_methodName}. ");
                }

                //construct a JsonTestCase as the return object
                var qry = testJson.GroupBy(
                        r => new { r.ProjectName, r.ClassName, r.MethodName, r.TestScenario, r.TestCase },
                        r => new JsonTestFile { TestFile = r.TestFile, Json = r.Json },
                        (key, g) =>
                            new JsonTestCase { ProjectName = key.ProjectName,
                                    ClassName = key.ClassName, MethodName = key.MethodName,
                                    TestScenario  = key.TestScenario,
                                    TestCase = key.TestCase,
                                JsonTestFiles = g.ToList()
                            });

                //return all objects
                foreach (var rec in qry.AsEnumerable())
                    yield return new object[] { rec };

            }
        }

    }

}
