using LiteDB;
using Microsoft.Extensions.Logging;
using System;
using TreeStore.Model;

namespace TreeStore.LiteDb
{
    public class FacetRepository : LiteDbRepositoryBase<Facet>
    {
        public FacetRepository(LiteRepository db, ILogger<FacetRepository> logger) : base(db, "facets", logger)
        {
        }

        protected override ILiteCollection<Facet> IncludeRelated(ILiteCollection<Facet> from)
        {
            throw new NotImplementedException();
        }
    }
}