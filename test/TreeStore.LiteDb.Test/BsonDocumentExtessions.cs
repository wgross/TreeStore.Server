using LiteDB;

namespace TreeStore.LiteDb.Test
{
    public static class BsonDocumentExtessions
    {
        public static BsonValue BsonValue(this BsonDocument root, params string[] names)
        {
            var current = root;
            foreach (var n in names[0..^1])
                current = current.AsDocument[n].AsDocument;

            return current[names[^1]];
        }
    }
}