namespace System.Collections.Generic
{
    internal interface ISortKeyCollectionV40<in TKey>
    {
        IComparer<TKey> KeyComparer { get; }
    }
}

