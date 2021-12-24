using Microsoft.Extensions.DependencyInjection;
using Moq;
using PowerShellFilesystemProviderBase;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Linq;
using System.Threading;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using TreeStoreFS.Nodes;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStoreFS.Test.Nodes
{
    public class CategoryNodeAdapterTest : NodeBaseTest
    {
        private readonly Mock<ITreeStoreService> treeStoreServiceMock;
        private readonly RootCategoryAdapter rootCategoryAdapter;

        public CategoryNodeAdapterTest()
        {
            this.treeStoreServiceMock = this.Mocks.Create<ITreeStoreService>();
            this.rootCategoryAdapter = new RootCategoryAdapter(this.treeStoreServiceMock.Object);
        }

        [Fact]
        public void Get_item()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel();
            var category = DefaultCategoryModel(root);

            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.ToCategoryResult());

            var categoryAdapter = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Id);

            // ACT
            var result = categoryAdapter.GetRequiredService<IGetItem>().GetItem();

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(category.Id, result.Property<Guid>("Id"));
            Assert.Equal(category.Name, result.Property<string>("Name"));
            Assert.Equal(category.Parent.Id, result.Property<Guid>("ParentId"));
        }

        [Fact]
        public void Creates_child_category()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel();

            this.treeStoreServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(root.ToCategoryResult());

            var child = DefaultCategoryModel(root, WithDefaultProperty);

            CreateCategoryRequest request = default;
            this.treeStoreServiceMock
                .Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .Callback<CreateCategoryRequest, CancellationToken>((r, _) => request = r)
                .ReturnsAsync(child.ToCategoryResult());

            // ACT
            var result = this.rootCategoryAdapter.GetService<INewChildItem>().NewChildItem(child.Name, "category", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(child.Name, request.Name);
            Assert.Equal(root.Id, request.ParentId);
        }

        [Fact]
        public void Creates_grandchild_category()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel();
            this.treeStoreServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(root.ToCategoryResult());

            var child = DefaultCategoryModel(root, WithDefaultProperty);
            this.treeStoreServiceMock
                .Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(child.ToCategoryResult());
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(child.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(child.ToCategoryResult());
            var childCategory = (ContainerNode)this.rootCategoryAdapter.GetService<INewChildItem>().NewChildItem(child.Name, "category", null);

            var grandchild = DefaultCategoryModel(child, WithDefaultProperty);

            CreateCategoryRequest request = default;
            this.treeStoreServiceMock
                .Setup(s => s.CreateCategoryAsync(It.IsAny<CreateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .Callback<CreateCategoryRequest, CancellationToken>((r, _) => request = r)
                .ReturnsAsync(child.ToCategoryResult());

            // ACT
            var result = childCategory.NewChildItem(grandchild.Name, "category", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(grandchild.Name, request.Name);
            Assert.Equal(child.Id, request.ParentId);
        }

        [Fact]
        public void Copies_child_category()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel();
            this.treeStoreServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(root.ToCategoryResult());

            var child = DefaultCategoryModel(root);
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(child.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(child.ToCategoryResult());
            var childNode = new ContainerNode(child.Name, new CategoryNodeAdapter(this.treeStoreServiceMock.Object, child.Id));

            var copied = DefaultCategoryModel(root);

            this.treeStoreServiceMock
                .Setup(s => s.CopyCategoryToAsync(child.Id, root.Id, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(copied.ToCategoryResult());

            // ACT
            var result = (ContainerNode)this.rootCategoryAdapter.GetService<ICopyChildItem>().CopyChildItem(childNode, new[] { "child2" });

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public void Copies_child_category_to_existing_destination()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel();
            
            var child = DefaultCategoryModel(root);
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(child.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(child.ToCategoryResult());
            var childNode = new ContainerNode(child.Name, new CategoryNodeAdapter(this.treeStoreServiceMock.Object, child.Id));

            var destination = DefaultCategoryModel(root, c => c.Name = "dest");
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(destination.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(destination.ToCategoryResult());
            var destinationNode = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, destination.Id);

            var copied = DefaultCategoryModel(root);
            this.treeStoreServiceMock
                .Setup(s => s.CopyCategoryToAsync(child.Id, destination.Id, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(copied.ToCategoryResult());

            // ACT
            var result = (ContainerNode)destinationNode.GetService<ICopyChildItem>().CopyChildItem(childNode, Array.Empty<string>());

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public void Copies_child_category_recursive()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel(WithDefaultProperty);
            this.treeStoreServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(root.ToCategoryResult());

            var child = DefaultCategoryModel(root, WithDefaultProperty);
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(child.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(child.ToCategoryResult());
            var childNode = new ContainerNode(child.Name, new CategoryNodeAdapter(this.treeStoreServiceMock.Object, child.Id));

            var copied = DefaultCategoryModel(root);

            this.treeStoreServiceMock
                .Setup(s => s.CopyCategoryToAsync(child.Id, root.Id, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(copied.ToCategoryResult());

            // ACT
            var result = (ContainerNode)this.rootCategoryAdapter.GetService<ICopyChildItemRecursive>().CopyChildItemRecursive(childNode, new[] { "child2" });

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public void Copies_child_entity_to_existing_destination()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel();

            var entity = DefaultEntityModel(root, e => e.Name = "entity");
            this.treeStoreServiceMock
                .Setup(s => s.GetEntityByIdAsync(entity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity.ToEntityResult());

            var destination = DefaultCategoryModel(root, c => c.Name = "dest");
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(destination.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(destination.ToCategoryResult());
            var destinationNode = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, destination.Id);

            var copied = DefaultEntityModel(destination);
            this.treeStoreServiceMock
                .Setup(s => s.CopyEntityToAsync(entity.Id, destination.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(copied.ToEntityResult());

            var entityNode = new LeafNode(entity.Name, new EntityNodeAdapter(this.treeStoreServiceMock.Object, entity.Id));

            // ACT
            var result = (LeafNode)destinationNode.GetService<ICopyChildItem>().CopyChildItem(entityNode, Array.Empty<string>());

            // ASSERT
            Assert.NotNull(result);
        }

        [Fact]
        public void Creates_child_entity()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel();

            this.treeStoreServiceMock
               .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(root.ToCategoryResult());

            var child = DefaultEntityModel(WithEntityCategory(root));

            CreateEntityRequest request = default;
            this.treeStoreServiceMock
                .Setup(s => s.CreateEntityAsync(It.IsAny<CreateEntityRequest>(), It.IsAny<CancellationToken>()))
                .Callback<CreateEntityRequest, CancellationToken>((r, _) => request = r)
                .ReturnsAsync(child.ToEntityResult());

            // ACT
            var result = this.rootCategoryAdapter.GetService<INewChildItem>().NewChildItem(child.Name, "entity", null);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(child.Name, request.Name);
            Assert.Equal(root.Id, request.CategoryId);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Removes_child_category(bool recurse)
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult(categories: category.Yield(), entities: Array.Empty<EntityModel>()));
            var parentCategory = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Parent.Id);

            this.treeStoreServiceMock
                .Setup(s => s.DeleteCategoryAsync(category.Parent.Id, category.Name, recurse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            // tell the node to remove the category child.
            parentCategory.GetRequiredService<IRemoveChildItem>().RemoveChildItem(category.Name, recurse);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Removes_child_entity(bool recurse)
        {
            // ARRANGE
            // put an entity under the root category
            var entity = DefaultEntityModel(DefaultRootCategoryModel());
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(entity.Category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity.Category.ToCategoryResult(categories: Array.Empty<CategoryModel>(), entities: entity.Yield()));

            var parentCategory = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, entity.Category.Id);

            this.treeStoreServiceMock
                .Setup(s => s.DeleteEntityAsync(entity.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            // tell the node to remote the entity child
            parentCategory.GetRequiredService<IRemoveChildItem>().RemoveChildItem(entity.Name, recurse);
        }

        [Fact]
        public void Reads_child_categories_without_children()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult());
            var parentCategory = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Parent.Id);

            // ACT
            var result = parentCategory.GetRequiredService<IGetChildItems>().GetChildItems();

            // ASSERT
            Assert.False(parentCategory.HasChildItems());
            Assert.Empty(result);
        }

        [Fact]
        public void Reads_child_categories_with_entities()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());

            var entity = DefaultEntityModel(WithEntityCategory(category));
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult(categories: new[] { category }, entities: null));

            var parentCategoryNode = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Parent.Id);

            this.treeStoreServiceMock
                .Setup(s => s.GetCategoriesByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { category.ToCategoryResult(categories: null, entities: new[] { entity }) });

            // ACT
            var result = parentCategoryNode.GetRequiredService<IGetChildItems>().GetChildItems();

            // ASSERT
            Assert.True(parentCategoryNode.HasChildItems());
            Assert.NotNull(result);
            Assert.Equal(category.Name, result.Single().Name);
        }

        [Fact]
        public void Reads_child_categories_with_categories()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            var subcatageory = DefaultCategoryModel(category);
            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult(categories: new[] { subcatageory }, entities: null));
            var parentCategory = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Parent.Id);

            this.treeStoreServiceMock
              .Setup(s => s.GetCategoriesByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(new[] { category.ToCategoryResult() });

            // ACT
            var result = parentCategory.GetRequiredService<IGetChildItems>().GetChildItems();

            // ASSERT
            Assert.True(parentCategory.HasChildItems());
        }

        [Fact]
        public void Reading_child_categories_returns_empty_list_on_unkown_parent()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            var subcategory = DefaultCategoryModel(category);

            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult(categories: new[] { subcategory }, entities: null));
            var parentNode = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Parent.Id);

            this.treeStoreServiceMock
                .Setup(s => s.GetCategoriesByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((CategoryResult[])null);

            // ACT
            var result = parentNode.GetRequiredService<IGetChildItems>().GetChildItems();

            // ASSERT
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("child")]
        [InlineData("CHILD")]
        public void Renames_child_category(string childName)
        {
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            var subcategory = DefaultCategoryModel(category, c => c.Name = childName.ToLowerInvariant());

            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult(categories: new[] { subcategory }, entities: null));
            var parentNode = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Parent.Id);

            UpdateCategoryRequest request = null;
            this.treeStoreServiceMock
                .Setup(s => s.UpdateCategoryAsync(subcategory.Id, It.IsAny<UpdateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, UpdateCategoryRequest, CancellationToken>((_, r, __) => request = r)
                .ReturnsAsync(subcategory.ToCategoryResult());

            // ACT
            parentNode.GetRequiredService<IRenameChildItem>().RenameChildItem(childName, "changed");

            // ASSERT
            Assert.Equal("changed", request.Name);
        }

        [Fact]
        public void Renaming_child_fails_on_unkown_category_name()
        {
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            var subcategory = DefaultCategoryModel(category);

            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult(categories: new[] { subcategory }, entities: null));
            var parentNode = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Parent.Id);

            // ACT
            var result = Assert.Throws<InvalidOperationException>(() => parentNode.GetRequiredService<IRenameChildItem>().RenameChildItem("unkown", "changed"));

            // ASSERT
            Assert.Equal("Child item (name='unkown') wasn't renamed: It doesn't exist", result.Message);
        }

        [Theory]
        [InlineData("child")]
        [InlineData("CHILD")]
        public void Renames_child_entity(string childName)
        {
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            var entity = DefaultEntityModel(WithEntityCategory(category), e => e.Name = childName.ToLowerInvariant());

            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.Parent.ToCategoryResult(categories: null, entities: new[] { entity }));
            var parentNode = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Id);

            UpdateEntityRequest request = null;
            this.treeStoreServiceMock
                .Setup(s => s.UpdateEntityAsync(entity.Id, It.IsAny<UpdateEntityRequest>(), It.IsAny<CancellationToken>()))
                .Callback<Guid, UpdateEntityRequest, CancellationToken>((_, r, __) => request = r)
                .ReturnsAsync(entity.ToEntityResult());

            // ACT
            parentNode.GetRequiredService<IRenameChildItem>().RenameChildItem(childName, "changed");

            // ASSERT
            Assert.Equal("changed", request.Name);
        }

        [Fact]
        public void Renaming_child_fails_on_unkown_entity_name()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            var entity = DefaultEntityModel(WithEntityCategory(category));

            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.ToCategoryResult(categories: null, entities: new[] { entity }));
            var parentNode = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Id);

            // ACT
            var result = Assert.Throws<InvalidOperationException>(() => parentNode.GetRequiredService<IRenameChildItem>().RenameChildItem("unkown", "changed"));

            // ASSERT
            Assert.Equal("Child item (name='unkown') wasn't renamed: It doesn't exist", result.Message);
        }

        [Fact]
        public void Renaming_child_fails_on_duplicate_name()
        {
            // ARRANGE
            var category = DefaultCategoryModel(DefaultRootCategoryModel());
            var entity = DefaultEntityModel(WithEntityCategory(category));
            var subcategory = DefaultCategoryModel(category);

            this.treeStoreServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.ToCategoryResult(categories: new[] { subcategory }, entities: new[] { entity }));
            var parentNode = new CategoryNodeAdapter(this.treeStoreServiceMock.Object, category.Id);

            // ACT
            var result = Assert.Throws<InvalidOperationException>(() => parentNode.GetRequiredService<IRenameChildItem>().RenameChildItem(entity.Name, subcategory.Name));

            // ASSERT
            Assert.Equal("Child item (name='e') wasn't renamed: There is already a child with name='c'", result.Message);
        }
    }
}