using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace EDennis.NetCoreTestingUtilities.Json {
    public class Json2Xml {

        public const string ROOT = "root";
        public const string JSONXML_CONVERTER_NAMESPACE_URI = "http://edennis.com/2013/jsonxml";
        public const string JSONXML_CONVERTER_NAMESPACE_PREFIX = "jx";

        public const string TAG_CLEAN_PATTERN = "(^[^A-Za-z]*)|([^A-Za-z0-9\\\\-_]*)";
        private static Regex regex = new Regex(TAG_CLEAN_PATTERN);

        internal enum JsonContextType {
            ARRAY,
            OBJECT,
            KEY
        }


        internal class JsonReaderToken {
            public JsonToken Type { get; set; }
            public object Value { get; set; }
        }

        internal class Parent {
            public JsonContextType Type { get; set; }
            public string Key { get; set; }
            public int ChildCount { get; set; }
        }

        private Stack<Parent> lineage = new Stack<Parent>();
        private Stack<int> arrays = new Stack<int>();

        JsonReaderToken currentToken = null;
        string currentKey = ROOT;
        bool isRoot = true;

        XmlWriter xwriter = null;
        XmlDocument doc = new XmlDocument();

        public XmlDocument ConvertToXml(JToken jToken) {

            var stringBuilder = new StringBuilder();

            var settings = new XmlWriterSettings {
                OmitXmlDeclaration = true,
                Indent = true,
                NewLineOnAttributes = true
            };

            //using(xwriter = XmlWriter.Create("f:\\x2.xml", settings)) {
            using (xwriter = doc.CreateNavigator().AppendChild()) {
                //using (xwriter = XmlWriter.Create("f:\\x.xml", settings)) {
                using (var reader = new JsonTextReader(new StringReader(jToken.ToString()))) {

                    while (reader.Read()) {

                        currentToken = new JsonReaderToken { Type = reader.TokenType };

                        if (reader.Value != null)
                            currentToken.Value = reader.Value;

                        switch (currentToken.Type) {
                            case JsonToken.StartObject: {
                                    WriteObjectOrArray(JsonContextType.OBJECT);
                                    break;
                                }
                            case JsonToken.StartArray: {
                                    arrays.Push(Guid.NewGuid().ToString().GetHashCode());
                                    WriteObjectOrArray(JsonContextType.ARRAY);
                                    break;
                                }
                            case JsonToken.PropertyName: {
                                    currentKey = currentToken.Value.ToString();
                                    currentKey = regex.Replace(currentKey, "");
                                    lineage.Push(new Parent { Key = currentKey, Type = JsonContextType.KEY });
                                    xwriter.WriteStartElement(currentKey);
                                    break;
                                }
                            case JsonToken.String: {
                                    string value = currentToken.Value.ToString();
                                    WriteValue(value, JsonValueType.STRING);
                                    break;
                                }
                            case JsonToken.Integer: {
                                    int value = Convert.ToInt32(currentToken.Value.ToString());
                                    WriteValue(value, JsonValueType.INTEGER);
                                    break;
                                }
                            case JsonToken.Float: {
                                    decimal value = Convert.ToDecimal(currentToken.Value.ToString());
                                    WriteValue(value, JsonValueType.DECIMAL);
                                    break;
                                }
                            case JsonToken.Boolean: {
                                    bool value = Convert.ToBoolean(currentToken.Value.ToString());
                                    WriteValue(value, JsonValueType.BOOLEAN);
                                    break;
                                }
                            case JsonToken.Date: {
                                    DateTime value = DateTime.Parse(currentToken.Value.ToString());
                                    WriteValue(value, JsonValueType.BYTES);
                                    break;
                                }
                            case JsonToken.Bytes: {
                                    byte[] value = (byte[])(currentToken.Value);
                                    WriteValue(value, JsonValueType.BYTES);
                                    break;
                                }
                            case JsonToken.Null: {
                                    WriteValue("", JsonValueType.NULL);
                                    break;
                                }
                            case JsonToken.EndObject: {
                                    lineage.Pop();
                                    xwriter.WriteProcessingInstruction($"object-end", $"");
                                    if (lineage.Peek().Key != ROOT)
                                        xwriter.WriteEndElement();
                                    break;
                                }
                            case JsonToken.EndArray: {
                                    lineage.Pop();
                                    xwriter.WriteProcessingInstruction($"array-end", $"{arrays.Pop().GetHashCode()}");
                                    break;
                                }
                            default:
                                break;
                        }

                    }

                    xwriter.WriteEndDocument();
                    lineage.Pop();
                }
            }


            return doc;
        }


        private void WriteObjectOrArray(JsonContextType jsonContextType) {

            if (isRoot)
                WriteRoot(jsonContextType);
            else {
                if (lineage.Peek().Type == JsonContextType.ARRAY) {
                    if (lineage.Peek().ChildCount > 0)
                        xwriter.WriteStartElement(lineage.Peek().Key);
                    lineage.Peek().ChildCount++;
                }

                if (lineage.Peek().Type == JsonContextType.ARRAY)
                    xwriter.WriteProcessingInstruction($"array-item", $"{arrays.Peek().GetHashCode()}");

                if (jsonContextType == JsonContextType.OBJECT)
                    xwriter.WriteProcessingInstruction($"object-start", "");

                lineage.Push(new Parent { Key = currentKey, Type = jsonContextType });
            }
        }


        private void WriteRoot(JsonContextType jsonContextType) {
            isRoot = false;
            xwriter.WriteStartElement(
                    JSONXML_CONVERTER_NAMESPACE_PREFIX,
                    ROOT,
                    JSONXML_CONVERTER_NAMESPACE_URI);
            xwriter.WriteAttributeString("xmlns",
                    JSONXML_CONVERTER_NAMESPACE_PREFIX, null,
                    JSONXML_CONVERTER_NAMESPACE_URI);

            lineage.Push(new Parent { Key = currentKey, Type = jsonContextType });

            if (jsonContextType == JsonContextType.OBJECT)
                xwriter.WriteProcessingInstruction($"object-start", "");

            return;
        }

        private void WriteValue<T>(T value, string jsonValueType) {

            if (lineage.Peek().Type == JsonContextType.ARRAY) {
                if (lineage.Peek().ChildCount > 0) {
                    xwriter.WriteStartElement(lineage.Peek().Key);
                }
                xwriter.WriteProcessingInstruction($"array-item", $"{arrays.Peek().GetHashCode()}");
                lineage.Peek().ChildCount++;
            } else if (lineage.Peek().Type == JsonContextType.KEY) {
                lineage.Pop();
            }
            xwriter.WriteProcessingInstruction($"{jsonValueType}-start", null);
            xwriter.WriteValue(value);
            xwriter.WriteProcessingInstruction($"{jsonValueType}-end", null);
            xwriter.WriteEndElement();
        }


    }


}
