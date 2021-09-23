using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Management.Automation;
using TreeStore.Server.Client;
using TreeStoreFS.Nodes;

namespace TreeStoreFS.Test
{
    public abstract class CmdletProviderTestBase
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected PowerShell PowerShell { get; }

        protected TreeStoreTestServer TreeStoreServer { get; }

        public CmdletProviderTestBase()
        {
            this.PowerShell = PowerShell.Create();
            this.TreeStoreServer = new TreeStoreTestServer();
        }

        public void Dispose()
        {
            this.Mocks.VerifyAll();
        }

        public void ArrangeFileSystem()
        {
            TreeStoreFileSystemProvider.RootNodeProvider = _ =>
            {
                var client = this.TreeStoreServer.CreateClient();

                return new RootCategoryAdapter(new TreeStoreClient(this.TreeStoreServer.CreateClient(), new NullLogger<TreeStoreClient>()));
            };

            this.PowerShell.AddCommand("Import-Module")
                .AddArgument("./TreeStoreFS.dll")
                .Invoke();
            this.PowerShell.Commands.Clear();
            this.PowerShell.AddCommand("New-PSDrive")
                .AddParameter("PSProvider", "TreeStoreFS")
                .AddParameter("Name", "test")
                .AddParameter("Root", "")
                .Invoke();
            this.PowerShell.Commands.Clear();
        }
    }
}