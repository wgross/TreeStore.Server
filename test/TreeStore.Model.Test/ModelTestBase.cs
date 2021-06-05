using Moq;
using System;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model.Test
{
    public class ModelTestBase : IDisposable
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected Mock<ITreeStoreModel> Persistence { get; }

        public ModelTestBase()
        {
            this.Persistence = this.Mocks.Create<ITreeStoreModel>();
        }

        public void Dispose() => this.Mocks.VerifyAll();

        protected T Setup<T>(T t, Action<T> setup = null)
        {
            setup?.Invoke(t);
            return t;
        }

        protected TreeStoreModel NewModel()
        {
            return new TreeStoreModel(this.Persistence.Object);
        }

        protected Tag DefaultTag(Action<Tag> setup = null) => Setup(new Tag("t", new Facet("f", new FacetProperty("p"))), setup);

        protected Entity DefaultEntity(Action<Entity> setup = null, params Tag[] tags) => Setup(new Entity("e", tags), setup);

        protected Entity DefaultEntity(Action<Entity> setup = null) => Setup(new Entity("e", DefaultTag()), setup);

        protected Relationship DefaultRelationship(Action<Relationship> setup = null) => DefaultRelationship(DefaultEntity(), DefaultEntity(), setup);

        protected Relationship DefaultRelationship(Entity from, Entity to, Action<Relationship> setup = null) => Setup(new Relationship("r", from, to), setup);
    }
}