using System;
using System.Linq;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Server.Host.Test.Serialization
{
    public class EntitySerializationTest : SerializationTestBase
    {
        [Fact]
        public void EntityResult_json_roundtrip()
        {
            // ARRANGE
            var entityResult = DefaultEntityModel(WithDefaultCategory, WithDefaultTag, WithDefaultPropertyValues).ToEntityResult();

            // ACT
            var result = SerializationRoundTrip(entityResult);

            // ASSERT
            Assert.Equal(entityResult.Id, result.Id);
            Assert.Equal(entityResult.Name, result.Name);
            Assert.Equal(entityResult.CategoryId, result.CategoryId);
            Assert.Equal(entityResult.TagIds.Single(), result.TagIds.Single());

            FacetPropertyValueResult getResultValue(Guid id) => result.Values.First(v => v.Id == id);

            Assert.All(entityResult.Values, erv =>
            {
                Assert.Equal(erv.Type, getResultValue(erv.Id).Type);
                Assert.Equal(erv.Value, getResultValue(erv.Id).Value);
            });
        }

        [Fact]
        public void CreateEntityRequest_json_roundtrip()
        {
            // ARRANGE
            var entityModel = DefaultEntityModel(WithDefaultCategory, WithDefaultTag, WithDefaultPropertyValues);

            var request = new CreateEntityRequest(
                Name: entityModel.Name,
                CategoryId: entityModel.Category.Id,
                Tags: new CreateEntityTagsRequest(new[]
                {
                    new AssignTagRequest(TagId:entityModel.Tags.Single().Id)
                }),
                Values: new FacetPropertyValuesRequest(entityModel
                    .GetFacetPropertyValues()
                    .Select(fpv => new UpdateFacetPropertyValueRequest(fpv.facetProperty.Id, fpv.facetProperty.Type, fpv.value))
                    .ToArray()));

            // ACT
            var result = SerializationRoundTrip(request);

            // ASSERT
            Assert.Equal(entityModel.Name, result.Name);
            Assert.Equal(entityModel.Category.Id, result.CategoryId);
            Assert.Equal(entityModel.Tags.Single().Id, result.Tags.Assigns.Single().TagId);

            UpdateFacetPropertyValueRequest getResultValue(Guid id) => result.Values.Updates.First(v => v.Id == id);

            Assert.All(entityModel.GetFacetPropertyValues(), fpv =>
            {
                Assert.Equal(fpv.facetProperty.Type, getResultValue(fpv.facetProperty.Id).Type);
                Assert.Equal(fpv.value, getResultValue(fpv.facetProperty.Id).Value);
            });
        }

        [Fact]
        public void UpdateEntityRequest_json_roundtrip()
        {
            // ARRANGE
            var entityModel = DefaultEntityModel(WithDefaultCategory, WithDefaultTag, WithDefaultPropertyValues);

            var request = new UpdateEntityRequest(
                Name: entityModel.Name,
                Tags: new UpdateEntityTagsRequest(new[]
                {
                    new AssignTagRequest(TagId:entityModel.Tags.Single().Id)
                }),
                Values: new FacetPropertyValuesRequest(entityModel
                    .GetFacetPropertyValues()
                    .Select(fpv => new UpdateFacetPropertyValueRequest(fpv.facetProperty.Id, fpv.facetProperty.Type, fpv.value))
                    .ToArray()));

            // ACT
            var result = SerializationRoundTrip(request);

            // ASSERT
            Assert.Equal(entityModel.Name, result.Name);
            Assert.Equal(entityModel.Tags.Single().Id, result.Tags.Assigns.Single().TagId);

            UpdateFacetPropertyValueRequest getResultValue(Guid id) => result.Values.Updates.First(v => v.Id == id);

            Assert.All(entityModel.GetFacetPropertyValues(), fpv =>
            {
                Assert.Equal(fpv.facetProperty.Type, getResultValue(fpv.facetProperty.Id).Type);
                Assert.Equal(fpv.value, getResultValue(fpv.facetProperty.Id).Value);
            });
        }
    }
}