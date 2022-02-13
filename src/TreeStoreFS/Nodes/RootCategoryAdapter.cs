using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;

namespace TreeStoreFS.Nodes
{
    public sealed class RootCategoryAdapter : CategoryNodeAdapterBase, INewChildItem
    {
        private readonly Lazy<CategoryResult> rootCategory;

        public RootCategoryAdapter(ITreeStoreService treeStoreService)
            : base(treeStoreService)
        {
            this.rootCategory = new Lazy<CategoryResult>(() => Await(this.ReadRootCategory()));
        }

        private async Task<CategoryResult> ReadRootCategory() => await this.TreeStoreService.GetRootCategoryAsync(CancellationToken.None).ConfigureAwait(false);

        protected override CategoryResult Category => this.rootCategory.Value;

        #region INewChildItem

        protected override ProviderNode? NewChildItemImpl(string childName, string? itemTypeName, object? newItemType)
        {
            var itemTypeNameSafe = itemTypeName ?? "category";

            if (itemTypeNameSafe.Equals("entity", StringComparison.OrdinalIgnoreCase) || itemTypeNameSafe.Equals("file", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Can't create entities in drive root");

            return base.NewChildItemImpl(childName, itemTypeName ?? "category", newItemType);
        }

        #endregion INewChildItem
    }
}