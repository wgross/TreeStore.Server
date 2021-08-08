using System.Linq;
using TreeStore.Model;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Server.Host.Test.Serialization
{
    public class TagSerializationTest : SerializationTestBase
    {
        [Fact]
        public void TagResult_json_roundtrip()
        {
            // ARRAnGE
            var tagResult = DefaultTagModel().ToTagResult();

            var result = SerializationRoundTrip(tagResult);

            Assert.Equal(tagResult.Id, result.Id);
            Assert.Equal(tagResult.Name, result.Name);
            Assert.Equal(tagResult.Facet.Id, result.Facet.Id);
            Assert.Equal(tagResult.Facet.Properties.Single().Id, result.Facet.Properties.Single().Id);
            Assert.Equal(tagResult.Facet.Properties.Single().Name, result.Facet.Properties.Single().Name);
            Assert.Equal(tagResult.Facet.Properties.Single().Type, result.Facet.Properties.Single().Type);
        }
    }
}