using EDennis.NetCoreTestingUtilities.Extensions;
using JsonDiffPatch;
using Newtonsoft.Json.Linq;
using System;

namespace EDennis.NetCoreTestingUtilities {

    /// <summary>
    /// This class is used to provide more specific feedback when a JsonAssert
    /// unit test fails.  The expected values and actual values are returned as
    /// formatted JSON.  Also, a JsonDiffPatch is calculated and presented.  The
    /// class makes heavy use of Newtonsoft and JsonDiffPatch libraries.
    /// </summary>
    [Obsolete]
    public class JsonAssertException : Exception {
        public JsonAssertException(object expected, object actual) :
            base(new {  Expected = expected,
                        Actual = actual,
                        JsonDiff = GetJsonDiff(expected, actual)
                        }.ToJsonString()) { }

        /// <summary>
        /// Generates a JsonDiffPatch (array of DiffOperation objects)
        /// that summarizes the difference between expected and actual
        /// </summary>
        /// <param name="expected">The expected value for the JSON</param>
        /// <param name="actual">The actual value for the JSON</param>
        /// <returns></returns>
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
