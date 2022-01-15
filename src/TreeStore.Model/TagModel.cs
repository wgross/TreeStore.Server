using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public class TagModel : FacetingModelBase, ITag
    {
        public TagModel()
            : base(string.Empty, new FacetModel(string.Empty))
        { }

        public TagModel(string name)
            : base(name, new FacetModel(name))
        { }

        public TagModel(string name, FacetModel facet)
            : base(name, facet)
        { }
    }
}