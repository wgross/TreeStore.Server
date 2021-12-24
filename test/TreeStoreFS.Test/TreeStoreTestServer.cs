using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using TreeStore.Server.Host;

namespace TreeStoreFS.Test
{
    public sealed class TreeStoreTestServer : WebApplicationFactory<Startup>
    {
        protected override IHostBuilder CreateHostBuilder()
        {
            return Microsoft.Extensions.Hosting.Host
               .CreateDefaultBuilder()
               .ConfigureWebHostDefaults(webBuilder =>
               {
                   webBuilder.UseStartup(this.StartUpFactory);
               });
        }

        private Startup StartUpFactory(WebHostBuilderContext arg) => new FileSystemTestStartup(arg.Configuration);
    }
}