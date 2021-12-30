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
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category"))
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            Assert.Equal("child", result.Property<string>("PSChildName"));
            Assert.True(result.Property<bool>("PSIsContainer"));
            Assert.Equal("test", result.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", result.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", result.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:", result.Property<string>("PSParentPath"));
        }

        [Fact]
        public void PowerShell_creates_category_with_itemtype_directory()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "Directory"))
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            Assert.Equal("child", result.Property<string>("PSChildName"));
            Assert.True(result.Property<bool>("PSIsContainer"));
            Assert.Equal("test", result.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", result.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", result.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:", result.Property<string>("PSParentPath"));
        }

        [Fact]
        public void PowerShell_creates_entity()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "entity"))
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            Assert.Equal("child", result.Property<string>("PSChildName"));
            Assert.False(result.Property<bool>("PSIsContainer"));
            Assert.Equal("test", result.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", result.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", result.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:", result.Property<string>("PSParentPath"));
        }

        [Fact]
        public void PowerShell_creates_entity_with_itemtype_file()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "file"))
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            Assert.Equal("child", result.Property<string>("PSChildName"));
            Assert.False(result.Property<bool>("PSIsContainer"));
            Assert.Equal("test", result.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", result.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", result.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:", result.Property<string>("PSParentPath"));
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
        public void PowerShell_reads_roots_child_categories()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category"))
                .ToArray();

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("Get-ChildItem")
                .AddParameter("Path", @"test:\"))
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
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:", psobject.Property<string>("PSParentPath"));
        }

        [Fact]
        public void PowerShell_reads_root_child_entities()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "entity"))
                .ToArray();

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("Get-ChildItem")
                .AddParameter("Path", @"test:\"))
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
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:", psobject.Property<string>("PSParentPath"));
        }

        #endregion Get-ChildItem -Path -Recurse

        #region GetChildItem -Path -Name

        [Fact]
        public void PowerShell_reads_roots_child_category_names()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category"));

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("Get-ChildItem")
                .AddParameter("Path", @"test:\")
                .AddParameter("Name"))
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);

            var psobject = result[0];

            Assert.IsType<string>(psobject.ImmediateBaseObject);
            Assert.Equal("child", psobject.ImmediateBaseObject as string);
            Assert.Equal("child", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:", psobject.Property<string>("PSParentPath"));
        }

        [Fact]
        public void PowerShell_reads_root_child_entitiy_names()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "entity"))
                .ToArray();

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("Get-ChildItem")
                .AddParameter("Path", @"test:\")
                .AddParameter("Name"))
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);

            var psobject = result[0];

            Assert.IsType<string>(psobject.ImmediateBaseObject);
            Assert.Equal("child", psobject.ImmediateBaseObject as string);
            Assert.Equal("child", psobject.Property<string>("PSChildName"));
            Assert.False(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:", psobject.Property<string>("PSParentPath"));
        }

        #endregion GetChildItem -Path -Name

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

        #region Move-Item -Path -Destination

        [Fact]
        public void PowerShell_moves_child_category_keeping_name()
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
            // move child2 under child1
            this.PowerShell.AddCommand("Move-Item")
                .AddParameter("Path", @"test:\child2")
                .AddParameter("Destination", @"test:\child1")
                .Invoke();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            this.PowerShell.Commands.Clear();
            var movedChild = this.PowerShell
                .AddCommand("Get-Item")
                .AddParameter("Path", @"test:\child1\child2")
                .Invoke()
                .Single();

            Assert.Equal(childBeforeCopy.Property<Guid>("Id"), movedChild.Property<Guid>("Id"));
            Assert.Equal(childBeforeCopy.Property<string>("Name"), movedChild.Property<string>("Name"));

            this.PowerShell.Commands.Clear();
            var sourceExists = this.PowerShell
                .AddCommand("Test-Path")
                .AddParameter("Path", @"test:\child2")
                .Invoke()
                .Single();

            Assert.False(this.PowerShell.HadErrors);
            Assert.False((bool)sourceExists.BaseObject);
        }

        [Fact]
        public void PowerShell_moves_child_entity_keeping_name()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            // create a destination category
            var childBeforeCopy = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child1")
                .AddParameter("ItemType", "entity")
                .Invoke()
                .Single();
            this.PowerShell.Commands.Clear();

            _ = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child2")
                .AddParameter("ItemType", "category")
                .Invoke()
                .Single();
            this.PowerShell.Commands.Clear();

            // ACT
            // move child2 under child1
            this.PowerShell.AddCommand("Move-Item")
                .AddParameter("Path", @"test:\child1")
                .AddParameter("Destination", @"test:\child2")
                .Invoke();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            this.PowerShell.Commands.Clear();
            var movedChild = this.PowerShell
               .AddCommand("Get-Item")
               .AddParameter("Path", @"test:\child2\child1")
               .Invoke()
               .Single();

            Assert.Equal(childBeforeCopy.Property<Guid>("Id"), movedChild.Property<Guid>("Id"));
            Assert.Equal(childBeforeCopy.Property<string>("Name"), movedChild.Property<string>("Name"));

            this.PowerShell.Commands.Clear();
            var sourceExists = this.PowerShell
                .AddCommand("Test-Path")
                .AddParameter("Path", @"test:\child1")
                .Invoke()
                .Single();

            Assert.False(this.PowerShell.HadErrors);
            Assert.False((bool)sourceExists.BaseObject);
        }

        #endregion Move-Item -Path -Destination

        #region Resolve-Path -Path

        [Fact]
        public void Resolve_root_path()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.InvokeAndClear(ps => ps
                .AddCommand("Set-Location")
                .AddParameter("Path", @"test:\"));

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("Resolve-Path")
                .AddParameter("Path", "."))
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.NotNull(result);

            var pathInfo = result.ImmediateBaseObject is PathInfo pi ? pi : throw new Exception("nope");

            Assert.Equal(@"test:\", pathInfo.Path);
        }

        [Fact]
        public void Resolve_child_category_path()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category"))
                .Single();

            this.InvokeAndClear(ps => ps
                .AddCommand("Set-Location")
                .AddParameter("Path", @"test:\"));

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("Resolve-Path")
                .AddParameter("Path", "child"))
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.NotNull(result);

            var pathInfo = result.ImmediateBaseObject is PathInfo pi ? pi : throw new Exception("nope");

            Assert.Equal(@"test:\child", pathInfo.Path);
        }

        [Fact]
        public void Resolve_child_category_path_with_wildcard()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category"))
                .Single();

            this.InvokeAndClear(ps => ps
                .AddCommand("Set-Location")
                .AddParameter("Path", @"test:\"));

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("Resolve-Path")
                .AddParameter("Path", "ch*"))
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.NotNull(result);

            var pathInfo = result.ImmediateBaseObject is PathInfo pi ? pi : throw new Exception("nope");

            Assert.Equal(@"test:\child", pathInfo.Path);
        }

        [Fact]
        public void Resolve_child_entity_path_with_wildcard()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.InvokeAndClear(ps => ps
                .AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "entity"))
                .Single();

            this.InvokeAndClear(ps => ps
                .AddCommand("Set-Location")
                .AddParameter("Path", @"test:\"));

            // ACT
            var result = this.InvokeAndClear(ps => ps
                .AddCommand("Resolve-Path")
                .AddParameter("Path", "ch*"))
                .Single();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.NotNull(result);

            var pathInfo = result.ImmediateBaseObject is PathInfo pi ? pi : throw new Exception("nope");

            Assert.Equal(@"test:\child", pathInfo.Path);
        }

        #endregion Resolve-Path -Path
    }
}