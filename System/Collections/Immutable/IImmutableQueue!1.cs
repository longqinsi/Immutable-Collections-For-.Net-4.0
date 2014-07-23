namespace System.Collections.Immutable
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IImmutableQueue<T> : IEnumerable<T>, IEnumerable
    {
        IImmutableQueue<T> Clear();
        IImmutableQueue<T> Dequeue();
        IImmutableQueue<T> Enqueue(T value);
        T Peek();

        bool IsEmpty { get; }
    }
}

