namespace System.Collections.Immutable
{
    using System;

    internal interface ISecurePooledObjectUser
    {
        Guid PoolUserId { get; }
    }
}

