using System;
using System.Collections.Generic;
using System.Linq;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public static class ModelMappers
    {
        /// <summary>
        /// Maps an <see cref="EntityModel"/> to an instance of <see cref="EntityReferenceResult"/> to send over the wire.
        /// </summary>
        public static EntityReferenceResult ToEntityReferenceResult(this EntityModel entity)
        {
            return new EntityReferenceResult(
                Id: entity.Id,
                Name: entity.Name);
        }

        /// <summary>
        /// Maps an <see cref="EntityModel"/> to an instance of <see cref="EntityResult"/> to send it over the wire.
        /// The <see cref="FacetPropertyValueResult"/> is extended with name and type information from the categories facet property
        /// definition
        /// </summary>
        public static EntityResult ToEntityResult(this EntityModel entity)
        {
            return new EntityResult(
                Id: entity.Id,
                Name: entity.Name,
                CategoryId: entity.Category!.Id,
                TagIds: entity.Tags.Select(t => t.Id).ToArray(),
                Values: entity.FacetPropertyValues().Select(fpv => new FacetPropertyValueResult(
                    Id: fpv.facetProperty.Id,
                    Name: fpv.facetProperty.Name,
                    Type: fpv.facetProperty.Type,
                    Value: fpv.value)).ToArray());
        }

        public static TagResult ToTagResult(this TagModel tag)
        {
            return new TagResult(tag!.Id, tag!.Name, tag!.Facet!.ToFacetResult());
        }

        public static CategoryReferenceResult ToCategoryReferenceResult(this CategoryModel category)
        {
            return new CategoryReferenceResult(Id: category.Id, Name: category.Name);
        }

        /// <summary>
        /// Maps the <see cref="CategoryModel"/> to the <see cref="CategoryResult"/> to be sent over the wire.
        /// This creates a empty category result without the references to child categories or entities.
        /// </summary>
        public static CategoryResult ToCategoryResult(this CategoryModel category)
        {
            if (category.Parent is null)
            {
                // 'no parent' is indicated with Guid.Empty as parent id
                return new CategoryResult(category.Id, category.Name, Guid.Empty, category.Facet?.ToFacetResult());
            }
            else
            {
                return new CategoryResult(category.Id, category.Name, category.Parent!.Id, category.Facet?.ToFacetResult());
            }
        }

        /// <summary>
        /// Maps the <see cref="CategoryModel"/> to the <see cref="CategoryResult"/> to be sent over the wire.
        /// This may include child categories and entities as <see cref="CategoryReferenceResult"/> and <see cref="EntityReferenceResult"/>.
        /// </summary>
        public static CategoryResult ToCategoryResult(this CategoryModel category, IEnumerable<CategoryModel> categories, IEnumerable<EntityModel> entities)
        {
            if (category.Parent is null)
            {
                // 'no parent' is indicated with Guid.Empty as parent id
                return new CategoryResult(category.Id, category.Name, Guid.Empty, category.Facet?.ToFacetResult())
                {
                    Categories = categories?.Select(c => c.ToCategoryReferenceResult())?.ToArray() ?? Array.Empty<CategoryResult>(),
                    Entities = entities?.Select(e => e.ToEntityReferenceResult())?.ToArray() ?? Array.Empty<EntityResult>()
                };
            }
            else
            {
                return new CategoryResult(category.Id, category.Name, category.Parent!.Id, category.Facet?.ToFacetResult())
                {
                    Categories = categories?.Select(c => c.ToCategoryReferenceResult())?.ToArray() ?? Array.Empty<CategoryResult>(),
                    Entities = entities?.Select(e => e.ToEntityReferenceResult())?.ToArray() ?? Array.Empty<EntityResult>()
                };
            }
        }

        public static FacetResult ToFacetResult(this FacetModel facet)
        {
            return new FacetResult(facet.Id, facet.Name, facet.Properties.Select(p => p.ToFacetPropertyResult()).ToArray());
        }

        public static FacetPropertyResult ToFacetPropertyResult(this FacetPropertyModel facetProperty)
        {
            return new FacetPropertyResult(facetProperty.Id, facetProperty.Name, facetProperty.Type);
        }
    }
}