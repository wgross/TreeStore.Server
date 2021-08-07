using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeStore.Model.Base
{
    public abstract class TaggedBase : NamedBase
    {
        public TaggedBase(string name, TagModel[] tags)
            : base(name)
        {
            this.Tags = tags.ToList();
        }

        public List<TagModel> Tags { get; set; } = new List<TagModel>();

        public void AddTag(TagModel tag)
        {
            if (tag is null)
                throw new ArgumentNullException(nameof(tag));

            this.Tags = this.Tags.Union(tag.Yield()).ToList();
        }

        public void RemoveTag(TagModel tag)
        {
            if (this.Tags.Remove(tag))
            {
                foreach (var property in tag.Facet.Properties)
                {
                    this.Values.Remove(property.Id.ToString());
                }
            }
        }

        public Dictionary<string, object?> Values { get; set; } = new Dictionary<string, object?>();

        public void SetFacetProperty<T>(FacetPropertyModel facetProperty, T value)
        {
            if (facetProperty.CanAssignValue(value))
                this.Values[facetProperty.Id.ToString()] = value;
            else throw new InvalidOperationException($"property(name='{facetProperty.Name}') doesn't accept value of type {typeof(T)}");
        }

        public (bool hasValue, object? value) TryGetFacetProperty(FacetPropertyModel facetProperty)
            => (this.Values.TryGetValue(facetProperty.Id.ToString(), out var value), value);
    }
}