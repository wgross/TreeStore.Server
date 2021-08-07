using LiteDB;
using Microsoft.Extensions.Logging;
using System;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class FacetRepository : LiteDbRepositoryBase<FacetModel>
    {
        public FacetRepository(LiteRepository db, ILogger<FacetRepository> logger) : base(db, "facets", logger)
        {
        }

        protected override ILiteCollection<FacetModel> IncludeRelated(ILiteCollection<FacetModel> from)
        {
            throw new NotImplementedException();
        }
    }
}