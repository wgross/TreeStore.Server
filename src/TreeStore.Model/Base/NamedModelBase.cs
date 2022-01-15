using System;
using TreeStore.Model.Abstractions;

namespace TreeStore.Model
{
    public abstract class NamedModelBase : IIdentifiable, INamed, IEquatable<object?>
    {
        #region Construction and initialization of this instance

        public NamedModelBase(string name)
        {
            this.Name = name;
        }

        public NamedModelBase()
            : this(string.Empty)

        {
        }

        #endregion Construction and initialization of this instance

        #region IIdentfiable

        /// <inheritdoc/>
        public Guid Id { get; set; } = Guid.NewGuid();

        #endregion IIdentfiable

        #region INamed

        public string Name
        {
            get => this.name;
            set
            {
                var oldValue = this.name;
                if (StringComparer.Ordinal.Equals(oldValue, value))
                    return;

                this.name = value;
                this.OnNameChanged(oldValue, this.name);
            }
        }

        private string name = string.Empty;

        protected virtual void OnNameChanged(string oldName, string name)
        {
        }

        #endregion INamed

        #region IEquatable

        public override bool Equals(object? obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            if (obj is NamedModelBase nb)
                return (this.GetType(), this.Id).Equals((obj.GetType(), nb.Id));

            return false;
        }

        public override int GetHashCode() => (this.GetType(), this.Id).GetHashCode();

        #endregion IEquatable
    }
}