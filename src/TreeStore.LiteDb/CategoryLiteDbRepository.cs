using LiteDB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public sealed partial class CategoryLiteDbRepository : LiteDbRepositoryBase<CategoryModel>, ICategoryRepository
    {
        public const string collectionName = "categories";

        static CategoryLiteDbRepository()
        {
            // https://github.com/mbdavid/LiteDB/issues/642
            BsonMapper.Global.EmptyStringToNull = false;
            BsonMapper.Global.Entity<CategoryModel>()
                .DbRef(c => c.Parent, collectionName);
        }

        public CategoryLiteDbRepository(TreeStoreLiteDbPersistence treeStoreLiteDbPersistence, ILogger<CategoryLiteDbRepository> logger)
            : base(treeStoreLiteDbPersistence.LiteRepository, collectionName: collectionName, logger: logger)
        {
            this.treeStoreLiteDbPersistence = treeStoreLiteDbPersistence;
            this.treeStoreLiteDbPersistence
                .LiteRepository
                .Database
                .GetCollection(this.CollectionName)
                .EnsureIndex(
                    name: nameof(CategoryModel.UniqueName),
                    expression: $"$.{nameof(CategoryModel.UniqueName)}",
                    unique: true);

            this.logger = logger;
        }

        #region Ensure persistent root always exist

        private readonly TreeStoreLiteDbPersistence treeStoreLiteDbPersistence;
        private readonly ILogger<CategoryLiteDbRepository> logger;

        /// <summary>
        /// Returns the root node of the repository. If not exists it is created.
        /// </summary>
        public CategoryModel Root() => this.FindRootCategory() ?? this.CreateRootCategory();

        // todo: abandon completely loaded root tree
        private CategoryModel? FindRootCategory()
        {
            var rootCategory = this.LiteRepository
                .Query<CategoryModel>(CollectionName)
                .Include(c => c.Parent)
                .Where(c => c.Parent == null)
                .FirstOrDefault();

            if (rootCategory is not null)
                this.logger.LogFoundExistingRootCategory(rootCategory.Id);

            return rootCategory;
        }

        private CategoryModel CreateRootCategory()
        {
            var rootCategory = new CategoryModel(string.Empty);
            this.LiteCollection().Upsert(rootCategory);

            this.logger.LogFoundExistingRootCategory(rootCategory.Id);

            return rootCategory;
        }

        #endregion Ensure persistent root always exist

        #region Create, Read, Update, Delete categories

        public override CategoryModel Upsert(CategoryModel category)
        {
            using var scope = this.BeginScope(category);

            if (category.Parent is null)
            {
                if (category.Id != this.Root().Id)
                {
                    throw new InvalidOperationException("Category must have parent.");
                }
            }

            try
            {
                return base.Upsert(category);
            }
            catch (LiteException le) when (le.ErrorCode == LiteException.INDEX_DUPLICATE_KEY)
            {
                throw new InvalidOperationException($"Can't write Category(name='{category.Name}'): duplicate name", le);
            }
        }

        public override CategoryModel? FindById(Guid id)
        {
            var result = base.FindById(id);
            if (result is null)
                return result;

            if (result.Parent is not null)
                result.Parent = this.FindById(result.Parent.Id);

            return result;
        }

        public CategoryModel? FindByParentAndName(CategoryModel category, string name)
        {
            using var scope = this.BeginScope(category);

            return this
                .FindByParent(category)
                // matched in result set, could be mathed to expression
                .SingleOrDefault(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<CategoryModel> FindByParent(CategoryModel category)
        {
            using var scope = this.BeginScope(category);

            return this.QueryRelated()
                // broken with 5.0.7 : // .Where(c => c.Parent != null && c.Parent.Id == category.Id)
                .Find(Query.And(
                    Query.Not("$.Parent.$id", BsonValue.Null),
                    Query.EQ("$.Parent.$id", new BsonValue(category.Id))
                ))
                .ToArray();
        }

        protected override ILiteCollection<CategoryModel> IncludeRelated(ILiteCollection<CategoryModel> from) => from.Include(c => c.Parent);

        private ILiteCollection<CategoryModel> QueryRelated() => IncludeRelated(this.LiteCollection());

        public bool Delete(CategoryModel category, bool recurse)
        {
            using var scope = this.BeginScope(category);

            CategoryRemovalTraverser removalTraverser = new(this, this.treeStoreLiteDbPersistence.Entities);

            return recurse
                ? removalTraverser.DeleteRecursively(category)
                : removalTraverser.DeleteIfEmpty(category);
        }

        public CategoryModel CopyTo(CategoryModel sourceCategory, CategoryModel destinationParentCategory, bool recurse)
        {
            using var scope = this.BeginScope(sourceCategory);

            CategoryCopyTraverser categoryCopyTraverser = new(this, this.treeStoreLiteDbPersistence.Entities);

            return recurse
                ? categoryCopyTraverser.CopyCategoryRecursive(sourceCategory, destinationParentCategory)
                : categoryCopyTraverser.CopyCategory(sourceCategory, destinationParentCategory);
        }

        public EntityModel CopyTo(EntityModel sourceEntity, CategoryModel destinationParentCategory)
        {
            using var scope = this.BeginScope(destinationParentCategory);

            CategoryCopyTraverser categoryCopyTraverser = new(this, this.treeStoreLiteDbPersistence.Entities);

            return this.treeStoreLiteDbPersistence.Entities.Upsert(categoryCopyTraverser.CopyToNewParentCategory(sourceEntity, destinationParentCategory));
        }

        #endregion Create, Read, Update, Delete categories
    }
}