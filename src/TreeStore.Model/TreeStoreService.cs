using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public sealed class TreeStoreService : ITreeStoreService
    {
        private readonly ITreeStoreModel model;

        public TreeStoreService(ITreeStoreModel model)
        {
            this.model = model;
        }

        /// <summary>
        /// Provides the root <see cref="Category"/> of this model.
        /// </summary>
        public Category GetRootCategory() => this.model.Categories.Root();
    }
}