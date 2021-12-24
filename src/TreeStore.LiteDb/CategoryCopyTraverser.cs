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

        internal CategoryModel CopyCategory(CategoryModel src, CategoryModel dst)
        {
            return this.CopyAndSaveCategory(src, dst);
        }

        internal CategoryModel CopyCategoryRecursive(CategoryModel src, CategoryModel dst)
        {
            // copy the top most src as child of the dst
            var srcClone = this.CopyAndSaveCategory(src, dst);

            // descend in src and continue with sub categories
            foreach (var srcChild in this.categoryRepository.FindByParent(src))
                this.CopyCategoryRecursive(srcChild, srcClone);

            // copy all entities in src to dst
            foreach (var srcEntity in this.entityRepository.FindByCategory(src))
                this.CopyAndSaveEntity(srcEntity, srcClone);

            return srcClone;
        }

        private CategoryModel CopyAndSaveCategory(CategoryModel src, CategoryModel dst) => this.categoryRepository.Upsert(this.CopyToNewParentCategory(src, dst));

        private void CopyAndSaveEntity(EntityModel srcEntity, CategoryModel dstCategory) => this.entityRepository.Upsert(this.CopyToNewParentCategory(srcEntity, dstCategory));

        private CategoryModel CopyToNewParentCategory(CategoryModel category, CategoryModel dstParent)
        {
            var tmp = new CategoryModel(category.Name);
            tmp.Facet.Name = category.Facet.Name;

            // TODO: this is wrong: property ids must be changed and also the entity values have to be transferred to the
            // properties with the new Ids.
            tmp.Facet.Properties = category.Facet.Properties;
            dstParent.AddSubCategory(tmp);
            return tmp;
        }

        internal EntityModel CopyToNewParentCategory(EntityModel entity, CategoryModel dstCategory)
        {
            var tmp = (EntityModel)entity.Clone();
            tmp.SetCategory(dstCategory);
            return tmp;
        }
    }
}