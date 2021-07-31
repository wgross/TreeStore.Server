using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public static class ModelMappers
    {
        public static EntityResult ToEntityResponse(this Entity entity)
        {
            return new EntityResult(entity.Id, entity.Name, entity.Category!.Id);
        }

        public static CategoryResult ToCategoryResponse(this Category category)
        {
            // category without a parent category isn't an allow model state
            return new CategoryResult(category.Id, category.Name, category.Parent!.Id);
        }
    }
}