﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeStore.Model
{
    public class CategoryModel : FacetingEntityBase, ICloneable
    {
        public CategoryModel()
            : this(string.Empty, new FacetModel())
        { }

        public CategoryModel(string name)
            : this(name, new FacetModel(name))
        { }

        public CategoryModel(string name, FacetModel ownFacet, params CategoryModel[] subcategories)
            : base(name, ownFacet)
        {
            foreach (var c in subcategories)
                c.Parent = this;
        }

        #region Category owns a facet

        public IEnumerable<FacetModel> Facets()
        {
            if (this.Parent is null)
                return this.Facet.Yield();
            return this.Facet.Yield().Union(this.Parent.Facets());
        }

        #endregion Category owns a facet

        #region Category is hierarchical

        public CategoryModel? Parent { get; set; }

        public void AddSubCategory(CategoryModel subcategory)
        {
            subcategory.Parent = this;
        }

        #region Category has a unique within the parenet catagory

        public string UniqueName
        {
            get => $"{this.Name.ToLower()}_{this.Parent?.Id.ToString() ?? "<root>"}";
            set { }
        }

        #endregion Category has a unique within the parenet catagory

        public void DetachSubCategory(CategoryModel subcategory)
        {
            subcategory.Parent = null;
        }

        #endregion Category is hierarchical

        #region ICloneable

        public object Clone() => new CategoryModel(this.Name);

        #endregion ICloneable
    }
}