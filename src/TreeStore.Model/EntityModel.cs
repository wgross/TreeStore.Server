using System;
using System.Collections.Generic;
using System.Linq;
using TreeStore.Model.Abstractions;
using TreeStore.Model.Base;

namespace TreeStore.Model
{
    public sealed class EntityModel : CategorizedModelBase, IEntity, ICloneable
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
            : base(string.Empty, Array.Empty<TagModel>())
        { }

        #endregion Construction and initialization of this instance

        #region IClonable

        public object Clone() => new EntityModel(this.Name, this.Tags.ToArray(), this.Values);

        #endregion IClonable
    }
}