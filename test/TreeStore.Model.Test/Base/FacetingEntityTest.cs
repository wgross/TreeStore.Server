using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TreeStore.Model.Test.Base
{
    public class FacetingEntityTest
    {
        public static IEnumerable<object[]> GetFactingInstances()
        {
            yield return new TagModel().Yield().ToArray();
            yield return new CategoryModel().Yield().ToArray();
        }

        [Theory]
        [MemberData(nameof(GetFactingInstances))]
        public void FacetingEntity_has_empty_facet_at_beginning(FacetingEntityBase faceted)
        {
            // ACT

            var result = faceted.Facet;

            // ASSERT

            Assert.Empty(result.Properties);
        }
    }
}