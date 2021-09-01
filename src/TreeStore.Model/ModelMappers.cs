﻿using System.Linq;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public static class ModelMappers
    {
        public static EntityResult ToEntityResult(this EntityModel entity)
        {
            return new EntityResult(
                Id: entity.Id,
                Name: entity.Name,
                CategoryId: entity.Category!.Id,
                TagIds: entity.Tags.Select(t => t.Id).ToArray(),
                Values: entity.FacetPropertyValues().Select(fpv => new FacetPropertyValueResult(fpv.facetProperty.Id, fpv.facetProperty.Type, fpv.value)).ToArray());
        }

        public static TagResult ToTagResult(this TagModel tag)
        {
            return new TagResult(tag!.Id, tag!.Name, tag!.Facet!.ToFacetResult());
        }

        public static CategoryResult ToCategoryResult(this CategoryModel category)
        {
            // category without a parent category isn't an allow model state
            return new CategoryResult(category.Id, category.Name, category.Parent!.Id);
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