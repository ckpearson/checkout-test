using System;

namespace api.Utils
{
    public sealed class Option<T>
    {
        private readonly T _value;

        private Option(bool hasValue, T value = default(T))
        {
            HasValue = hasValue;
            _value = value;
        }

        public bool HasValue {get;}
        
        public T GetValueOrThrow()
            => HasValue ? _value : throw new InvalidOperationException("Option does not contain a value");

        public static Option<T> Some(T value) => new Option<T>(true, value);
        public static Option<T> None => new Option<T>(false);
    }
}