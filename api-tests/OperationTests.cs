using System;
using api.Services;
using Moq;
using Xunit;

namespace api_tests
{
    public class Fixture : IDisposable
    {
        public Fixture()
        {
            Store = new DummyDataStore();
        }
        public void Dispose()
        {
        }

        public IDataStore Store {get;}
    }
    public class OperationTests : IClassFixture<Fixture>
    {
        private readonly Fixture _fixture;

        public OperationTests(Fixture fixture)
        {
            _fixture = fixture;
        }
    }
}