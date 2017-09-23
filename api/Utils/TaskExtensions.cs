using System;
using System.Threading.Tasks;

namespace api.Utils
{
    public static class TaskExtensions
    {
        public static Task<TR> Bind<T, TR>(
            this Task<T> task,
            Func<T, Task<TR>> bindFunc)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (bindFunc == null) throw new ArgumentNullException(nameof(bindFunc));

            var tcs = new TaskCompletionSource<TR>();

            void HandleNonSuccess(Task specificTask)
            {
                if (specificTask.IsFaulted)
                {
                    tcs.SetException(specificTask.Exception);
                    return;
                }

                if (specificTask.IsCanceled)
                {
                    tcs.SetCanceled();
                    return;
                }
            }

            task.ContinueWith(t =>
            {
                HandleNonSuccess(t);

                bindFunc(t.Result).ContinueWith(ti =>
                {
                    HandleNonSuccess(ti);

                    if (ti.IsCompletedSuccessfully)
                    {
                        tcs.SetResult(ti.Result);
                        return;
                    }
                });
            });

            return tcs.Task;
        }

        public static Task<TR> Map<T, TR>(
            this Task<T> task,
            Func<T, TR> mapFunc)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (mapFunc == null) throw new ArgumentNullException(nameof(mapFunc));

            return task.Bind(v => Task.FromResult(mapFunc(v)));
        }

        public static Task<TR> Select<T, TR>(
            this Task<T> task,
            Func<T, TR> projection)
        {
            return task.Map(projection);
        }

        public static Task<TR> SelectMany<T, TI, TR>(
            this Task<T> task,
            Func<T, Task<TI>> bindFunc,
            Func<T, TI, TR> projection)
        {
            return task.Bind(outer => bindFunc(outer).Bind(inner => Task.FromResult(projection(outer, inner))));
        }

        // Task-carrying inner-monad binders
        public static Task<Option<TR>> Select<T, TR>(
            this Task<Option<T>> optionTask,
            Func<T, TR> projection
        )
        {
            return optionTask.Map(o => o.Map(projection));
        }

        public static Task<Result<TNewSuccess, TError>> Select<TSuccess, TNewSuccess, TError>(
            this Task<Result<TSuccess, TError>> resultTask,
            Func<TSuccess, TNewSuccess> projection
        )
        {
            return resultTask.Map(r => r.Map(projection));
        }

        public static Task<Result<TNewSuccess, TError>> SelectMany<TSuccess, TISuccess, TNewSuccess, TError>(
            this Task<Result<TSuccess, TError>> resTask,
            Func<TSuccess, Task<Result<TISuccess, TError>>> bindFunc,
            Func<TSuccess, TISuccess, TNewSuccess> projection)
        {
            return
                resTask
                .Bind(outerRes =>
                    outerRes.Match(
                        outerVal =>
                            bindFunc(outerVal)
                            .Map(innerRes => innerRes.Map(innerVal => projection(outerVal, innerVal))),
                        outerErr => Task.FromResult(Result<TNewSuccess, TError>.AsError(outerErr))
                    ));
        }

    }
}