using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using TreeStore.Model;
using Xunit;

namespace TreeStore.LiteDb.Test
{
    public class KosmographLiteDbPersistenceTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly TreeStoreLiteDbPersistence persistence;

        public KosmographLiteDbPersistenceTest()
        {
            this.persistence = TreeStoreLiteDbPersistence.InMemory(new NullLoggerFactory());
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact(Skip = "ignore relationships")]
        public void KosmographLiteDbPersistence_removes_entity_with_relationship()
        {
            // ARRANGE

            var entity1 = this.persistence.Entities.Upsert(new Entity("e1"));
            var entity2 = this.persistence.Entities.Upsert(new Entity("e2"));
            var relationship = this.persistence.Relationships.Upsert(new Relationship("r", entity1, entity2));

            // ACT

            this.persistence.RemoveWithRelationship(entity1);
        }
    }
}