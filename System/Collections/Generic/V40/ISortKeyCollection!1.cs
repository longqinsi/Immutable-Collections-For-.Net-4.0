namespace System.Collections.Generic.V40
{
    internal interface ISortKeyCollection<TKey>
    {
        IComparer<TKey> KeyComparer { get; }
    }
}

