using LiteDB;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using TreeStore.Model;
using TreeStore.Model.Abstractions;

namespace TreeStore.LiteDb
{
    public abstract class LiteDbRepositoryBase
    {
        protected string CollectionName { get; init; }

        /// <summary>
        /// Provides low oleve access to underlying the lite db.
        /// </summary>
        public LiteRepository LiteRepository { get; }

        public ILiteCollection<BsonDocument> LiteCollectionDocuments() => this.LiteRepository.Database.GetCollection(this.CollectionName);

        protected LiteDbRepositoryBase(LiteRepository repository, string collectionName)
        {
            this.LiteRepository = repository;
            this.CollectionName = collectionName;
        }
    }

    public abstract class LiteDbRepositoryBase<T> : LiteDbRepositoryBase
        where T : NamedBase, IIdentifiable
    {
        protected ILogger Logger { get; }

        protected IDisposable BeginScope(T t) => this.Logger.BeginScope($"{nameof(T)}(id='{{id}}')", t.Id);

        static LiteDbRepositoryBase() => BsonMapper.Global.Entity<T>().Id(v => v.Id);

        public LiteDbRepositoryBase(LiteRepository repository, string collectionName, ILogger<LiteDbRepositoryBase<T>> logger)
            : base(repository, collectionName)
        {
            this.Logger = logger;
        }

        public virtual T Upsert(T item)
        {
            var succeeded = this.LiteRepository.Upsert(item, CollectionName);

            if (succeeded)
                this.Logger.UpsertedLiteDbItem(item);
            else
                this.Logger.UpsertedLiteDbItemFailed(item);

            return item;
        }

        public ILiteCollection<T> LiteCollection() => this.LiteRepository.Database.GetCollection<T>(this.CollectionName);

        public T FindById(Guid id) => this.IncludeRelated(this.LiteCollection()).FindById(id);

        public IEnumerable<T> FindAll() => this.IncludeRelated(this.LiteCollection()).FindAll();

        public virtual bool Delete(T item)
        {
            using var scope = this.BeginScope(item);

            this.Logger.DeletingLiteDbItem(item);

            return this.LiteCollection().Delete(item.Id);
        }

        abstract protected ILiteCollection<T> IncludeRelated(ILiteCollection<T> from);
    }
}