namespace System.Collections.Generic.V40
{
    internal interface ISortKeyCollection<in TKey>
    {
        IComparer<TKey> KeyComparer { get; }
    }
}

