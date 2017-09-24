using System;
using System.Threading.Tasks;

namespace api.Utils
{
    public static class ResultExtensions
    {
        public static TR Match<TSuccess, TError, TR>(
            this Result<TSuccess, TError> result,
            Func<TSuccess, TR> ifSuccess,
            Func<TError, TR> ifError)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (ifSuccess == null) throw new ArgumentNullException(nameof(ifSuccess));
            if (ifError == null) throw new ArgumentNullException(nameof(ifError));

            return result.IsSuccess
                ? ifSuccess(result.SuccessValOrThrow())
                : ifError(result.ErrorValOrThrow());
        }

        public static Result<TNewSuccess, TError> Bind<TSuccess, TNewSuccess, TError>(
            this Result<TSuccess, TError> result,
            Func<TSuccess, Result<TNewSuccess, TError>> bindFunc)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (bindFunc == null) throw new ArgumentNullException(nameof(bindFunc));

            return result.Match(bindFunc, e => Result<TNewSuccess, TError>.AsError(e));
        }

        public static Result<TNewSuccess, TError> Map<TSuccess, TNewSuccess, TError>(
            this Result<TSuccess, TError> result,
            Func<TSuccess, TNewSuccess> mapFunc)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (mapFunc == null) throw new ArgumentNullException(nameof(mapFunc));

            return result.Bind(v => Result<TNewSuccess, TError>.AsSuccess(mapFunc(v)));
        }

        /*
            Not used, preserved for example.

            Unit test was written to use it, builds and passes, but vscode gets terribly upset
            and claims it can't find it.
         */
        // public static Result<TNewSuccess, TError> SelectMany<TSuccess, TError, TISuccess, TNewSuccess>(
        //     this Result<TSuccess, TError> result,
        //     Func<TSuccess, Result<TISuccess, TError>> bindFunc,
        //     Func<TSuccess, TISuccess, TNewSuccess> projection)
        //     => result.Bind(outer => bindFunc(outer).Bind(inner => Result<TNewSuccess, TError>.AsSuccess(projection(outer, inner))));

        public static Result<TSuccess, TError> ResOfOption<TSuccess, TError>(
            this Option<TSuccess> option,
            Func<TError> errorProvider)
        {
            return option.Match(v => Result<TSuccess, TError>.AsSuccess(v),
                () => Result<TSuccess, TError>.AsError(errorProvider()));
        }

        public static Task<Result<TSuccess, TError>> ResOfOption<TSuccess, TError>(
            this Task<Option<TSuccess>> optionTask,
            Func<TError> errorProvider)
            => optionTask.Map(option => option.ResOfOption(errorProvider));

    }
}