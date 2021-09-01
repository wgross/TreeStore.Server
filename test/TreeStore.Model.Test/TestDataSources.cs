//using System;
//using System.Linq;
//using TreeStore.Model.Abstractions;

//namespace TreeStore.Model.Test
//{
//    public static class TestDataSources
//    {
//        #region Default Tag

//        public static TagModel DefaultTagModel(params Action<TagModel>[] setup)
//        {
//            var tmp = new TagModel("t", new FacetModel("f", DefaultFacetPropertyModel()));
//            setup.ForEach(s => s(tmp));
//            return tmp;
//        }

//        public static void WithDefaultProperty(TagModel tag)
//        {
//            tag.Facet.Properties = Array.Empty<FacetPropertyModel>();
//            tag.Facet.AddProperty(DefaultFacetPropertyModel());
//        }

//        public static void WithoutProperty(TagModel tag) => tag.Facet.Properties = Array.Empty<FacetPropertyModel>();

//        public static Action<TagModel> WithProperty(string name, FacetPropertyTypeValues type)
//        {
//            return tag => tag.Facet.AddProperty(new FacetPropertyModel(name, type));
//        }

//        public static FacetPropertyModel DefaultFacetPropertyModel(params Action<FacetPropertyModel>[] setup)
//        {
//            var tmp = new FacetPropertyModel("p", FacetPropertyTypeValues.String);
//            setup.ForEach(s => s(tmp));
//            return tmp;
//        }

//        #endregion Default Tag

//        #region Default Entity

//        public static EntityModel DefaultEntityModel(params Action<EntityModel>[] setup)
//        {
//            var tmp = new EntityModel("e");
//            setup.ForEach(s => s(tmp));
//            return tmp;
//        }

//        public static void WithDefaultTag(EntityModel entity) => entity.Tags.Add(DefaultTagModel(WithDefaultProperty));

//        public static Action<EntityModel> WithDefaultPropertySet<V>(V value)
//            => e => e.SetFacetProperty(e.Tags.First().Facet.Properties.First(), value);

//        public static void WithoutTags(EntityModel entity) => entity.Tags.Clear();

//        public static Action<EntityModel> WithEntityCategory(CategoryModel c) => e => e.SetCategory(c);

//        #endregion Default Entity

//        #region Default Category

//        public static CategoryModel DefaultCategoryModel(CategoryModel parent, params Action<CategoryModel>[] setup)
//        {
//            var tmp = new CategoryModel("c");
//            parent.AddSubCategory(tmp);
//            setup.ForEach(s => s(tmp));
//            return tmp;
//        }

//        #endregion Default Category
//    }
//}