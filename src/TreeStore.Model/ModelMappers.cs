using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public static class ModelMappers
    {
        public static EntityResponse ToEntityResponse(this Entity entity)
        {
            return new EntityResponse(entity.Id, entity.Name, entity.Category!.Id);
        }

        public static CategoryResponse ToCategoryResponse(this Category category)
        {
            // category without a parent category isn't an allow model state
            return new CategoryResponse(category.Id, category.Name, category.Parent!.Id);
        }
    }
}