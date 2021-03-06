using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Server.Host.Test.Controllers
{
    public partial class CategoryControllerTest
    {
        #region CREATE

        [Fact]
        public async Task Create_new_category()
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.CreateCategoryAsync(It.Is<CreateCategoryRequest>(r => category.Name.Equals(r.Name)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.ToCategoryResult());

            // ACT
            var result = await this.clientService
                .CreateCategoryAsync(new(category.Name, this.rootCategory.Id), CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.Parent.Id, result.ParentId);

            FacetPropertyResult getFacetProperty(Guid id) => result.Facet.Properties.Single(fp => fp.Id == id);

            Assert.All(category.Facet.Properties, fp =>
            {
                Assert.Equal(fp.Name, getFacetProperty(fp.Id).Name);
                Assert.Equal(fp.Type, getFacetProperty(fp.Id).Type);
            });
        }

        [Fact]
        public async Task Create_new_category_rethrows_if_create_fails()
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.CreateCategoryAsync(It.Is<CreateCategoryRequest>(r => category.Name.Equals(r.Name)), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("fail"));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.clientService
                .CreateCategoryAsync(new(category.Name, this.rootCategory.Id), CancellationToken.None)).ConfigureAwait(false);

            // ASSERT
            Assert.Equal("fail", result.Message);
        }

        #endregion CREATE

        #region READ

        [Fact]
        public async Task Read_root_category()
        {
            // ARRANGE
            this.ModelServiceMock
                .Setup(s => s.GetRootCategoryAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(this.rootCategory.ToCategoryResult());

            // ACT
            var result = await this.clientService
                .GetRootCategoryAsync(CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.Equal(this.rootCategory.Id, result.Id);
            Assert.Equal(this.rootCategory.Name, result.Name);
            Assert.Equal(Guid.Empty, result.ParentId);

            FacetPropertyResult getFacetProperty(Guid id) => result.Facet.Properties.Single(fp => fp.Id == id);

            Assert.All(this.rootCategory.Facet.Properties, fp =>
            {
                Assert.Equal(fp.Name, getFacetProperty(fp.Id).Name);
                Assert.Equal(fp.Type, getFacetProperty(fp.Id).Type);
            });
        }

        [Fact]
        public async Task Read_category_by_id()
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.ToCategoryResult());

            // ACT
            var result = await this.clientService
                .GetCategoryByIdAsync(category.Id, CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.Parent.Id, result.ParentId);

            FacetPropertyResult getFacetProperty(Guid id) => result.Facet.Properties.Single(fp => fp.Id == id);

            Assert.All(category.Facet.Properties, fp =>
            {
                Assert.Equal(fp.Name, getFacetProperty(fp.Id).Name);
                Assert.Equal(fp.Type, getFacetProperty(fp.Id).Type);
            });
        }

        [Fact]
        public async Task Read_categories_by_id()
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.GetCategoriesByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { category.ToCategoryResult() });

            // ACT
            var result = await this.clientService
                .GetCategoriesByIdAsync(category.Parent.Id, CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.Equal(category.Id, result.Single().Id);
            Assert.Equal(category.Name, result.Single().Name);
            Assert.Equal(category.Parent.Id, result.Single().ParentId);

            FacetPropertyResult getFacetProperty(Guid id) => result.Single().Facet.Properties.Single(fp => fp.Id == id);

            Assert.All(category.Facet.Properties, fp =>
            {
                Assert.Equal(fp.Name, getFacetProperty(fp.Id).Name);
                Assert.Equal(fp.Type, getFacetProperty(fp.Id).Type);
            });
        }

        [Fact]
        public async Task Reading_unknown_category_by_id_returns_null()
        {
            // ARRANGE
            this.ModelServiceMock
                .Setup(s => s.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CategoryResult)null);

            // ACT
            var result = await this.clientService
                .GetCategoryByIdAsync(Guid.NewGuid(), CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task Read_categories_by_id_return_null_on_missing_category()
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.GetCategoriesByIdAsync(category.Parent.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IEnumerable<CategoryResult>)null);

            // ACT
            var result = await this.clientService
                .GetCategoriesByIdAsync(category.Parent.Id, CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.Null(result);
        }

        #endregion READ

        #region UPDATE

        [Fact]
        public async Task Update_category()
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.UpdateCategoryAsync(category.Id, It.IsAny<UpdateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.ToCategoryResult());

            // ACT
            var result = await this.clientService
                .UpdateCategoryAsync(category.Id, new UpdateCategoryRequest(category.Name), CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.Equal(category.Id, result.Id);
            Assert.Equal(category.Name, result.Name);
            Assert.Equal(category.Parent.Id, result.ParentId);

            FacetPropertyResult getFacetProperty(Guid id) => result.Facet.Properties.Single(fp => fp.Id == id);

            Assert.All(category.Facet.Properties, fp =>
            {
                Assert.Equal(fp.Name, getFacetProperty(fp.Id).Name);
                Assert.Equal(fp.Type, getFacetProperty(fp.Id).Type);
            });
        }

        [Fact]
        public async Task Update_rethrows_if_update_fails()
        {
            // ARRANGE
            var entity = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.UpdateCategoryAsync(entity.Id, It.IsAny<UpdateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("fail"));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.clientService
                .UpdateCategoryAsync(entity.Id, new UpdateCategoryRequest(entity.Name), CancellationToken.None))
                .ConfigureAwait(false);

            // ASSERT
            Assert.Equal("fail", result.Message);
        }

        #endregion UPDATE

        #region DELETE

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Delete_category(bool recurse)
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.DeleteCategoryAsync(category.Id, recurse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await this.clientService
                .DeleteCategoryAsync(category.Id, recurse, CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.True(result);
        }

        [Fact]
        public async Task Delete_category_fails()
        {
            // ARRANGE
            var category = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.DeleteCategoryAsync(category.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT
            var result = await this.clientService
                .DeleteCategoryAsync(category.Id, false, CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.False(false);
        }

        #endregion DELETE

        #region COPY

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Copy_category(bool recurse)
        {
            // ARRANGE
            var sourceCategory = DefaultCategoryModel(this.rootCategory);
            var destinationCategory = DefaultCategoryModel(this.rootCategory);
            var copiedCatagory = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.CopyCategoryToAsync(sourceCategory.Id, destinationCategory.Id, recurse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(copiedCatagory.ToCategoryResult());

            // ACT
            var result = await this.clientService
                .CopyCategoryToAsync(sourceCategory.Id, destinationCategory.Id, recurse: recurse, CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(copiedCatagory.Id, result.Id);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Copy_category_rethrows_if_copy_fails(bool recurse)
        {
            // ARRANGE
            var sourceCategory = DefaultCategoryModel(this.rootCategory);
            var destinationCategory = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.CopyCategoryToAsync(sourceCategory.Id, destinationCategory.Id, recurse, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("fail"));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.clientService
               .CopyCategoryToAsync(sourceCategory.Id, destinationCategory.Id, recurse: recurse, CancellationToken.None)).ConfigureAwait(false);

            // ASSERT
            Assert.Equal("fail", result.Message);
        }

        #endregion COPY

        #region MOVE

        [Fact]
        public async Task Move_category()
        {
            // ARRANGE
            var sourceCategory = DefaultCategoryModel(this.rootCategory);
            var destinationCategory = DefaultCategoryModel(this.rootCategory);
            var copiedCatagory = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.MoveCategoryToAsync(sourceCategory.Id, destinationCategory.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(copiedCatagory.ToCategoryResult());

            // ACT
            var result = await this.clientService
                .MoveCategoryToAsync(sourceCategory.Id, destinationCategory.Id, CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(copiedCatagory.Id, result.Id);
        }

        [Fact]
        public async Task Move_category_rethrows_if_move_fails()
        {
            // ARRANGE
            var sourceCategory = DefaultCategoryModel(this.rootCategory);
            var destinationCategory = DefaultCategoryModel(this.rootCategory);

            this.ModelServiceMock
                .Setup(s => s.MoveCategoryToAsync(sourceCategory.Id, destinationCategory.Id, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("fail"));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.clientService
               .MoveCategoryToAsync(sourceCategory.Id, destinationCategory.Id, CancellationToken.None)).ConfigureAwait(false);

            // ASSERT
            Assert.Equal("fail", result.Message);
        }

        #endregion MOVE
    }
}