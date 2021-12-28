using PowerShellFilesystemProviderBase;
using System.Linq;
using TreeStore.Model.Abstractions;
using Xunit;

namespace TreeStoreFS.Test
{
    [Collection(nameof(PowerShell))]
    public class DynamicPropertyCmdletProviderTest : CmdletProviderTestBase
    {
        #region New-ItemProperty -Path -Name -Type

        [Fact]
        public void Creates_new_facet_property()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            _ = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category")
                .Invoke()
                .Single();
            this.PowerShell.Commands.Clear();

            // ACT
            this.PowerShell
                .AddCommand("New-ItemProperty")
                .AddParameter("Path", @"test:\child")
                .AddParameter("Name", "p1")
                .AddParameter("PropertyType", "long")
                .Invoke();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
        }

        #endregion New-ItemProperty -Path -Name -Type

        #region Rename-ItemProperty -Path -Name

        [Fact]
        public void Rename_facet_property()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            _ = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category")
                .Invoke()
                .Single();
            this.PowerShell.Commands.Clear();

            this.PowerShell
              .AddCommand("New-ItemProperty")
              .AddParameter("Path", @"test:\child")
              .AddParameter("Name", "p1")
              .AddParameter("PropertyType", "long")
              .Invoke();
            this.PowerShell.Commands.Clear();

            this.PowerShell
               .AddCommand("Rename-ItemProperty")
               .AddParameter("Path", @"test:\child")
               .AddParameter("Name", "p1")
               .AddParameter("NewName", "p2")
               .Invoke();
            this.PowerShell.Commands.Clear();

            // ACT
            var result = this.PowerShell
                .AddCommand("Get-ItemProperty")
                .AddParameter("Path", @"test:\child")
                .AddParameter("Name", "p2")
                .Invoke()
                .Single();

            // ASSERT
            Assert.Equal(FacetPropertyTypeValues.Long, result.Property<FacetPropertyTypeValues>("p2"));
        }

        #endregion Rename-ItemProperty -Path -Name
    }
}