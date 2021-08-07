using System.Collections.Generic;

namespace TreeStore.Model
{
    public interface IRelationshipRepository : IRepository<Relationship>
    {
        IEnumerable<Relationship> FindByEntity(EntityModel entity);

        IEnumerable<Relationship> FindByTag(TagModel tag);

        void Delete(IEnumerable<Relationship> relationships);
    }
}