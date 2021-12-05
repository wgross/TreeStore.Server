using System;
using System.Collections.Generic;

namespace TreeStore.Model.Abstractions
{
    public sealed record TagResult(Guid Id, string Name, FacetResult Facet) : ITag;

    public sealed record FacetResult(Guid Id, string Name, IEnumerable<FacetPropertyResult> Properties) : IFacet<FacetPropertyResult>;

    public sealed record FacetPropertyResult(Guid Id, string Name, FacetPropertyTypeValues Type) : IFacetProperty;

    public record CategoryReferenceResult(Guid Id, string Name);

    public sealed record CategoryResult(Guid Id, string Name, Guid ParentId, FacetResult? Facet) : CategoryReferenceResult(Id, Name)
    {
        public CategoryReferenceResult[] Categories { get; init; } = Array.Empty<CategoryReferenceResult>();

        public EntityReferenceResult[] Entities { get; init; } = Array.Empty<EntityReferenceResult>();
    }

    /// <summary>
    /// An <see cref="EntityReferenceResult"/> represents the connection fro an <see cref="CategoryResult"/> to an <see cref="EntityResult"/>.
    /// It avoids to transfer the whole entity data where only the name and Id is required.
    /// </summary>
    public record EntityReferenceResult(Guid Id, string Name);

    public sealed record EntityResult(Guid Id, string Name, Guid CategoryId, Guid[] TagIds, IEnumerable<FacetPropertyValueResult> Values)
        : EntityReferenceResult(Id, Name);

    public sealed record FacetPropertyValueResult(Guid Id, FacetPropertyTypeValues Type, object? Value);
}