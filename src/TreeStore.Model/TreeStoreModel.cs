using System;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public class TreeStoreModel : IDisposable
    {
        private ITreeStoreModel? persistence;

        public TreeStoreModel(ITreeStoreModel persistence)
        {
            this.persistence = persistence;
        }

        public ITagRepository Tags => this.persistence!.Tags;

        public IEntityRepository Entities => this.persistence!.Entities;

        public IRelationshipRepository Relationships => this.persistence!.Relationships;

        public void Dispose()
        {
            this.persistence?.Dispose();
            this.persistence = null;
        }
    }
}