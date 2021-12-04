namespace TreeStore.Model
{
    public abstract class FacetingEntityBase : NamedBase
    {
        protected FacetingEntityBase(string name, FacetModel facet)
            : base(name)
        {
            this.Facet = facet;
        }

        public FacetModel Facet
        {
            get => this.facet;
            set
            {
                var oldFacet = this.facet;
                if (oldFacet == value)
                    return;
                this.facet = value;
                this.OnAfterFacetChanged(oldFacet, this.facet);
            }
        }

        private FacetModel facet;

        // public void AssignFacet(FacetModel facet) => this.Facet = facet;

        protected virtual void OnAfterFacetChanged(FacetModel oldFacet, FacetModel newFacet)
        {
        }
    }

    //public static class FacetingEntityExtensions
    //{
    //    public static T AssignFacet<T>(this T faceting, string name, Action<FacetModel> configure) where T : FacetingEntityBase
    //    {
    //        var facet = new FacetModel(name);
    //        configure(facet);
    //        faceting.AssignFacet(facet);
    //        return faceting;
    //    }
    //}
}