using Moq;
using Xunit;

namespace TreeStore.Model.Test
{
    public class TreeStoreModelTest : ModelTestBase
    {
        private readonly Mock<ICategoryRepository> categoryRepository;
        private readonly Mock<ITagRepository> tagRepository;
        private readonly TreeStoreModel model;

        public TreeStoreModelTest()
        {
            this.categoryRepository = this.Mocks.Create<ICategoryRepository>();
            this.tagRepository = this.Mocks.Create<ITagRepository>();
            this.model = this.NewModel();
            // just touch the message bus to satify the strict mock repo
        }

        [Fact]
        public void TreeStoreModel_provides_tags()
        {
            // ARRANGE

            this.Persistence
                .Setup(p => p.Tags)
                .Returns(this.tagRepository.Object);

            // ACT

            var result = this.model.Tags;

            // ASSERT

            Assert.Same(result, this.tagRepository.Object);
        }

        [Fact]
        public void TreeStoreModel_disposes_persistence_on_model_disposing()
        {
            // ARRANGE

            this.Persistence
                .Setup(p => p.Dispose());

            // ACT

            this.model.Dispose();
        }
    }
}