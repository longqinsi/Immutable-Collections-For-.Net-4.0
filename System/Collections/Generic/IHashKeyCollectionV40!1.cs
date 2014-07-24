namespace System.Collections.Generic
{
    internal interface IHashKeyCollectionV40<in TKey>
    {
        IEqualityComparer<TKey> KeyComparer { get; }
    }
}

