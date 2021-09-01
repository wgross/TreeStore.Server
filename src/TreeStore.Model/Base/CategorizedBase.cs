using System.Collections.Generic;
using System.Linq;

namespace TreeStore.Model.Base
{
    /// <summary>
    /// All items are categorized
    /// </summary>
    public abstract class CategorizedBase : TaggedBase
    {
        public CategorizedBase(string name, TagModel[] tags)
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

        public override IEnumerable<FacetPropertyModel> FacetProperties()
        {
            if (this.Category is null)
                return base.FacetProperties();

            return this.Category.Facets().SelectMany(f => f.Properties).Union(base.FacetProperties());
        }
    }
}