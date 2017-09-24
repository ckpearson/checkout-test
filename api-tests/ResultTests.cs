using System;
using api.Utils;
using Xunit;

namespace api_tests
{
    public class ResultTests
    {
        [Fact]
        public void Result_AsSuccess_ProducesSuccessResult()
        {
            var sut = Result<int,string>.AsSuccess(10);
            Assert.True(sut.IsSuccess);
        }

        [Fact]
        public void Result_AsError_ProducesErrorResult()
        {
            var sut = Result<int,string>.AsError("test");
            Assert.False(sut.IsSuccess);
        }

        [Fact]
        public void Result_SuccessValOrThrow_ReturnsValForSuccess()
        {
            const int val = 5;
            var sut = Result<int,string>.AsSuccess(val);

            Assert.Equal(val, sut.SuccessValOrThrow());
        }

        [Fact]
        public void Result_SuccessValOrThrow_ThrowsForError()
        {
            var sut = Result<int,string>.AsError("");
            Assert.Throws<InvalidOperationException>(() => sut.SuccessValOrThrow());
        }

        [Fact]
        public void Result_ErrorValOrThrow_ReturnsValForError()
        {
            const string err = "some error";
            var sut = Result<int,string>.AsError(err);

            Assert.Equal(err, sut.ErrorValOrThrow());
        }

        [Fact]
        public void Result_ErrorValOrThrow_ThrowsForSuccess()
        {
            var sut = Result<int,string>.AsSuccess(10);
            Assert.Throws<InvalidOperationException>(() => sut.ErrorValOrThrow());
        }
    }
}