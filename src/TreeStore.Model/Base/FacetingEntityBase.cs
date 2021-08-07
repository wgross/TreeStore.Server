using System;

namespace TreeStore.Model
{
    public abstract class FacetingEntityBase : NamedBase
    {
        public FacetingEntityBase(string name, FacetModel facet)
            : base(name)
        {
            this.Facet = facet;
        }

        public FacetModel Facet { get; set; }

        public void AssignFacet(FacetModel facet) => this.Facet = facet;
    }

    public static class FacetingEntityExtensions
    {
        public static T AssignFacet<T>(this T faceting, string name, Action<FacetModel> configure) where T : FacetingEntityBase
        {
            var facet = new FacetModel(name);
            configure(facet);
            faceting.AssignFacet(facet);
            return faceting;
        }
    }
}