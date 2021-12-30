using PowerShellFilesystemProviderBase;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace TreeStoreFS.Test
{
    [Collection(nameof(PowerShell))]
    public class ItemCmdletProviderTest : CmdletProviderTestBase
    {
        #region Get-Item -Path

        [Fact]
        public void Powershell_reads_root_category()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            // ACT
            var result = this.PowerShell.AddCommand("Get-Item")
                .AddParameter("Path", @"test:\")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            var psobject = result.Single();

            Assert.Equal(string.Empty, psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\", psobject.Property<string>("PSPath"));
            Assert.Equal(string.Empty, psobject.Property<string>("PSParentPath"));
        }

        [Fact]
        public void Powershell_reads_roots_child_category()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            var child = this.PowerShell
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category")
                .Invoke()
                .Single();
            this.PowerShell.Commands.Clear();

            // ACT
            var result = this.PowerShell.AddCommand("Get-Item")
                .AddParameter("Path", @"test:\child")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            var psobject = result.Single();

            Assert.Equal(child.Property<Guid>("Id"), psobject.Property<Guid>("Id"));
            Assert.Equal("child", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:", psobject.Property<string>("PSParentPath"));
        }

        [Fact]
        public void Powershell_reads_roots_child_entity()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            var child = this.PowerShell
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "entity")
                .Invoke()
                .Single();
            this.PowerShell.Commands.Clear();

            // ACT
            var result = this.PowerShell.AddCommand("Get-Item")
                .AddParameter("Path", @"test:\child")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            var psobject = result.Single();

            Assert.Equal(child.Property<Guid>("Id"), psobject.Property<Guid>("Id"));
            Assert.Equal("child", psobject.Property<string>("PSChildName"));
            Assert.False(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:", psobject.Property<string>("PSParentPath"));
        }

        [Fact]
        public void Powershell_reads_root_category_grandchild()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            var child = this.PowerShell
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child1")
                .AddParameter("ItemType", "category")
                .Invoke()
                .Single();
            this.PowerShell.Commands.Clear();

            var grandChild = this.PowerShell
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child1\child2")
                .AddParameter("ItemType", "category")
                .Invoke()
                .Single();
            this.PowerShell.Commands.Clear();

            // ACT
            var result = this.PowerShell.AddCommand("Get-Item")
                .AddParameter("Path", @"test:\child1\child2")
                .Invoke()
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            Assert.NotEqual(child.Property<Guid>("Id"), grandChild.Property<Guid>("Id"));
            Assert.NotEqual(child.Property<Guid>("Id"), result.Property<Guid>("Id"));
            Assert.Equal(grandChild.Property<Guid>("Id"), result.Property<Guid>("Id"));
            Assert.Equal("child2", result.Property<string>("PSChildName"));
            Assert.True(result.Property<bool>("PSIsContainer"));
            Assert.Equal("test", result.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", result.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child1\child2", result.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child1", result.Property<string>("PSParentPath"));
        }

        #endregion Get-Item -Path

        #region Test-Path -Path -ItemType

        [Fact]
        public void Powershell_tests_category_path_as_container()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            _ = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category")
                .Invoke()
                .ToArray();
            this.PowerShell.Commands.Clear();

            // ACT
            var result = this.PowerShell
                .AddCommand("Test-Path")
                .AddParameter("Path", @"test:\child")
                .AddParameter("PathType", "container")
                .Invoke()
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True((bool)result.BaseObject);
        }

        [Fact]
        public void Powershell_tests_category_path_as_leaf()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            _ = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category")
                .Invoke()
                .ToArray();
            this.PowerShell.Commands.Clear();

            // ACT
            var result = this.PowerShell
                .AddCommand("Test-Path")
                .AddParameter("Path", @"test:\child")
                .AddParameter("PathType", "leaf")
                .Invoke()
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.False((bool)result.BaseObject);
        }

        [Fact]
        public void Powershell_tests_entity_path_as_leaf()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            _ = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "entity")
                .Invoke()
                .ToArray();
            this.PowerShell.Commands.Clear();

            // ACT
            var result = this.PowerShell
                .AddCommand("Test-Path")
                .AddParameter("Path", @"test:\child")
                .AddParameter("PathType", "Leaf")
                .Invoke()
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True((bool)result.BaseObject);
        }

        #endregion Test-Path -Path -ItemType
    }
}