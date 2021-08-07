namespace TreeStore.Model
{
    public interface ITagRepository : IRepository<TagModel>
    {
        TagModel? FindByName(string name);
    }
}