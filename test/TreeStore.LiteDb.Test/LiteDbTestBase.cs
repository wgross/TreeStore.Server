using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using TreeStore.Model;
using TreeStore.Model.Abstractions;

namespace TreeStore.LiteDb.Test
{
    public abstract class LiteDbTestBase : IDisposable
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected IEntityRepository EntityRepository => this.Persistence.Entities;

        protected ITagRepository TagRepository => this.Persistence.Tags;

        protected ICategoryRepository CategoryRepository => this.Persistence.Categories;

        protected TreeStoreLiteDbPersistence Persistence { get; } = TreeStoreLiteDbPersistence.InMemory(new NullLoggerFactory());

        protected T Setup<T>(T t, Action<T> setup = null)
        {
            setup?.Invoke(t);
            return t;
        }

        public void Dispose() => this.Mocks.VerifyAll();

        #region Default Tag

        protected TagModel DefaultTag(params Action<TagModel>[] setup)
        {
            var tmp = new TagModel("tag", new("facet"));
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        public static void WithoutProperties(TagModel tag) => tag.Facet.Properties = Array.Empty<FacetPropertyModel>();

        public static void WithDefaultProperty(TagModel tag) => tag.Facet.AddProperty(new("prop", FacetPropertyTypeValues.Guid));

        #endregion Default Tag

        #region Default Entity

        protected EntityModel DefaultEntity(params Action<EntityModel>[] setup)
        {
            var tmp = new EntityModel("e");

            // an entity is never w/o a catagory
            WithEntityCategory(this.CategoryRepository.Root())(tmp);

            setup.ForEach(s => s(tmp));
            return tmp;
        }

        public void WithDefaultTag(EntityModel entity) => entity.AddTag(this.DefaultTag(WithDefaultProperty));

        public static void WithoutTags(EntityModel entity) => entity.Tags.Clear();

        public static Action<EntityModel> WithEntityCategory(CategoryModel c) => e => e.SetCategory(c);

        public static void WithoutCategory(EntityModel e) => e.Category = null;

        #endregion Default Entity

        #region Default Category

        /// <summary>
        /// Creates a default category under the root category <see cref="CategoryRepository.Root"/>
        /// </summary>
        protected CategoryModel DefaultCategory(params Action<CategoryModel>[] setup)
        {
            var tmp = new CategoryModel("c");
            this.CategoryRepository.Root().AddSubCategory(tmp);
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        /// <summary>
        /// Detaches the category from its current category and add ist to the <paramref name="parentCategory"/>
        /// </summary>
        protected Action<CategoryModel> WithParentCategory(CategoryModel parentCategory)
        {
            return c =>
            {
                c.Parent.DetachSubCategory(c);
                parentCategory.AddSubCategory(c);
            };
        }

        #endregion Default Category
    }
}