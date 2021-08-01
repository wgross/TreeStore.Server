using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    internal static class RequestExtensions
    {
        internal static void Apply(this UpdateCategoryRequest updateCategoryRequest, Category category)
        {
            category.Name = updateCategoryRequest.Name;
        }

        internal static void Apply(this UpdateEntityRequest updateEntityRequest, Entity entity)
        {
            entity.Name = updateEntityRequest.Name;
        }
    }
}