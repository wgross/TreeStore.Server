using Moq;
using System;

namespace TreeStore.Model.Test.Base
{
    public class TreeStoreModelTestBase : IDisposable
    {
        protected MockRepository Mocks { get; } = new(MockBehavior.Strict);

        public void Dispose() => this.Mocks.VerifyAll();
    }
}