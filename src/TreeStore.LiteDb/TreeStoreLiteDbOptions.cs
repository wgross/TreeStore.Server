namespace TreeStore.LiteDb
{
    public sealed class TreeStoreLiteDbOptions
    {
        /// <summary>
        /// LiteDB filename (https://www.litedb.org/docs/connection-string/).
        /// <list type="bullet">
        /// <item>a path to a lite db file</item>
        /// <item>':memory:' for in memory db</item>
        /// <item>':temp:' for temporary database</item>
        /// </list>
        /// </summary>
        public string FileName { get; set; } = ":memory:";
    }
}