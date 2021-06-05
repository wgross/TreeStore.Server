using Moq;
using TreeStore.Model.Abstractions;
using TreeStore.Model.Test.Base;
using Xunit;

namespace TreeStore.Model.Test
{
    public class TreeStoreServiceTest : TreeStoreModelTestBase
    {
        private readonly Mock<ICategoryRepository> categoryRepositoryMock;
        private readonly Mock<ITreeStoreModel> modelMock;
        private readonly TreeStoreService service;
        private Category rootCategory;

        public TreeStoreServiceTest()
        {
            this.categoryRepositoryMock = this.Mocks.Create<ICategoryRepository>();
            this.modelMock = this.Mocks.Create<ITreeStoreModel>();
            this.service = new TreeStoreService(this.modelMock.Object);
        }

        [Fact]
        public void TreeStoreService_reads_root_category()
        {
            // ARRANGE
            this.modelMock
                .Setup(m => m.Categories)
                .Returns(this.categoryRepositoryMock.Object);

            this.categoryRepositoryMock
                .Setup(r => r.Root())
                .Returns(DefaultRootCategory());

            // ACT
            var result = this.service.GetRootCategory();

            // ASSERT
            Assert.Same(this.DefaultRootCategory(), result);
        }

        private Category DefaultRootCategory()
        {
            this.rootCategory ??= new Category();
            return this.rootCategory;
        }
    }
}