using System.Collections.Generic;

namespace TreeStore.Model.Abstractions
{
    public interface IFacet<FacetPropertyImpl> : INamed
        where FacetPropertyImpl : IFacetProperty
    {
        IEnumerable<IFacetProperty> Properties { get; }
    }
}