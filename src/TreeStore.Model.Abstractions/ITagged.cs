using System.Collections.Generic;

namespace TreeStore.Model.Abstractions
{
    public interface ITagged
    {
        IEnumerable<ITag> Tags { get; }
    }
}