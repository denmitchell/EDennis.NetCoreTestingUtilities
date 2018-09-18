using EDennis.NetCoreTestingUtilities.TestApi;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using Xunit;

namespace EDennis.NetCoreTestingUtilities.Tests {
    public class IActionResultExtensionsTests {

        public static HttpClient _client;
        static IActionResultExtensionsTests() {
            var webHostBuilder = Program.CreateWebHostBuilder(Array.Empty<string>());
            var server = new TestServer(webHostBuilder);
            _client = server.CreateClient();
        }

        [Theory]
        [InlineData("ok-object-result/int",200,"1")]
        [InlineData("ok-object-result/string", 200, "red")]
        [InlineData("ok-object-result/person",200,"{\"firstName\":\"Bob\",\"lastName\":\"Barker\"}")]
        [InlineData("bad-request-object-result/int",400,"1")]
        [InlineData("bad-request-object-result/string",400,"red")]
        [InlineData("bad-request-object-result/person",400, "{\"firstName\":\"Bob\",\"lastName\":\"Barker\"}")]
        [InlineData("json-result/person", 200, "{\"firstName\":\"Bob\",\"lastName\":\"Barker\"}")]
        [InlineData("ok-result",200,"")]
        [InlineData("not-found",404,"")]
        [InlineData("not-found-null", 404, "")]
        [InlineData("forbid",403,"")]
        public void Get(string path, int expectedStatusCode, string expectedObject) {
            var response = _client.GetAsync("api/test/" + path).Result;
            var actualStatusCode = (int)response.StatusCode;
            var actualObject = response.Content.ReadAsStringAsync().Result;
            if (expectedObject != null && expectedObject.Contains("firstName"))
                actualObject = CompressJson(actualObject);
            Assert.Equal(expectedStatusCode, actualStatusCode);
            Assert.Equal(expectedObject, actualObject);
        }

        private string CompressJson(string json) {
            return JToken.Parse(json).ToString(Newtonsoft.Json.Formatting.None);
        }


    }
}
