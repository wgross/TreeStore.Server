using System.Collections.Generic;

namespace TreeStore.Model
{
    public interface IEntityRepository : IRepository<EntityModel>
    {
        IEnumerable<EntityModel> FindByTag(TagModel tag);

        IEnumerable<EntityModel> FindByCategory(CategoryModel category);

        EntityModel? FindByCategoryAndName(CategoryModel category, string name);
    }
}