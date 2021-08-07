using System;
using System.Collections.Generic;
using System.Linq;
using TreeStore.Model.Abstractions;
using TreeStore.Model.Base;

namespace TreeStore.Model
{
    public sealed class EntityModel : TaggedBase, IEntity, ICloneable
    {
        #region Construction and initialization of this instance

        public EntityModel(string name, params TagModel[] tags)
            : this(name, tags.AsEnumerable())
        {
        }

        public EntityModel(string name, IEnumerable<TagModel> tags)
            : base(name, tags.ToArray())
        {
        }

        private EntityModel(string name, IEnumerable<TagModel> tags, IDictionary<string, object?> values)
            : base(name, tags.ToArray())
        {
            this.Values = new Dictionary<string, object?>(values);
        }

        public EntityModel()
            : base(string.Empty, new TagModel[0])
        { }

        #endregion Construction and initialization of this instance

        #region Entity has Categories

        public CategoryModel? Category { get; set; }

        public string UniqueName
        {
            get => $"{this.Name.ToLower()}_{this.Category!.Id}";
            set { }
        }

        public IEnumerable<FacetModel> Facets()
        {
            if (this.Category is null)
                return this.Tags.Select(t => t.Facet);
            return this.Category.Facets().Union(this.Tags.Select(t => t.Facet));
        }

        public void SetCategory(CategoryModel category)
        {
            this.Category = category;
        }

        public object Clone() => new EntityModel(this.Name, this.Tags.ToArray(), this.Values);

        #endregion Entity has Categories
    }
}