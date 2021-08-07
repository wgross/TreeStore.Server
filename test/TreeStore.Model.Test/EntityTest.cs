﻿using System;
using System.Linq;
using Xunit;

namespace TreeStore.Model.Test
{
    public class EntityTest
    {
        [Fact]
        public void Entity_has_Category()
        {
            // ARRANGE

            var category = new CategoryModel();
            var entity = new EntityModel();

            // ACT

            entity.SetCategory(category);

            // ASSERT

            Assert.Same(category, entity.Category);
        }

        [Fact]
        public void Entity_has_calculated_index_property()
        {
            // ARRANGE

            var category = new CategoryModel();
            var entity = new EntityModel("Name");

            // ACT

            entity.SetCategory(category);

            // ASSERT

            Assert.Equal($"name_{category.Id}", entity.UniqueName);
        }

        [Fact]
        public void Entity_adds_Tag()
        {
            // ARRANGE

            var tag = new TagModel();
            var entity = new EntityModel();

            // ACT

            entity.AddTag(tag);

            // ASSERT

            Assert.Equal(tag, entity.Tags.Single());
        }

        [Fact]
        public void Entity_adding_null_Tag_fails()
        {
            // ARRANGE

            var entity = new EntityModel();

            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => entity.AddTag((TagModel)null));

            // ASSERT

            Assert.Equal("tag", result.ParamName);
        }

        [Fact]
        public void Entity_adding_Tag_ignores_duplicate()
        {
            // ARRANGE

            var tag = new TagModel();
            var entity = new EntityModel();
            entity.AddTag(tag);

            // ACT

            entity.AddTag(tag);

            // ASSERT

            Assert.Equal(tag, entity.Tags.Single());
        }

        [Fact]
        public void Entity_has_Facet_from_Category()
        {
            // ARRANGE

            var facet = new FacetModel();
            var category = new CategoryModel("cat", facet);
            var entity = new EntityModel();
            entity.SetCategory(category);

            // ACT

            var result = entity.Facets().ToArray();

            // ASSERT

            Assert.Equal(facet, result.Single());
        }

        [Fact]
        public void Entity_has_Facet_from_Tag()
        {
            // ARRANGE

            var tag = new TagModel("tag", new FacetModel("facet", new FacetPropertyModel()));

            var entity = new EntityModel();
            entity.AddTag(tag);

            // ACT

            var result = entity.Facets().ToArray();

            // ASSERT

            Assert.Equal(tag.Facet, result.Single());
        }

        [Fact]
        public void Entity_has_Facet_from_Category_and_Tag()
        {
            // ARRANGE

            var tag = new TagModel("tag", new FacetModel("facet", new FacetPropertyModel()));
            var category = new CategoryModel("cat", new FacetModel("facet", new FacetPropertyModel()));

            var entity = new EntityModel();
            entity.AddTag(tag);
            entity.SetCategory(category);

            // ACT

            var result = entity.Facets().ToArray();

            // ASSERT

            Assert.Equal(new[] { category.Facet, tag.Facet }, result);
        }

        [Fact]
        public void Entity_has_Facet_from_Category_and_Tag_ignores_duplicate()
        {
            // ARRANGE

            var facet = new FacetModel("facet", new FacetPropertyModel());
            var tag = new TagModel("tag", facet);
            var category = new CategoryModel("cat", facet);

            var entity = new EntityModel();
            entity.AddTag(tag);
            entity.SetCategory(category);

            // ACT

            var result = entity.Facets().ToArray();

            // ASSERT

            Assert.Equal(facet, result.Single());
        }

        [Fact]
        public void Entity_sets_value_of_FacetProperty()
        {
            // ARRANGE

            var facet = new FacetModel("facet", new FacetPropertyModel("name"));
            var tag = new TagModel("tag", facet);
            var entity = new EntityModel("e", tag);

            // ACT

            entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), "1");

            // ASSERT

            Assert.Equal("1", entity.TryGetFacetProperty(facet.Properties.Single()).value);
        }

        [Fact]
        public void Entity_setting_value_of_FacetProperty_fails_on_wrong_type()
        {
            // ARRANGE

            var facet = new FacetModel("facet", new FacetPropertyModel("name"));
            var tag = new TagModel("tag", facet);
            var entity = new EntityModel("e", tag);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), 1));

            // ASSERT

            Assert.Equal($"property(name='name') doesn't accept value of type {typeof(int)}", result.Message);
        }

        [Fact]
        public void Entity_getting_value_of_FacetProperty_returns_false_on_missing_value()
        {
            // ARRANGE

            var facet = new FacetModel("facet", new FacetPropertyModel("prop"));
            var tag = new TagModel("tag", facet);
            var entity = new EntityModel();
            entity.AddTag(tag);

            // ACT

            var (result, _) = entity.TryGetFacetProperty(facet.Properties.Single());

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void Entity_repves_Tag_with_assigned_values()
        {
            // ARRANGE

            var facet = new FacetModel("facet", new FacetPropertyModel("name"));
            var tag = new TagModel("tag", facet);
            var entity = new EntityModel("e", tag);
            entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), "1");

            // ACT

            entity.RemoveTag(tag);

            // ASSERT

            Assert.Empty(entity.Values);
        }

        [Fact]
        public void Entity_clones_with_new_id()
        {
            // ARRANGE

            var facet = new FacetModel("facet", new FacetPropertyModel("prop"));
            var tag = new TagModel("tag", facet);
            var entity = new EntityModel();
            entity.AddTag(tag);
            entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), "1");

            // ACT

            var result = (EntityModel)entity.Clone();

            // ASSERT

            Assert.NotEqual(entity.Id, result.Id);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Tags.Single(), result.Tags.Single());
            Assert.Equal(entity.Values.Single().Key, result.Values.Single().Key);
            Assert.Equal(entity.Values.Single().Value, result.Values.Single().Value);
        }
    }
}