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
        /// Provides low level access to underlying the lite db.
        /// </summary>
        public LiteRepository LiteRepository { get; }

        public ILiteCollection<BsonDocument> LiteCollectionDocuments() => this.LiteRepository.Database.GetCollection(this.CollectionName);

        protected LiteDbRepositoryBase(LiteRepository repository, string collectionName)
        {
            this.LiteRepository = repository;
            this.CollectionName = collectionName;
        }
    }

    public abstract partial class LiteDbRepositoryBase<T> : LiteDbRepositoryBase
        where T : NamedModelBase, IIdentifiable
    {
        private readonly ILogger logger;

        protected IDisposable BeginScope(T t) => this.logger.BeginScope($"{nameof(T)}(id='{{id}}')", t.Id);

        static LiteDbRepositoryBase() => BsonMapper.Global.Entity<T>().Id(v => v.Id);

        public LiteDbRepositoryBase(LiteRepository repository, string collectionName, ILogger<LiteDbRepositoryBase<T>> logger)
            : base(repository, collectionName)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Generic update method used by all derived repositories.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual T Upsert(T item)
        {
            var succeeded = this.LiteRepository.Upsert(item, CollectionName);

            if (succeeded)
                this.LogUpsertedLiteDbItem(item);
            else
                this.LogUpsertingLiteDbItemFailed(item);

            return item;
        }

        public ILiteCollection<T> LiteCollection() => this.LiteRepository.Database.GetCollection<T>(this.CollectionName);

        public virtual T? FindById(Guid id) => this.IncludeRelated(this.LiteCollection()).FindById(id);

        public IEnumerable<T> FindAll() => this.IncludeRelated(this.LiteCollection()).FindAll();

        public virtual bool Delete(T item)
        {
            using var scope = this.BeginScope(item);

            this.LogDeletingLiteDbItem(item);

            return this.LiteCollection().Delete(item.Id);
        }

        abstract protected ILiteCollection<T> IncludeRelated(ILiteCollection<T> from);

        #region Logging

        protected void LogDeletingLiteDbItem(T instance) => this.logger.LogDeletingLiteDbItem(nameof(T), instance.Id);

        protected void LogUpsertedLiteDbItem(T instance) => this.logger.LogUpsertedLiteDbItem(nameof(T), instance.Id);

        protected void LogUpsertingLiteDbItemFailed(T instance) => this.logger.LogUpsertingLiteDbItemFailed(nameof(T), instance.Id);

        #endregion Logging
    }
}