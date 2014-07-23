namespace System.Collections.Immutable
{
    using System;

    internal interface IImmutableDictionaryInternal<TKey, TValue>
    {
        bool ContainsValue(TValue value);
    }
}

