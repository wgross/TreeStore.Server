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

            this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category"));

            // ACT
            this.InvokeAndClear(ps => ps
                .AddCommand("New-ItemProperty")
                .AddParameter("Path", @"test:\child")
                .AddParameter("Name", "p1")
                .AddParameter("PropertyType", "long"));

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            var result = this.InvokeAndClear(ps => ps
                .AddCommand("Get-ItemProperty")
                .AddParameter("Path", @"test:\child")
                .AddParameter("Name", "p1"))
                .Single();

            Assert.Equal(FacetPropertyTypeValues.Long, result.Property<FacetPropertyTypeValues>("p1"));
        }

        #endregion New-ItemProperty -Path -Name -Type

        #region Rename-ItemProperty -Path -Name

        [Fact]
        public void Rename_facet_property()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category"));

            this.InvokeAndClear(ps => ps
                .AddCommand("New-ItemProperty")
                .AddParameter("Path", @"test:\child")
                .AddParameter("Name", "p1")
                .AddParameter("PropertyType", "long"));

            this.InvokeAndClear(ps => ps
               .AddCommand("Rename-ItemProperty")
               .AddParameter("Path", @"test:\child")
               .AddParameter("Name", "p1")
               .AddParameter("NewName", "p2"));

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("Get-ItemProperty")
                .AddParameter("Path", @"test:\child")
                .AddParameter("Name", "p2"))
                .Single();

            // ASSERT
            Assert.Equal(FacetPropertyTypeValues.Long, result.Property<FacetPropertyTypeValues>("p2"));
        }

        #endregion Rename-ItemProperty -Path -Name
    }
}