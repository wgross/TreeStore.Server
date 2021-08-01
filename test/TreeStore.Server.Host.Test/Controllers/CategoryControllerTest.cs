using Moq;
using System;
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
            var category = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.CreateCategoryAsync(It.Is<CreateCategoryRequest>(r => category.Name.Equals(r.Name)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.ToCategoryResult());

            // ACT
            var result = await this.service
                .CreateCategoryAsync(new(category.Name, this.rootCategory.Id), CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.Equal(category.ToCategoryResult(), result);
        }

        [Fact]
        public async Task Create_new_category_rethrows_if_create_fails()
        {
            // ARRANGE
            var category = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.CreateCategoryAsync(It.Is<CreateCategoryRequest>(r => category.Name.Equals(r.Name)), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("fail"));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service
                .CreateCategoryAsync(new(category.Name, this.rootCategory.Id), CancellationToken.None));

            // ASSERT
            Assert.Equal("fail", result.Message);
        }

        #endregion CREATE

        #region READ

        [Fact]
        public async Task Read_category_by_id()
        {
            // ARRANGE
            var category = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.GetCategoryByIdAsync(category.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.ToCategoryResult());

            // ACT
            var result = await this.service
                .GetCategoryByIdAsync(category.Id, CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.Equal(category.ToCategoryResult(), result);
        }

        [Fact]
        public async Task Reading_unknown_category_by_id_returns_null()
        {
            // ARRANGE
            this.serviceMock
                .Setup(s => s.GetCategoryByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CategoryResult)null);

            // ACT
            var result = await this.service
                .GetCategoryByIdAsync(Guid.NewGuid(), CancellationToken.None)
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
            var category = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.UpdateCategoryAsync(category.Id, It.IsAny<UpdateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(category.ToCategoryResult());

            // ACT
            var result = await this.service
                .UpdateCategoryAsync(category.Id, new UpdateCategoryRequest(category.Name), CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.Equal(category.ToCategoryResult(), result);
        }

        [Fact]
        public async Task Update_rethrows_if_update_fails()
        {
            // ARRANGE
            var entity = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.UpdateCategoryAsync(entity.Id, It.IsAny<UpdateCategoryRequest>(), It.IsAny<CancellationToken>()))
                .Throws(new InvalidOperationException("fail"));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service
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
            var category = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.DeleteCategoryAsync(category.Id, recurse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await this.service
                .DeleteCategoryAsync(category.Id, recurse, CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.True(result);
        }

        [Fact]
        public async Task Delete_category_fails()
        {
            // ARRANGE
            var category = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.DeleteCategoryAsync(category.Id, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // ACT
            var result = await this.service
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
            var sourceCategory = DefaultCategory(this.rootCategory);
            var destinationCategory = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.CopyCategoryToAsync(sourceCategory.Id, destinationCategory.Id, recurse, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CopyCategoryResponse());

            // ACT
            var result = await this.service
                .CopyCategoryToAsync(sourceCategory.Id, destinationCategory.Id, recurse: recurse, CancellationToken.None)
                .ConfigureAwait(false);

            // ASSERT
            Assert.NotNull(result);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Copy_category_rethrows_if_copy_fails(bool recurse)
        {
            // ARRANGE
            var sourceCategory = DefaultCategory(this.rootCategory);
            var destinationCategory = DefaultCategory(this.rootCategory);

            this.serviceMock
                .Setup(s => s.CopyCategoryToAsync(sourceCategory.Id, destinationCategory.Id, recurse, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("fail"));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidOperationException>(() => this.service
               .CopyCategoryToAsync(sourceCategory.Id, destinationCategory.Id, recurse: recurse, CancellationToken.None));

            // ASSERT
            Assert.Equal("fail", result.Message);
        }

        #endregion COPY
    }
}