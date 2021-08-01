using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TreeStore.Model;
using TreeStore.Model.Abstractions;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Server.Host.Test.Controllers
{
    public class TagControllerTest : TreeStoreServerHostTestBase
    {
        public TagControllerTest()
        {
        }

        #region CREATE

        [Fact]
        public async Task Create_new_tag()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.serviceMock
                .Setup(s => s.CreateTagAsync(It.Is<CreateTagRequest>(r => tag.Name.Equals(r.Name)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tag.ToTagResult());

            // ACT
            var result = await this.service.CreateTagAsync(new CreateTagRequest(tag.Name), CancellationToken.None);

            // ASSERT
            Assert.Equal(tag.ToTagResult(), result);
        }

        [Fact]
        public async Task Creating_tag_rethrows()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.serviceMock
                .Setup(s => s.CreateTagAsync(It.IsAny<CreateTagRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidModelException("fail", new Exception("innerFail")));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidModelException>(() => this.service.CreateTagAsync(new CreateTagRequest(tag.Name), CancellationToken.None));

            // ASSERT
            Assert.Equal("fail: innerFail", result.Message);
        }

        #endregion CREATE

        #region READ

        [Fact]
        public async Task Read_tag_by_id()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.serviceMock
                .Setup(s => s.GetTagByIdAsync(tag.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tag.ToTagResult());

            // ACT
            var result = await this.service.GetTagByIdAsync(tag.Id, CancellationToken.None);

            // ASSERT
            Assert.Equal(tag.ToTagResult(), result);
        }

        [Fact]
        public async Task Reading_unknown_tag_by_id_returns_null()
        {
            // ARRANGE
            this.serviceMock
                .Setup(s => s.GetTagByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((TagResult)null);

            // ACT
            var result = await this.service.GetTagByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task Read_all_tags()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.serviceMock
                .Setup(s => s.GetTagsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { tag.ToTagResult() });

            // ACT
            var result = await this.service.GetTagsAsync(CancellationToken.None);

            // ASSERT
            Assert.Equal(tag.ToTagResult(), result.Single());
        }

        #endregion READ

        #region UPDATE

        [Fact]
        public async Task Update_tag()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.serviceMock
                .Setup(s => s.UpdateTagAsync(tag.Id, It.IsAny<UpdateTagRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tag.ToTagResult());

            // ACT
            var result = await this.service.UpdateTagAsync(tag.Id, new UpdateTagRequest(tag.Name), CancellationToken.None);

            // ASSERT
            Assert.Equal(tag.ToTagResult(), result);
        }

        [Fact]
        public async Task Updating_tag_fails()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.serviceMock
                .Setup(s => s.UpdateTagAsync(tag.Id, It.IsAny<UpdateTagRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidModelException("fail", new Exception("innerFail")));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidModelException>(() => this.service.UpdateTagAsync(tag.Id, new UpdateTagRequest(tag.Name), CancellationToken.None));

            // ASSERT
            Assert.Equal("fail: innerFail", result.Message);
        }

        #endregion UPDATE

        #region DELETE

        [Fact]
        public async Task Delete_tag()
        {
            // ARRANGE
            var tag = DefaultTag();

            this.serviceMock
                .Setup(s => s.DeleteTagAsync(tag.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await this.service.DeleteTagAsync(tag.Id, CancellationToken.None);

            // ASSERT
            Assert.True(result);
        }

        #endregion DELETE
    }
}