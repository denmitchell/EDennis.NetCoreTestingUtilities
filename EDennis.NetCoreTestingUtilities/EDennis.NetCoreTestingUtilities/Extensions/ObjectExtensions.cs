using NL = Newtonsoft.Json.Linq;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using EDennis.NetCoreTestingUtilities.Json;
using N = Newtonsoft.Json;
using Xunit.Abstractions;
using Xunit;
using EDennis.JsonUtils;
using System.Collections;
using System.Data.SqlClient;

namespace EDennis.NetCoreTestingUtilities.Extensions {

    /// <summary>
    /// This class provides extensions to the Object class.  The 
    /// extensions are designed to be used in unit testing.
    /// </summary>
    public static class ObjectExtensions {

        public const int DEFAULT_MAXDEPTH = 99;

        /// <summary>
        /// Creates a deep copy of the current object
        /// </summary>
        /// <typeparam name="T">The type of the object to be copied</typeparam>
        /// <param name="obj">The object to be copied</param>
        /// <returns>A full copy of the object</returns>
        public static T Copy<T>(this T obj) {
            return NL.JToken.FromObject(obj).ToObject<T>();
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
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqual{T}(object, T, string[])"/>
        public static bool IsEqual<T>(this object obj1, T obj2, bool ignoreArrayElementOrder = false) {

            //string json1 = JsonConvert.SerializeObject(obj1,
            //    Formatting.Indented, new SafeJsonSerializerSettings());
            //string json2 = JsonConvert.SerializeObject(obj2,
            //    Formatting.Indented, new SafeJsonSerializerSettings());
            string json1 = SafeJsonSerializer.Serialize(obj1, 99, true, null, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, 99, true, null, ignoreArrayElementOrder);

            //if (ignoreArrayElementOrder) {
            //    json1 = JsonSorter.Sort(json1);
            //    json2 = JsonSorter.Sort(json2);
            //}

            return json1 == json2;
        }

        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="maxDepth">The maximum depth of the object graph to serialize (1=flat)</param>
        /// <param name="propertiesToIgnore">a string array of 
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// property names that will be ignored.</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqual{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqual<T>(this object obj1, T obj2, 
            int maxDepth, string[] propertiesToIgnore, bool ignoreArrayElementOrder = false) {

            CheckDepth(obj1, maxDepth);

            string json1 = SafeJsonSerializer.Serialize(obj1, maxDepth, true, propertiesToIgnore, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, maxDepth, true, propertiesToIgnore, ignoreArrayElementOrder);


            //string json1 = JsonConvert.SerializeObject(obj1,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth,propertiesToIgnore));
            //string json2 = JsonConvert.SerializeObject(obj2,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth, propertiesToIgnore));

            //if (ignoreArrayElementOrder) {
            //    json1 = JsonSorter.Sort(json1);
            //    json2 = JsonSorter.Sort(json2);
            //}

            return json1 == json2;
        }


        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="maxDepth">The maximum depth of the object graph to serialize (1=flat)</param>
        /// <param name="propertiesToIgnore">a string array of
        /// property names that will be ignored.</param>
        /// <param name="moduloTransform">properties to which a modulo transform will be applied</param>
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqual{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqual<T>(this object obj1, T obj2,
            int maxDepth, string[] propertiesToIgnore,
            Dictionary<string, ulong> moduloTransform,
            bool ignoreArrayElementOrder = false) {

            CheckDepth(obj1, maxDepth);

            string json1 = N.JsonConvert.SerializeObject(obj1,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    maxDepth, propertiesToIgnore, moduloTransform));
            string json2 = N.JsonConvert.SerializeObject(obj2,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    maxDepth, propertiesToIgnore, moduloTransform));

            if (ignoreArrayElementOrder) {
                json1 = JsonSorter.Sort(json1);
                json2 = JsonSorter.Sort(json2);
            }

            return json1 == json2;
        }




        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="maxDepth">The maximum depth of the object graph to serialize (1=flat)</param>
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqual{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqual<T>(this object obj1, T obj2, 
            int maxDepth, bool ignoreArrayElementOrder = false) {

            CheckDepth(obj1, maxDepth);

            string json1 = SafeJsonSerializer.Serialize(obj1, maxDepth, true, null, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, maxDepth, true, null, ignoreArrayElementOrder);

            //string json1 = JsonConvert.SerializeObject(obj1,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth));
            //string json2 = JsonConvert.SerializeObject(obj2,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth));

            //if (ignoreArrayElementOrder) {
            //    json1 = JsonSorter.Sort(json1);
            //    json2 = JsonSorter.Sort(json2);
            //}

            return json1 == json2;
        }


        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="propertiesToIgnore">a string array of 
        /// property names that will be ignored.</param>
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqual{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqual<T>(this object obj1, T obj2, 
            string[] propertiesToIgnore, bool ignoreArrayElementOrder = false) {

            string json1 = SafeJsonSerializer.Serialize(obj1, DEFAULT_MAXDEPTH, true, propertiesToIgnore, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, DEFAULT_MAXDEPTH, true, propertiesToIgnore, ignoreArrayElementOrder);

            //string json1 = JsonConvert.SerializeObject(obj1,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        DEFAULT_MAXDEPTH, propertiesToIgnore));
            //string json2 = JsonConvert.SerializeObject(obj2,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        DEFAULT_MAXDEPTH, propertiesToIgnore));

            //if (ignoreArrayElementOrder) {
            //    json1 = JsonSorter.Sort(json1);
            //    json2 = JsonSorter.Sort(json2);
            //}

            return json1 == json2;
        }



        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="propertiesToIgnore">a string array of 
        /// property names that will be ignored.</param>
        /// <param name="moduloTransform">properties to which a modulo transform will be applied</param>
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqual{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqual<T>(this object obj1, T obj2,
            string[] propertiesToIgnore, Dictionary<string,ulong> moduloTransform,
            bool ignoreArrayElementOrder = false) {

            string json1 = N.JsonConvert.SerializeObject(obj1,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    DEFAULT_MAXDEPTH, propertiesToIgnore, moduloTransform));
            string json2 = N.JsonConvert.SerializeObject(obj2,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    DEFAULT_MAXDEPTH, propertiesToIgnore, moduloTransform));

            if (ignoreArrayElementOrder) {
                json1 = JsonSorter.Sort(json1);
                json2 = JsonSorter.Sort(json2);
            }

            return json1 == json2;
        }



        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties.  Note: this is a
        /// deep comparison.  This method will print side-by-side
        /// comparisons if the two objects are not equal.
        /// NOTE: Requires Xunit
        /// NOTE: The current object should be the "actual" object
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="output">object used by Xunit to print to console</param>
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqualOrWrite{T}(object, T, string[])"/>
        public static bool IsEqualOrWrite<T>(this object obj1, T obj2, 
            ITestOutputHelper output, bool ignoreArrayElementOrder = false) {

            string json1 = SafeJsonSerializer.Serialize(obj1, DEFAULT_MAXDEPTH, true, null, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, DEFAULT_MAXDEPTH, true, null, ignoreArrayElementOrder);

            //string json1 = JsonConvert.SerializeObject(obj1,
            //    Formatting.Indented, new SafeJsonSerializerSettings());
            //string json2 = JsonConvert.SerializeObject(obj2,
            //    Formatting.Indented, new SafeJsonSerializerSettings());

            //if (ignoreArrayElementOrder) {
            //    json1 = JsonSorter.Sort(json1);
            //    json2 = JsonSorter.Sort(json2);
            //}

            var isEqual = json1 == json2;

            if (!isEqual)
                output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            return isEqual;
        }



        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.  This method will print side-by-side
        /// comparisons if the two objects are not equal.
        /// NOTE: Requires Xunit
        /// NOTE: The current object should be the "actual" object
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="maxDepth">The maximum depth of the object graph to serialize (1=flat)</param>
        /// <param name="propertiesToIgnore">a string array of 
        /// property names that will be ignored.</param>
        /// <param name="output">object used by Xunit to print to console</param>
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqualOrWrite{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqualOrWrite<T>(this object obj1, T obj2, 
            int maxDepth, string[] propertiesToIgnore, ITestOutputHelper output, bool ignoreArrayElementOrder = false) {

            CheckDepth(obj1, maxDepth);

            string json1 = SafeJsonSerializer.Serialize(obj1, maxDepth, true, propertiesToIgnore, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, maxDepth, true, propertiesToIgnore, ignoreArrayElementOrder);

            //string json1 = JsonConvert.SerializeObject(obj1,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth, propertiesToIgnore));
            //string json2 = JsonConvert.SerializeObject(obj2,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth, propertiesToIgnore));

            //if (ignoreArrayElementOrder) {
            //    json1 = JsonSorter.Sort(json1);
            //    json2 = JsonSorter.Sort(json2);
            //}

            var isEqual = (json1 == json2);

            if (!isEqual)
                output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            return isEqual;

        }


        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.  This method will print side-by-side
        /// comparisons if the two objects are not equal.
        /// NOTE: Requires Xunit
        /// NOTE: The current object should be the "actual" object
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="maxDepth">The maximum depth of the object graph to serialize (1=flat)</param>
        /// <param name="propertiesToIgnore">a string array of 
        /// property names that will be ignored.</param>
        /// <param name="moduloTransform">properties to which a modulo transform will be applied</param>
        /// <param name="output">object used by Xunit to print to console</param>
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqualOrWrite{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqualOrWrite<T>(this object obj1, T obj2,
            int maxDepth, string[] propertiesToIgnore, Dictionary<string,ulong> moduloTransform, 
            ITestOutputHelper output, bool ignoreArrayElementOrder = false) {

            CheckDepth(obj1, maxDepth);

            string json1 = N.JsonConvert.SerializeObject(obj1,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    maxDepth, propertiesToIgnore,moduloTransform));
            string json2 = N.JsonConvert.SerializeObject(obj2,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    maxDepth, propertiesToIgnore, moduloTransform));

            if (ignoreArrayElementOrder) {
                json1 = JsonSorter.Sort(json1);
                json2 = JsonSorter.Sort(json2);
            }

            var isEqual = (json1 == json2);

            if (!isEqual)
                output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            return isEqual;

        }


        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.  This method will print side-by-side
        /// comparisons if the two objects are not equal.
        /// NOTE: Requires Xunit
        /// NOTE: The current object should be the "actual" object
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="maxDepth">The maximum depth of the object graph to serialize (1=flat)</param>
        /// <param name="output">object used by Xunit to print to console</param>
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqualOrWrite{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqualOrWrite<T>(this object obj1, T obj2, 
            int maxDepth, ITestOutputHelper output, bool ignoreArrayElementOrder = false) {


            CheckDepth(obj1, maxDepth);

            string json1 = SafeJsonSerializer.Serialize(obj1, maxDepth, true, null, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, maxDepth, true, null, ignoreArrayElementOrder);

            //string json1 = JsonConvert.SerializeObject(obj1,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth));
            //string json2 = JsonConvert.SerializeObject(obj2,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth));

            //if (ignoreArrayElementOrder) {
            //    json1 = JsonSorter.Sort(json1);
            //    json2 = JsonSorter.Sort(json2);
            //}

            var isEqual = (json1 == json2);

            if (!isEqual)
                output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            return isEqual;

        }




        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.  This method will print side-by-side
        /// comparisons if the two objects are not equal.
        /// NOTE: Requires Xunit
        /// NOTE: The current object should be the "actual" object
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="propertiesToIgnore">a string array of 
        /// property names that will be ignored.</param>
        /// <param name="output">object used by Xunit to print to console</param>
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqualOrWrite{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqualOrWrite<T>(this object obj1, T obj2, 
            string[] propertiesToIgnore, ITestOutputHelper output, bool ignoreArrayElementOrder = false) {

            string json1 = N.JsonConvert.SerializeObject(obj1,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    DEFAULT_MAXDEPTH, propertiesToIgnore));
            string json2 = N.JsonConvert.SerializeObject(obj2,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    DEFAULT_MAXDEPTH, propertiesToIgnore));

            if (ignoreArrayElementOrder) {
                json1 = JsonSorter.Sort(json1);
                json2 = JsonSorter.Sort(json2);
            }

            var isEqual = (json1 == json2);

            if (!isEqual)
                output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            return isEqual;

        }


        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.  This method will print side-by-side
        /// comparisons if the two objects are not equal.
        /// NOTE: Requires Xunit
        /// NOTE: The current object should be the "actual" object
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="propertiesToIgnore">a string array of 
        /// property names that will be ignored.</param>
        /// <param name="moduloTransform">properties to which a modulo transform will be applied</param>
        /// <param name="output">object used by Xunit to print to console</param>
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqualOrWrite{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqualOrWrite<T>(this object obj1, T obj2,
            string[] propertiesToIgnore, Dictionary<string,ulong> moduloTransform, 
            ITestOutputHelper output, bool ignoreArrayElementOrder = false) {

            string json1 = N.JsonConvert.SerializeObject(obj1,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    DEFAULT_MAXDEPTH, propertiesToIgnore, moduloTransform));
            string json2 = N.JsonConvert.SerializeObject(obj2,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    DEFAULT_MAXDEPTH, propertiesToIgnore, moduloTransform));

            if (ignoreArrayElementOrder) {
                json1 = JsonSorter.Sort(json1);
                json2 = JsonSorter.Sort(json2);
            }

            var isEqual = (json1 == json2);

            if (!isEqual)
                output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            return isEqual;

        }


        /// <summary>
        /// Generates a side-by-side comparison of two objects
        /// as JSON strings.  This version uses a default value
        /// for maximum depth (99) and no property filters
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="label1">Label for the current object</param>
        /// <param name="label2">Label for the object to compare</param>
        /// <returns>side-by-side rendering of objects</returns>
        /// <seealso cref="Juxtapose{T}(object, T, string[])"/>
        public static string Juxtapose<T>(this object obj1, T obj2, string label1, string label2) {

            string json1 = SafeJsonSerializer.Serialize(obj1);
            string json2 = SafeJsonSerializer.Serialize(obj2);

            //string json1 = JsonConvert.SerializeObject(obj1,
            //    Formatting.Indented, new SafeJsonSerializerSettings());
            //string json2 = JsonConvert.SerializeObject(obj2,
            //    Formatting.Indented, new SafeJsonSerializerSettings());

            return FileStringComparer.GetSideBySideFileStrings(json1, json2, label1, label2);
        }


        /// <summary>
        /// Generates a side-by-side comparison of two objects
        /// as JSON strings.  This version allows specification of
        /// maximum depth and property filters
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="label1">Label for the current object</param>
        /// <param name="label2">Label for the object to compare</param>
        /// <param name="maxDepth">The maximum depth of the object graph to serialize (1=flat)</param>
        /// <param name="propertiesToIgnore">a string array of 
        /// property names that will be ignored.</param>
        /// <returns>side-by-side rendering of objects</returns>
        /// <seealso cref="Juxtapose{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static string Juxtapose<T>(this object obj1, T obj2, string label1, string label2, int maxDepth, string[] propertiesToIgnore) {

            CheckDepth(obj1, maxDepth);

            string json1 = SafeJsonSerializer.Serialize(obj1, maxDepth, true, propertiesToIgnore, false);
            string json2 = SafeJsonSerializer.Serialize(obj2, maxDepth, true, propertiesToIgnore, false);

            //string json1 = JsonConvert.SerializeObject(obj1,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth, propertiesToIgnore));
            //string json2 = JsonConvert.SerializeObject(obj2,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth, propertiesToIgnore));

            return FileStringComparer.GetSideBySideFileStrings(json1, json2, label1, label2);

        }


        /// <summary>
        /// Generates a side-by-side comparison of two objects
        /// as JSON strings.  This version allows specification of
        /// maximum depth and property filters
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="label1">Label for the current object</param>
        /// <param name="label2">Label for the object to compare</param>
        /// <param name="maxDepth">The maximum depth of the object graph to serialize (1=flat)</param>
        /// <param name="propertiesToIgnore">a string array of 
        /// property names that will be ignored.</param>
        /// <param name="moduloTransform">properties to which a modulo transform will be applied</param>
        /// <returns>side-by-side rendering of objects</returns>
        /// <seealso cref="Juxtapose{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static string Juxtapose<T>(this object obj1, T obj2, string label1, string label2, 
            int maxDepth, string[] propertiesToIgnore, Dictionary<string,ulong> moduloTransform) {

            CheckDepth(obj1, maxDepth);

            string json1 = N.JsonConvert.SerializeObject(obj1,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    maxDepth, propertiesToIgnore, moduloTransform));
            string json2 = N.JsonConvert.SerializeObject(obj2,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    maxDepth, propertiesToIgnore, moduloTransform));

            return FileStringComparer.GetSideBySideFileStrings(json1, json2, label1, label2);

        }


        /// <summary>
        /// Generates a side-by-side comparison of two objects
        /// as JSON strings.  This version allows specification of
        /// maximum depth, but no property filters
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="label1">Label for the current object</param>
        /// <param name="label2">Label for the object to compare</param>
        /// <param name="maxDepth">The maximum depth of the object graph to serialize (1=flat)</param>
        /// <returns>side-by-side rendering of objects</returns>
        /// <seealso cref="Juxtapose{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static string Juxtapose<T>(this object obj1, T obj2, string label1, string label2, int maxDepth) {

            CheckDepth(obj1, maxDepth);

            string json1 = SafeJsonSerializer.Serialize(obj1, maxDepth, true, null, false);
            string json2 = SafeJsonSerializer.Serialize(obj2, maxDepth, true, null, false);

            //string json1 = JsonConvert.SerializeObject(obj1,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth));
            //string json2 = JsonConvert.SerializeObject(obj2,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth));

            return FileStringComparer.GetSideBySideFileStrings(json1, json2, label1, label2);

        }

        /// <summary>
        /// Generates a side-by-side comparison of two objects
        /// as JSON strings.  This version uses a default value
        /// for maximum depth (99), but allows specification of
        /// property filters
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="label1">Label for the current object</param>
        /// <param name="label2">Label for the object to compare</param>
        /// <param name="propertiesToIgnore">a string array of 
        /// property names that will be ignored.</param>
        /// <returns>side-by-side rendering of objects</returns>
        /// <seealso cref="Juxtapose{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static string Juxtapose<T>(this object obj1, T obj2, string label1, string label2, string[] propertiesToIgnore) {

            string json1 = SafeJsonSerializer.Serialize(obj1, DEFAULT_MAXDEPTH, true, propertiesToIgnore, false);
            string json2 = SafeJsonSerializer.Serialize(obj2, DEFAULT_MAXDEPTH, true, propertiesToIgnore, false);

            //string json1 = N.JsonConvert.SerializeObject(obj1,
            //    N.Formatting.Indented, new SafeJsonSerializerSettings(
            //        DEFAULT_MAXDEPTH, propertiesToIgnore));
            //string json2 = N.JsonConvert.SerializeObject(obj2,
            //    N.Formatting.Indented, new SafeJsonSerializerSettings(
            //        DEFAULT_MAXDEPTH, propertiesToIgnore));

            return FileStringComparer.GetSideBySideFileStrings(json1, json2, label1, label2);

        }


        /// <summary>
        /// Generates a side-by-side comparison of two objects
        /// as JSON strings.  This version uses a default value
        /// for maximum depth (99), but allows specification of
        /// property filters
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="label1">Label for the current object</param>
        /// <param name="label2">Label for the object to compare</param>
        /// <param name="propertiesToIgnore">a string array of 
        /// property names that will be ignored.</param>
        /// <param name="moduloTransform">properties to which a modulo transform will be applied</param>
        /// <returns>side-by-side rendering of objects</returns>
        /// <seealso cref="Juxtapose{T}(object, T)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static string Juxtapose<T>(this object obj1, T obj2, string label1, string label2, 
            string[] propertiesToIgnore, Dictionary<string,ulong> moduloTransform) {

            string json1 = N.JsonConvert.SerializeObject(obj1,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    DEFAULT_MAXDEPTH, propertiesToIgnore, moduloTransform));
            string json2 = N.JsonConvert.SerializeObject(obj2,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    DEFAULT_MAXDEPTH, propertiesToIgnore, moduloTransform));

            return FileStringComparer.GetSideBySideFileStrings(json1, json2, label1, label2);

        }


        /// <summary>
        /// Serializes an object to a JSON string.  This version
        /// assumes no property filters and uses a default value
        /// for maximum depth (99)
        /// </summary>
        /// <param name="obj">the current object</param>
        /// <returns>A JSON string representation of the object</returns>
        /// <seealso cref="ToJsonString(object)"/>
        public static string ToJsonString(this object obj) {
            return SafeJsonSerializer.Serialize(obj);
            //return N.JsonConvert.SerializeObject(obj,
            //    N.Formatting.Indented, new SafeJsonSerializerSettings());
        }


        /// <summary>
        /// Serializes an object to a JSON string.  This version
        /// allows specification of maximum depth and property 
        /// filters 
        /// </summary>
        /// <param name="obj">the current object</param>
        /// <param name="maxDepth">The maximum depth of the object graph to serialize (1=flat)</param>
        /// <param name="propertiesToIgnore">a string array of JSON Paths, whose
        /// associated property values should be ignored.</param>
        /// <returns>A JSON string representation of the object</returns>
        public static string ToJsonString(this object obj, int maxDepth, string[] propertiesToIgnore) {


            return SafeJsonSerializer.Serialize(obj, maxDepth, true, propertiesToIgnore, false);

            //return JsonConvert.SerializeObject(obj,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth, propertiesToIgnore));
        }


        /// <summary>
        /// Serializes an object to a JSON string.  This version
        /// allows specification of maximum depth and property 
        /// filters 
        /// </summary>
        /// <param name="obj">the current object</param>
        /// <param name="maxDepth">The maximum depth of the object graph to serialize (1=flat)</param>
        /// <param name="propertiesToIgnore">a string array of JSON Paths, whose
        /// associated property values should be ignored.</param>
        /// <param name="moduloTransform">properties to which a modulo transform will be applied</param>
        /// <returns>A JSON string representation of the object</returns>
        public static string ToJsonString(this object obj, int maxDepth, 
            string[] propertiesToIgnore, Dictionary<string,ulong> moduloTransform) {


            return N.JsonConvert.SerializeObject(obj,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    maxDepth, propertiesToIgnore, moduloTransform));
        }




        /// <summary>
        /// Serializes an object to a JSON string.  This version
        /// allows specification of maximum depth, but no property 
        /// filters 
        /// </summary>
        /// <param name="obj">the current object</param>
        /// <param name="maxDepth">The maximum depth of the object graph to serialize (1=flat)</param>
        /// associated property values should be ignored.</param>
        /// <returns>A JSON string representation of the object</returns>
        public static string ToJsonString(this object obj, int maxDepth) {

            CheckDepth(obj, maxDepth);

            return SafeJsonSerializer.Serialize(obj, maxDepth, true, null, false);

            //return JsonConvert.SerializeObject(obj,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        maxDepth));
        }


        /// <summary>
        /// Serializes an object to a JSON string.  This version
        /// allows specification of property filters, but uses 
        /// a default value for maximum depth (99)
        /// </summary>
        /// <param name="obj">the current object</param>
        /// <param name="propertiesToIgnore">a string array of JSON Paths, whose
        /// associated property values should be ignored.</param>
        /// <returns>A JSON string representation of the object</returns>
        public static string ToJsonString(this object obj, string[] propertiesToIgnore) {

            return SafeJsonSerializer.Serialize(obj, DEFAULT_MAXDEPTH, true, null, false);

            //return JsonConvert.SerializeObject(obj,
            //    Formatting.Indented, new SafeJsonSerializerSettings(
            //        DEFAULT_MAXDEPTH, propertiesToIgnore));
        }


        /// <summary>
        /// Serializes an object to a JSON string.  This version
        /// allows specification of property filters, but uses 
        /// a default value for maximum depth (99)
        /// </summary>
        /// <param name="obj">the current object</param>
        /// <param name="propertiesToIgnore">a string array of JSON Paths, whose
        /// associated property values should be ignored.</param>
        /// <param name="moduloTransform">properties to which a modulo transform will be applied</param>
        /// <returns>A JSON string representation of the object</returns>
        public static string ToJsonString(this object obj, 
            string[] propertiesToIgnore, Dictionary<string,ulong> moduloTransform) {

            return N.JsonConvert.SerializeObject(obj,
                N.Formatting.Indented, new SafeJsonSerializerSettings(
                    DEFAULT_MAXDEPTH, propertiesToIgnore, moduloTransform));
        }




        /// <summary>
        /// Deserializes a JSON string into an object
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="obj">The object to deserialize</param>
        /// <param name="json">The JSON representation of the object</param>
        /// <returns>A new object initialized with the JSON properties</returns>
        public static T FromJsonString<T>(this T obj, string json){
            T objNew = JsonSerializer.Deserialize<T>(json);
            //T objNew = JToken.Parse(json).ToObject<T>();
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
            NL.JToken jtoken = NL.JToken.Parse(json);
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
            MatchCollection mc = Regex.Matches(jsonFileObjectPath, @".*\.json(\\|/|\.)?");
            if (mc.Count == 0)
                throw new FormatException($"jsonFileObjectPath value ({jsonFileObjectPath}) must be a .json file name optionally followed by \\ and then path to the object");

            string[] paths = Regex.Split(jsonFileObjectPath, @"\.json(\\|/|\.)?");

            string filePath = paths[0] + ".json";

            string json = System.IO.File.ReadAllText(filePath);
            NL.JToken jtoken = NL.JToken.Parse(json);

            if (paths.Length == 3) {
                var objectPath = paths[2].Replace(@"\", ".").Replace(@"/", ".");
                jtoken = jtoken.SelectToken(objectPath);

                if (jtoken == null) {
                    throw new FormatException($"{filePath} does not contain the target json path: \"{objectPath}\".");
                }

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
        public static T FromJsonPath<T>(this T obj, NL.JToken jtoken, string objectPath) {
            jtoken = jtoken.SelectToken(objectPath.Replace(@"\", ".").Replace(@"/", "."));

            T objNew = jtoken.ToObject<T>();
            obj = objNew;

            return obj;
        }



        /// <summary>
        /// Retrieves JSON from a TestJson table having the following structure,
        /// and deserializes the JSON into an object.
        /// CREATE TABLE _maintenance.TestJson(
        ///     Project varchar(100),
	    ///     Class varchar(100),
	    ///     Method varchar(100),
	    ///     FileName varchar(100),
	    ///     Json varchar(max),
	    ///     constraint pk_maintenanceTestJson
        ///         primary key(Project, Class, Method, FileName)
        ///);
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj">The current object</param>
        /// <param name="context">The Entity Framework DB Context.</param>
        /// <param name="testJsonSchema">The schema for the TestJson table</param>
        /// <param name="testJsonTable">The name of the TestJson table</param>
        /// <param name="projectName">The project name for the test json</param>
        /// <param name="className">The class name for the test json</param>
        /// <param name="methodName">The method name for the test json</param>
        /// <param name="testScenario">The general scenario to be tested</param>
        /// <param name="testCase">The specific test case to be tested</param>
        /// <param name="testFile">The JSON to be used in the test (e.g., Input, Expected)</param>
        /// <returns>A new object based deserialized from the retrieved json</returns>
        public static T FromTestJsonTable<T>(this T obj, DbContext context,
                string testJsonSchema, string testJsonTable,
                string projectName, string className, string methodName, string testScenario,
                string testCase, string testFile) {

            var dbConnection = context.Database.GetDbConnection().ConnectionString;
            var schema = (testJsonSchema == null) ? "" : (testJsonSchema + ".");
            var sql = $"select json from {schema}{testJsonTable} " +
                $"where ProjectName = '{projectName}' " +
                $"and ClassName = '{className}' " +
                $"and MethodName = '{methodName}' " +
                $"and TestScenario = '{testScenario}' " +
                $"and TestCase = '{testCase}' " +
                $"and TestFile = '{testFile}' ";

            string json = null;

            using (SqlConnection cxn = new SqlConnection(context.Database.GetDbConnection().ConnectionString)) {
                using SqlCommand cmd = new SqlCommand(sql, cxn);
                cxn.Open();
                var returnValue = cmd.ExecuteScalar();
                json = returnValue?.ToString();
            }

            if (json == null) {
                throw new MissingRecordException($"No Json found for \"{sql}\"");
            }

            //T objNew = JsonSerializer.Deserialize<T>(json);
            //parse the returned JSON
            NL.JToken jtoken = NL.JToken.Parse(json);

            //convert the JSON to an object
            T objNew = jtoken.ToObject<T>();
            obj = objNew;

            return obj;

        }

        /// <summary>
        /// Retrieves a JSON string from a JSON Test table
        /// </summary>
        /// <param name="obj">Any string (used merely to execute the method)</param>
        /// <param name="context">The Entity Framework DB Context.</param>
        /// <param name="testJsonSchema">The schema for the TestJson table</param>
        /// <param name="testJsonTable">The name of the TestJson table</param>
        /// <param name="projectName">The project name for the test json</param>
        /// <param name="className">The class name for the test json</param>
        /// <param name="methodName">The method name for the test json</param>
        /// <param name="testScenario">The general scenario to be tested</param>
        /// <param name="testCase">The specific test case to be tested</param>
        /// <param name="testFile">The JSON to be used in the test (e.g., Input, Expected)</param>
        /// <returns>JSON retrieved from a table</returns>
        public static string FromTestJsonTable(this String obj, DbContext context,
                string testJsonSchema, string testJsonTable,
                string projectName, string className, string methodName, string testScenario,
                string testCase, string testFile) {

            var dbConnection = context.Database.GetDbConnection().ConnectionString;
            var schema = (testJsonSchema == null) ? "" : (testJsonSchema + ".");
            var sql = $"select json from {schema}{testJsonTable} " +
                $"where ProjectName = '{projectName}' " +
                $"and ClassName = '{className}' " +
                $"and MethodName = '{methodName}' " +
                $"and TestScenario = '{testScenario}' " +
                $"and TestCase = '{testCase}' " +
                $"and TestFile = '{testFile}' ";

            string json = null;

            using (SqlConnection cxn = new SqlConnection(context.Database.GetDbConnection().ConnectionString)) {
                using SqlCommand cmd = new SqlCommand(sql, cxn);
                cxn.Open();
                var returnValue = cmd.ExecuteScalar();
                json = returnValue?.ToString();
            }

            if (json == null) {
                throw new MissingRecordException($"No Json found for \"{sql}\"");
            }

            return json;

        }

        /// <summary>
        /// Provides all properties associated with a JToken object,
        /// cast as a JObject.
        /// </summary>
        /// <param name="jtoken">The JToken object whose properties are desired</param>
        /// <returns>A list of properties</returns>
        public static List<NL.JProperty> Properties(this NL.JToken jtoken) {
            var jobject = (NL.JObject)jtoken;
            return jobject.Properties().ToList();
        }



        /// <summary>
        /// Removes all specified Json Paths from the provided JToken
        /// </summary>
        /// <param name="jtoken">A valid Json.NET JToken object</param>
        /// <param name="pathsToRemove">An array of valid Json Paths
        /// or an array of property names</param>
        /// <returns></returns>
        public static NL.JToken Filter(this NL.JToken jtoken, string[] pathsToRemove) {
            return JsonFilterer.ApplyFilter(jtoken, pathsToRemove);
        }



        /// <summary>
        /// Ensures that the maxDepth is not less than the minimum depth 
        /// for an object or collection type
        /// </summary>
        /// <param name="obj">The object or collection to inspect</param>
        /// <param name="maxDepth">The provided maximum depth of the object graph</param>
        private static void CheckDepth(object obj, int maxDepth) {

            Type objType = obj.GetType().GetInterface(nameof(IEnumerable));
            Type itemType = null;
            if (objType != null) {
                itemType = GetElementTypeOfEnumerable(obj);
                if (itemType != null) {
                    if (itemType.IsClass && maxDepth == 1) {
                        throw new ArgumentException("maxDepth must be 2 or greater for a collection");
                    }
                }
            }

        }

        private static Type GetElementTypeOfEnumerable(object o) {
            // if it's not an enumerable why do you call this method all ?
            if (!(o is IEnumerable enumerable))
                return null;

            Type[] interfaces = enumerable.GetType().GetInterfaces();

            return (from i in interfaces
                    where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                    select i.GetGenericArguments()[0]).FirstOrDefault();
        }





    }
}
