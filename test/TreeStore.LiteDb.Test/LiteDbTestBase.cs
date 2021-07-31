using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using TreeStore.Model;

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

        protected Tag DefaultTag(params Action<Tag>[] setup)
        {
            var tmp = new Tag("tag", new("facet"));
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        public static void WithoutProperties(Tag tag) => tag.Facet.Properties.Clear();

        public static void WithDefaultProperty(Tag tag) => tag.Facet.AddProperty(new("prop", FacetPropertyTypeValues.Guid));

        #endregion Default Tag

        #region Default Entity

        protected Entity DefaultEntity(params Action<Entity>[] setup)
        {
            var tmp = new Entity("e");

            // an entity is never w/o a catagory
            WithEntityCategory(this.CategoryRepository.Root())(tmp);

            setup.ForEach(s => s(tmp));
            return tmp;
        }

        public void WithDefaultTag(Entity entity) => entity.AddTag(this.DefaultTag(WithDefaultProperty));

        public static void WithoutTags(Entity entity) => entity.Tags.Clear();

        public static Action<Entity> WithEntityCategory(Category c) => e => e.SetCategory(c);

        public static void WithoutCategory(Entity e) => e.Category = null;

        #endregion Default Entity

        #region Default Category

        /// <summary>
        /// Creates a default category under the root category <see cref="CategoryRepository.Root"/>
        /// </summary>
        protected Category DefaultCategory(params Action<Category>[] setup)
        {
            var tmp = new Category("c");
            this.CategoryRepository.Root().AddSubCategory(tmp);
            setup.ForEach(s => s(tmp));
            return tmp;
        }

        /// <summary>
        /// Detaches the category from its current category and add ist to the <paramref name="parentCategory"/>
        /// </summary>
        protected Action<Category> WithParentCategory(Category parentCategory)
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