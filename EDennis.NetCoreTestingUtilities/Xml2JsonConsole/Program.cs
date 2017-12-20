using EDennis.NetCoreTestingUtilities.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Xml;

namespace Xml2JsonConsole
{
    class Program
    {
        static void Main(string[] args)
        {

            var j = new JsonToJxml();
            var json = File.ReadAllText("persons.json");
            var jtoken = JToken.Parse(json);
            XmlDocument doc = j.ConvertToJxml(jtoken);
            doc.Save("e:\\persons-jxml.xml");
            
            var x = new JxmlToJson();
            var jtoken2 = x.ConvertToJson(doc);
            var json2 = jtoken2.ToString();
            File.WriteAllText("e:\\persons-jxml.json",json2);
        }
    }
}
