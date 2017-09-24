using System;
using System.Threading.Tasks;
using api.Utils;
using Xunit;

namespace api_tests
{
    public class ResultExtensionTests
    {
        [Fact]
        public void Match_ProducesExpectedValue_ForSuccess()
        {
            const int initial = 10;
            const int desired = 50;

            var sut = Result<int, string>.AsSuccess(initial);

            var result = sut.Match(_ => desired, _ => throw new NotImplementedException());

            Assert.Equal(desired, result);
        }

        [Fact]
        public void Match_ProducesExpectedValue_ForError()
        {
            const string initial = "err";
            const int desired = 10;

            var sut = Result<int, string>.AsError(initial);

            var result = sut.Match(_ => throw new NotImplementedException(), _ => desired);

            Assert.Equal(desired, result);
        }

        [Fact]
        public void Bind_ShortsForError()
        {
            const string err = "some error";

            var sut = Result<int, string>.AsError(err);

            var result = sut.Bind<int, int, string>(_ => throw new NotImplementedException());

            Assert.Equal(err, result.ErrorValOrThrow());
        }

        [Fact]
        public void Bind_ProducesResultForSuccess()
        {
            const int initial = 10;
            var desired = initial * 2;

            var sut = Result<int,string>.AsSuccess(initial);
            
            var result = sut.Bind(i => Result<int,string>.AsSuccess(i * 2));

            Assert.Equal(desired, result.SuccessValOrThrow());
        }

        [Fact]
        public void Map_ShortsForError()
        {
            const string err = "error";

            var sut = Result<int,string>.AsError(err);

            var result = sut.Map<int,int, string>(_ => throw new NotImplementedException());

            Assert.Equal(err, result.ErrorValOrThrow());
        }

        [Fact]
        public void Map_ProducesValueForSuccess()
        {
            const int initial = 10;
            var desired = initial - 5;

            var sut = Result<int,string>.AsSuccess(initial);

            var result = sut.Map(i => i - 5);

            Assert.Equal(desired, result.SuccessValOrThrow());
        }

        [Fact]
        public void ResOfOption_ProducesValueResultForSome()
        {
            const int value = 50;
            var option = Option<int>.Some(value);

            var sut = option.ResOfOption<int,string>(() => throw new NotImplementedException());

            Assert.Equal(value, sut.SuccessValOrThrow());
        }

        [Fact]
        public void ResOfOption_ProducesErrorResultForNone()
        {
            const string err = "some other error";
            var option = Option<string>.None;

            var sut = option.ResOfOption(() => err);

            Assert.Equal(err, sut.ErrorValOrThrow());
        }

        [Fact]
        public async Task ResOfOption_OptionTask_ProducesSuccessResultTaskForSome()
        {
            const int value = 5;

            var optTask = Task.FromResult(Option<int>.Some(value));

            var res = await optTask.ResOfOption<int,string>(() => throw new NotImplementedException());

            Assert.Equal(value, res.SuccessValOrThrow());
        }

        [Fact]
        public async Task ResOfOption_OptionTask_ProducesErrorResultForNone()
        {
            const string err = "hello";

            var optTask = Task.FromResult(Option<int>.None);

            var res = await optTask.ResOfOption(() => err);

            Assert.Equal(err, res.ErrorValOrThrow());
        }

        // [Fact]
        // public void SelectMany_QueryExpression_HappyPath()
        // {
        //     var res = 
        //         from x in Result<int,string>.AsSuccess(5)
        //         from y in Result<int,string>.AsSuccess(10)
        //         select x + y;

        //     Assert.Equal(15, res.SuccessValOrThrow());
        // }
    }
}