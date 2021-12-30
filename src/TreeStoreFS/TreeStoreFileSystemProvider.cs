using Microsoft.Extensions.Logging.Abstractions;
using PowerShellFilesystemProviderBase.Providers;
using System;
using System.Management.Automation;
using System.Management.Automation.Provider;
using System.Net.Http;
using TreeStore.Server.Client;
using TreeStoreFS.Nodes;

namespace TreeStoreFS
{
    [CmdletProvider(Id, ProviderCapabilities.None)]
    public sealed class TreeStoreFileSystemProvider : PowerShellFileSystemProviderBase
    {
        public const string Id = "TreeStoreFS";

        /// <summary>
        /// Creates the root node. The input string is the drive name.
        /// </summary>
        public static Func<string, IServiceProvider>? RootNodeProvider { get; set; }

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            if (RootNodeProvider is null)
            {
                // Consume the URI from the root.
                // The drive will be initialized with "/" in any case.
                if (!Uri.TryCreate(drive.Root, new(), out var uri))
                    throw new ArgumentException($"Root('{drive.Root}' couldn't be resolved as URL");

                var client = new HttpClient
                {
                    BaseAddress = uri
                };

                RootNodeProvider = _ => new RootCategoryAdapter(new TreeStoreClient(client, new NullLogger<TreeStoreClient>()));
            }

            return new TreeStoreFileSystemDriveInfo(RootNodeProvider, new PSDriveInfo(
               name: drive.Name,
               provider: drive.Provider,
               root: $@"{drive.Name}:\",
               description: drive.Description,
               credential: drive.Credential));
        }
    }
}