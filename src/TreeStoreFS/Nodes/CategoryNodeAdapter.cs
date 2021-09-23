using System;
using System.Threading;
using TreeStore.Model.Abstractions;

namespace TreeStoreFS.Nodes
{
    public sealed class CategoryNodeAdapter : CategoryNodeAdapterBase
    {
        private readonly Lazy<CategoryResult?> category;

        public CategoryNodeAdapter(ITreeStoreService treeStoreService, Guid categoryId)
            : base(treeStoreService)
        {
            this.category = new(() => Await(this.TreeStoreService.GetCategoryByIdAsync(categoryId, CancellationToken.None)));
        }

        protected override CategoryResult Category => this.category.Value ?? throw new InvalidOperationException("Category wasn't loaded");
    }
}