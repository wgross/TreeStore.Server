using System.Linq;
using TreeStore.Model.Abstractions;
using Xunit;
using static TreeStore.Model.Test.TestDataSources;

namespace TreeStore.Model.Test
{
    public class TagTest
    {
        [Fact]
        public void Tag_contains_facet_with_same_name()
        {
            // ACT

            var result = new TagModel("t");

            // ASSERT

            Assert.Equal("t", result.Facet.Name);
        }

       
    }
}