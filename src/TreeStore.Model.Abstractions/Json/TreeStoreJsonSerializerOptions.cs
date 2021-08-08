using System.Text.Json;

namespace TreeStore.Model.Abstractions.Json
{
    public static class TreeStoreJsonSerializerOptions
    {
        static TreeStoreJsonSerializerOptions() => Default = Apply(new JsonSerializerOptions());

        public static JsonSerializerOptions Apply(JsonSerializerOptions options)
        {
            options.PropertyNameCaseInsensitive = true;
            options.Converters.Add(new FacetPropertyValueResultConverter());
            options.Converters.Add(new UpdateFacetPropertyValueRequestConverter());
            return options;
        }

        public static JsonSerializerOptions Default { get; }
    }
}