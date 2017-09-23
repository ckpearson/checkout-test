using System;
using Microsoft.AspNetCore.Mvc;

namespace api.Utils
{
    public static class OptionExtensions
    {
        public static TR Match<T, TR>(
            this Option<T> option,
            Func<T, TR> ifSome,
            Func<TR> ifNone)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));
            if (ifSome == null) throw new ArgumentNullException(nameof(ifSome));
            if (ifNone == null) throw new ArgumentNullException(nameof(ifNone));

            return option.HasValue
                ? ifSome(option.GetValueOrThrow())
                : ifNone();
        }

        public static Option<TR> Bind<T, TR>(
            this Option<T> option,
            Func<T, Option<TR>> bindFunc)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));
            if (bindFunc == null) throw new ArgumentNullException(nameof(bindFunc));

            return option.Match(bindFunc, () => Option<TR>.None);
        }

        public static Option<TR> Map<T, TR>(
            this Option<T> option,
            Func<T, TR> mapFunc)
        {
            if (option == null) throw new ArgumentNullException(nameof(option));
            if (mapFunc == null) throw new ArgumentNullException(nameof(mapFunc));

            return option.Bind(v => Option<TR>.Some(mapFunc(v)));
        }

        public static T ValueOrElse<T>(this Option<T> option, T elseValue)
            => option.Match(v => v, () => elseValue);

        public static Option<TR> SelectMany<T, T2, TR>(
            this Option<T> option,
            Func<T, Option<T2>> binder,
            Func<T, T2, TR> projection)
            => option.Bind(outer => binder(outer).Bind(inner => Option<TR>.Some(projection(outer, inner))));
    }
}