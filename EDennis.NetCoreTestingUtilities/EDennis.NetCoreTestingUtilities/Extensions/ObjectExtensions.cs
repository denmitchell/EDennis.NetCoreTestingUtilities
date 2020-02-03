using EDennis.JsonUtils;
using EDennis.NetCoreTestingUtilities.Json;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Xunit.Abstractions;
using NL = Newtonsoft.Json.Linq;

namespace EDennis.NetCoreTestingUtilities.Extensions {

    /// <summary>
    /// This class provides extensions to the Object class.  The 
    /// extensions are designed to be used in unit testing.
    /// </summary>
    public static class ObjectExtensions {

        public const int DEFAULT_MAXDEPTH = 99;


        public static Dictionary<string, object> ToPropertyDictionary(this ExpandoObject expando) {
            return new Dictionary<string, object>(expando);
        }


        public static Dictionary<string, object> ToPropertyDictionary<T>(this T obj) {
            var itemType = obj.GetType();
            var expando = new ExpandoObject();
            foreach (var propertyInfo in itemType.GetProperties()) {
                try {
                    expando.TryAdd(propertyInfo.Name, propertyInfo.GetValue(obj));
                } catch { }
            }
            return new Dictionary<string, object>(expando);
        }

        public static List<Dictionary<string, object>> ToPropertyDictionaryList<T>(this IEnumerable<ExpandoObject> list) {
            var expandoList = new List<Dictionary<string, object>>();
            foreach (var expando in list) {
                expandoList.Add(new Dictionary<string, object>(expando));
            }
            return expandoList;
        }


        public static List<Dictionary<string, object>> ToPropertyDictionaryList<T>(this IEnumerable<T> list) {
            var expandoList = new List<Dictionary<string, object>>();
            foreach (var item in list) {
                var itemType = item.GetType();
                if (itemType == typeof(ExpandoObject)) {
                    expandoList.Add(new Dictionary<string, object>(new Dictionary<string, object>(item as ExpandoObject)));
                } else {
                    var expando = new ExpandoObject();
                    foreach (var propertyInfo in itemType.GetProperties()) {
                        try {
                            expando.TryAdd(propertyInfo.Name, propertyInfo.GetValue(item));
                        } catch { }
                    }
                    expandoList.Add(new Dictionary<string, object>(expando));
                }
            }
            return expandoList;
        }


        /// <summary>
        /// Creates a deep copy of the current object
        /// </summary>
        /// <typeparam name="T">The type of the object to be copied</typeparam>
        /// <param name="obj">The object to be copied</param>
        /// <returns>A full copy of the object</returns>
        public static T Copy<T>(this T obj)
        where T : class, new() {
            //public static T Copy<T>(this T obj) {
            //    return NL.JToken.FromObject(obj).ToObject<T>();
            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Creates a deep copy of a dynamic object
        /// </summary>
        /// <typeparam name="T">The type of the object to be copied</typeparam>
        /// <param name="obj">The object to be copied</param>
        /// <returns>A full copy of the object</returns>
        public static T CopyFromDynamic<T>(dynamic obj)
            where T : class, new() {
            var oldObj = new T();
            return Merge<T>(oldObj, obj);
        }

        /// <summary>
        /// Creates a deep copy of the current object
        /// </summary>
        /// <typeparam name="T">The (base class) type of the object to be copied</typeparam>
        /// <typeparam name="S">The subclass type</typeparam>
        /// <param name="obj">The object to be copied</param>
        /// <returns>A full copy of the object</returns>
        public static S CopyFromBaseClass<T, S>(T obj)
            where T : class, new()
            where S : T {
            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<S>(json);
        }

        /// <summary>
        /// Merges properties from two objects -- a regular 
        /// object and a dynamic object, where property
        /// values from the dynamic object overwrite
        /// corresponding property values from the other object.
        /// </summary>
        /// <typeparam name="T">The type of the regular object</typeparam>
        /// <param name="oldObj">a regular object</param>
        /// <param name="newObj">a dynamic object, holding new object properties</param>
        /// <returns></returns>
        public static T Merge<T>(T oldObj, dynamic newObj)
            where T : class {

            using JsonDocument newDoc = JsonDocument.Parse(JsonSerializer.Serialize(newObj));
            using JsonDocument oldDoc = JsonDocument.Parse(JsonSerializer.Serialize(oldObj));
            var newRoot = newDoc.RootElement;
            var oldRoot = oldDoc.RootElement;
            var newProps = newRoot.EnumerateObject();
            var oldProps = oldRoot.EnumerateObject();

            var props = typeof(T).GetProperties();

            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            writer.WriteStartObject();
            foreach (var prop in props) {
                writer.WritePropertyName(prop.Name);

                if (newRoot.TryGetProperty(prop.Name, out var newProp)) {
                    if ((newProp.ValueKind == JsonValueKind.Number ||
                        newProp.ValueKind == JsonValueKind.False ||
                        newProp.ValueKind == JsonValueKind.True
                        ) && prop.PropertyType == typeof(string))
                        writer.WriteStringValue(newProp.GetRawText());
                    else
                        newProp.WriteTo(writer);
                } else if (oldRoot.TryGetProperty(prop.Name, out var oldProp)) {
                    if ((oldProp.ValueKind == JsonValueKind.Number ||
                        oldProp.ValueKind == JsonValueKind.False ||
                        oldProp.ValueKind == JsonValueKind.True
                        ) && prop.PropertyType == typeof(string))
                        writer.WriteStringValue(oldProp.GetRawText());
                    else
                        oldProp.WriteTo(writer);
                }
            }
            writer.WriteEndObject();
            writer.Flush();
            string json = Encoding.UTF8.GetString(stream.ToArray());
            return JsonSerializer.Deserialize<T>(json);
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

            string json1 = SafeJsonSerializer.Serialize(obj1, 99, true, null, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, 99, true, null, ignoreArrayElementOrder);

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
        public static bool IsEqual<T>(this object obj1, T obj2,
            int maxDepth, string[] propertiesToIgnore, bool ignoreArrayElementOrder = false) {

            CheckDepth(obj1, maxDepth);

            string json1 = SafeJsonSerializer.Serialize(obj1, maxDepth, true, propertiesToIgnore, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, maxDepth, true, propertiesToIgnore, ignoreArrayElementOrder);

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
        public static bool IsEqual<T>(this object obj1, T obj2,
            int maxDepth, bool ignoreArrayElementOrder = false) {

            CheckDepth(obj1, maxDepth);

            string json1 = SafeJsonSerializer.Serialize(obj1, maxDepth, true, null, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, maxDepth, true, null, ignoreArrayElementOrder);

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
        public static bool IsEqual<T>(this object obj1, T obj2,
            string[] propertiesToIgnore, bool ignoreArrayElementOrder = false) {

            string json1 = SafeJsonSerializer.Serialize(obj1, DEFAULT_MAXDEPTH, true, propertiesToIgnore, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, DEFAULT_MAXDEPTH, true, propertiesToIgnore, ignoreArrayElementOrder);

            return json1 == json2;
        }



        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties.  Note: this is a
        /// deep comparison.  This method will print side-by-side
        /// comparisons whether or not the two objects are equal.
        /// NOTE: Requires Xunit
        /// NOTE: The current object should be the "actual" object
        /// </summary>
        /// <typeparam name="T">The type of the current object</typeparam>
        /// <param name="obj1">The current object</param>
        /// <param name="obj2">The object to compare</param>
        /// <param name="output">object used by Xunit to print to console</param>
        /// <param name="ignoreArrayElementOrder">Whether to ignore the order of array elements</param>
        /// <returns>true, if equal; false, otherwise</returns>
        /// <seealso cref="IsEqualOrWrite{T}(object, T, int, ITestOutputHelper, bool)"/>
        public static bool IsEqualAndWrite<T>(this object obj1, T obj2,
            ITestOutputHelper output, bool ignoreArrayElementOrder = false) {

            string json1 = SafeJsonSerializer.Serialize(obj1, DEFAULT_MAXDEPTH, true, null, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, DEFAULT_MAXDEPTH, true, null, ignoreArrayElementOrder);

            var isEqual = json1 == json2;

            output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            return isEqual;
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
        /// <seealso cref="IsEqualAndWrite{T}(object, T, ITestOutputHelper, bool)"/>
        public static bool IsEqualOrWrite<T>(this object obj1, T obj2,
            ITestOutputHelper output, bool ignoreArrayElementOrder = false) {

            string json1 = SafeJsonSerializer.Serialize(obj1, DEFAULT_MAXDEPTH, true, null, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, DEFAULT_MAXDEPTH, true, null, ignoreArrayElementOrder);

            var isEqual = json1 == json2;

            if (!isEqual)
                output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            return isEqual;
        }


        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.  This method will print side-by-side
        /// comparisons whether or not the two objects are equal.
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
        /// <seealso cref="IsEqualOrWrite{T}(object, T, int, string[], ITestOutputHelper, bool)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqualAndWrite<T>(this object obj1, T obj2,
            int maxDepth, string[] propertiesToIgnore, ITestOutputHelper output, bool ignoreArrayElementOrder = false) {

            CheckDepth(obj1, maxDepth);

            string json1 = SafeJsonSerializer.Serialize(obj1, maxDepth, true, propertiesToIgnore, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, maxDepth, true, propertiesToIgnore, ignoreArrayElementOrder);

            var isEqual = (json1 == json2);

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
        /// <seealso cref="IsEqualAndWrite{T}(object, T, int, string[], ITestOutputHelper, bool)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqualOrWrite<T>(this object obj1, T obj2,
            int maxDepth, string[] propertiesToIgnore, ITestOutputHelper output, bool ignoreArrayElementOrder = false) {

            CheckDepth(obj1, maxDepth);

            string json1 = SafeJsonSerializer.Serialize(obj1, maxDepth, true, propertiesToIgnore, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, maxDepth, true, propertiesToIgnore, ignoreArrayElementOrder);

            var isEqual = (json1 == json2);

            if (!isEqual)
                output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            return isEqual;

        }



        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.  This method will print side-by-side
        /// comparisons whether or not the two objects are equal.
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
        /// <seealso cref="IsEqualOrWrite{T}(object, T, int, ITestOutputHelper, bool)"/>
        /// <see href="https://github.com/json-path/JsonPath"/>
        public static bool IsEqualAndWrite<T>(this object obj1, T obj2,
            int maxDepth, ITestOutputHelper output, bool ignoreArrayElementOrder = false) {


            CheckDepth(obj1, maxDepth);

            string json1 = SafeJsonSerializer.Serialize(obj1, maxDepth, true, null, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, maxDepth, true, null, ignoreArrayElementOrder);

            var isEqual = (json1 == json2);

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

            var isEqual = (json1 == json2);

            if (!isEqual)
                output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            return isEqual;

        }


        /// <summary>
        /// Determines if two objects have the same exact values for
        /// all of their corresponding properties, excluding the properties
        /// associated with pathsToIgnore.  This method will print side-by-side
        /// comparisons whether or not the two objects are equal.
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
        /// <seealso cref="IsEqualOrWrite{T}(object, T, string[], ITestOutputHelper, bool)"/>
        public static bool IsEqualAndWrite<T>(this object obj1, T obj2,
            string[] propertiesToIgnore, ITestOutputHelper output, bool ignoreArrayElementOrder = false) {


            string json1 = SafeJsonSerializer.Serialize(obj1, DEFAULT_MAXDEPTH, true, propertiesToIgnore, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, DEFAULT_MAXDEPTH, true, propertiesToIgnore, ignoreArrayElementOrder);

            var isEqual = (json1 == json2);

            output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            return isEqual;


            //string json1 = N.JsonConvert.SerializeObject(obj1,
            //    N.Formatting.Indented, new SafeJsonSerializerSettings(
            //        DEFAULT_MAXDEPTH, propertiesToIgnore));
            //string json2 = N.JsonConvert.SerializeObject(obj2,
            //    N.Formatting.Indented, new SafeJsonSerializerSettings(
            //        DEFAULT_MAXDEPTH, propertiesToIgnore));

            //if (ignoreArrayElementOrder) {
            //    json1 = JsonSorter.Sort(json1);
            //    json2 = JsonSorter.Sort(json2);
            //}

            //var isEqual = (json1 == json2);

            //output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            //return isEqual;

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
        /// <seealso cref="IsEqualAndWrite{T}(object, T, string[], ITestOutputHelper, bool)"/>
        public static bool IsEqualOrWrite<T>(this object obj1, T obj2,
            string[] propertiesToIgnore, ITestOutputHelper output, bool ignoreArrayElementOrder = false) {

            string json1 = SafeJsonSerializer.Serialize(obj1, DEFAULT_MAXDEPTH, true, propertiesToIgnore, ignoreArrayElementOrder);
            string json2 = SafeJsonSerializer.Serialize(obj2, DEFAULT_MAXDEPTH, true, propertiesToIgnore, ignoreArrayElementOrder);

            var isEqual = (json1 == json2);

            if(!isEqual)
            output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            return isEqual;

            //string json1 = N.JsonConvert.SerializeObject(obj1,
            //    N.Formatting.Indented, new SafeJsonSerializerSettings(
            //        DEFAULT_MAXDEPTH, propertiesToIgnore));
            //string json2 = N.JsonConvert.SerializeObject(obj2,
            //    N.Formatting.Indented, new SafeJsonSerializerSettings(
            //        DEFAULT_MAXDEPTH, propertiesToIgnore));

            //if (ignoreArrayElementOrder) {
            //    json1 = JsonSorter.Sort(json1);
            //    json2 = JsonSorter.Sort(json2);
            //}

            //var isEqual = (json1 == json2);

            //if (!isEqual)
            //    output.WriteLine(FileStringComparer.GetSideBySideFileStrings(json2, json1, "EXPECTED", "ACTUAL"));

            //return isEqual;

        }


        /// <summary>
        /// Determines if two JSON structures are equivalent, while optionally ignoring
        /// certain properties, optionally ordering property keys, and optionally
        /// ignoring the order of elements in arrays.  The method also produces a
        /// side-by-side comparison of the two JSON structures in "pathalized" form
        /// (a flat, key-value pairing where the key is the JSON path to the value.)
        /// This method is designed to be used with Xunit tests.
        /// </summary>
        /// <param name="json1">The first JSON structure</param>
        /// <param name="json2">The second JSON structure</param>
        /// <param name="output">An Xunit ITestOutputHelper, for writing output during tests</param>
        /// <param name="propertiesToIgnore">properties that will be omitted during the comparison</param>
        /// <param name="orderProperties">whether to order properties (default is true)</param>
        /// <param name="ignoreArrayOrder">whether to ignore the order of array elements (default is false)</param>
        /// <returns>true if the two JSON structures are equivalent, after taking into consideration
        /// the various parameters.  NOTE: whitespace, formatting, and comments are always ignored</returns>
        public static bool IsEqualAndWrite(string json1, string json2, ITestOutputHelper output,
            string[] propertiesToIgnore = null, bool orderProperties = true, bool ignoreArrayOrder = false) {
            var result = true;
            
            //pathalize the two JSON strings
            var pathalizedJson1 = JsonPathalizer.Pathalize(json1, propertiesToIgnore, orderProperties, ignoreArrayOrder);
            var pathalizedJson2 = JsonPathalizer.Pathalize(json2, propertiesToIgnore, orderProperties, ignoreArrayOrder);

            //compare the two pathalized JSON structures
            if (pathalizedJson1.Count == pathalizedJson2.Count) {
                foreach (var path in pathalizedJson1) {
                    if (pathalizedJson2.TryGetValue(path.Key, out object val2)) {
                        //fail if the path value is different for one of the pathalized JSON structures
                        if ((val2 == null && path.Value != null) ||
                            (val2 != null && path.Value == null) ||
                            (val2?.ToString() != path.Value?.ToString())) {
                            result = false;
                            break;
                        } else {
                            continue;
                        }
                    //fail if the second pathalized JSON structure is missing a path
                    } else {
                        result = false;
                        break;
                    }
                }
            //fail if the two pathalized JSON structures have a different number of paths
            } else {
                result = false;
            }

            //produce a side-by-side comparison of the JSON
            JsonPathalizer.Juxtapose(pathalizedJson1, pathalizedJson2, output);

            return result;
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

            return SafeJsonSerializer.Serialize(obj, DEFAULT_MAXDEPTH, true, propertiesToIgnore, false);
        }



        /// <summary>
        /// Deserializes a JSON string into an object
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="_">The object to deserialize</param>
        /// <param name="json">The JSON representation of the object</param>
        /// <returns>A new object initialized with the JSON properties</returns>
        public static T FromJsonString<T>(this T _, string json) {
            T objNew = JsonSerializer.Deserialize<T>(json);
            _ = objNew;
            return _;
        }

        /// <summary>
        /// Deserializes an embedded object represented at a particular JSON Path and held
        /// in a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="_">The current object</param>
        /// <param name="filePath">The path for the JSON file</param>
        /// <param name="objectPath">The JSON path to the embedded object</param>
        /// <returns>A new object initialized with the JSON properties</returns>
        /// <seealso cref="FromJsonPath{T}(T, string)"/>
        /// <seealso cref="FromJsonPath{T}(T, JToken, string)"/>
        public static T FromJsonPath<T>(this T _, string filePath, string objectPath) {

            string json = System.IO.File.ReadAllText(filePath);
            NL.JToken jtoken = NL.JToken.Parse(json);
            jtoken = jtoken.SelectToken(objectPath.Replace(@"\", ".").Replace(@"/", "."));

            if (jtoken == null) {
                throw new FormatException($"{filePath} does not contain the target json path: \"{objectPath}\".");
            }

            T objNew = jtoken.ToObject<T>();
            _ = objNew;

            return _;

        }

        /// <summary>
        /// Deserializes an embedded object represented at a particular JSON Path and held
        /// in a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="_">The current object</param>
        /// <param name="jsonFileObjectPath">The file path and JSON path to the embedded object</param>
        /// <returns>A new object initialized with the JSON properties</returns>
        /// <seealso cref="FromJsonPath{T}(T, string, string)"/>
        /// <seealso cref="FromJsonPath{T}(T, JToken, string)"/>
        public static T FromJsonPath<T>(this T _, string jsonFileObjectPath) {

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
            _ = objNew;

            return _;

        }


        /// <summary>
        /// Deserializes an embedded object represented at a particular JSON Path and held
        /// in a JSON.NET JToken object.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize</typeparam>
        /// <param name="_">The current object</param>
        /// <param name="jtoken">The JToken object holding the JSON data</param>
        /// <param name="jsonPath">The path to an embedded object</param>
        /// <returns>A new object initialized with the JSON properties</returns>
        /// <seealso cref="FromJsonPath{T}(T, string, string)"/>
        /// <seealso cref="FromJsonPath{T}(T, string)"/>
        public static T FromJsonPath<T>(this T _, NL.JToken jtoken, string objectPath) {
            jtoken = jtoken.SelectToken(objectPath.Replace(@"\", ".").Replace(@"/", "."));

            T objNew = jtoken.ToObject<T>();
            _ = objNew;

            return _;
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
        /// <param name="_">The current object</param>
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
        public static T FromTestJsonTable<T>(this T _, DbContext context,
                string testJsonSchema, string testJsonTable,
                string projectName, string className, string methodName, string testScenario,
                string testCase, string testFile) {

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

            NL.JToken jtoken = NL.JToken.Parse(json);

            //convert the JSON to an object
            T objNew = jtoken.ToObject<T>();
            _ = objNew;

            return _;

        }

        /// <summary>
        /// Retrieves a JSON string from a JSON Test table
        /// </summary>
        /// <param name="_">Any string (used merely to execute the method)</param>
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
        public static string FromTestJsonTable(this string _, DbContext context,
                string testJsonSchema, string testJsonTable,
                string projectName, string className, string methodName, string testScenario,
                string testCase, string testFile) {

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
