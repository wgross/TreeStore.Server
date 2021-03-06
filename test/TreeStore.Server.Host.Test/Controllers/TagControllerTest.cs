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
            var tag = DefaultTagModel();

            this.ModelServiceMock
                .Setup(s => s.CreateTagAsync(It.Is<CreateTagRequest>(r => tag.Name.Equals(r.Name)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tag.ToTagResult());

            // ACT
            var result = await this.clientService.CreateTagAsync(new CreateTagRequest(
                Name: tag.Name,
                Facet: new FacetRequest(new CreateFacetPropertyRequest(
                    Name: tag.Facet.Properties.Single().Name,
                    Type: tag.Facet.Properties.Single().Type)
                )), CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            var tagResult = tag.ToTagResult();

            Assert.Equal(tagResult.Facet.Properties, result.Facet.Properties);
            Assert.Equal(tagResult.Facet.Name, result.Facet.Name);
            Assert.Equal(tagResult.Facet.Id, result.Facet.Id);
            Assert.Equal(tagResult.Name, result.Name);
            Assert.Equal(tagResult.Id, result.Id);
        }

        [Fact]
        public async Task Creating_tag_rethrows()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            this.ModelServiceMock
                .Setup(s => s.CreateTagAsync(It.IsAny<CreateTagRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidModelException("fail", new Exception("innerFail")));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidModelException>(() => this.clientService.CreateTagAsync(new CreateTagRequest(tag.Name), CancellationToken.None)).ConfigureAwait(false);

            // ASSERT
            Assert.Equal("fail: innerFail", result.Message);
        }

        #endregion CREATE

        #region READ

        [Fact]
        public async Task Read_tag_by_id()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            this.ModelServiceMock
                .Setup(s => s.GetTagByIdAsync(tag.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tag.ToTagResult());

            // ACT
            var result = await this.clientService.GetTagByIdAsync(tag.Id, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            var tagResult = tag.ToTagResult();

            Assert.Equal(tagResult.Facet.Properties, result.Facet.Properties);
            Assert.Equal(tagResult.Facet.Name, result.Facet.Name);
            Assert.Equal(tagResult.Facet.Id, result.Facet.Id);
            Assert.Equal(tagResult.Name, result.Name);
            Assert.Equal(tagResult.Id, result.Id);
        }

        [Fact]
        public async Task Reading_unknown_tag_by_id_returns_null()
        {
            // ARRANGE
            this.ModelServiceMock
                .Setup(s => s.GetTagByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((TagResult)null);

            // ACT
            var result = await this.clientService.GetTagByIdAsync(Guid.NewGuid(), CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public async Task Read_all_tags()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            this.ModelServiceMock
                .Setup(s => s.GetTagsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { tag.ToTagResult() });

            // ACT
            var result = await this.clientService.GetTagsAsync(CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            var tagResult = tag.ToTagResult();

            Assert.Equal(tagResult.Facet.Properties, result.Single().Facet.Properties);
            Assert.Equal(tagResult.Facet.Name, result.Single().Facet.Name);
            Assert.Equal(tagResult.Facet.Id, result.Single().Facet.Id);
            Assert.Equal(tagResult.Name, result.Single().Name);
            Assert.Equal(tagResult.Id, result.Single().Id);
        }

        #endregion READ

        #region UPDATE

        [Fact]
        public async Task Update_tag()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            this.ModelServiceMock
                .Setup(s => s.UpdateTagAsync(tag.Id, It.IsAny<UpdateTagRequest>(), It.IsAny<CancellationToken>()))
                //.Callback<Guid, UpdateTagRequest, CancellationToken>((id, updt, c) => updt.Apply(tag))
                .ReturnsAsync(() => tag.ToTagResult());

            // ACT
            var updateTagRequest = new UpdateTagRequest(
                Name: "changed",
                Facet: new(
                    new UpdateFacetPropertyRequest(
                        Id: tag.Facet.Properties.First().Id,
                        Name: "changed"),
                    new CreateFacetPropertyRequest(
                        Name: "new",
                        Type: FacetPropertyTypeValues.DateTime)));

            var result = await this.clientService.UpdateTagAsync(tag.Id, updateTagRequest, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            var tagResult = tag.ToTagResult();

            Assert.Equal(tagResult.Facet.Properties, result.Facet.Properties);
            Assert.Equal(tagResult.Facet.Name, result.Facet.Name);
            Assert.Equal(tagResult.Facet.Id, result.Facet.Id);
            Assert.Equal(tagResult.Name, result.Name);
            Assert.Equal(tagResult.Id, result.Id);
        }

        [Fact]
        public async Task Updating_tag_fails()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            this.ModelServiceMock
                .Setup(s => s.UpdateTagAsync(tag.Id, It.IsAny<UpdateTagRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidModelException("fail", new Exception("innerFail")));

            // ACT
            var result = await Assert.ThrowsAsync<InvalidModelException>(() => this.clientService.UpdateTagAsync(tag.Id, new UpdateTagRequest(tag.Name), CancellationToken.None)).ConfigureAwait(false);

            // ASSERT
            Assert.Equal("fail: innerFail", result.Message);
        }

        #endregion UPDATE

        #region DELETE

        [Fact]
        public async Task Delete_tag()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            this.ModelServiceMock
                .Setup(s => s.DeleteTagAsync(tag.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // ACT
            var result = await this.clientService.DeleteTagAsync(tag.Id, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            Assert.True(result);
        }

        #endregion DELETE
    }
}