using System;
using TreeStore.Model.Abstractions;

namespace TreeStoreFS
{
    public record EntityItem
    {
        private readonly EntityResult underlying;

        public EntityItem(EntityResult underlying)
        {
            this.underlying = underlying;
        }

        public Guid Id => this.underlying.Id;

        public string Name => this.underlying.Name;

        public EntityResult GetUnderlying() => this.underlying;
    }
}