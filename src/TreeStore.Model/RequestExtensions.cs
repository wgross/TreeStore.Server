using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public static class RequestExtensions
    {
        public static void Apply(this UpdateCategoryRequest updateCategoryRequest, CategoryModel category)
        {
            category.Name = updateCategoryRequest.Name;
        }

        public static TagModel Apply(this CreateTagRequest createTagRequest)
        {
            var tag = new TagModel
            {
                Name = createTagRequest.Name
            };

            createTagRequest.Facet?.Apply(tag.Facet);

            return tag;
        }

        public static TagModel Apply(this UpdateTagRequest updateTagRequest, TagModel tag)
        {
            tag.Name = updateTagRequest.Name ?? tag.Name;

            updateTagRequest.Facet?.Apply(tag.Facet);

            return tag;
        }

        private static void Apply(this FacetRequest updateFacetRequest, FacetModel facet)
        {
            updateFacetRequest.DeleteProperties?.ForEach(deletion =>
            {
                facet.RemoveProperty(deletion.Id);
            });

            updateFacetRequest.UpdateProperties?.ForEach(update =>
            {
                var facetProperty = facet.GetProperty(update.Id);
                if (facetProperty is not null)
                    update.Apply(facetProperty);
            });

            updateFacetRequest.CreateProperties?.ForEach(creation =>
            {
                facet.AddProperty(creation.Apply());
            });
        }

        public static FacetPropertyModel Apply(this UpdateFacetPropertyRequest updateFacetPropertyRequest, FacetPropertyModel facetProperty)
        {
            facetProperty.Name = updateFacetPropertyRequest.Name ?? facetProperty.Name;

            return facetProperty;
        }

        public static FacetPropertyModel Apply(this CreateFacetPropertyRequest createFacetPropertyRequest)
        {
            return new FacetPropertyModel
            {
                Name = createFacetPropertyRequest.Name,
                Type = createFacetPropertyRequest.Type,
            };
        }

        public static void Apply(this UpdateEntityRequest updateEntityRequest, EntityModel entity)
        {
            entity.Name = updateEntityRequest.Name;
        }
    }
}