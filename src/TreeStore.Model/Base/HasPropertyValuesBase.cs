using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeStore.Model.Base
{
    public abstract class HasPropertyValuesBase : NamedBase
    {
        public HasPropertyValuesBase(string name)
            : base(name)
        { }

        public Dictionary<string, object?> Values { get; set; } = new Dictionary<string, object?>();

        public void SetFacetProperty<T>(FacetPropertyModel facetProperty, T value)
        {
            if (facetProperty.CanAssignValue(value))
                this.Values[facetProperty.Id.ToString()] = value;
            else throw new InvalidOperationException($"property(name='{facetProperty.Name}') doesn't accept value of type {typeof(T)}");
        }

        public IEnumerable<(FacetPropertyModel facetProperty, bool hasValue, object? value)> FacetPropertyValues()
        {
            foreach (var facetProperty in this.FacetProperties())
                yield return GetFacetPropertyValue(facetProperty);
        }

        public (FacetPropertyModel facetProerty, bool hasValue, object? value) GetFacetPropertyValue(FacetPropertyModel facetProperty)
            => (facetProperty, this.Values.TryGetValue(facetProperty.Id.ToString(), out var value), value);

        /// <summary>
        /// Returns all <see cref="FacetPropertyModel"/> associeted woth this item
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<FacetPropertyModel> FacetProperties() => Enumerable.Empty<FacetPropertyModel>();

        protected void RemoveObsoletePropertyValues()
        {
            var assignedFacetPropertes = this.FacetProperties()
                .ToDictionary(fp => fp.Id.ToString());

            foreach (var propertyId in this.Values.Keys.ToArray())
            {
                if (assignedFacetPropertes.ContainsKey(propertyId))
                    continue;

                // property is no longer part of the assigned properties
                // -> remove it.
                this.Values.Remove(propertyId);
            }
        }
    }
}