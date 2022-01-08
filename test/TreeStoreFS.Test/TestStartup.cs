using Microsoft.Extensions.Configuration;
using TreeStore.Server.Host;

namespace TreeStoreFS.Test
{
    public class FileSystemTestStartup : Startup
    {
        public FileSystemTestStartup(IConfiguration configuration) : base(configuration)
        {
        }
    }
}