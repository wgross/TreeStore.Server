using Moq;
using System;
using TreeStore.Model;
using Xunit;

namespace TreeStore.LiteDb.Test
{
    public class KosmographLiteDbPersistenceTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly TreeStoreLiteDbPersistence kosmographPersistence;

        public KosmographLiteDbPersistenceTest()
        {
            this.kosmographPersistence = TreeStoreLiteDbPersistence.InMemory();
        }
        
        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact(Skip = "ignore relationships")]
        public void KosmographLiteDbPersistence_removes_entity_with_relationship()
        {
            // ARRANGE

            var entity1 = this.kosmographPersistence.Entities.Upsert(new Entity("e1"));
            var entity2 = this.kosmographPersistence.Entities.Upsert(new Entity("e2"));
            var relationship = this.kosmographPersistence.Relationships.Upsert(new Relationship("r", entity1, entity2));

            // ACT

            this.kosmographPersistence.RemoveWithRelationship(entity1);
        }
    }
}