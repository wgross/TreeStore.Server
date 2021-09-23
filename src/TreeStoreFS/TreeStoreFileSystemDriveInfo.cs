using PowerShellFilesystemProviderBase.Providers;
using System;
using System.Management.Automation;

namespace TreeStoreFS
{
    public sealed class TreeStoreFileSystemDriveInfo : PowershellFileSystemDriveInfo
    {
        public TreeStoreFileSystemDriveInfo(Func<string, IServiceProvider> rootNodeProvider, PSDriveInfo driveInfo) : base(driveInfo, rootNodeProvider)
        {
        }
    }
}