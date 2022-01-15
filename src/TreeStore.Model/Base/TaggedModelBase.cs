using System;
using System.Collections.Generic;
using System.Linq;

namespace TreeStore.Model.Base
{
    public abstract class TaggedModelBase : FacetedModelBase
    {
        protected TaggedModelBase(string name, TagModel[] tags)
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

        /// <summary>
        /// A tagged model item receives its <see cref="FacetModel"/> from the assigned set of <see cref="TagModel"/>.
        /// </summary>
        public override IEnumerable<FacetModel> Facets() => this.Tags.Select(t => t.Facet);
    }
}