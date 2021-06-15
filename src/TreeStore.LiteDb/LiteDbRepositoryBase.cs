using LiteDB;
using System;
using System.Collections.Generic;
using TreeStore.Model;

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
        where T : NamedBase
    {
        static LiteDbRepositoryBase() => BsonMapper.Global.Entity<T>().Id(v => v.Id);

        public LiteDbRepositoryBase(LiteRepository repository, string collectionName)
            : base(repository, collectionName)
        { }

        public virtual T Upsert(T item)
        {
            this.LiteRepository.Upsert(item, CollectionName);
            return item;
        } 

        public ILiteCollection<T> LiteCollection() => this.LiteRepository.Database.GetCollection<T>(this.CollectionName);

        public T FindById(Guid id) => this.IncludeRelated(this.LiteCollection()).FindById(id);

        public IEnumerable<T> FindAll() => this.IncludeRelated(this.LiteCollection()).FindAll();

        public virtual bool Delete(T entity) => this.LiteCollection().Delete(entity.Id);

        abstract protected ILiteCollection<T> IncludeRelated(ILiteCollection<T> from);
    }
}