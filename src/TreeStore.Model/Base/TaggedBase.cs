using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeStore.Model.Base
{
    public abstract class TaggedBase : HasPropertyValuesBase
    {
        protected TaggedBase(string name, TagModel[] tags)
            : base(name)
        {
            this.Tags = tags.ToList();
        }

        public List<TagModel> Tags { get; set; } = new List<TagModel>();

        /// <summary>
        /// Adds a reference to the <see cref="TagModel"/> <paramref name="tag"/> to the entity.
        /// </summary>
        /// <exception cref="ArgumentNullException">if <paramref name="tag"/> is null</exception>
        public void AddTag(TagModel? tag)
        {
            if (tag is null)
                throw new ArgumentNullException(nameof(tag));

            this.Tags = this.Tags.Union(tag.Yield()).ToList();
        }

        public void RemoveTag(TagModel tag)
        {
            if (this.Tags.Remove(tag))
            {
                this.RemoveObsoletePropertyValues();
            }
        }

        /// <summary>
        /// Removes a reference to a <see cref="TagModel"/> from the entity.
        /// </summary>
        public void RemoveTag(Guid tagId)
        {
            var tag = this.Tags.Find(t => t.Id == tagId);
            if (tag is not null)
                this.RemoveTag(tag);
        }

        public override IEnumerable<FacetPropertyModel> FacetProperties()
            => this.Tags.SelectMany(t => t.Facet.Properties).Union(base.FacetProperties());
    }
}