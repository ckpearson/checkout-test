using System;

namespace api.Utils
{
    public sealed class Result<TSuccess, TError>
    {
        private readonly TSuccess _successVal;
        private readonly TError _errorVal;
        private Result(
            bool isSuccess,
            TSuccess successVal = default(TSuccess),
            TError errorVal = default(TError))
        {
            IsSuccess = isSuccess;
            _successVal = successVal;
            _errorVal = errorVal;
        }

        public bool IsSuccess { get; }

        public TSuccess SuccessValOrThrow()
            => IsSuccess ? _successVal : throw new InvalidOperationException("Result is not for success");

        public TError ErrorValOrThrow()
            => !IsSuccess ? _errorVal : throw new InvalidOperationException("Result is not for error");

        public static Result<TSuccess, TError> AsSuccess(TSuccess value)
            => new Result<TSuccess, TError>(true, value);

        public static Result<TSuccess, TError> AsError(TError value)
            => new Result<TSuccess, TError>(false, errorVal: value);
    }
}