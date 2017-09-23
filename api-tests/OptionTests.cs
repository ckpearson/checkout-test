using System;
using Xunit;
using api.Utils;

namespace api_tests
{
    public class OptionTests
    {
        [Fact]
        public void Option_None_ReportsNoValue()
        {
            Assert.False(Option<string>.None.HasValue);
        }

        [Fact]
        public void Option_Some_ReportsValue()
        {
            Assert.True(Option<int>.Some(10).HasValue);
        }

        [Fact]
        public void Option_Some_ValueOrThrow_ProducesValue()
        {
            const int val = 20;
            var sut = Option<int>.Some(val);

            Assert.Equal(val, sut.GetValueOrThrow());
        }

        [Fact]
        public void Option_None_ValueOrThrow_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() => Option<string>.None.GetValueOrThrow());
        }
    }
}
