namespace System.Collections.Immutable
{
    using System;

    internal class ValuesCollectionAccessor<TKey, TValue> : KeysOrValuesCollectionAccessor<TKey, TValue, TValue>
    {
        internal ValuesCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionary) : base(dictionary, dictionary.Values)
        {
        }

        public override bool Contains(TValue item)
        {
            ImmutableSortedDictionary<TKey, TValue> dictionary = base.Dictionary as ImmutableSortedDictionary<TKey, TValue>;
            if (dictionary != null)
            {
                return dictionary.ContainsValue(item);
            }
            IImmutableDictionaryInternal<TKey, TValue> internal2 = base.Dictionary as IImmutableDictionaryInternal<TKey, TValue>;
            if (internal2 == null)
            {
                throw new NotSupportedException();
            }
            return internal2.ContainsValue(item);
        }
    }
}

