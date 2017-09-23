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

            var thrown = await Assert.ThrowsAsync<AggregateException>(() => sut);
            Assert.Equal(ex, thrown.GetBaseException());
        }

        [Fact]
        public async Task Bind_ProducesValueAsExpected()
        {
            var tcs = new TaskCompletionSource<string>();
            var sut = tcs.Task.Bind(s => Task.FromResult(s));
            const string message = "hey there";

            tcs.SetResult(message);

            Assert.Equal(message, await sut);
        }
        
        [Fact]
        public async Task Map_ThrowsExIfOuterIsFaulted()
        {
            var tcs = new TaskCompletionSource<int>();
            var ex = new DivideByZeroException();

            var sut = tcs.Task.Map(i => i * 2);

            tcs.SetException(ex);

            var thrown = await Assert.ThrowsAsync<AggregateException>(() => sut);
            Assert.Equal(ex, thrown.GetBaseException());
        }

        [Fact]
        public async Task Map_ProducesResultAsExpected()
        {
            var tcs = new TaskCompletionSource<int>();
            
            const int initial = 10;
            var asString = initial.ToString();

            var sut = tcs.Task.Map(i => i.ToString());

            tcs.SetResult(initial);

            Assert.Equal(TaskStatus.WaitingForActivation, sut.Status);

            Assert.Equal(asString, await sut);
        }
    }
}