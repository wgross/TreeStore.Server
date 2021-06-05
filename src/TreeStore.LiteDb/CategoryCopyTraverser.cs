using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class CategoryCopyTraverser
    {
        private readonly ICategoryRepository categoryRepository;
        private readonly IEntityRepository entityRepository;

        public CategoryCopyTraverser(CategoryLiteDbRepository categoryRepository, EntityLiteDbRepository entityRepository)
        {
            this.categoryRepository = categoryRepository;
            this.entityRepository = entityRepository;
        }

        public void CopyCategory(Category src, Category dst)
        {
            this.CopyAndSaveCategory(src, dst);
        }

        public void CopyCategoryRecursively(Category src, Category dst)
        {
            // copy the top most src as child of the dst
            var srcClone = this.CopyAndSaveCategory(src, dst);

            // descend n src and continue wioth sub categories
            foreach (var srcChild in this.categoryRepository.FindByParent(src))
                this.CopyCategoryRecursively(srcChild, srcClone);

            // copy all entities in src to dst
            foreach (var srcEntity in this.entityRepository.FindByCategory(src))
                this.CopyAndSaveEntity(srcEntity, srcClone);
        }

        private Category CopyAndSaveCategory(Category src, Category dst)
        {
            return this.categoryRepository.Upsert(this.CopyToNewParentCategory(src, dst));
        }

        private void CopyAndSaveEntity(Entity srcEntity, Category dstCategory) => this.entityRepository.Upsert(this.CopyToNewParentCategory(srcEntity, dstCategory));

        private Category CopyToNewParentCategory(Category category, Category dstParent)
        {
            var tmp = new Category(category.Name);
            dstParent.AddSubCategory(tmp);
            return tmp;
        }

        private Entity CopyToNewParentCategory(Entity entity, Category dstCategory)
        {
            var tmp = (Entity)entity.Clone();
            tmp.SetCategory(dstCategory);
            return tmp;
        }
    }
}