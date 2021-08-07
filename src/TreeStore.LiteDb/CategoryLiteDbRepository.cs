using LiteDB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public sealed class CategoryLiteDbRepository : LiteDbRepositoryBase<CategoryModel>, ICategoryRepository
    {
        public const string collectionName = "categories";

        static CategoryLiteDbRepository()
        {
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

            this.rootNode = new Lazy<CategoryModel>(() => this.FindRootCategory() ?? this.CreateRootCategory());
            this.logger = logger;
        }

        #region Ensure persistent root always exist

        private readonly Lazy<CategoryModel> rootNode;
        private readonly TreeStoreLiteDbPersistence treeStoreLiteDbPersistence;
        private readonly ILogger<CategoryLiteDbRepository> logger;

        /// <summary>
        /// return the root node of the repositorty. If not exists it is created.
        /// </summary>
        public CategoryModel Root() => this.rootNode.Value;

        // todo: abandon completely loaded root tree
        private CategoryModel? FindRootCategory()
        {
            var rootCategory = this.LiteRepository
                .Query<CategoryModel>(CollectionName)
                .Include(c => c.Parent)
                .Where(c => c.Parent == null)
                .FirstOrDefault();

            if (rootCategory is not null)
                this.Logger.FoundExistingRootCategory(rootCategory);

            return rootCategory;
        }

        private CategoryModel CreateRootCategory()
        {
            var rootCategory = new CategoryModel(string.Empty);
            this.LiteCollection().Upsert(rootCategory);

            this.Logger.FoundExistingRootCategory(rootCategory);

            return rootCategory;
        }

        #endregion Ensure persistent root always exist

        #region Create, Read, Update, Delete categories

        public override CategoryModel Upsert(CategoryModel category)
        {
            using var scope = this.BeginScope(category);

            if (category.Parent is null)
                throw new InvalidOperationException("Category must have parent.");

            return base.Upsert(category);
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
                .Where(c => c.Parent != null && c.Parent.Id == category.Id)
                .ToArray();
        }

        protected override ILiteCollection<CategoryModel> IncludeRelated(ILiteCollection<CategoryModel> from) => from.Include(c => c.Parent);

        private ILiteQueryable<CategoryModel> QueryRelated() => this.LiteCollection().Query().Include(c => c.Parent);

        public bool Delete(CategoryModel category, bool recurse)
        {
            using var scope = this.BeginScope(category);

            CategoryRemovalTraverser removalTraverser = new(this, this.treeStoreLiteDbPersistence.Entities);

            return recurse
                ? removalTraverser.DeleteRecursively(category)
                : removalTraverser.DeleteIfEmpty(category);
        }

        public void CopyTo(CategoryModel sourceCategory, CategoryModel destinationParentCategory, bool recurse)
        {
            using var scope = this.BeginScope(sourceCategory);

            CategoryCopyTraverser categoryCopyTraverser = new(this, this.treeStoreLiteDbPersistence.Entities);

            if (recurse)
                categoryCopyTraverser.CopyCategoryRecursive(sourceCategory, destinationParentCategory);
            else
                categoryCopyTraverser.CopyCategory(sourceCategory, destinationParentCategory);
        }

        #endregion Create, Read, Update, Delete categories
    }
}