using System;
using api.Utils;
using Xunit;

namespace api_tests
{
    public class OptionExtensionTests
    {
        [Fact]
        public void Match_ProducesExpectedValue_ForSome()
        {
            const int initial = 50;
            var desired = initial.ToString();
            
            var sut = Option<int>.Some(initial);

            var result = sut.Match(v => desired, () => throw new NotImplementedException());

            Assert.Equal(desired, result);
        }

        [Fact]
        public void Match_ProducesExpectedValue_ForNone()
        {
            const string desired = "output";

            var sut = Option<int>.None;

            var result = sut.Match(_ => throw new NotImplementedException(), () => desired);

            Assert.Equal(desired, result);
        }

        [Fact]
        public void Bind_ShortsOnOuterNone()
        {
            var sut = Option<string>.None;

            var result = sut.Bind(v => Option<string>.Some(v));

            Assert.False(result.HasValue);
        }

        [Fact]
        public void Bind_GoesAheadOnSome()
        {
            const int initial = 10;
            var desired = initial.ToString();

            var sut = Option<int>.Some(initial);
            var result = sut.Bind(v => Option<string>.Some(desired));

            Assert.Equal(desired, result.GetValueOrThrow());
        }

        [Fact]
        public void Map_ShortsOnOuterNone()
        {
            var sut = Option<string>.None;

            var result = sut.Map(s => s);

            Assert.False(result.HasValue);
        }

        [Fact]
        public void Map_GoesAheadOnSome()
        {
            const string initial = "hello";
            var desired = "bye";

            var sut = Option<string>.Some(initial);

            var result = sut.Map(_ => desired);

            Assert.Equal(desired, result.GetValueOrThrow());
        }

        [Fact]
        public void ValueOrElse_ProducesValueForSome()
        {
            const int value = 50;

            var sut = Option<int>.Some(value);

            var result = sut.ValueOrElse(1000);

            Assert.Equal(value, result);
        }

        [Fact]
        public void ValueOrElse_ProducesValueForNone()
        {
            const int @else = 10;

            var sut = Option<int>.None;

            var result = sut.ValueOrElse(@else);

            Assert.Equal(@else, result);
        }

        [Fact]
        public void SelectMany_Query_HappyPath()
        {
            var result = 
                from x in Option<int>.Some(10)
                from y in Option<int>.Some(5)
                select x + y;

            Assert.Equal(15, result.GetValueOrThrow());
        }

        [Fact]
        public void SelectMany_Query_ShortsAsExpected()
        {
            var result =
                from a in Option<int>.Some(10)
                from b in Option<int>.None
                from c in Option<int>.Some(5)
                select a + b + c;

            Assert.False(result.HasValue);
        }
    }
}