using LiteDB;
using Moq;
using System;
using System.IO;
using TreeStore.Model;

namespace TreeStore.LiteDb.Test
{
    public abstract class LiteDbTestBase : IDisposable
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected EntityLiteDbRepository EntityRepository { get; }

        protected TagLiteDbRepository TagRepository { get; }

        protected CategoryLiteDbRepository CategoryRepository { get; }

        protected LiteRepository LiteDb { get; } = new LiteRepository(new MemoryStream());

        public LiteDbTestBase()
        {
            this.EntityRepository = new EntityLiteDbRepository(this.LiteDb);
            this.TagRepository = new TagLiteDbRepository(this.LiteDb);
            this.CategoryRepository = new CategoryLiteDbRepository(this.LiteDb);
        }

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

        protected Category DefaultCategory(params Action<Category>[] setup)
        {
            var tmp = new Category("c");
            this.CategoryRepository.Root().AddSubCategory(tmp);
            setup.ForEach(s => s(tmp));
            return tmp;
        }

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