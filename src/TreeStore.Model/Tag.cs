using TreeStore.Model.Abstractions;
using TreeStore.Model.Base;

namespace TreeStore.Model
{
    public class Tag : FacetingEntityBase, IIdentifiable
    {
        public Tag()
            : base(string.Empty, new Facet(string.Empty))
        { }

        public Tag(string name)
            : base(name, new Facet(name))
        { }

        public Tag(string name, Facet facet)
            : base(name, facet)
        { }
    }
}