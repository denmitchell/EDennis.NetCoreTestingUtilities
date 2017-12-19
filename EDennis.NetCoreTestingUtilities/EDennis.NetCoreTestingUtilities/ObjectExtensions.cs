using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EDennis.NetCoreTestingUtilities.Json;

namespace EDennis.NetCoreTestingUtilities.Extensions {

    /// <summary>
    /// This class provides extensions to the Object class.  The 
    /// extensions are designed to be used in unit testing.
    /// </summary>
    public static class ObjectExtensions {

        /// <summary>
        /// Creates a deep copy of the current object
        /// </summary>
        /// <typeparam name="T">The type of the object to be copied</typeparam>
        /// <param name="obj">The object to be copied</param>
        /// <returns>A full copy of the object</returns>
        public static T Copy<T>(this T obj) {
            return JToken.FromObject(obj).ToObject<T>();
        }

        /// <summary>
        /// Determines if the object variable references the same
        /// object in memory.  This relies upon a comparison of
        /// hashcodes.  Occasionally, by accident, two different
        /// objects may have the same hashcode.  Consequently, this
        /// method also compares the object properties.  It is 
        /// highly unlikely that two different object variables 
        /// with the same property values and hashcodes actually
        /// reference different objects.
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <returns>true, if the same object; false, otherwise</returns>
        public static bool IsSame<T>(this object obj1, T obj2) {
            return obj1.GetHashCode() == obj2.GetHashCode()
                && obj1.IsEqual(obj2);
        }

        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties.  Note: this is a
        /// deep comparison.
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqual{T}(object, T, string[])"/>
        public static bool IsEqual<T>(this object obj1, T obj2) {
            return JToken.FromObject(obj1).ToString()
                    == JToken.FromObject(obj2).ToString();
        }

        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="pathsToIgnore">a string array of JSON Paths, whose
        /// associated property values should be ignored.</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqual{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqual<T>(this object obj1, T obj2, string[] pathsToIgnore) {

            var jtokens = new JToken[] { JToken.FromObject(obj1), JToken.FromObject(obj2) };

            return jtokens[0].Filter(pathsToIgnore).ToString() 
                == jtokens[1].Filter(pathsToIgnore).ToString();

        }


        /// <summary>
        /// Serializes an object to a JSON string
        /// </summary>
        /// <param name="obj">the current object</param>
        /// <returns>A JSON string representation of the object</returns>
        public static string ToJsonString(this object obj) {
            return JToken.FromObject(obj).ToString();
        }


        /// <summary>
        /// Deserializes a JSON string into an object
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="obj">The object to deserialize</param>
        /// <param name="json">The JSON representation of the object</param>
        /// <returns>A new object initialized with the JSON properties</returns>
        public static T FromJsonString<T>(this T obj, string json){
            T objNew = JToken.Parse(json).ToObject<T>();
            obj = objNew;
            return obj;
        }

        /// <summary>
        /// Deserializes an embedded object represented at a particular JSON Path and held
        /// in a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="obj">The current object</param>
        /// <param name="filePath">The path for the JSON file</param>
        /// <param name="objectPath">The JSON path to the embedded object</param>
        /// <returns>A new object initialized with the JSON properties</returns>
        /// <seealso cref="FromJsonPath{T}(T, string)"/>
        /// <seealso cref="FromJsonPath{T}(T, JToken, string)"/>
        public static T FromJsonPath<T>(this T obj, string filePath, string objectPath) {

            string json = System.IO.File.ReadAllText(filePath);
            JToken jtoken = JToken.Parse(json);
            jtoken = jtoken.SelectToken(objectPath.Replace(@"\", ".").Replace(@"/", "."));

            if (jtoken == null) {
                throw new FormatException($"{filePath} does not contain the target json path: \"{objectPath}\".");
            }

            T objNew = jtoken.ToObject<T>();
            obj = objNew;

            return obj;

        }

        /// <summary>
        /// Deserializes an embedded object represented at a particular JSON Path and held
        /// in a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="obj">The current object</param>
        /// <param name="jsonFileObjectPath">The file path and JSON path to the embedded object</param>
        /// <returns>A new object initialized with the JSON properties</returns>
        /// <seealso cref="FromJsonPath{T}(T, string, string)"/>
        /// <seealso cref="FromJsonPath{T}(T, JToken, string)"/>
        public static T FromJsonPath<T>(this T obj, string jsonFileObjectPath) {

            //use regular expression to split jsonFileObjectPath into a 
            //separate file path and object path
            MatchCollection mc = Regex.Matches(jsonFileObjectPath, @".*\.json(\\|/|\.)");
            if (mc.Count == 0)
                throw new FormatException($"jsonFileObjectPath value ({jsonFileObjectPath}) must be a .json file name followed by \\ and then path to the object");

            string[] paths = Regex.Split(jsonFileObjectPath, @"\.json(\\|/|\.)");

            string filePath = paths[0] + ".json";
            string objectPath = paths[2].Replace(@"\", ".").Replace(@"/", ".");

            string json = System.IO.File.ReadAllText(filePath);
            JToken jtoken = JToken.Parse(json);
            jtoken = jtoken.SelectToken(objectPath);

            if (jtoken == null) {
                throw new FormatException($"{filePath} does not contain the target json path: \"{objectPath}\".");
            }

            T objNew = jtoken.ToObject<T>();
            obj = objNew;

            return obj;

        }


        /// <summary>
        /// Deserializes an embedded object represented at a particular JSON Path and held
        /// in a JSON.NET JToken object.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="obj">The current object</param>
        /// <param name="jtoken">The JToken object holding the JSON data</param>
        /// <param name="jsonPath">The path to an embedded object</param>
        /// <returns>A new object initialized with the JSON properties</returns>
        /// <seealso cref="FromJsonPath{T}(T, string, string)"/>
        /// <seealso cref="FromJsonPath{T}(T, string)"/>
        public static T FromJsonPath<T>(this T obj, JToken jtoken, string objectPath) {
            jtoken = jtoken.SelectToken(objectPath.Replace(@"\", ".").Replace(@"/", "."));

            T objNew = jtoken.ToObject<T>();
            obj = objNew;

            return obj;
        }


        /// <summary>
        /// Constructs a new object from the results of a SQL Server FOR JSON query.
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj">The current object</param>
        /// <param name="sqlForJsonFile">A SQL Server .sql file having a FOR JSON clause</param>
        /// <param name="context">The Entity Framework DB Context.</param>
        /// <returns></returns>
        public static T FromSql<T>(this T obj, string sqlForJsonFile,
            DbContext context) {
            string connectionString = context.Database.GetDbConnection().ConnectionString;
            return FromSql(obj, sqlForJsonFile, connectionString);
        }


        /// <summary>
        /// Constructs a new object from the results of a SQL Server FOR JSON query.
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj">The current object</param>
        /// <param name="sqlForJsonFile">A SQL Server .sql file having a FOR JSON clause</param>
        /// <param name="connectionString">A valid connection string</param>
        /// <returns></returns>
        public static T FromSql<T>(this T obj, string sqlForJsonFile,
            string connectionString){

            string sql = System.IO.File.ReadAllText(sqlForJsonFile);
            string json = null;

            //use Entity Framework class to hold results 
            using (var context = new JsonResultContext(connectionString)) {
                json = context.JsonResults
                        .FromSql(sql)
                        .FirstOrDefault()
                        .Json;
            }

            //parse the returned JSON
            JToken jtoken = JToken.Parse(json);

            //convert the JSON to an object
            T objNew = jtoken.ToObject<T>();
            obj = objNew;

            return obj;
        }


        /// <summary>
        /// Provides all properties associated with a JToken object,
        /// cast as a JObject.
        /// </summary>
        /// <param name="jtoken">The JToken object whose properties are desired</param>
        /// <returns>A list of properties</returns>
        public static List<JProperty> Properties(this JToken jtoken) {
            var jobject = (JObject)jtoken;
            return jobject.Properties().ToList();
        }



        /// <summary>
        /// Removes all specified Json Paths from the provided JToken
        /// </summary>
        /// <param name="jtoken">A valid Json.NET JToken object</param>
        /// <param name="pathsToRemove">An array of valid Json Paths
        /// or an array of property names</param>
        /// <returns></returns>
        public static JToken Filter(this JToken jtoken, string[] pathsToRemove) {
            return JsonFilterer.ApplyFilter(jtoken, pathsToRemove);
        }
    }
}
