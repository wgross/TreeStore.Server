using System.Linq;
using TreeStore.Model.Abstractions;
using Xunit;
using static TreeStore.Model.Test.TestDataSources;

namespace TreeStore.Model.Test
{
    public class TagRequestTest
    {
        [Fact]
        public void CreateTagRequest_updates_name()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            // ACT
            // var result = new CreateTagRequest(Name: "created").Apply(tag);

            // ASSERT
            // Assert.Equal("created", result.Name);
        }

        [Fact]
        public void UpdateTagRequest_updates_name()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            // ACT
            var result = new UpdateTagRequest(Name: "changed").Apply(tag);

            // ASSERT
            Assert.Equal("changed", result.Name);
            Assert.Single(tag.Facet.Properties);
        }

        [Fact]
        public void UpdateTagRequest_updates_facet_property()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            var request = new UpdateTagRequest(
                Facet: new FacetRequest(
                    Properties: new UpdateFacetPropertyRequest(tag.Facet.Properties.Single().Id, Name: "changed")));

            // ACT
            var result = request.Apply(tag);

            // ASSERT
            Assert.Equal("t", result.Name);
            Assert.Equal("changed", result.Facet.Properties.Single().Name);
        }

        [Fact]
        public void UpdateTagRequest_removes_identified_facet_property()
        {
            // ARRANGE
            var tag = DefaultTagModel(t => t.Facet.AddProperty(DefaultFacetPropertyModel(fp => fp.Name = "q")));

            var request = new UpdateTagRequest(Facet: new FacetRequest
            {
                DeleteProperties = new[] { new DeleteFacetPropertyRequest(Id: tag.Facet.Properties.First().Id) }
            });

            // ACT
            var result = request.Apply(tag);

            // ASSERT
            Assert.Equal("t", result.Name);
            Assert.Single(result.Facet.Properties);
            Assert.Equal("q", result.Facet.Properties.Single().Name);
        }

        [Fact]
        public void UpdateTagRequest_creates_facet_property()
        {
            // ARRANGE
            var tag = DefaultTagModel();

            var request = new UpdateTagRequest(
                Facet: new FacetRequest(new CreateFacetPropertyRequest("q", FacetPropertyTypeValues.Bool)));

            // ACT
            var result = request.Apply(tag);

            // ASSERT
            Assert.Equal("t", result.Name);
            Assert.Equal(2, result.Facet.Properties.Count());
            Assert.Equal("p", result.Facet.Properties.ElementAt(0).Name);
            Assert.Equal(FacetPropertyTypeValues.String, result.Facet.Properties.ElementAt(0).Type);
            Assert.Equal("q", result.Facet.Properties.ElementAt(1).Name);
            Assert.Equal(FacetPropertyTypeValues.Bool, result.Facet.Properties.ElementAt(1).Type);
        }
    }
}