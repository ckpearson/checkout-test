using System;
using System.Threading.Tasks;

namespace api.Utils
{
    public static class TaskExtensions
    {
        /*
            Monadic bind for the Task<T> monad.

            Given a task and a function mapping from the task's value to a new task:
                1. Propagate any failures from anywhere in the chain forward
                2. Only bind upon successful completion of the original task
                3. Propagate the resultant task's value outwards when ready.
         */
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

        /*
            Monadic map for the Task<T> monad.

            Given a task, perform a mapping over the value, if and only if it runs to completion.
         */
        public static Task<TR> Map<T, TR>(
            this Task<T> task,
            Func<T, TR> mapFunc)
        {
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (mapFunc == null) throw new ArgumentNullException(nameof(mapFunc));

            return task.Bind(v => Task.FromResult(mapFunc(v)));
        }

        /*
            Query expression bind function for Task<Result> -> Task<Result>

            This just performs a task bind from a result-returning task, to another result-returning task,
            and conforms to the query expression structure for introducing a projection.
         */
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