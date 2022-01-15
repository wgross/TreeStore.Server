using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace TreeStore.Server.Host.IntegTest
{
    public class TreeStoreTestServer : WebApplicationFactory<Startup>
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            return Microsoft.Extensions.Hosting.Host
                .CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseEnvironment("IntegTest")
                    .UseStartup(this.StartUpFactory));
        }

        private Startup StartUpFactory(WebHostBuilderContext arg) => new IntegTestStartup(arg.Configuration);
    }
}