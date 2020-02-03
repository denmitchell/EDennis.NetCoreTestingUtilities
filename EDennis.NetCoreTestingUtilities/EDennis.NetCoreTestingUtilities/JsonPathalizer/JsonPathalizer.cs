using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;
using Xunit.Abstractions;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace EDennis.NetCoreTestingUtilities {


    /// <summary>
    /// 
    /// </summary>
    public class JsonPathalizer {


        private static readonly Regex CONTENT_KEY_PATTERN = new Regex(@"<<[^>]+>>");

        private readonly SortedDictionary<string, object> _dict = new SortedDictionary<string, object>();
        private readonly Dictionary<string, string> _children = new Dictionary<string, string>();
        private readonly Dictionary<string, int> _parents = new Dictionary<string, int>();

        private List<string> _propertiesToIgnore;
        private bool _orderProperties;


        /// <summary>
        /// Transforms a (possibly) hierarchical JSON structure into a single dimensional structure
        /// of key-value pairs, where the key is the JSON path to the provided value.
        /// <see cref="https://goessner.net/articles/JsonPath/"/>
        /// </summary>
        /// <param name="json">a valid JSON string</param>
        /// <param name="propertiesToIgnore">JSON properties to omit in the transformation</param>
        /// <param name="orderProperties">whether to order property keys (default is true)</param>
        /// <param name="ignoreArrayOrder">whether to ignore the order of array elements (default is false)</param>
        /// <returns>SortedDictionary of values, keyed by their JSON paths</returns>
        public SortedDictionary<string,object> Pathalize(string json, string[] propertiesToIgnore, bool orderProperties = true, bool ignoreArrayOrder = false) {
            var jdoc = JsonDocument.Parse(json);
            return Pathalize(jdoc, propertiesToIgnore, orderProperties, ignoreArrayOrder);
        }


        /// <summary>
        /// Transforms a (possibly) hierarchical JSON structure into a single dimensional structure
        /// of key-value pairs, where the key is the JSON path to the provided value.
        /// <see cref="https://goessner.net/articles/JsonPath/"/>
        /// </summary>
        /// <param name="json">a valid JsonDocument</param>
        /// <param name="propertiesToIgnore">JSON properties to omit in the transformation</param>
        /// <param name="orderProperties">whether to order property keys (default is true)</param>
        /// <param name="ignoreArrayOrder">whether to ignore the order of array elements (default is false)</param>
        /// <returns>SortedDictionary of values, keyed by their JSON paths</returns>
        public SortedDictionary<string, object> Pathalize(JsonDocument doc, string[] propertiesToIgnore = null, bool orderProperties = true, bool ignoreArrayOrder = false) {
            _propertiesToIgnore = (propertiesToIgnore ?? new string[] { }).ToList();
            _orderProperties = orderProperties;
            PathalizeInternal(doc);
            if (ignoreArrayOrder)
                Sort();
            return _dict;
        }


        /// <summary>
        /// Pathalizes a JsonDocument
        /// </summary>
        /// <param name="doc">A valid JsonDocument</param>
        private void PathalizeInternal(JsonDocument doc) {
            var rootElement = doc.RootElement;
            PathalizeInternal(rootElement);
        }


        /// <summary>
        /// Executes the recursive pathalize algorithm.
        /// </summary>
        /// <param name="element">Current JsonElement</param>
        /// <param name="path">Current path</param>
        /// <param name="depth">Curren depth</param>
        private void PathalizeInternal(JsonElement element, string path = null, int depth = 0) {
            var parentPath = path ?? "$";
            //handle JSON objects
            if (element.ValueKind == JsonValueKind.Object) {
                if (_orderProperties)
                    foreach (var prop in element.EnumerateObject().ToList().OrderBy(p => p.Name)) {
                        if (!_propertiesToIgnore.Contains(prop.Name))
                            PathalizeInternal(prop.Value, $"{parentPath}['{prop.Name}']", depth + 1);
                    }
                else
                    foreach (var prop in element.EnumerateObject()) {
                        if (!_propertiesToIgnore.Contains(prop.Name))
                            PathalizeInternal(prop.Value, $"{parentPath}['{prop.Name}']", depth + 1);
                    }
            //handle JSON arrays
            } else if (element.ValueKind == JsonValueKind.Array) {
                _parents.Add(parentPath, depth);
                var children = element.EnumerateArray().ToList();
                for (int i = 0; i < children.Count; i++) {
                    var childPath = $"{parentPath}[{i.ToString("D7")}]";
                    _children[childPath] = parentPath;
                    PathalizeInternal(children[i], childPath);
                }
            //handle booleans
            } else if (element.ValueKind == JsonValueKind.False || element.ValueKind == JsonValueKind.True) {
                _dict.Add(path, element.GetBoolean());
            //handle numbers
            } else if (element.ValueKind == JsonValueKind.Number) {
                if (element.TryGetByte(out byte byteValue))
                    _dict.Add(path, byteValue);
                else if (element.TryGetInt16(out short shortValue))
                    _dict.Add(path, shortValue);
                else if (element.TryGetInt32(out int intValue))
                    _dict.Add(path, intValue);
                else if (element.TryGetInt64(out long longValue))
                    _dict.Add(path, longValue);
                else if (element.TryGetDecimal(out decimal decimalValue))
                    _dict.Add(path, longValue);
            //handle strings (including dates and times)
            } else if (element.ValueKind == JsonValueKind.String) {
                if (TimeSpan.TryParse(element.GetString(), out TimeSpan timeSpanValue))
                    _dict.Add(path, timeSpanValue);
                else if (element.TryGetDateTime(out DateTime dateTimeValue))
                    _dict.Add(path, dateTimeValue);
                else if (element.TryGetDateTimeOffset(out DateTimeOffset dateTimeOffsetValue))
                    _dict.Add(path, dateTimeOffsetValue);
                else
                    _dict.Add(path, element.GetString());
            } else if (element.ValueKind == JsonValueKind.Null)
                _dict.Add(path, null);

        }


        /// <summary>
        /// Performs a sort on the pathalized JSON structure.  Note that child elements in
        /// arrays are sorted by the content of each child element, which ensures that any
        /// two arrays with matchable children are equivalent after sorting.  Even when
        /// two arrays have approximately matchable children, it is likely that the sorting
        /// will tend to align the two arrays.
        /// </summary>
        private void Sort() {
            using MD5 md5Hash = MD5.Create();
            var depths = _parents.Values.Distinct().OrderByDescending(d => d);
            foreach (var depth in depths) {
                foreach (var parent in _parents.Where(e => e.Value == depth).ToList()) {
                    //map between original child key and temporary content key
                    SortedDictionary<string, string> map = new SortedDictionary<string, string>();

                    //build a temporary content key for each child to replace the original index key
                    //so that when the content keys are ordered, then any two arrays that have matchable
                    //children (possibly in a different order) will be ordered the same way.
                    foreach (var child in _children.Where(e => e.Value == parent.Key)) {
                        var sb = new List<string>();
                        var paths = _dict.Where(p => p.Key.StartsWith(child.Key)).OrderBy(p => p.Key).ToList();
                        foreach (var path in paths) {
                            var relativePath = path.Key.Substring(child.Key.Length);
                            sb.Add($"{relativePath}\u001F{path.Value}");
                        }
                        var childContentKey = string.Join("\u001E", sb);
                        var cnt = _dict.Count(a => a.Key.Contains(childContentKey));
                        var newKey = $"<<{childContentKey}-{cnt.ToString("D7")}>>";
                        map.Add(newKey, child.Key);
                        ReplaceLastIndexWithContentKey(_dict, child.Key, newKey);
                    }
                    //now that array elements are ordered by content, replace content
                    //keys with ascending index values
                    int i = -1;
                    foreach(var entry in map) {
                        ReplaceContentKeyWithIndex(_dict, entry.Key, ++i);
                    }

                }
            }
        }




        static void ReplaceLastIndexWithContentKey<T>(SortedDictionary<string, T> dict, string targetKey, string contentKey) {
            var keys = dict.Keys.Where(a => a.StartsWith(targetKey)).ToArray();
            foreach (var key in keys) {
                var value = dict[key];
                dict.Remove(key);
                var indexStart = targetKey.LastIndexOf("[");
                var newKey = targetKey.Substring(0, indexStart) + $"[{contentKey}]" + key.Substring(targetKey.Length);

                dict.Add(newKey, value);

            }
        }


        static void ReplaceContentKeyWithIndex<T>(SortedDictionary<string, T> dict, string prefix, int index) {
            var keys = dict.Where(d => d.Key.Contains(prefix)).Select(x=>x.Key).ToList();
            foreach (var key in keys) {
                var value = dict[key];
                dict.Remove(key);
                var newKey = CONTENT_KEY_PATTERN.Replace(key, index.ToString("D7"));
                dict.Add(newKey, value);
            }
        }


        /// <summary>
        /// Produces side-by-side output of two "pathalized" JSON structures.
        /// This method is designed to be used with Xunit testing.
        /// </summary>
        /// <param name="pathalizedJson1">The first pathalized JSON structure</param>
        /// <param name="pathalizedJson2">The second pathalized JSON structure</param>
        /// <param name="output">can write to output during Xunit testing</param>
        public static void Juxtapose(
            SortedDictionary<string, object> pathalizedJson1,
            SortedDictionary<string, object> pathalizedJson2,
            ITestOutputHelper output) {

            var allPaths = pathalizedJson1.Keys.Union(pathalizedJson2.Keys);

            var maxLen = allPaths.Select(x => x.Length).Max();
            var max1 = pathalizedJson1.Values.Where(x => x != null).Select(x => x.ToString().Length).Max();
            var max2 = pathalizedJson2.Values.Where(x => x != null).Select(x => x.ToString().Length).Max();
            foreach (var path in allPaths) {
                var sb = new StringBuilder();
                sb.Append(path);
                if (maxLen > path.Length)
                    sb.Append(new string(' ', maxLen - path.Length));
                sb.Append(" ");
                string s = "";
                if (pathalizedJson1.TryGetValue(path, out object val1))
                    s = val1 == null ? "" : val1.ToString();
                else if (max1 > s.Length)
                    s = new string('~', max1 - s.Length);

                sb.Append(s);
                if (max1 > s.Length)
                    sb.Append(new string(' ', max1 - s.Length));
                sb.Append(" ");

                if (pathalizedJson2.TryGetValue(path, out object val2))
                    s = val2 == null ? "" : val2.ToString();
                else if (max2 > s.Length)
                    s = new string('~', max2 - s.Length);

                sb.Append(s);
                if (max2 > s.Length)
                    sb.Append(new string(' ', max2 - s.Length));
                sb.Append(" ");

                if (val1?.ToString() != val2?.ToString())
                    sb.Append("X");
                else
                    sb.Append("");

                output.WriteLine(sb.ToString());
            }

        }



        /// <summary>
        /// Produces side-by-side output of two "pathalized" JSON structures.
        /// This method writes to the System Console.
        /// </summary>
        /// <param name="pathalizedJson1">The first pathalized JSON structure</param>
        /// <param name="pathalizedJson2">The second pathalized JSON structure</param>
        public static void Juxtapose(
            SortedDictionary<string, object> pathalizedJson1,
            SortedDictionary<string, object> pathalizedJson2) {

            var allPaths = pathalizedJson1.Keys.Union(pathalizedJson2.Keys);

            var maxLen = allPaths.Select(x => x.Length).Max();
            var max1 = pathalizedJson1.Values.Where(x => x != null).Select(x => x.ToString().Length).Max();
            var max2 = pathalizedJson2.Values.Where(x => x != null).Select(x => x.ToString().Length).Max();
            foreach (var path in allPaths) {
                Console.Write(path);
                if (maxLen > path.Length)
                    Console.Write(new string(' ', maxLen - path.Length));
                Console.Write(" ");
                string s = "";
                if (pathalizedJson1.TryGetValue(path, out object val1))
                    s = val1 == null ? "" : val1.ToString();
                else if (max1 > s.Length)
                    s = new string('~', max1 - s.Length);

                Console.Write(s);
                if (max1 > s.Length)
                    Console.Write(new string(' ', max1 - s.Length));
                Console.Write(" ");

                if (pathalizedJson2.TryGetValue(path, out object val2))
                    s = val2 == null ? "" : val2.ToString();
                else if (max2 > s.Length)
                    s = new string('~', max2 - s.Length);

                Console.Write(s);
                if (max2 > s.Length)
                    Console.Write(new string(' ', max2 - s.Length));
                Console.Write(" ");

                if (val1?.ToString() != val2?.ToString())
                    Console.WriteLine("X");
                else
                    Console.WriteLine("");
            }

        }


    }


}
