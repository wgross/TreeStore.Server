using PowerShellFilesystemProviderBase;
using System.Linq;
using TreeStore.Model.Abstractions;
using Xunit;

namespace TreeStoreFS.Test
{
    [Collection(nameof(PowerShell))]
    public class ItemPropertyCommandProviderTest : CmdletProviderTestBase
    {
        #region Get-ItemProperty -Path -Name

        [Fact]
        public void Reads_facet_property_by_name()
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

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("Get-ItemProperty")
                .AddParameter("Path", @"test:\child")
                .AddParameter("Name", "p1"))
                .Single();

            // ASSERT
            Assert.Equal(FacetPropertyTypeValues.Long, result.Property<FacetPropertyTypeValues>("p1"));
        }

        [Fact]
        public void Reads_facet_property_value_by_name()
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
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child\item")
                .AddParameter("ItemType", "entity"));

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("Get-ItemProperty")
                .AddParameter("Path", @"test:\child\item")
                .AddParameter("Name", "p1"))
                .Single();

            // ASSERT
            Assert.Null(result.Property<long?>("p1"));
        }

        #endregion Get-ItemProperty -Path -Name

        #region Set-ItemProperty -Path -Name -Value

        [Fact]
        public void Update_item_property_value()
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
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child\item")
                .AddParameter("ItemType", "entity"));

            // ACT
            this.InvokeAndClear(ps => ps
                .AddCommand("Set-ItemProperty")
                .AddParameter("Path", @"test:\child\item")
                .AddParameter("Name", "p1")
                .AddParameter("Value", (long)17));

            // ASSERT
            var result = this.InvokeAndClear(ps => ps
               .AddCommand("Get-ItemProperty")
               .AddParameter("Path", @"test:\child\item")
               .AddParameter("Name", "p1"))
               .Single();

            Assert.Equal(17, result.Property<long?>("p1"));
        }

        [Theory]
        [InlineData((object)(int)17)]
        [InlineData((object)(short)17)]
        [InlineData((object)(ushort)17)]
        [InlineData((object)(uint)17)]
        public void Update_item_long_property_value_with_shorter_int(object shorterInt)
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
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child\item")
                .AddParameter("ItemType", "entity"));

            // ACT
            this.InvokeAndClear(ps => ps
                .AddCommand("Set-ItemProperty")
                .AddParameter("Path", @"test:\child\item")
                .AddParameter("Name", "p1")
                .AddParameter("Value", shorterInt));

            // ASSERT
            var result = this.InvokeAndClear(ps => ps
               .AddCommand("Get-ItemProperty")
               .AddParameter("Path", @"test:\child\item")
               .AddParameter("Name", "p1"))
               .Single();

            Assert.Equal(17, result.Property<long?>("p1"));
        }

        #endregion Set-ItemProperty -Path -Name -Value

        #region Clear-ItemProperty -Path -Name

        [Fact]
        public void Clear_item_property_value()
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
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child\item")
                .AddParameter("ItemType", "entity"));

            this.InvokeAndClear(ps => ps
                .AddCommand("Set-ItemProperty")
                .AddParameter("Path", @"test:\child\item")
                .AddParameter("Name", "p1")
                .AddParameter("Value", (long)17));

            // ACT
            this.InvokeAndClear(ps => ps
                .AddCommand("Clear-ItemProperty")
                .AddParameter("Path", @"test:\child\item")
                .AddParameter("Name", "p1"));

            // ASSERT
            var result = this.InvokeAndClear(ps => ps
               .AddCommand("Get-ItemProperty")
               .AddParameter("Path", @"test:\child\item")
               .AddParameter("Name", "p1"))
               .Single();

            Assert.Null(result.Property<long?>("p1"));
        }

        #endregion Clear-ItemProperty -Path -Name
    }
}