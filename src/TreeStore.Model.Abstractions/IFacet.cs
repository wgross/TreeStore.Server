using System.Collections.Generic;

namespace TreeStore.Model.Abstractions
{
    /// <summary>
    /// A Facet has a collection of name property definitions <see cref="Properties"/>
    /// </summary>
    /// <typeparam name="F"></typeparam>
    public interface IFacet<F> : INamed
        where F : IFacetProperty
    {
        /// <summary>
        /// Defines the properties of a facte folowing the contract <see cref="IFacetProperty"/>
        /// </summary>
        IEnumerable<F> Properties { get; }
    }
}