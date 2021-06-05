using System;

namespace TreeStore.Model.Abstractions
{
    public interface ITreeStoreModel : IDisposable
    {
        ITagRepository Tags { get; }

        ICategoryRepository Categories { get; }

        IEntityRepository Entities { get; }

        IRelationshipRepository Relationships { get; }

        bool DeleteCategory(Category category, bool recurse);

        void CopyCategory(Category category, Category parent, bool recurse);
    }
}