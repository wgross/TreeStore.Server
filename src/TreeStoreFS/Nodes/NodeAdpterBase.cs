using System;
using System.Threading.Tasks;
using TreeStore.Model.Abstractions;

namespace TreeStoreFS.Nodes
{
    public abstract class NodeAdapterBase : IServiceProvider
    {
        protected NodeAdapterBase(ITreeStoreService treeStoreService)
        {
            this.TreeStoreService = treeStoreService;
        }

        protected ITreeStoreService TreeStoreService { get; }

        #region IServiceProvider

        /// <inheritdoc/>
        public object? GetService(Type serviceType)
        {
            if (this.GetType().IsAssignableTo(serviceType))
                return this;

            return null;
        }

        #endregion IServiceProvider

        protected static T Await<T>(Task<T> action) => action.ConfigureAwait(false).GetAwaiter().GetResult();

        protected static void Await(Task action) => action.ConfigureAwait(false).GetAwaiter().GetResult();
    }
}