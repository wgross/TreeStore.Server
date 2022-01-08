using Microsoft.AspNetCore.Mvc.Testing;
using TreeStore.Server.Host;

namespace TreeStoreFS.Test
{
    /// <summary>
    /// The FS integration test uses the default startup using the in memory database.
    /// </summary>
    public sealed class TreeStoreTestServer : WebApplicationFactory<Startup>
    {
    }
}