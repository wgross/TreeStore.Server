using System;
using Xunit;

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

        [Fact]
        public void Tag_rejects_null_facet()
        {
            // ARRANGE
            var tag = new TagModel("t");

            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => tag.Facet = null);

            // ASSERT
            Assert.Equal("value", result.ParamName);
        }
    }
}