using System;

namespace TreeStore.Model
{
    public class FacetingEntityBase : NamedBase
    {
        public FacetingEntityBase(string name, Facet facet)
            : base(name)
        {
            this.Facet = facet;
        }

        public Facet Facet { get; set; }

        public void AssignFacet(Facet facet) => this.Facet = facet;
    }

    public static class FacetingEntityExtensions
    {
        public static T AssignFacet<T>(this T faceting, string name, Action<Facet> configure) where T : FacetingEntityBase
        {
            var facet = new Facet(name);
            configure(facet);
            faceting.AssignFacet(facet);
            return faceting;
        }
    }
}