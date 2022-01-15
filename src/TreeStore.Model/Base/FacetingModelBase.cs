using TreeStore.Common;

namespace TreeStore.Model
{
    /// <summary>
    /// Base class for model item introducing <see cref="FacetModel"/>.
    /// It is expected that each class implementing faceting only provide a single facet.
    /// </summary>
    public abstract class FacetingModelBase : NamedModelBase
    {
        protected FacetingModelBase(string name, FacetModel facet)
            : base(name)
        {
            this.facet = facet;
        }

        public FacetModel Facet
        {
            get => this.facet;
            set
            {
                var oldFacet = this.facet;
                if (oldFacet == value)
                    return;
                this.facet = Guard.Against.Null(value, nameof(value));
                this.OnAfterFacetChanged(oldFacet, this.facet);
            }
        }

        private FacetModel facet;

        protected virtual void OnAfterFacetChanged(FacetModel oldFacet, FacetModel newFacet)
        {
        }
    }
}