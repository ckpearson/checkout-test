using System;
using System.Threading.Tasks;
using api.Utils;
using Xunit;

namespace api_tests
{
    public class TaskExtensionTests
    {
        [Fact]
        public async Task Bind_ForwardsExceptionFromOriginalTask()
        {
            var tcs = new TaskCompletionSource<int>();
            var ex = new InvalidOperationException("some exception");

            var sut = tcs.Task.Bind(i => Task.FromResult(i));

            tcs.SetException(ex);

            Assert.Equal(TaskStatus.WaitingForActivation, sut.Status);

            var thrown = await Assert.ThrowsAsync<AggregateException>(() => sut);
            Assert.Equal(ex, thrown.GetBaseException());
        }
    }
}