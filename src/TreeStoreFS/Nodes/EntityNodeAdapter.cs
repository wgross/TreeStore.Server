using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Management.Automation;
using System.Threading;
using TreeStore.Model.Abstractions;

namespace TreeStoreFS.Nodes
{
    public sealed class EntityNodeAdapter : NodeAdapterBase,
        // ItemProvider
        IGetItem

    {
        private readonly Lazy<EntityResult?> entity;

        public EntityNodeAdapter(ITreeStoreService treeStoreService, Guid entityId)
            : base(treeStoreService)
        {
            this.entity = new Lazy<EntityResult?>(() => Await(this.TreeStoreService.GetEntityByIdAsync(entityId, CancellationToken.None)));
        }

        /// <summary>
        /// Any tree store node has an Id.
        /// </summary>
        public Guid Id => this.Entity.Id;

        public EntityResult Entity => this.entity.Value ?? throw new InvalidOperationException("Entity wasn't loaded");

        PSObject? IGetItem.GetItem() => PSObject.AsPSObject(this.Entity);
    }
}