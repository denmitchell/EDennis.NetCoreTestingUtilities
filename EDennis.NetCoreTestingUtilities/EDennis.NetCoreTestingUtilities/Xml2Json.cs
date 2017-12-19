using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace EDennis.NetCoreTestingUtilities.Json {

    public class Xml2Json {

        public JToken ConvertToJson(XmlDocument doc) {

            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            string jsonValueType = null;
            HashSet<int> arrays = new HashSet<int>();

            using (JsonWriter jwriter = new JsonTextWriter(sw)) {

                using (XmlReader xreader = XmlReader.Create(new StringReader(doc.InnerXml))) {

                    while (xreader.Read()) {
                        switch (xreader.NodeType) {
                            case XmlNodeType.Document:
                                break;
                            case XmlNodeType.Element: {
                                    if (xreader.Prefix != Json2Xml.JSONXML_CONVERTER_NAMESPACE_PREFIX
                                            && xreader.Name != Json2Xml.ROOT
                                            && jwriter.WriteState != Newtonsoft.Json.WriteState.Array)
                                        jwriter.WritePropertyName(xreader.Name);

                                    break;
                                }
                            case XmlNodeType.Text: {
                                    switch (jsonValueType) {
                                        case JsonValueType.BOOLEAN:
                                            jwriter.WriteValue(Convert.ToBoolean(xreader.Value.ToLower()));
                                            break;
                                        case JsonValueType.BYTES:
                                            jwriter.WriteValue(Encoding.ASCII.GetBytes(xreader.Value));
                                            break;
                                        case JsonValueType.DATE:
                                            jwriter.WriteValue(Convert.ToDateTime(xreader.Value));
                                            break;
                                        case JsonValueType.DECIMAL:
                                            jwriter.WriteValue(Convert.ToDecimal(xreader.Value));
                                            break;
                                        case JsonValueType.INTEGER:
                                            jwriter.WriteValue(Convert.ToInt32(xreader.Value));
                                            break;
                                        case JsonValueType.STRING:
                                            jwriter.WriteValue(xreader.Value);
                                            break;
                                        default:
                                            jwriter.WriteValue((bool?)null);
                                            break;
                                    }
                                    break;
                                }
                            case XmlNodeType.EndElement:
                                break;
                            case XmlNodeType.ProcessingInstruction: {
                                    if (xreader.Name == "object-start")
                                        jwriter.WriteStartObject();
                                    else if (xreader.Name == "object-end")
                                        jwriter.WriteEndObject();
                                    else if (xreader.Name == "array-item") {
                                        int id = Convert.ToInt32(xreader.Value);
                                        if (!arrays.Contains(id))
                                            jwriter.WriteStartArray();
                                        arrays.Add(id);
                                    } else if (xreader.Name == "array-end") {
                                        int id = Convert.ToInt32(xreader.Value);
                                        if (arrays.Contains(id))
                                            jwriter.WriteEndArray();
                                        arrays.Remove(id);
                                    } else if (xreader.Name.EndsWith("-start"))
                                        jsonValueType = xreader.Name.Remove(xreader.Name.Length - 6);
                                    break;
                                }
                            default:
                                break;
                        }
                    }

                }
            }

            return JToken.Parse(sb.ToString());
        }


    }

}
