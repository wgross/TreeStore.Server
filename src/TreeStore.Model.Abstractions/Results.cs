using System;
using System.Collections.Generic;

namespace TreeStore.Model.Abstractions
{
    public sealed record TagResult(Guid Id, string Name, FacetResult Facet) : ITag;

    public sealed record FacetResult(Guid Id, string Name, IEnumerable<FacetPropertyResult> Properties) : IFacet<FacetPropertyResult>;

    public sealed record FacetPropertyResult(Guid Id, string Name, FacetPropertyTypeValues Type) : IFacetProperty;

    public sealed record CategoryResult(Guid Id, string Name, Guid ParentId);

    public sealed record EntityResult(Guid Id, string Name, Guid CategoryId);
}