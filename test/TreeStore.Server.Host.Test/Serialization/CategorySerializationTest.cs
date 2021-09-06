using System.Linq;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
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

        [Fact]
        public void CreateCategoryRequest_json_roundtrip()
        {
            // ARRANGE
            var categoryModel = DefaultCategoryModel(DefaultRootCategoryModel(), WithDefaultProperties);

            var request = new CreateCategoryRequest(
                Name: categoryModel.Name,
                ParentId: categoryModel.Parent.Id,
                Facet: new FacetRequest(categoryModel.Facet.Properties.Select(fp => new CreateFacetPropertyRequest(fp.Name, fp.Type)).ToArray())
            );

            // ACT
            var result = SerializationRoundTrip(request);

            // ASSERT
            Assert.Equal(categoryModel.Name, result.Name);
            Assert.Equal(categoryModel.Parent.Id, result.ParentId);

            CreateFacetPropertyRequest getResultValue(string name) => result.Facet.Creates.First(c => c.Name == name);

            Assert.All(categoryModel.Facet.Properties, fp =>
            {
                Assert.Equal(fp.Type, getResultValue(fp.Name).Type);
            });
        }
    }
}