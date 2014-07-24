namespace System.Collections.Immutable
{
    using System;

    internal class KeysCollectionAccessor<TKey, TValue> : KeysOrValuesCollectionAccessor<TKey, TValue, TKey>
    {
        internal KeysCollectionAccessor(IImmutableDictionary<TKey, TValue> dictionaryV40) : base(dictionaryV40, dictionaryV40.Keys)
        {
        }

        public override bool Contains(TKey item)
        {
            return base.DictionaryV40.ContainsKey(item);
        }
    }
}

