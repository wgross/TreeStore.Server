using PowerShellFilesystemProviderBase;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace TreeStoreFS.Test
{
    [Collection(nameof(PowerShell))]
    public class ContainerCommandProviderTest : CmdletProviderTestBase
    {
        #region New-Item -Path -ItemType -Value

        [Fact]
        public void PowerShell_creates_category()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            // ACT
            var result = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category")
                .Invoke()
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            Assert.Equal("child", result.Property<string>("PSChildName"));
            Assert.True(result.Property<bool>("PSIsContainer"));
            Assert.Equal("test", result.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", result.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", result.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\", result.Property<string>("PSParentPath"));
        }

        [Fact]
        public void PowerShell_creates_entity()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            // ACT
            var result = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "entity")
                .Invoke()
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            Assert.Equal("child", result.Property<string>("PSChildName"));
        }

        #endregion New-Item -Path -ItemType -Value

        #region Remove-Item -Path

        [Fact]
        public void PowerShell_removes_root_child_category()
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
            _ = this.PowerShell.AddCommand("Remove-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("Recurse") // TODO: remove asks b/c HasChildItems is always true => Avoid.
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            this.PowerShell.Commands.Clear();
            var childAfterRemove = this.PowerShell
                .AddCommand("Test-Path")
                .AddParameter("Path", @"test:\child")
                .Invoke()
                .Single();

            Assert.False((bool)childAfterRemove.BaseObject);
        }

        [Fact]
        public void PowerShell_removes_root_child_entity()
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
            _ = this.PowerShell.AddCommand("Remove-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("Recurse") // TODO: remove asks b/c HasChildItems is always true => Avoid.
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            this.PowerShell.Commands.Clear();
            var childAfterRemove = this.PowerShell
                .AddCommand("Test-Path")
                .AddParameter("Path", @"test:\child")
                .Invoke()
                .Single();

            Assert.False((bool)childAfterRemove.BaseObject);
        }

        #endregion Remove-Item -Path

        #region Get-ChildItem -Path -Recurse

        [Fact]
        public void PowerShell_retrieves_roots_child_categories()
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
            var result = this.PowerShell.AddCommand("Get-ChildItem")
                .AddParameter("Path", @"test:\")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);

            var psobject = result[0];

            Assert.Equal("child", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\", psobject.Property<string>("PSParentPath"));
        }

        [Fact]
        public void PowerShell_retrieves_root_child_entities()
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
            var result = this.PowerShell.AddCommand("Get-ChildItem")
                .AddParameter("Path", @"test:\")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);

            var psobject = result[0];

            Assert.Equal("child", psobject.Property<string>("PSChildName"));
            Assert.False(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\", psobject.Property<string>("PSParentPath"));
        }

        #endregion Get-ChildItem -Path -Recurse

        #region Copy-Item -Path -Destination -Recurse

        [Fact]
        public void PowerShell_copies_child_category_keeping_name()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            // create a destination category
            _ = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child1")
                .AddParameter("ItemType", "category")
                .Invoke()
                .ToArray();
            this.PowerShell.Commands.Clear();

            var childBeforeCopy = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child2")
                .AddParameter("ItemType", "category")
                .Invoke()
                .Single();
            this.PowerShell.Commands.Clear();

            // ACT
            // copy child2 under child1
            this.PowerShell.AddCommand("Copy-Item")
                .AddParameter("Path", @"test:\child2")
                .AddParameter("Destination", @"test:\child1")
                .Invoke();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            this.PowerShell.Commands.Clear();
            var newChild = this.PowerShell
                .AddCommand("Get-Item")
                .AddParameter("Path", @"test:\child1\child2")
                .Invoke()
                .Single();

            Assert.False(this.PowerShell.HadErrors);

            Assert.NotEqual(childBeforeCopy.Property<Guid>("Id"), newChild.Property<Guid>("Id"));
            Assert.Equal(childBeforeCopy.Property<string>("Name"), newChild.Property<string>("Name"));
        }

        [Fact]
        public void PowerShell_copies_child_entity_keeping_name()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            // create a destination category
            _ = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child1")
                .AddParameter("ItemType", "category")
                .Invoke()
                .ToArray();
            this.PowerShell.Commands.Clear();

            var childBeforeCopy = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child2")
                .AddParameter("ItemType", "entity")
                .Invoke()
                .Single();
            this.PowerShell.Commands.Clear();

            // ACT
            // copy child2 under child1
            this.PowerShell.AddCommand("Copy-Item")
                .AddParameter("Path", @"test:\child2")
                .AddParameter("Destination", @"test:\child1")
                .Invoke();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            this.PowerShell.Commands.Clear();
            var newChild = this.PowerShell
                .AddCommand("Get-Item")
                .AddParameter("Path", @"test:\child1\child2")
                .Invoke()
                .Single();

            Assert.False(this.PowerShell.HadErrors);

            Assert.NotEqual(childBeforeCopy.Property<Guid>("Id"), newChild.Property<Guid>("Id"));
            Assert.Equal(childBeforeCopy.Property<string>("Name"), newChild.Property<string>("Name"));
        }

        #endregion Copy-Item -Path -Destination -Recurse

        #region Rename-Item -Path

        [Fact]
        public void PowerShell_renames_category()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category")
                .Invoke();
            this.PowerShell.Commands.Clear();

            // ACT
            this.PowerShell.AddCommand("Rename-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("NewName", "changed")
                .Invoke();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            this.PowerShell.Commands.Clear();
            var exists = this.PowerShell.AddCommand("Test-Path")
                .AddParameter("Path", @"test:\changed")
                .AddParameter("PathType", "Container")
                .Invoke()
                .Single();

            Assert.True((bool)exists.BaseObject);
        }

        #endregion Rename-Item -Path
    }
}