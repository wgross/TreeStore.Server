using System;
using System.Collections.Generic;
using System.Linq;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public class FacetModel : NamedBase, IFacet<FacetPropertyModel>
    {
        #region Construction and initialization of this instance

        public FacetModel(string name, params FacetPropertyModel[] properties)
            : base(name)
        {
            this.Properties = properties.ToList();
        }

        public FacetModel()
            : base(string.Empty)
        { }

        #endregion Construction and initialization of this instance

        public IEnumerable<FacetPropertyModel> Properties { get; set; } = Array.Empty<FacetPropertyModel>();

        public void AddProperty(FacetPropertyModel property)
        {
            if (this.Properties.Any(p => p.Name.Equals(property.Name)))
                throw new InvalidOperationException($"duplicate property name: {property.Name}");

            this.Properties = this.Properties.Append(property).ToList();
        }

        /// <summary>
        /// Removes the <see cref="FacetPropertyModel"/> <paramref name="property"/> from the property collection.
        /// </summary>
        /// <param name="property"></param>
        public void RemoveProperty(FacetPropertyModel property) => this.RemoveProperty(property.Id);

        public void RemoveProperty(Guid id) => this.Properties = this.Properties.Where(p => p.Id != id).ToList();

        /// <summary>
        /// Returns the <see cref="FacetPropertyModel"/> identified by <paramref name="id"/> or null.
        /// </summary>
        public FacetPropertyModel? GetProperty(Guid id) => this.Properties.FirstOrDefault(fp => fp.Id == id);

        /// <summary>
        /// Returns the <see cref="FacetPropertyModel"/> identified by <paramref name="name"/> or null.
        /// </summary>
        public FacetPropertyModel? GetProperty(string name) => this.Properties.FirstOrDefault(fp => fp.Name.Equals(name));
    }
}