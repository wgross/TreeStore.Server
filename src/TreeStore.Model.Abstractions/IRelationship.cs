namespace TreeStore.Model.Abstractions
{
    public interface IRelationship : IIdentifiable
    {
        IEntity? From { get; }
        IEntity? To { get; }
    }
}