﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;
using Xunit.Abstractions;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace EDennis.NetCoreTestingUtilities {


    /// <summary>
    /// The JsonPathalizer class transforms regular JSON structures into "pathalized" form
    /// -- a Dictionary of key-value pairs, where the key is a sortable version of JSON path 
    /// in bracket notation and the value is the value of the node at the path.
    /// </summary>
    public static class JsonPathalizer {


        private static readonly Regex CONTENT_KEY_PATTERN = new Regex(@"<<[^>]+>>");


        class PathalizerObj {
            public PathalizedJson Paths { get; set; } = new PathalizedJson();
            public Dictionary<string, string> Children { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, int> Parents { get; set; } = new Dictionary<string, int>();

            public int MaxChildren { get; set; } = 0;
            public string IndexFormat { get; set; }
            public List<string> PropertiesToIgnore { get; set; }
            public bool OrderProperties { get; set; }
        }


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
        public static PathalizedJson Pathalize(string json, string[] propertiesToIgnore, bool orderProperties = true, bool ignoreArrayOrder = false) {
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
        private static PathalizedJson Pathalize(JsonDocument doc, string[] propertiesToIgnore = null, bool orderProperties = true, bool ignoreArrayOrder = false) {
            var po = new PathalizerObj {
                PropertiesToIgnore = (propertiesToIgnore ?? new string[] { }).ToList(),
                OrderProperties = orderProperties
            };

            PathalizeInternal(po, doc);

            po.IndexFormat = $"D{Math.Ceiling(Math.Log(po.MaxChildren - 1, 10))}";

            if (ignoreArrayOrder)
                Sort(po);
            return po.Paths;
        }


        /// <summary>
        /// Pathalizes a JsonDocument
        /// </summary>
        /// <param name="doc">A valid JsonDocument</param>
        private static void PathalizeInternal(PathalizerObj po, JsonDocument doc) {
            var rootElement = doc.RootElement;
            PathalizeInternal(po, rootElement);
        }


        /// <summary>
        /// Executes the recursive pathalize algorithm.
        /// </summary>
        /// <param name="element">Current JsonElement</param>
        /// <param name="path">Current path</param>
        /// <param name="depth">Curren depth</param>
        private static void PathalizeInternal(PathalizerObj po, JsonElement element, string path = null, int depth = 0) {
            var parentPath = path ?? "$";
            //handle JSON objects
            if (element.ValueKind == JsonValueKind.Object) {
                if (po.OrderProperties)
                    foreach (var prop in element.EnumerateObject().ToList().OrderBy(p => p.Name)) {
                        if (!po.PropertiesToIgnore.Contains(prop.Name))
                            PathalizeInternal(po, prop.Value, $"{parentPath}['{prop.Name}']", depth + 1);
                    }
                else
                    foreach (var prop in element.EnumerateObject()) {
                        if (!po.PropertiesToIgnore.Contains(prop.Name))
                            PathalizeInternal(po, prop.Value, $"{parentPath}['{prop.Name}']", depth + 1);
                    }
                //handle JSON arrays
            } else if (element.ValueKind == JsonValueKind.Array) {
                if (!po.Parents.ContainsKey(parentPath))
                    po.Parents.Add(parentPath, depth);
                var children = element.EnumerateArray().ToList();
                if (children.Count > po.MaxChildren)
                    po.MaxChildren = children.Count;
                for (int i = 0; i < children.Count; i++) {
                    var childPath = $"{parentPath}[{i.ToString("D7")}]";
                    po.Children[childPath] = parentPath;
                    PathalizeInternal(po, children[i], childPath, depth + 1);
                }
                //handle booleans
            } else if (element.ValueKind == JsonValueKind.False || element.ValueKind == JsonValueKind.True) {
                po.Paths.Add(path, element.GetBoolean());
                //handle numbers
            } else if (element.ValueKind == JsonValueKind.Number) {
                if (element.TryGetByte(out byte byteValue))
                    po.Paths.Add(path, byteValue);
                else if (element.TryGetInt16(out short shortValue))
                    po.Paths.Add(path, shortValue);
                else if (element.TryGetInt32(out int intValue))
                    po.Paths.Add(path, intValue);
                else if (element.TryGetInt64(out long longValue))
                    po.Paths.Add(path, longValue);
                else if (element.TryGetDecimal(out decimal decimalValue))
                    po.Paths.Add(path, longValue);
                //handle strings (including dates and times)
            } else if (element.ValueKind == JsonValueKind.String) {
                if (TimeSpan.TryParse(element.GetString(), out TimeSpan timeSpanValue))
                    po.Paths.Add(path, timeSpanValue);
                else if (element.TryGetDateTime(out DateTime dateTimeValue))
                    po.Paths.Add(path, dateTimeValue);
                else if (element.TryGetDateTimeOffset(out DateTimeOffset dateTimeOffsetValue))
                    po.Paths.Add(path, dateTimeOffsetValue);
                else
                    po.Paths.Add(path, element.GetString());
            } else if (element.ValueKind == JsonValueKind.Null)
                po.Paths.Add(path, null);

        }


        /// <summary>
        /// Performs a sort on the pathalized JSON structure.  Note that child elements in
        /// arrays are sorted by the content of each child element, which ensures that any
        /// two arrays with matchable children are equivalent after sorting.  Even when
        /// two arrays have approximately matchable children, it is likely that the sorting
        /// will tend to align the two arrays.
        /// </summary>
        private static void Sort(PathalizerObj po) {
            using MD5 md5Hash = MD5.Create();
            var depths = po.Parents.Values.Distinct().OrderByDescending(d => d);
            foreach (var depth in depths) {
                foreach (var parent in po.Parents.Where(e => e.Value == depth).ToList()) {
                    //map between original child key and temporary content key
                    SortedDictionary<string, string> map = new SortedDictionary<string, string>();

                    //build a temporary content key for each child to replace the original index key
                    //so that when the content keys are ordered, then any two arrays that have matchable
                    //children (possibly in a different order) will be ordered the same way.
                    foreach (var child in po.Children.Where(e => e.Value == parent.Key)) {
                        var sb = new List<string>();
                        var paths = po.Paths.Where(p => p.Key.StartsWith(child.Key)).OrderBy(p => p.Key).ToList();
                        foreach (var path in paths) {
                            var relativePath = path.Key.Substring(child.Key.Length);
                            sb.Add($"{relativePath}\u001F{path.Value}");
                        }
                        var childContentKey = string.Join("\u001E", sb);
                        var cnt = po.Paths.Count(a => a.Key.Contains(childContentKey));
                        var newKey = $"<<{childContentKey}-{cnt.ToString("D7")}>>";
                        map.Add(newKey, child.Key);
                        ReplaceLastIndexWithContentKey(po.Paths, child.Key, newKey);
                    }
                    //now that array elements are ordered by content, replace content
                    //keys with ascending index values
                    int i = -1;
                    foreach (var entry in map) {
                        ReplaceContentKeyWithIndex(po.Paths, entry.Key, ++i, po.IndexFormat);
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


        static void ReplaceContentKeyWithIndex<T>(SortedDictionary<string, T> dict, string prefix, int index, string format) {
            var keys = dict.Where(d => d.Key.Contains(prefix)).Select(x => x.Key).ToList();
            foreach (var key in keys) {
                var value = dict[key];
                dict.Remove(key);
                var newKey = CONTENT_KEY_PATTERN.Replace(key, index.ToString(format));
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
            PathalizedJson pathalizedJson1,
            PathalizedJson pathalizedJson2,
            ITestOutputHelper output) {

            ReformatIndexes(pathalizedJson1, pathalizedJson2);

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

                s = "";
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
            PathalizedJson pathalizedJson1,
            PathalizedJson pathalizedJson2) {

            ReformatIndexes(pathalizedJson1, pathalizedJson2);

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


        private static void ReformatIndexes(PathalizedJson jp1, PathalizedJson jp2) {
            if (jp1.IndexFormat != jp2.IndexFormat) {
                if (jp1.MaxChildren > jp2.MaxChildren) {
                    var paths = jp2.Keys;
                    foreach (var path in paths) {
                        var value = jp2[path];
                        jp2.Remove(path);
                        var newPath = path.Replace("[0", "[0" + new string('0', jp1.MaxChildren - jp2.MaxChildren));
                        jp2.Add(newPath, value);
                    }
                }
                if (jp2.MaxChildren > jp1.MaxChildren) {
                    var paths = jp1.Keys;
                    foreach (var path in paths) {
                        var value = jp1[path];
                        jp1.Remove(path);
                        var newPath = path.Replace("[0", "[0" + new string('0', jp2.MaxChildren - jp1.MaxChildren));
                        jp1.Add(newPath, value);
                    }
                }
            }
        }




    }


}
