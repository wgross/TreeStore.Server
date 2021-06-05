using System.Linq;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public static class KosmographPersistenceExtensions
    {
        public static bool RemoveWithRelationship(this ITreeStoreModel thisPersistence, Entity entity)
        {
            foreach (var affectedRelationship in thisPersistence.Relationships.FindByEntity(entity).ToArray())
                thisPersistence.Relationships.Delete(affectedRelationship);

            return thisPersistence.Entities.Delete(entity);
        }
    }
}