using System;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;

namespace TreeStoreFS.Nodes
{
    public sealed class RootCategoryAdapter : CategoryNodeAdapterBase
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

        #endregion INewChildItem
    }
}