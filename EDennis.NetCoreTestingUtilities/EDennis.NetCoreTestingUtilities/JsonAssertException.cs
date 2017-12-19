using EDennis.NetCoreTestingUtilities.Extensions;
using JsonDiffPatch;
using Newtonsoft.Json.Linq;
using System;

namespace EDennis.NetCoreTestingUtilities {

    public class JsonAssertException : Exception {
        public JsonAssertException(object expected, object actual) :
            base(new {  Expected = expected,
                        Actual = actual,
                        JsonDiff = GetJsonDiff(expected, actual)
                        }.ToJsonString()) { }


        private static DiffOperation[] GetJsonDiff(object expected, object actual) {
            var expectedJson = JToken.FromObject(expected);
            var actualJson = JToken.FromObject(actual);
            var patchDoc = new JsonDiffer().Diff(expectedJson, actualJson, false);
            return JArray.Parse(patchDoc.ToString()).ToObject<DiffOperation[]>();
        }

        /// <summary>
        /// This class is used to hold the JsonDiff operations required
        /// to go from the ExpectedOutput to the ActualOutput in 
        /// the Error object.  
        /// </summary>
        internal class DiffOperation {
            public string op { get; set; }
            public string path { get; set; }
        }

    }
}
