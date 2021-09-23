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
    public abstract class CategoryNodeAdapterBase :
        // Enable provider node access
        IServiceProvider,
        // Path Traversal
        IGetChildItems,
        // ItemCmdletProvider
        IGetItem, IItemExists,
        // ContaierCmdletProvider
        INewChildItem, IRemoveChildItem, ICopyChildItemRecursive
    {
        public CategoryNodeAdapterBase(ITreeStoreService treeStoreService)
        {
            this.TreeStoreService = treeStoreService;
        }

        protected ITreeStoreService TreeStoreService { get; }

        protected abstract CategoryResult Category { get; }

        /// <summary>
        /// Any tree store node has an Id.
        /// </summary>
        public Guid Id => this.Category.Id;

        #region IServiceProvider

        /// <inheritdoc/>
        public object? GetService(Type serviceType)
        {
            if (this.GetType().IsAssignableTo(serviceType))
                return this;

            return null;
        }

        #endregion IServiceProvider

        #region IGetChildItems

        /// <inheritdoc/>
        public bool HasChildItems() => this.Category.Entities.Any() || this.Category.Categories.Any();

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

            return result.Select(c => this.CreateCategoryNode(c)).ToArray();
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
            Guard.AgainstNull(childName, nameof(childName));
            Guard.AgainstNull(itemTypeName, nameof(itemTypeName));

            return itemTypeName.ToLowerInvariant() switch
            {
                "category" => this.CreateCategoryNode(Await(this.NewChildContainer(this.Category.Id, childName))),

                _ => throw new NotImplementedException()
            };
        }

        private async Task<CategoryResult?> NewChildContainer(Guid parentId, string childName)
        {
            return await this.TreeStoreService.CreateCategoryAsync(
                new CreateCategoryRequest(
                    Name: childName,
                    ParentId: parentId,
                    Facet: null),
                    CancellationToken.None).ConfigureAwait(false);
        }

        #endregion INewChildItem

        protected ProviderNode CreateCategoryNode(CategoryResult category)
        {
            return new ContainerNode(category.Name, new CategoryNodeAdapter(this.TreeStoreService, category.Id));
        }

        protected static T? Await<T>(Task<T?> action) => action.ConfigureAwait(false).GetAwaiter().GetResult();

        #region IRemoveChildItem

        /// <inheritdoc/>
        void IRemoveChildItem.RemoveChildItem(string childName, bool recurse)
        {
            Guard.AgainstNull(childName, nameof(childName));

            Await(this.TreeStoreService.DeleteCategoryAsync(this.Category.Id, childName, recurse, CancellationToken.None));
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
            Guard.AgainstNull(nodeToCopy, nameof(nodeToCopy));

            CategoryResult? result = null;

            if (nodeToCopy.Underlying is CategoryNodeAdapterBase categoryNode)
                result = Await(this.TreeStoreService.CopyCategoryToAsync(categoryNode.Id, this.Category.Id, false, CancellationToken.None));

            return new ContainerNode(result!.Name, new CategoryNodeAdapter(this.TreeStoreService, result!.Id));
        }

        /// <inheritdoc/>
        ProviderNode? ICopyChildItemRecursive.CopyChildItemRecursive(ProviderNode nodeToCopy, string[] destination)
        {
            Guard.AgainstNull(nodeToCopy, nameof(nodeToCopy));

            CategoryResult? result = null;
            if (nodeToCopy.Underlying is CategoryNodeAdapterBase categoryNode)
                result = Await(this.TreeStoreService.CopyCategoryToAsync(categoryNode.Id, this.Category.Id, true, CancellationToken.None));

            return new ContainerNode(result!.Name, new CategoryNodeAdapter(this.TreeStoreService, result!.Id));
        }

        #endregion ICopyChildItem, ICopyChildItemRecursive
    }
}