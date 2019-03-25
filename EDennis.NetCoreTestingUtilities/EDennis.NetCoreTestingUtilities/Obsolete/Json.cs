using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace EDennis.NetCoreTestingUtilities.Json
{
    /// <summary>
    /// This class provides a fluent API wrapper around
    /// some selected Json.NET operations.
    /// </summary>
    [Obsolete]
    public class Json {

        /// <summary>
        /// The Json class stores a Json.NET JToken
        /// </summary>
        public JToken JToken { get; set; }


        /// <summary>
        /// "Constructs" a new Json instance from any object
        /// </summary>
        /// <param name="obj">The source object</param>
        /// <returns>A reference to the current Json instance</returns>
        public Json FromObject(Object obj) {
            if (obj.GetType() == typeof(JToken))
                JToken = (JToken)obj;
            else
                JToken = JToken.FromObject(obj);

            return this;
        }

        /// <summary>
        /// "Constructs" a new Json instance from a JSON string
        /// </summary>
        /// <param name="json">Valid JSON string</param>
        /// <returns>A reference to the current Json instance</returns>
        public Json FromString(string json) {
            JToken = JToken.Parse(json);
            return this;
        }

        /// <summary>
        /// "Constructs" a new Json instance from a file path 
        /// and a valid JSON path (which may include forward or backward
        /// slashes, instead of dots)
        /// </summary>
        /// <param name="filePath">Valid path to file</param>
        /// <param name="objectPath">Valid JSON path</param>
        /// <returns>A reference to the current Json instance</returns>
        public Json FromPath(string filePath, string objectPath) {
            string json = System.IO.File.ReadAllText(filePath);
            JToken = JToken.Parse(json);
            JToken = JToken.SelectToken(objectPath.Replace(@"\", ".").Replace(@"/", "."));

            if (JToken == null) {
                throw new FormatException($"{filePath} does not contain the target json path: \"{objectPath}\".");
            }
            return this;
        }

        /// <summary>
        /// "Constructs" a new Json instance from a combined file path 
        /// and a valid JSON path (which may include forward or backward
        /// slashes, instead of dots)
        /// </summary>
        /// <param name="jsonFileObjectPath">Combined file and json path</param>
        /// <returns>A reference to the current Json instance</returns>
        public Json FromPath(string jsonFileObjectPath) {

            //use regular expression to split jsonFileObjectPath into a 
            //separate file path and object path
            MatchCollection mc = Regex.Matches(jsonFileObjectPath, @".*\.json(\\|/|\.)?");
            if (mc.Count == 0)
                throw new FormatException($"jsonFileObjectPath value ({jsonFileObjectPath}) must be a .json file name followed by \\, /, or . and then path to the object");



            string[] paths = Regex.Split(jsonFileObjectPath, @"\.json(\\|/|\.)");

            string filePath = paths[0].Replace(".json","") + ".json";

            string json = System.IO.File.ReadAllText(filePath);
            JToken = JToken.Parse(json);
            if (paths.Length == 3) {
                JToken = JToken.SelectToken(paths[2].Replace(@"/",".").Replace(@"\",""));
            }

            if (JToken == null) {
                throw new FormatException($"{filePath} does not contain the target json path: \"{paths[2]}\".");
            }

            return this;

        }

        /// <summary>
        /// "Constructs" a new Json instance from a Json.NET JToken 
        /// and a valid JSON path (which may include forward or backward
        /// slashes, instead of dots)
        /// </summary>
        /// <param name="jtoken">Json.NET JToken</param>
        /// <param name="objectPath">Valid JSON path</param>
        /// <returns>A reference to the current Json instance</returns>
        public Json FromPath(JToken jtoken, string objectPath) {
            JToken = jtoken.SelectToken(objectPath.Replace(@"\", ".").Replace(@"/", "."));
            if (jtoken == null) {
                throw new FormatException($"{jtoken.ToString()} does not contain the target json path: \"{objectPath}\".");
            }
            return this;
        }

        /// <summary>
        /// "Constructs" a new Json instance from a SQL Server .sql file 
        /// that uses a FOR JSON clause to return JSON
        /// </summary>
        /// <param name="sqlForJsonFile">SQL Server .sql file with FOR JSON</param>
        /// <param name="context">Subclass of Entity Framework DbContext</param>
        /// <returns>A reference to the current Json instance</returns>
        public Json FromSql(string sqlForJsonFile, DbContext context) {
            return FromSql(sqlForJsonFile, context.Database.GetDbConnection().ConnectionString);
        }

        /// <summary>
        /// "Constructs" a new Json instance from a SQL Server .sql file 
        /// that uses a FOR JSON clause to return JSON
        /// </summary>
        /// <param name="sqlForJsonFile">SQL Server .sql file with FOR JSON</param>
        /// <param name="connectionString">SQL Server connection string</param>
        /// <returns>A reference to the current Json instance</returns>
        public Json FromSql(string sqlForJsonFile, string connectionString) {

            string sql = System.IO.File.ReadAllText(sqlForJsonFile);
            string json = null;

            using (var context = new JsonResultContext(connectionString)) {
                json = context.JsonResults
                        .FromSql(sql)
                        .FirstOrDefault()
                        .Json;
            }
            JToken = JToken.Parse(json);
            return this;
        }

        /// <summary>
        /// Nullifies the specified path in a JSON file so that they are
        /// effectively ignored during a comparison of two Json objects.
        /// NOTE: multiple paths can be removed by method chaining.
        /// </summary>
        /// <param name="pathToRemove">Valid JSON path</param>
        /// <returns>A reference to the current Json instance</returns>
        public Json Filter(string pathToRemove) {
            if (JToken == null)
                throw new JsonException("Json is null.  Can't filter");
            try {
                JToken = JsonFilterer.ApplyFilter(JToken, new string[] { pathToRemove });
            } catch (Exception) {
                throw new JsonException($"pathToRemove ({pathToRemove}) is not a valid path for {JToken?.ToString()}.");
            }
            return this;
        }

        /// <summary>
        /// Deserializes the Json instance to an object
        /// This is a simple wrapper around JToken.ToObject
        /// </summary>
        /// <typeparam name="T">The destination type of the object</typeparam>
        /// <returns>an Object</returns>
        public T ToObject<T>() {
            return JToken.ToObject<T>();
        }

        /// <summary>
        /// Generates a formatted JSON string
        /// </summary>
        /// <returns>Formatted JSON string</returns>
        public override string ToString() {
            return JToken?.ToString();
        }

        /// <summary>
        /// Performs a deep comparison of the current object
        /// and the provided object
        /// </summary>
        /// <param name="obj">Object to compare</param>
        /// <returns>True if the objects have the same property values</returns>
        public override bool Equals(object obj) {
            if (obj == null)
                return false;
            else if (obj.GetType() != typeof(Json))
                return false;
            else {
                var objJson = (Json)obj;
                return JToken.DeepEquals(JToken,objJson.JToken);
            }
        }

        /// <summary>
        /// This is a simple wrapper around GetHashCode()
        /// </summary>
        /// <returns>the hashcode associated with the current instance</returns>
        public override int GetHashCode() {
            return base.GetHashCode();
        }

    }
}
