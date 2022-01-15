using System.Collections.Generic;
using System.Linq;

namespace TreeStore.Model.Base
{
    /// <summary>
    /// All items are categorized
    /// </summary>
    public abstract class CategorizedModelBase : TaggedModelBase
    {
        public CategorizedModelBase(string name, TagModel[] tags)
            : base(name, tags)
        {
        }

        public CategoryModel? Category { get; set; }

        public void SetCategory(CategoryModel category)
        {
            this.Category = category;
            this.RemoveObsoletePropertyValues();
        }

        public string UniqueName
        {
            get => $"{this.Name.ToLower()}_{this.Category!.Id}";
            set { }
        }

        /// <summary>
        /// A categorized model item receives its <see cref="FacetModel"/> from the category hierarchy and
        /// the additionally assigned <see cref="TagModel"/>.
        /// </summary>
        override public IEnumerable<FacetModel> Facets()
        {
            return this.Category is null
                ? base.Facets()
                : this.Category.Facets().Union(base.Facets());
        }
    }
}