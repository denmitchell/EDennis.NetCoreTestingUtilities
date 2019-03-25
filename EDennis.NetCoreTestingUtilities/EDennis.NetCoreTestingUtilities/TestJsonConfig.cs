using Newtonsoft.Json.Linq;

namespace EDennis.NetCoreTestingUtilities {

    public class TestJsonConfig {
        public string DatabaseName { get; set; }
        public string ProjectName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public string TestScenario { get; set; }
        public string TestCase { get; set; }
        public string ServerName { get; set; }
        public string ConnectionString { get; set; }
        public string TestJsonSchema { get; set; }
        public string TestJsonTable { get; set; }


        public override string ToString() {
            return JToken.FromObject(this).ToString(Newtonsoft.Json.Formatting.None);
        }

    }

}
