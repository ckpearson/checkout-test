using System.Collections.Generic;

namespace api.Utils
{
    public static class DictionaryExtensions
    {
        public static Option<TVal> GetValueOrNone<TKey, TVal>(
            this IDictionary<TKey, TVal> dict,
            TKey key)
        {
            TVal val = default(TVal);
            return dict.TryGetValue(key, out val)
                ? Option<TVal>.Some(val)
                : Option<TVal>.None;
        }
    }
}