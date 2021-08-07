
using TreeStore.Model.Abstractions;
using TreeStore.Model.Base;

namespace TreeStore.Model
{
    public class Relationship : TaggedBase, IRelationship
    {
        public Relationship(string name, EntityModel? from, EntityModel? to, params TagModel[] tags)
            : base(name, tags)
        {
            this.From = from;
            this.To = to;
        }

        public Relationship(string name)
            : this(name, null, null)
        {
        }

        public Relationship()
            : base(string.Empty, new TagModel[0])
        {
        }

        public EntityModel? From { get; set; }

        public EntityModel? To { get; set; }

        IEntity? IRelationship.From => this.From;

        IEntity? IRelationship.To => this.To;
    }
}