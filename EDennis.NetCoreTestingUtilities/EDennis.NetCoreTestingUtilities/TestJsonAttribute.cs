using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        private string _additionalCriteria;

        /// <summary>
        /// Constructs a new TestJson attribute with the given parameters
        /// for the database and query criteria
        /// </summary>
        /// <param name="connectionString">SQL Server Connection String</param>
        /// <param name="testJsonSchema">Schema name (e.g., _maintenance)</param>
        /// <param name="testJsonTable">Table name</param>
        /// <param name="projectName">Project name</param>
        /// <param name="className">Class Name</param>
        public TestJsonAttribute(string connectionString, string testJsonSchema, string testJsonTable,
            string projectName, string className) {

            _connectionString = connectionString;
            _testJsonSchema = testJsonSchema;
            _testJsonTable = testJsonTable;
            _projectName = projectName;
            _className = className;
            _additionalCriteria = "";
        }


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
            string projectName, string className, string additionalCriteria) {

            _connectionString = connectionString;
            _testJsonSchema = testJsonSchema;
            _testJsonTable = testJsonTable;
            _projectName = projectName;
            _className = className;
            _additionalCriteria = " AND ( " + additionalCriteria + " )";

        }



        /// <summary>
        /// Returns all relevant TestJson records.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public override IEnumerable<object[]> GetData(MethodInfo methodInfo) {

            using (var cxn = new SqlConnection(_connectionString)) {
                var testJson = cxn.Query<TestJson>(
                    "select Project, Class, Method, FileName, Json from "
                    + $"{_testJsonSchema}.{_testJsonTable} where "
                    + "Project = @_projectName and Class = @_className"
                    + $"{_additionalCriteria}",
                    new { _projectName, _className });

                foreach (var rec in testJson)
                    yield return new object[] { rec.Project, rec.Class, rec.Method, rec.FileName, rec.Json };

            }
        }

    }

}
