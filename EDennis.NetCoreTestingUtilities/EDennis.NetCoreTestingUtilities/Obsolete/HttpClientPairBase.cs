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
    [Obsolete]
    public abstract class HttpClientPairBase<TInternalStartup,TExternalStartup> : IDisposable 
        where TInternalStartup : class 
        where TExternalStartup : class
        {

        //clients for internal and external APIs
        public static HttpClient InternalClient { get; set; }
        public static HttpClient ExternalClient { get; set; }

        public abstract IConfigurationBuilder InternalBuilder { get; }
        public abstract IConfigurationBuilder ExternalBuilder { get; }



        public HttpClientPairBase() {

            var ports = PortInspector.GetAvailablePorts(5000, 2);

            //configure internal WebHostBuilder
            var iwBuilder = new WebHostBuilder();
            iwBuilder.UseStartup<TInternalStartup>();
            if (InternalBuilder != null)
                iwBuilder.UseConfiguration(InternalBuilder.Build());

            //setup the internal server and client
            TestServer internalServer = new TestServer(iwBuilder);
            internalServer.BaseAddress = new Uri($"http://localhost:{ports[0]}/");
            InternalClient = internalServer.CreateClient();


            //configure external WebHostBuilder
            var ewBuilder = new WebHostBuilder();
            ewBuilder.UseStartup<TExternalStartup>();
            ewBuilder.ConfigureServices(services => {
                services.AddSingleton(InternalClient);
            });
            if (ExternalBuilder != null)
                ewBuilder.UseConfiguration(ExternalBuilder.Build());


            //setup the external server and client
            TestServer externalServer = new TestServer(ewBuilder);
            externalServer.BaseAddress = new Uri($"http://localhost:{ports[1]}/");
            ExternalClient = externalServer.CreateClient();

        }

        /// <summary>
        /// No need to dispose anything explicitly
        /// </summary>
        public void Dispose() { }
    }

}
