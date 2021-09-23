using PowerShellFilesystemProviderBase;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace TreeStoreFS.Test
{
    public class DriveCmdletProviderTest : CmdletProviderTestBase
    {
        public DriveCmdletProviderTest()
        {
            TreeStoreFileSystemProvider.RootNodeProvider = _ => null; //  new DictionaryContainerAdapter(new Dictionary<string, object?>());

            this.PowerShell.AddCommand("Import-Module").AddArgument("./TreeStoreFS.dll").Invoke();
            this.PowerShell.Commands.Clear();
        }

        [Fact]
        public void Powershell_creates_new_drive()
        {
            // ACT
            var result = this.PowerShell.AddCommand("New-PSDrive")
              .AddParameter("PSProvider", "TreeStoreFS")
              .AddParameter("Name", "test")
              .AddParameter("Root", "")
              .Invoke()
              .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            var psdriveInfo = result.Single().Unwrap<PSDriveInfo>();

            Assert.Equal("test", psdriveInfo.Name);
            Assert.Equal("", psdriveInfo.Root);
        }
    }
}