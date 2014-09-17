namespace System.Collections.Generic.V40
{
    internal interface IHashKeyCollection<TKey>
    {
        IEqualityComparer<TKey> KeyComparer { get; }
    }
}

