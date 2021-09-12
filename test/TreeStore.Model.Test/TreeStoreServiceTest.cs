using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;
using TreeStore.Model.Test.Base;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Model.Test
{
    public class TreeStoreServiceTest : TreeStoreModelTestBase
    {
        private readonly Mock<ICategoryRepository> categoryRepositoryMock;
        private readonly Mock<IEntityRepository> entityRepositoryMock;
        private readonly Mock<ITagRepository> tagRepositoryMock;
        private readonly Mock<ITreeStoreModel> modelMock;
        private readonly TreeStoreService service;

        public TreeStoreServiceTest()
        {
            this.categoryRepositoryMock = this.Mocks.Create<ICategoryRepository>();
            this.entityRepositoryMock = this.Mocks.Create<IEntityRepository>();
            this.tagRepositoryMock = this.Mocks.Create<ITagRepository>();
            this.modelMock = this.Mocks.Create<ITreeStoreModel>();
            this.service = new TreeStoreService(this.modelMock.Object, new NullLogger<TreeStoreService>());
        }

        #region Category

        [Fact]
        public async Task Reads_root_category()
        {
            // ARRANGE
            var rootCategory = DefaultRootCategoryModel();

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.Root())
                    .Returns(rootCategory);
            });

            // ACT
            var result = await this.service.GetRootCategoryAsync(CancellationToken.None);

            // ASSERT
            Assert.Equal(rootCategory.Name, result.Name);
            Assert.Equal(rootCategory.Id, result.Id);
            Assert.Equal(rootCategory.Name, result.Name);
            Assert.Equal(Guid.Empty, result.ParentId);
        }

        [Fact]
        public async Task Creates_category()
        {
            // ARRANGE
            var rootCategory = this.ArrangeRootCategory();
            var category = DefaultCategoryModel(rootCategory, WithDefaultProperties);

            CategoryModel categoryWritten = default;

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.Upsert(It.IsAny<CategoryModel>()))
                    .Callback<CategoryModel>(c => categoryWritten = c)
                    .Returns<CategoryModel>(c => c);
            });

            // ACT
            var request = new CreateCategoryRequest(
                Name: category.Name,
                ParentId: rootCategory.Id,
                Facet: new(category.Facet.Properties.Select(fp => new CreateFacetPropertyRequest(fp.Name, fp.Type)).ToArray()));

            var result = await this.service.CreateCategoryAsync(request, CancellationToken.None);

            // ASSERT
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(rootCategory.Id, result.ParentId);
            Assert.Equal(category.Name, categoryWritten.Name);
            Assert.Equal(rootCategory.Id, categoryWritten.Parent.Id);

            FacetPropertyResult getProperty(string name) => result.Facet.Properties.Single(pv => pv.Name == name);

            Assert.All(category.Facet.Properties, fp => Assert.Equal(fp.Type, getProperty(fp.Name).Type));
        }

        [Fact]
        public async Task Creating_category_fails_if_parent_doesnt_exists()
        {
            // ARRANGE
            var parentId = Guid.NewGuid();
            var category = DefaultCategoryModel(DefaultRootCategoryModel());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(parentId))
                    .Returns((CategoryModel)null);
            });

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() =>
               this.service.CreateCategoryAsync(new CreateCategoryRequest(category.Name, parentId),
               CancellationToken.None));

            // ASSERT
            Assert.Equal($"Category(name='{category.Name}' wasn't created: Category(id='{parentId}') wasn't found", result.Message);
        }

        [Fact]
        public async Task Reads_category()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns(category);
            });

            // ACT
            var result = await this.service.GetCategoryByIdAsync(category.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(category.ToCategoryResult(), result);
        }

        [Fact]
        public async Task Reading_category_returns_null_if_missing()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns((CategoryModel)null);
            });

            // ACT
            var result = await this.service.GetCategoryByIdAsync(category.Id, CancellationToken.None);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task Updates_category()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            CategoryModel writtenCategory = default;

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns(category);

                mock
                    .Setup(r => r.Upsert(It.IsAny<CategoryModel>()))
                    .Callback<CategoryModel>(c => writtenCategory = c)
                    .Returns(category);
            });

            // ACT
            var request = new UpdateCategoryRequest(
                Name: "changed");

            var result = await this.service.UpdateCategoryAsync(category.Id, request, CancellationToken.None);

            // ASSERT
            Assert.Equal("changed", writtenCategory.Name);
            Assert.Equal(category.Id, writtenCategory.Id);
            Assert.Equal(category.Parent.Id, writtenCategory.Parent.Id);
            // TODO: better test of facet property update
        }

        [Fact]
        public async Task Updates_category_facet()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            CategoryModel writtenCategory = default;

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns(category);

                mock
                    .Setup(r => r.Upsert(It.IsAny<CategoryModel>()))
                    .Callback<CategoryModel>(c => writtenCategory = c)
                    .Returns(category);
            });

            // ACT
            var request = new UpdateCategoryRequest(
                Facet: new FacetRequest(new CreateFacetPropertyRequest(
                    Name: "p1",
                    Type: FacetPropertyTypeValues.String)));

            var result = await this.service.UpdateCategoryAsync(category.Id, request, CancellationToken.None);

            // ASSERT
            Assert.Equal("c", result.Name);
            Assert.Equal(category.Name, writtenCategory.Name);
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(category.Id, writtenCategory.Id);
            Assert.Equal(category.Parent.Id, result.ParentId);
            Assert.Equal(category.Parent.Id, writtenCategory.Parent.Id);
            Assert.Equal("p1", writtenCategory.Facet.Properties.Single().Name);
            Assert.Equal(FacetPropertyTypeValues.String, writtenCategory.Facet.Properties.Single().Type);
            // TODO: better test of facet property update
        }

        [Fact]
        public async Task Updating_category_fails_on_missing_category()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns((CategoryModel)null);
            });

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(
                () => this.service.UpdateCategoryAsync(category.Id, new UpdateCategoryRequest(Name: "changed"), CancellationToken.None));

            // ASSERT
            Assert.Equal($"Category(id='{category.Id}') wasn't updated: Category(id='{category.Id}') doesn't exist", result.Message);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task Deletes_category_if_empty(bool recurse, bool deleteResult)
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns(category);

                mock
                    .Setup(r => r.Delete(category, recurse))
                    .Returns(deleteResult);
            });

            // ACT
            var result = await this.service.DeleteCategoryAsync(category.Id, recurse, CancellationToken.None);

            // ASSERT
            Assert.Equal(deleteResult, result);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Deleting_category_returns_false_on_missing_category(bool recurse)
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(category.Id))
                    .Returns((CategoryModel)null);
            });

            // ACT
            var result = await this.service
                .DeleteCategoryAsync(category.Id, recurse, CancellationToken.None);

            // ASSERT
            Assert.False(result);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Copies_category(bool recurse)
        {
            // ARRANGE
            var source = DefaultCategoryModel(DefaultRootCategoryModel());
            var destination = DefaultCategoryModel(DefaultRootCategoryModel());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(source.Id))
                    .Returns(source);

                mock
                    .Setup(r => r.FindById(destination.Id))
                    .Returns(destination);

                mock
                    .Setup(r => r.CopyTo(source, destination, recurse));
            });

            // ACT
            var result = await this.service
                .CopyCategoryToAsync(source.Id, destination.Id, recurse, CancellationToken.None);

            // ASSERT
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Copying_category_fails_on_missing_source(bool recurse)
        {
            // ARRANGE
            var source = DefaultCategoryModel(DefaultRootCategoryModel());
            var destination = DefaultCategoryModel(DefaultRootCategoryModel());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(source.Id))
                    .Returns((CategoryModel)null);
            });

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service
                 .CopyCategoryToAsync(source.Id, destination.Id, recurse, CancellationToken.None));

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal($"Category(id='{source.Id}') wasn't copied: Category(id='{source.Id}') doesn't exist", result.Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Copying_category_fails_on_missing_destination(bool recurse)
        {
            // ARRANGE
            var source = DefaultCategoryModel(DefaultRootCategoryModel());
            var destination = DefaultCategoryModel(DefaultRootCategoryModel());

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(source.Id))
                    .Returns(source);

                mock
                    .Setup(r => r.FindById(destination.Id))
                    .Returns((CategoryModel)null);
            });

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service
                 .CopyCategoryToAsync(source.Id, destination.Id, recurse, CancellationToken.None));

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal($"Category(id='{source.Id}') wasn't copied: Category(id='{destination.Id}') doesn't exist", result.Message);
        }

        private void ArrangeCategoryRepository(Action<Mock<ICategoryRepository>> arrange = null)
        {
            this.modelMock
                .Setup(m => m.Categories)
                .Returns(this.categoryRepositoryMock.Object);

            arrange?.Invoke(this.categoryRepositoryMock);
        }

        private CategoryModel ArrangeRootCategory()
        {
            var root = DefaultRootCategoryModel();

            this.categoryRepositoryMock
                .Setup(r => r.FindById(root.Id))
                .Returns(root);

            return root;
        }

        #endregion Category

        #region Entity

        [Fact]
        public async Task Creates_entity()
        {
            // ARRANGE
            var rootCategory = DefaultRootCategoryModel();
            var tag = DefaultTagModel();

            EntityModel storedEntity = null;

            this.ArrangeCategoryRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(rootCategory.Id))
                    .Returns(rootCategory);
            });

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns(tag);
            });

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.Upsert(It.IsAny<EntityModel>()))
                    .Callback<EntityModel>(e => storedEntity = e)
                    .Returns<EntityModel>(e => e);
            });

            // ACT
            var createEntityRequest = new CreateEntityRequest(
                Name: "e",
                CategoryId: rootCategory.Id,
                Tags: new()
                {
                    Assigns = new[] { new AssignTagRequest(tag.Id) }
                });

            var result = await this.service.CreateEntityAsync(createEntityRequest, CancellationToken.None);

            // ASSERT
            Assert.Equal("e", result.Name);
            Assert.Equal(tag.Id, result.TagIds.Single());

            Assert.Equal("e", storedEntity.Name);
            Assert.Equal(tag.Id, storedEntity.Tags.Single().Id);
        }

        [Fact]
        public async Task Reads_entity()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultRootCategoryModel());

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns(entity);
            });

            // ACT
            var result = await this.service.GetEntityByIdAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.ToEntityResult(), result);
        }

        [Fact]
        public async Task Reading_entity_returns_null_if_missing()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultRootCategoryModel());

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns((EntityModel)null);
            });

            // ACT
            var result = await this.service.GetEntityByIdAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task Updates_entity()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultRootCategoryModel(WithDefaultProperties));

            EntityModel writtenEntity = default;

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns(entity);

                mock
                    .Setup(r => r.Upsert(It.IsAny<EntityModel>()))
                    .Callback<EntityModel>(c => writtenEntity = c)
                    .Returns(entity);
            });

            // ACT
            var result = await this.service.UpdateEntityAsync(entity.Id, new UpdateEntityRequest(Name: "changed"), CancellationToken.None);

            // ASSERT
            Assert.Equal("changed", writtenEntity.Name);
            Assert.Equal(entity.Id, writtenEntity.Id);
            Assert.Equal(entity.Category.Id, writtenEntity.Category.Id);
        }

        [Fact]
        public async Task Updates_entity_name()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultRootCategoryModel());

            EntityModel writtenEntity = default;

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns(entity);

                mock
                    .Setup(r => r.Upsert(It.IsAny<EntityModel>()))
                    .Callback<EntityModel>(c => writtenEntity = c)
                    .Returns(entity);
            });

            // ACT
            var result = await this.service.UpdateEntityAsync(entity.Id, new UpdateEntityRequest(Name: "changed"), CancellationToken.None);

            // ASSERT
            Assert.Equal("changed", writtenEntity.Name);
            Assert.Equal(entity.Id, writtenEntity.Id);
            Assert.Equal(entity.Category.Id, writtenEntity.Category.Id);
        }

        [Fact]
        public async Task Updates_entity_values()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultRootCategoryModel(WithDefaultProperty));

            EntityModel writtenEntity = default;

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns(entity);

                mock
                    .Setup(r => r.Upsert(It.IsAny<EntityModel>()))
                    .Callback<EntityModel>(c => writtenEntity = c)
                    .Returns(entity);
            });

            var value = Guid.NewGuid();

            // ACT
            var request = new UpdateEntityRequest(
                Values: new FacetPropertyValuesRequest(
                    new UpdateFacetPropertyValueRequest(entity.FacetProperties().Single().Id, entity.FacetProperties().Single().Type, value)));

            var result = await this.service.UpdateEntityAsync(entity.Id,request, CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.Name, writtenEntity.Name);
            Assert.Equal(entity.Id, writtenEntity.Id);
            Assert.Equal(entity.Category.Id, writtenEntity.Category.Id);
            Assert.Equal(value, entity.Values.Single().Value);
        }

        [Fact]
        public async Task Updates_entity_add_tag()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultRootCategoryModel());
            var tag = DefaultTagModel();

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns(tag);
            });

            EntityModel writtenEntity = default;

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns(entity);

                mock
                    .Setup(r => r.Upsert(It.IsAny<EntityModel>()))
                    .Callback<EntityModel>(c => writtenEntity = c)
                    .Returns(entity);
            });

            // ACT
            var updateEntityRequest = new UpdateEntityRequest(
                Tags: new UpdateEntityTagsRequest(new AssignTagRequest(tag.Id)));

            var result = await this.service.UpdateEntityAsync(entity.Id, updateEntityRequest, CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.Name, writtenEntity.Name);
            Assert.Equal(entity.Id, writtenEntity.Id);
            Assert.Equal(entity.Category.Id, writtenEntity.Category.Id);
            Assert.Equal(tag.Id, writtenEntity.Tags.Single().Id);
        }

        [Fact]
        public async Task Updates_entity_remove_tag()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultRootCategoryModel(), WithDefaultTag);
            var tag = entity.Tags.Single();

            EntityModel writtenEntity = default;

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns(entity);

                mock
                    .Setup(r => r.Upsert(It.IsAny<EntityModel>()))
                    .Callback<EntityModel>(c => writtenEntity = c)
                    .Returns(entity);
            });

            // ACT
            var updateEntityRequest = new UpdateEntityRequest(
                Tags: new UpdateEntityTagsRequest(new UnassignTagRequest(tag.Id)));

            var result = await this.service.UpdateEntityAsync(entity.Id, updateEntityRequest, CancellationToken.None);

            // ASSERT
            Assert.Equal(entity.Name, writtenEntity.Name);
            Assert.Equal(entity.Id, writtenEntity.Id);
            Assert.Equal(entity.Category.Id, writtenEntity.Category.Id);
            Assert.Empty(writtenEntity.Tags);
        }

        [Fact]
        public async Task Updating_entity_fails_on_missing_entity()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultRootCategoryModel());

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns((EntityModel)null);
            });

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service.UpdateEntityAsync(entity.Id, new UpdateEntityRequest(Name: "changed"), CancellationToken.None));

            // ASSERT
            Assert.Equal($"Entity(id='{entity.Id}') wasn't updated: Entity(id='{entity.Id}') doesn't exist", result.Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Deletes_entity(bool deleteResult)
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultRootCategoryModel());

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns(entity);

                mock
                    .Setup(r => r.Delete(entity))
                    .Returns(deleteResult);
            });

            // ACT
            var result = await this.service.DeleteEntityAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(deleteResult, result);
        }

        [Fact]
        public async Task Deleting_entity_returns_false_if_entity_missing()
        {
            // ARRANGE
            var entity = DefaultEntityModel(DefaultRootCategoryModel());

            this.ArrangeEntityRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(entity.Id))
                    .Returns((EntityModel)null);
            });

            // ACT
            var result = await this.service.DeleteEntityAsync(entity.Id, CancellationToken.None);

            // ASSERT
            Assert.False(result);
        }

        private void ArrangeEntityRepository(Action<Mock<IEntityRepository>> arrange)
        {
            this.modelMock
                .Setup(m => m.Entities)
                .Returns(this.entityRepositoryMock.Object);

            arrange?.Invoke(this.entityRepositoryMock);
        }

        #endregion Entity

        #region Tag

        [Fact]
        public async Task Creates_tag()
        {
            // ARRANGE
            TagModel storedTag = null;
            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.Upsert(It.IsAny<TagModel>()))
                    .Callback<TagModel>(t => storedTag = t)
                    .Returns<TagModel>(t => t);
            });

            // ACT
            var createTagRequest = new CreateTagRequest(
                Name: "t",
                Facet: new FacetRequest(
                    new CreateFacetPropertyRequest(
                        Name: "p",
                        Type: FacetPropertyTypeValues.String)));

            var result = await this.service.CreateTagAsync(createTagRequest, CancellationToken.None);

            // ASSERT
            Assert.Equal("t", result.Name);
            Assert.Equal("p", result.Facet.Properties.Single().Name);
            Assert.Equal(FacetPropertyTypeValues.String, result.Facet.Properties.Single().Type);

            Assert.Equal("t", storedTag.Name);
            Assert.Equal("p", storedTag.Facet.Properties.Single().Name);
            Assert.Equal(FacetPropertyTypeValues.String, storedTag.Facet.Properties.Single().Type);
        }

        [Fact]
        public async Task Creating_tag_fails_on_null()
        {
            // ACT
            var result = await Assert.ThrowsAsync<ArgumentNullException>(() => this.service.CreateTagAsync(null, CancellationToken.None));

            // ASSERT
            Assert.Equal("createTagRequest", result.ParamName);
        }

        [Fact]
        public async Task Reads_all_tags()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindAll())
                    .Returns(new[] { tag });
            });

            // ACT
            var result = await this.service.GetTagsAsync(CancellationToken.None);

            // ASSERT
            var tagResult = tag.ToTagResult();

            Assert.Equal(tagResult.Facet.Properties, result.Single().Facet.Properties);
            Assert.Equal(tagResult.Facet.Name, result.Single().Facet.Name);
            Assert.Equal(tagResult.Facet.Id, result.Single().Facet.Id);
            Assert.Equal(tagResult.Name, result.Single().Name);
            Assert.Equal(tagResult.Id, result.Single().Id);
        }

        [Fact]
        public async Task Reads_tag()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns(tag);
            });

            // ACT
            var result = await this.service.GetTagByIdAsync(tag.Id, CancellationToken.None);

            // ASSERT
            var tagResult = tag.ToTagResult();

            Assert.Equal(tagResult.Facet.Properties, result.Facet.Properties);
            Assert.Equal(tagResult.Facet.Name, result.Facet.Name);
            Assert.Equal(tagResult.Facet.Id, result.Facet.Id);
            Assert.Equal(tagResult.Name, result.Name);
            Assert.Equal(tagResult.Id, result.Id);
        }

        [Fact]
        public async Task Reading_tag_returns_null_if_missing()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns((TagModel)null);
            });

            // ACT
            var result = await this.service.GetTagByIdAsync(tag.Id, CancellationToken.None);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task Updates_tag()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            TagModel writtenTag = default;

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns(tag);

                mock
                    .Setup(r => r.Upsert(It.IsAny<TagModel>()))
                    .Callback<TagModel>(c => writtenTag = c)
                    .Returns(tag);
            });

            // ACT
            var result = await this.service.UpdateTagAsync(tag.Id, new UpdateTagRequest(Name: "changed"), CancellationToken.None);

            // ASSERT
            Assert.Equal("changed", writtenTag.Name);
            Assert.Equal(tag.Id, writtenTag.Id);
        }

        [Fact]
        public async Task Updating_tag_fails_on_missing_tag()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns((TagModel)null);
            });

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service.UpdateTagAsync(tag.Id, new UpdateTagRequest(Name: "changed"), CancellationToken.None));

            // ASSERT
            Assert.Equal($"Tag(id='{tag.Id}') wasn't updated: Tag(id='{tag.Id}') doesn't exist", result.Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Deletes_tag(bool deleteResult)
        {
            // ARRANGE
            var tag = DefaultTagModel();

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns(tag);

                mock
                    .Setup(r => r.Delete(tag))
                    .Returns(deleteResult);
            });

            // ACT
            var result = await this.service.DeleteTagAsync(tag.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(deleteResult, result);
        }

        [Fact]
        public async Task Deleting_tag_returns_false_if_tag_missing()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            this.ArrangeTagRepository(mock =>
            {
                mock
                    .Setup(r => r.FindById(tag.Id))
                    .Returns((TagModel)null);
            });

            // ACT
            var result = await this.service.DeleteTagAsync(tag.Id, CancellationToken.None);

            // ASSERT
            Assert.False(result);
        }

        private void ArrangeTagRepository(Action<Mock<ITagRepository>> arrange)
        {
            this.modelMock
                .Setup(m => m.Tags)
                .Returns(this.tagRepositoryMock.Object);

            arrange?.Invoke(this.tagRepositoryMock);
        }

        #endregion Tag
    }
}