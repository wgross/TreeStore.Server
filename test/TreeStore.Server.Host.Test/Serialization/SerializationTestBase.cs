using System.Text.Json;
using TreeStore.Model.Abstractions.Json;

namespace TreeStore.Server.Host.Test.Serialization
{
    public abstract class SerializationTestBase
    {
        protected T SerializationRoundTrip<T>(T instance)
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(instance, options: TreeStoreJsonSerializerOptions.Default), options: TreeStoreJsonSerializerOptions.Default);
        }
    }
}