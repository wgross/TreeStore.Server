using System;
using TreeStore.Model.Abstractions;

namespace TreeStoreFS
{
    public record CategoryItem
    {
        private readonly CategoryResult underlying;

        public CategoryItem(CategoryResult underlying)
        {
            this.underlying = underlying;
        }

        public Guid Id => this.underlying.Id;

        public string Name => this.underlying.Name;

        public CategoryResult GetUnderlying() => this.underlying;
    }
}