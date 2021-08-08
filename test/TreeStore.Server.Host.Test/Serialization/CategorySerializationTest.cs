using TreeStore.Model;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Server.Host.Test.Serialization
{
    public sealed class CategorySerializationTest : SerializationTestBase
    {
        [Fact]
        public void CategoryResult_serialization_roundtrip()
        {
            // ARRANGE
            var categoryResult = DefaultCategoryModel(DefaultRootCategoryModel()).ToCategoryResult();

            // ACT
            var result = this.SerializationRoundTrip(categoryResult);

            // ASSERT
            Assert.Equal(categoryResult.Id, result.Id);
            Assert.Equal(categoryResult.Name, result.Name);
            Assert.Equal(categoryResult.ParentId, result.ParentId);
        }
    }
}