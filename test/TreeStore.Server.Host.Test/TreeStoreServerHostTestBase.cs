using Moq;
using System;

namespace TreeStore.Server.Host.Test
{
    public abstract class TreeStoreServerHostTestBase : IDisposable
    {
        private bool disposedValue;

        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.Mocks.VerifyAll();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}