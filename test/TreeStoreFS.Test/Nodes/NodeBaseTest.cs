using Moq;
using System;

namespace TreeStoreFS.Test.Nodes
{
    public abstract class NodeBaseTest : IDisposable
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);

        public void Dispose() => this.Mocks.VerifyAll();
    }
}