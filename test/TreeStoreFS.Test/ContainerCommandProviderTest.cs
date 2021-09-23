﻿using PowerShellFilesystemProviderBase;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace TreeStoreFS.Test
{
    public class ContainerCommandProviderTest : CmdletProviderTestBase
    {
        #region New-Item -Path -ItemType -Value

        [Fact]
        public void Powershell_creates_category()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            // ACT
            var result = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            var psobject = result.Single();

            Assert.Equal("child", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\", psobject.Property<string>("PSParentPath"));
        }

        #endregion New-Item -Path -ItemType -Value

        #region Remove-Item -Path

        [Fact]
        public void Powershell_removes_root_child_node()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("ItemType", "category")
                .Invoke()
                .ToArray();
            this.PowerShell.Commands.Clear();

            // ACT
            var result = this.PowerShell.AddCommand("Remove-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("Recurse") // TODO: remove asks bc HasChildItems is always true => Avoid.
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
        public void Powershell_retrieves_roots_childnodes()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.PowerShell.AddCommand("New-Item")
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

            var psobject = result.ElementAt(0);

            Assert.Equal("child", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TreeStoreFS", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\child", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TreeStoreFS\TreeStoreFS::test:\", psobject.Property<string>("PSParentPath"));
        }

        #endregion Get-ChildItem -Path -Recurse

        #region Copy-Item -Path -Destination -Recurse

        [Fact]
        public void Powershell_copies_child_with_same_name()
        {
            // ARRANGE
            this.ArrangeFileSystem();

            this.PowerShell.AddCommand("New-Item")
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
    }
}