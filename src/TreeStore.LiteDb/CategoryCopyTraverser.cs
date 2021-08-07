using TreeStore.Model;

namespace TreeStore.LiteDb
{
    internal sealed class CategoryCopyTraverser
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IEntityRepository entityRepository;

        internal CategoryCopyTraverser(CategoryLiteDbRepository categoryRepository, IEntityRepository entityRepository)
        {
            this.categoryRepository = categoryRepository;
            this.entityRepository = entityRepository;
        }

        internal void CopyCategory(CategoryModel src, CategoryModel dst)
        {
            this.CopyAndSaveCategory(src, dst);
        }

        internal void CopyCategoryRecursive(CategoryModel src, CategoryModel dst)
        {
            // copy the top most src as child of the dst
            var srcClone = this.CopyAndSaveCategory(src, dst);

            // descend n src and continue wioth sub categories
            foreach (var srcChild in this.categoryRepository.FindByParent(src))
                this.CopyCategoryRecursive(srcChild, srcClone);

            // copy all entities in src to dst
            foreach (var srcEntity in this.entityRepository.FindByCategory(src))
                this.CopyAndSaveEntity(srcEntity, srcClone);
        }

        private CategoryModel CopyAndSaveCategory(CategoryModel src, CategoryModel dst)
        {
            return this.categoryRepository.Upsert(this.CopyToNewParentCategory(src, dst));
        }

        private void CopyAndSaveEntity(EntityModel srcEntity, CategoryModel dstCategory) => this.entityRepository.Upsert(this.CopyToNewParentCategory(srcEntity, dstCategory));

        private CategoryModel CopyToNewParentCategory(CategoryModel category, CategoryModel dstParent)
        {
            var tmp = new CategoryModel(category.Name);
            dstParent.AddSubCategory(tmp);
            return tmp;
        }

        private EntityModel CopyToNewParentCategory(EntityModel entity, CategoryModel dstCategory)
        {
            var tmp = (EntityModel)entity.Clone();
            tmp.SetCategory(dstCategory);
            return tmp;
        }
    }
}