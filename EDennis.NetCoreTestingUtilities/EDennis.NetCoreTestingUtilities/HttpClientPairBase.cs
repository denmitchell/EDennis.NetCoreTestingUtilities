using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
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
    public abstract class HttpClientPairBase<TInternalStartup,TExternalStartup> : IDisposable 
        where TInternalStartup : class 
        where TExternalStartup : class
        {

        //clients for internal and external APIs
        public static HttpClient internalClient;
        public static HttpClient externalClient;

        public abstract IConfigurationBuilder InternalBuilder { get; }
        public abstract IConfigurationBuilder ExternalBuilder { get; }



        public HttpClientPairBase() {

            var ports = PortInspector.GetAvailablePorts(5000, 2);

            //configure internal WebHostBuilder
            var iwBuilder = new WebHostBuilder();
            if (InternalBuilder != null)
                iwBuilder.UseConfiguration(InternalBuilder.Build());

            iwBuilder.UseStartup<TInternalStartup>();

            //setup the internal server and client
            TestServer internalServer = new TestServer(iwBuilder);
            internalServer.BaseAddress = new Uri($"http://localhost:{ports[0]}/");
            internalClient = internalServer.CreateClient();


            //configure external WebHostBuilder
            var ewBuilder = new WebHostBuilder();
            ewBuilder.ConfigureServices(services => {
                services.AddSingleton(internalClient);
            });
            if (ExternalBuilder != null)
                ewBuilder.UseConfiguration(ExternalBuilder.Build());

            ewBuilder.UseStartup<TExternalStartup>();

            //setup the external server and client
            TestServer externalServer = new TestServer(ewBuilder);
            externalServer.BaseAddress = new Uri($"http://localhost:{ports[1]}/");
            externalClient = externalServer.CreateClient();

        }

        /// <summary>
        /// No need to dispose anything explicitly
        /// </summary>
        public void Dispose() { }
    }

}
