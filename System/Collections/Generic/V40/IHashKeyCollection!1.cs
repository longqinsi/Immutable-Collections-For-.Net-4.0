namespace System.Collections.Generic.V40
{
    internal interface IHashKeyCollection<in TKey>
    {
        IEqualityComparer<TKey> KeyComparer { get; }
    }
}

