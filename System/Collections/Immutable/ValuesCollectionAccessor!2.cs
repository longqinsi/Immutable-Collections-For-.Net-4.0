namespace System.Collections.Immutable
{
    using System;

    internal class ValuesCollectionAccessor<TKey, TValue> : KeysOrValuesCollectionAccessor<TKey, TValue, TValue>
    {
        internal ValuesCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionaryV40) : base(dictionaryV40, dictionaryV40.Values)
        {
        }

        public override bool Contains(TValue item)
        {
            ImmutableSortedDictionary<TKey, TValue> dictionaryV40 = base.DictionaryV40 as ImmutableSortedDictionary<TKey, TValue>;
            if (dictionaryV40 != null)
            {
                return dictionaryV40.ContainsValue(item);
            }
            IImmutableDictionaryInternal<TKey, TValue> internal2 = base.DictionaryV40 as IImmutableDictionaryInternal<TKey, TValue>;
            if (internal2 == null)
            {
                throw new NotSupportedException();
            }
            return internal2.ContainsValue(item);
        }
    }
}

