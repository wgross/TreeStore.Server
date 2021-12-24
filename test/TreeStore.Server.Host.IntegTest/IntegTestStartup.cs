using Microsoft.Extensions.Configuration;

namespace TreeStore.Server.Host.IntegTest
{
    public class IntegTestStartup : Startup
    {
        public IntegTestStartup(IConfiguration configuration)
            : base(configuration)
        { }
    }
}