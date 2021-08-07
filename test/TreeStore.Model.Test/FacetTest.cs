using System;
using System.Linq;
using Xunit;

namespace TreeStore.Model.Test
{
    public class FacetTest
    {
        [Fact]
        public void Facet_hasnt_properties_at_the_beginning()
        {
            // ACT

            var facet = new FacetModel();

            // ASSERT

            Assert.Empty(facet.Properties);
        }

        [Fact]
        public void Facet_adds_property()
        {
            // ARRANGE

            var facet = new FacetModel();
            var property = new FacetPropertyModel();

            // ACT

            facet.AddProperty(property);

            // ASSERT

            Assert.Equal(property, facet.Properties.Single());
        }

        [Fact]
        public void Facet_gets_property_by_id()
        {
            // ARRANGE
            var facet = new FacetModel();
            var property = new FacetPropertyModel();

            facet.AddProperty(property);

            // ACT
            var result = facet.GetProperty(property.Id);

            // ASSERT
            Assert.Same(property, result);
        }

        [Fact]
        public void Facet_removes_property()
        {
            // ARRANGE

            var property1 = new FacetPropertyModel();
            var property2 = new FacetPropertyModel();
            var facet = new FacetModel("facet", property1, property2);

            // ACT

            facet.RemoveProperty(property2);

            // ASSERT

            Assert.Equal(property1, facet.Properties.Single());
        }

        [Fact]
        public void Facet_rejects_duplicate_property_name()
        {
            // ARRANGE

            var facet = new FacetModel(string.Empty, new FacetPropertyModel("name"));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => facet.AddProperty(new FacetPropertyModel("name")));

            // ASSERT

            Assert.Equal("duplicate property name: name", result.Message);
        }
    }
}