using PowerShellFilesystemProviderBase.Providers;
using System;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace TreeStoreFS
{
    [CmdletProvider(Id, ProviderCapabilities.None)]
    public sealed class TreeStoreFileSystemProvider : PowerShellFileSystemProviderBase
    {
        public const string Id = "TreeStoreFS";

        /// <summary>
        /// Creates the root node. The inout string ois the drive name.
        /// </summary>
        public static Func<string, IServiceProvider>? RootNodeProvider { get; set; }

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            if (RootNodeProvider is null)
                throw new InvalidOperationException(nameof(RootNodeProvider));

            return new TreeStoreFileSystemDriveInfo(RootNodeProvider, new PSDriveInfo(
               name: drive.Name,
               provider: drive.Provider,
               root: drive.Root,
               description: drive.Description,
               credential: drive.Credential));
        }
    }
}