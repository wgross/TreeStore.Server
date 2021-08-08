//using System;
//using TreeStore.Model.Abstractions;
//using Xunit;
//using static TreeStore.Test.Common.TreeStoreTestData;

//namespace TreeStore.Model.Test
//{
//    public class EntityRequestTest
//    {
//        [Fact]
//        public void CreateEntityRequest_sets_name()
//        {
//            // ARRANGE
//            var category = DefaultRootCategoryModel();

//            // ACT
//            var createEntityRequest = new CreateEntityRequest(
//                Name: "e",
//                CategoryId: Guid.NewGuid());

//            var result = createEntityRequest.Apply(category);

//            // ASSERT
//            Assert.Equal("e", result.Name);
//            Assert.Same(category, result.Category);
//        }

//        [Fact]
//        public void UpdateEntityRequest_updates_name()
//        {
//            // ARRANGE
//            var rootCategory = DefaultRootCategoryModel();
//            var entity = DefaultEntityModel(rootCategory);

//            // ACT
//            var updateEntityRequest = new UpdateEntityRequest(Name: "changed");
//            var result = updateEntityRequest.Apply(entity);

//            // ASSERT
//            Assert.Equal("changed", result.Name);
//        }

//        [Fact]
//        public void UpdateEntityRequest_add_tag()
//        {
//            // ARRANGE
//            var rootCategory = DefaultRootCategoryModel();
//            var entity = DefaultEntityModel(rootCategory);

//            // ACT
//            var updateEntityRequest = new UpdateEntityRequest(Name: "changed");
//            var result = updateEntityRequest.Apply(entity);

//            // ASSERT
//            Assert.Equal("changed", result.Name);
//        }
//    }
//}