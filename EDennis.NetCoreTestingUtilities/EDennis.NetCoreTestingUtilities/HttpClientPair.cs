using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;


namespace EDennis.NetCoreTestingUtilities {

    /// <summary>
    /// This class is analogous to the WebApplicationFactory class used
    /// as a ClassFixture in unit testing; however, this class wraps
    /// two TestServers (instead of one) to accommodate and internal
    /// API and external API.
    /// 
    /// For Unit tests, see https://github.com/denmitchell/SampleApi
    /// </summary>
    public class HttpClientPair<TInternalStartup,TExternalStartup> : IDisposable 
        where TInternalStartup : class 
        where TExternalStartup : class
        {

        //clients for internal and external APIs
        public static HttpClient internalClient;
        public static HttpClient externalClient;


        /// <summary>
        /// Constructs a new HttpClientCollection with 
        /// 
        /// </summary>
        public HttpClientPair() {

            var ports = PortInspector.GetAvailablePorts(5000, 2);

            //setup the internal server and client
            TestServer internalServer = new TestServer(new WebHostBuilder()
                .UseStartup<TInternalStartup>()
                );
            internalServer.BaseAddress = new Uri($"http://localhost:{ports[0]}/");
            internalClient = internalServer.CreateClient();


            //setup the external server and client
            TestServer externalServer = new TestServer(new WebHostBuilder()
                .UseStartup<TExternalStartup>()
                //*** add reference to internal client as singleton
                .ConfigureServices(services => {
                    services.AddSingleton(internalClient);
                    })
                );
            externalServer.BaseAddress = new Uri($"http://localhost:{ports[1]}/");
            externalClient = externalServer.CreateClient();

        }

        /// <summary>
        /// No need to dispose anything explicitly
        /// </summary>
        public void Dispose() { }
    }

}
