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

        protected TagModel DefaultTagModel(Action<TagModel> setup = null) => Setup(new TagModel("t", new FacetModel("f", new FacetPropertyModel("p"))), setup);

        protected EntityModel DefaultEntityModel(Action<EntityModel> setup = null, params TagModel[] tags) => Setup(new EntityModel("e", tags), setup);

        protected EntityModel DefaultEntityModel(Action<EntityModel> setup = null) => Setup(new EntityModel("e", DefaultTagModel()), setup);

        protected Relationship DefaultRelationshipModel(Action<Relationship> setup = null) => DefaultRelationshipModel(DefaultEntityModel(), DefaultEntityModel(), setup);

        protected Relationship DefaultRelationshipModel(EntityModel from, EntityModel to, Action<Relationship> setup = null) => Setup(new Relationship("r", from, to), setup);
    }
}