using System;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public abstract class NamedBase : IIdentifiable, INamed, IEquatable<object?>
    {
        #region Construction and initialization of this instance

        public NamedBase(string name)
        {
            this.Name = name;
        }

        public NamedBase()
            : this(string.Empty)

        {
        }

        #endregion Construction and initialization of this instance

        #region IIdentfiable

        /// <inheritdoc/>
        public Guid Id { get; set; } = Guid.NewGuid();

        #endregion IIdentfiable

        #region INamed

        public string Name { get; set; }

        #endregion INamed

        #region IEquatable

        public override bool Equals(object? obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            if (obj is NamedBase nb)
                return (this.GetType(), this.Id).Equals((obj.GetType(), nb.Id));

            return false;
        }

        public override int GetHashCode() => (this.GetType(), this.Id).GetHashCode();

        #endregion IEquatable
    }
}