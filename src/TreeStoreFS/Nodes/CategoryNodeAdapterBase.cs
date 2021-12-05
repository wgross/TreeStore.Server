using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Common;
using TreeStore.Model.Abstractions;

namespace TreeStoreFS.Nodes
{
    public abstract class CategoryNodeAdapterBase : NodeAdapterBase,

        // Path Traversal
        IGetChildItems,
        // ItemCmdletProvider
        IGetItem, IItemExists,
        // ContainerCmdletProvider
        INewChildItem, IRemoveChildItem, ICopyChildItemRecursive, IRenameChildItem
    {
        protected CategoryNodeAdapterBase(ITreeStoreService treeStoreService)
            : base(treeStoreService)
        { }

        protected abstract CategoryResult Category { get; }

        /// <summary>
        /// Any tree store node has an Id.
        /// </summary>
        public Guid Id => this.Category.Id;

        #region IGetChildItems

        /// <inheritdoc/>
        public bool HasChildItems() => this.Category.Entities.Length > 0 || this.Category.Categories.Length > 0;

        /// <inheritdoc/>
        public IEnumerable<ProviderNode> GetChildItems()
        {
            if (!this.HasChildItems())
            {
                // the node was fetched before with the ids of entites and categories belonging to it.
                // if none were there the call back to fetch the children again is avoided.
                // Since the nodes only lives as long as a single path traversal lasts there in no risk to represent the
                // backends state wrongly here.
                return Array.Empty<ProviderNode>();
            }

            var result = Await(this.TreeStoreService.GetCategoriesByIdAsync(this.Category.Id, CancellationToken.None));

            if (result is null)
                return Array.Empty<ProviderNode>();

            IEnumerable<ProviderNode> childCategories = this.Category.Categories.Select(c => this.CreateCategoryNode(c));
            IEnumerable<ProviderNode> childEntities = this.Category.Entities.Select(e => this.CreateEntityNode(e));

            return childCategories.Union(childEntities).ToArray();
        }

        #endregion IGetChildItems

        #region IGetItem

        /// <inheritdoc/>
        PSObject IGetItem.GetItem() => PSObject.AsPSObject(this.Category);

        #endregion IGetItem

        #region INewChildItem

        /// <inheritdoc/>
        ProviderNode? INewChildItem.NewChildItem(string childName, string? itemTypeName, object? newItemValue)
        {
            return Guard.Against.Null(itemTypeName, nameof(itemTypeName)).ToLowerInvariant() switch
            {
                "category" => this.CreateCategoryNode(Await(this.NewChildContainer(this.Category.Id, Guard.Against.Null(childName, nameof(childName))))),
                "entity" => this.CreateEntityNode(Await(this.NewChildEntity(this.Category.Id, Guard.Against.Null(childName, nameof(childName))))),

                _ => throw new NotImplementedException()
            };
        }

        private async Task<EntityResult> NewChildEntity(Guid parentId, string childName)
        {
            return await this.TreeStoreService.CreateEntityAsync(
               new CreateEntityRequest(
                   Name: childName,
                   CategoryId: parentId),
                   CancellationToken.None).ConfigureAwait(false);
        }

        private async Task<CategoryResult> NewChildContainer(Guid parentId, string childName)
        {
            return await this.TreeStoreService.CreateCategoryAsync(
                new CreateCategoryRequest(
                    Name: childName,
                    ParentId: parentId,
                    Facet: null),
                    CancellationToken.None).ConfigureAwait(false);
        }

        #endregion INewChildItem

        // TODO: Evaluate: entityResult might be a complete entity node instead of an EntityReference because it can't have children like the catagory
        private LeafNode CreateEntityNode(EntityReferenceResult entityResult) => new(entityResult.Name, new EntityNodeAdapter(this.TreeStoreService, entityResult.Id));

        protected ContainerNode CreateCategoryNode(CategoryReferenceResult category) => new(category.Name, new CategoryNodeAdapter(this.TreeStoreService, category.Id));

        #region IRemoveChildItem

        /// <inheritdoc/>
        void IRemoveChildItem.RemoveChildItem(string childName, bool recurse)
        {
            Guard.Against.NullOrEmpty(childName, nameof(childName));

            var category = this.Category.Categories.FirstOrDefault(c => childName.Equals(c.Name, StringComparison.OrdinalIgnoreCase));
            if (category is not null)
            {
                Await(this.TreeStoreService.DeleteCategoryAsync(parentId: this.Category.Id, childName, recurse, CancellationToken.None));
            }

            var entity = this.Category.Entities.FirstOrDefault(e => childName.Equals(e.Name, StringComparison.OrdinalIgnoreCase));
            if (entity is not null)
            {
                Await(this.TreeStoreService.DeleteEntityAsync(entity.Id, CancellationToken.None));
            }
        }

        #endregion IRemoveChildItem

        #region IItemExists

        /// <inheritdoc/>
        bool IItemExists.ItemExists() => true;

        #endregion IItemExists

        #region ICopyChildItem, ICopyChildItemRecursive

        /// <inheritdoc/>
        ProviderNode? ICopyChildItem.CopyChildItem(ProviderNode nodeToCopy, string[] destination)
        {
            CategoryResult? result = null;

            if (Guard.Against.Null(nodeToCopy, nameof(nodeToCopy)).Underlying is CategoryNodeAdapterBase categoryNode)
                result = Await(this.TreeStoreService.CopyCategoryToAsync(categoryNode.Id, this.Category.Id, false, CancellationToken.None));

            return new ContainerNode(result!.Name, new CategoryNodeAdapter(this.TreeStoreService, result!.Id));
        }

        /// <inheritdoc/>
        ProviderNode? ICopyChildItemRecursive.CopyChildItemRecursive(ProviderNode nodeToCopy, string[] destination)
        {
            CategoryResult? result = null;
            if (Guard.Against.Null(nodeToCopy, nameof(nodeToCopy)).Underlying is CategoryNodeAdapterBase categoryNode)
                result = Await(this.TreeStoreService.CopyCategoryToAsync(categoryNode.Id, this.Category.Id, true, CancellationToken.None));

            return new ContainerNode(result!.Name, new CategoryNodeAdapter(this.TreeStoreService, result!.Id));
        }

        #endregion ICopyChildItem, ICopyChildItemRecursive

        #region IRenameChildItem

        /// <inheritdoc/>
        void IRenameChildItem.RenameChildItem(string childName, string newName)
        {
            var childToRename = this.GetChildByName(Guard.Against.NullOrEmpty(childName, nameof(childName)));
            if (childToRename is null)
            {
                throw new InvalidOperationException($"Child item (name='{childName}') wasn't renamed: It doesn't exist");
            }

            var existingChild = this.GetChildByName(Guard.Against.NullOrEmpty(newName, nameof(newName)));
            if (existingChild is not null)
            {
                throw new InvalidOperationException($"Child item (name='{childName}') wasn't renamed: There is already a child with name='{newName}'");
            }

            Await(this.RenameChildItemAsync(childToRename, newName));
        }

        private object? GetChildByName(string childName)
            => (object?)Array.Find(this.Category.Categories, c => c.Name.Equals(childName, StringComparison.OrdinalIgnoreCase))
            ?? (object?)Array.Find(this.Category.Entities, e => e.Name.Equals(childName, StringComparison.OrdinalIgnoreCase));

        private async Task RenameChildItemAsync(object childToRename, string newName)
        {
            switch (childToRename)
            {
                case EntityReferenceResult er:
                    await this.TreeStoreService.UpdateEntityAsync(er.Id, new UpdateEntityRequest(Name: newName), CancellationToken.None).ConfigureAwait(false);
                    break;

                case CategoryReferenceResult cr:
                    await this.TreeStoreService.UpdateCategoryAsync(cr.Id, new UpdateCategoryRequest(Name: newName), CancellationToken.None).ConfigureAwait(false);
                    break;

                default:
                    throw new InvalidOperationException($"Unkown reference type: '{childToRename.GetType()}'");
            }
        }

        #endregion IRenameChildItem
    }
}