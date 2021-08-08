﻿using System;
using System.Linq;
using TreeStore.Model;
using TreeStore.Model.Abstractions;

namespace TreeStore.Test.Common
{
    public static class TreeStoreTestData
    {
        #region Default Tag

        public static TagModel DefaultTagModel(params Action<TagModel>[] setup)
        {
            var tmp = new TagModel("t", new FacetModel("f", new FacetPropertyModel("p", FacetPropertyTypeValues.String)));
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        public static void WithDefaultProperty(TagModel tag)
        {
            // clear the properties
            tag.Facet.Properties = Array.Empty<FacetPropertyModel>();
            tag.Facet.AddProperty(new FacetPropertyModel("string", FacetPropertyTypeValues.String));
            tag.Facet.AddProperty(new FacetPropertyModel("long", FacetPropertyTypeValues.Long));
            tag.Facet.AddProperty(new FacetPropertyModel("double", FacetPropertyTypeValues.Double));
            tag.Facet.AddProperty(new FacetPropertyModel("decimal", FacetPropertyTypeValues.Decimal));
            tag.Facet.AddProperty(new FacetPropertyModel("datetime", FacetPropertyTypeValues.DateTime));
            tag.Facet.AddProperty(new FacetPropertyModel("guid", FacetPropertyTypeValues.Guid));
            tag.Facet.AddProperty(new FacetPropertyModel("bool", FacetPropertyTypeValues.Bool));
        }

        public static void WithoutProperty(TagModel tag) => tag.Facet.Properties = Array.Empty<FacetPropertyModel>();

        public static Action<TagModel> WithProperty(string name, FacetPropertyTypeValues type)
        {
            return tag => tag.Facet.AddProperty(new FacetPropertyModel(name, type));
        }

        #endregion Default Tag

        #region Default Entity

        public static EntityModel DefaultEntityModel(params Action<EntityModel>[] setup)
        {
            var tmp = new EntityModel("e");
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        public static EntityModel DefaultEntityModel(CategoryModel category, params Action<EntityModel>[] setup)
        {
            var tmp = new EntityModel("e");
            tmp.SetCategory(category);
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        public static void WithDefaultCategory(EntityModel entity) => entity.SetCategory(DefaultRootCategoryModel());

        public static void WithDefaultTag(EntityModel entity) => entity.Tags.Add(DefaultTagModel(WithDefaultProperty));

        public static void WithDefaultPropertyValues(EntityModel entity)
        {
            entity.SetFacetProperty(entity.Tags.Single().Facet.GetProperty("string"), "value");
            entity.SetFacetProperty(entity.Tags.Single().Facet.GetProperty("long"), (long)1);
            entity.SetFacetProperty(entity.Tags.Single().Facet.GetProperty("double"), (double)2.0);
            entity.SetFacetProperty(entity.Tags.Single().Facet.GetProperty("decimal"), (decimal)3.0);
            entity.SetFacetProperty(entity.Tags.Single().Facet.GetProperty("datetime"), DateTime.Today);
            entity.SetFacetProperty(entity.Tags.Single().Facet.GetProperty("guid"), Guid.NewGuid());
            entity.SetFacetProperty(entity.Tags.Single().Facet.GetProperty("bool"), true);
        }

        public static Action<EntityModel> WithDefaultPropertySet<V>(V value)
            => e => e.SetFacetProperty(e.Tags.First().Facet.Properties.First(), value);

        public static void WithoutTags(EntityModel entity) => entity.Tags.Clear();

        public static Action<EntityModel> WithEntityCategory(CategoryModel c) => e => e.SetCategory(c);

        #endregion Default Entity

        #region Default Category

        public static CategoryModel DefaultRootCategoryModel(params Action<CategoryModel>[] setup)
        {
            var tmp = new CategoryModel("c");
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        public static CategoryModel DefaultCategoryModel(CategoryModel parent, params Action<CategoryModel>[] setup)
        {
            var tmp = new CategoryModel("c");
            parent.AddSubCategory(tmp);
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        #endregion Default Category
    }
}