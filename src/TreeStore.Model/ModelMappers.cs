using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public static class ModelMappers
    {
        public static EntityResult ToEntityResult(this Entity entity)
        {
            return new EntityResult(entity.Id, entity.Name, entity.Category!.Id);
        }

        public static TagResult ToTagResult(this Tag tag)
        {
            return new TagResult(tag.Id, tag.Name);
        }

        public static CategoryResult ToCategoryResult(this Category category)
        {
            // category without a parent category isn't an allow model state
            return new CategoryResult(category.Id, category.Name, category.Parent!.Id);
        }
    }
}