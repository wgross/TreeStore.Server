using System;
using System.Linq;
using Xunit;
using static TreeStore.Test.Common.TreeStoreTestData;

namespace TreeStore.Model.Test
{
    public class EntityTest
    {
        [Fact]
        public void Entity_has_no_category_and_no_tag()
        {
            // ARRANGE
            var entity = DefaultEntityModel();

            // ASSERT
            Assert.Null(entity.Category);
            Assert.Empty(entity.Tags);
            Assert.Empty(entity.Facets());
            Assert.Empty(entity.FacetProperties());
            Assert.Empty(entity.FacetPropertyValues());
        }

        [Fact]
        public void Entity_assigns_Category()
        {
            // ARRANGE
            var category = DefaultRootCategoryModel(WithDefaultProperties);
            var entity = DefaultEntityModel();

            // ACT
            entity.SetCategory(category);

            // ASSERT
            Assert.Same(category, entity.Category);
            Assert.Same(category.Facet, entity.Facets().Single());
            Assert.Equal(category.Facet.Properties, entity.FacetProperties());
            Assert.All(entity.FacetPropertyValues(), fpv => Assert.False(fpv.hasValue));
        }

        [Fact]
        public void Entity_has_calculated_index_property()
        {
            // ARRANGE
            var category = new CategoryModel();
            var entity = DefaultEntityModel(WithEntityCategory(category));

            // ACT
            var result = entity.UniqueName;

            // ASSERT
            Assert.Equal($"{entity.Name}_{category.Id}", entity.UniqueName);
        }

        [Fact]
        public void Entity_assigns_Tag()
        {
            // ARRANGE

            var tag = DefaultTagModel(WithDefaultProperties);
            var entity = DefaultEntityModel();

            // ACT

            entity.AddTag(tag);

            // ASSERT
            Assert.Equal(tag, entity.Tags.Single());
            Assert.Same(tag.Facet, entity.Facets().Single());
            Assert.Equal(tag.Facet.Properties, entity.FacetProperties());
            Assert.All(entity.FacetPropertyValues(), fpv => Assert.False(fpv.hasValue));
        }

        [Fact]
        public void Entity_adding_null_Tag_fails()
        {
            // ARRANGE
            var entity = DefaultEntityModel();

            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => entity.AddTag((TagModel)null));

            // ASSERT
            Assert.Equal("tag", result.ParamName);
        }

        [Fact]
        public void Entity_adding_Tag_ignores_duplicate()
        {
            // ARRANGE
            var tag = DefaultTagModel();
            var entity = DefaultEntityModel(WithTag(tag));

            // ACT
            entity.AddTag(tag);

            // ASSERT
            Assert.Equal(tag, entity.Tags.Single());
        }

        [Fact]
        public void Entity_has_Facet_from_Category_and_Tag()
        {
            // ARRANGE
            var tag = DefaultTagModel();
            var category = DefaultRootCategoryModel();
            var entity = DefaultEntityModel();

            // ACT
            entity.AddTag(tag);
            entity.SetCategory(category);

            // ASSERT
            Assert.Equal(new[] { category.Facet, tag.Facet }, entity.Facets());
            Assert.Equal(category.Facet.Properties.Union(tag.Facet.Properties), entity.FacetProperties());
            Assert.All(entity.FacetPropertyValues(), fpv => Assert.False(fpv.hasValue));
        }

        [Fact]
        public void Entity_sets_value_of_tags_facetproperty()
        {
            // ARRANGE
            var tag = DefaultTagModel(WithDefaultProperty);
            var entity = DefaultEntityModel(WithTag(tag));
            var value = Guid.NewGuid();

            // ACT
            entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), value);

            // ASSERT
            Assert.Equal(value, entity.GetFacetPropertyValue(tag.Facet.Properties.Single()).value);
        }

        [Fact]
        public void Entity_sets_value_of_categories_facetproperty()
        {
            // ARRANGE
            var category = DefaultRootCategoryModel(WithDefaultProperty);
            var entity = DefaultEntityModel(WithEntityCategory(category));
            var value = Guid.NewGuid();

            // ACT
            entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), value);

            // ASSERT
            Assert.Equal(value, entity.GetFacetPropertyValue(category.Facet.Properties.Single()).value);
        }

        [Fact]
        public void Entity_setting_value_of_tags_facetproperty_fails_on_wrong_type()
        {
            // ARRANGE
            var tag = DefaultTagModel(WithDefaultProperty);
            var entity = DefaultEntityModel(WithTag(tag));

            // ACT
            var result = Assert.Throws<InvalidOperationException>(() => entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), 1));

            // ASSERT
            Assert.Equal($"property(name='guid') doesn't accept value of type {typeof(int)}", result.Message);
        }

        [Fact]
        public void Entity_setting_value_of_categories_facetproperty_fails_on_wrong_type()
        {
            // ARRANGE
            var category = DefaultRootCategoryModel(WithDefaultProperty);
            var entity = DefaultEntityModel(WithEntityCategory(category));

            // ACT
            var result = Assert.Throws<InvalidOperationException>(() => entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), 1));

            // ASSERT
            Assert.Equal($"property(name='guid') doesn't accept value of type {typeof(int)}", result.Message);
        }

        [Fact]
        public void Entity_getting_value_of_tags_facetproperty_returns_false_on_missing_value()
        {
            // ARRANGE
            var tag = DefaultTagModel(WithDefaultProperty);
            var entity = DefaultEntityModel(WithTag(tag));

            // ACT
            var (fp, result, _) = entity.GetFacetPropertyValue(tag.Facet.Properties.Single());

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public void Entity_getting_value_of_categeories_facetproperty_returns_false_on_missing_value()
        {
            // ARRANGE
            var category = DefaultRootCategoryModel(WithDefaultProperty);
            var entity = DefaultEntityModel(WithEntityCategory(category));

            // ACT
            var (fp, result, _) = entity.GetFacetPropertyValue(category.Facet.Properties.Single());

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public void Entity_removes_tag_with_assigned_values()
        {
            // ARRANGE
            var tag = DefaultTagModel(WithDefaultProperty);
            var entity = DefaultEntityModel(WithTag(tag));
            entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), Guid.Empty);

            // ACT
            entity.RemoveTag(tag);

            // ASSERT
            Assert.Empty(entity.Values);
            Assert.All(entity.FacetPropertyValues(), fpv => Assert.False(fpv.hasValue));
        }

        [Fact]
        public void Entity_removes_values_on_changing_category()
        {
            // ARRANGE
            var root = DefaultRootCategoryModel(WithoutProperties);
            var category = DefaultCategoryModel(root, WithDefaultProperty);
            var entity = DefaultEntityModel(WithEntityCategory(category));

            entity.SetFacetProperty(category.Facet.Properties.Single(), Guid.Empty);

            // ACT
            entity.SetCategory(root);

            // ASSERT
            Assert.Empty(entity.Values);
            Assert.All(entity.FacetPropertyValues(), fpv => Assert.False(fpv.hasValue));
        }

        [Fact]
        public void Entity_clones_with_new_id()
        {
            // ARRANGE
            var category = DefaultRootCategoryModel(WithDefaultProperty);
            var tag = DefaultTagModel(WithDefaultProperty);
            var entity = DefaultEntityModel(WithEntityCategory(category), WithTag(tag));

            var value1 = Guid.NewGuid();
            entity.SetFacetProperty(tag.Facet.Properties.Single(), value1);

            var value2 = Guid.NewGuid();
            entity.SetFacetProperty(category.Facet.Properties.Single(), value2);

            // ACT
            var result = (EntityModel)entity.Clone();

            // ASSERT
            Assert.NotEqual(entity.Id, result.Id);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Tags.Single(), result.Tags.Single());
            Assert.Equal(value2, entity.Values[category.Facet.Properties.Single().Id.ToString()]);
            Assert.Equal(value1, entity.Values[tag.Facet.Properties.Single().Id.ToString()]);
        }
    }
}