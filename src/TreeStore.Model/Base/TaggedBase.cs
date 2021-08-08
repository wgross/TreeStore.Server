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

        /// <summary>
        /// Adds a reference to the <see cref="TagModel"/> <paramref name="tag"/> to the entity.
        /// </summary>
        /// <exception cref="ArgumentNullException">if <paramref name="tag"/> is null</exception>
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

        /// <summary>
        /// Removes a reference to a <see cref="TagModel"/> from the entity.
        /// </summary>
        public void RemoveTag(Guid tagId)
        {
            var tag = this.Tags.FirstOrDefault(t => t.Id == tagId);
            if (tag is not null)
                this.RemoveTag(tag);
        }

        public Dictionary<string, object?> Values { get; set; } = new Dictionary<string, object?>();

        public void SetFacetProperty<T>(FacetPropertyModel facetProperty, T value)
        {
            if (facetProperty.CanAssignValue(value))
                this.Values[facetProperty.Id.ToString()] = value;
            else throw new InvalidOperationException($"property(name='{facetProperty.Name}') doesn't accept value of type {typeof(T)}");
        }

        public IEnumerable<(FacetPropertyModel facetProperty, bool hasValue, object? value)> GetFacetPropertyValues()
        {
            foreach (var facetProperty in this.Tags.SelectMany(t => t.Facet.Properties))
                yield return GetFacetPropertyValue(facetProperty);
        }

        public (FacetPropertyModel facetProerty, bool hasValue, object? value) GetFacetPropertyValue(FacetPropertyModel facetProperty)
            => (facetProperty, this.Values.TryGetValue(facetProperty.Id.ToString(), out var value), value);
    }
}