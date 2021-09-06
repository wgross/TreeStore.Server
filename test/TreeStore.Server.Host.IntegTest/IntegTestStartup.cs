using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace TreeStore.Server.Host.IntegTest
{
    public class IntegTestStartup : Startup
    {
        public IntegTestStartup(IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
            : base(hostingEnvironment, configuration)
        { }
    }
}