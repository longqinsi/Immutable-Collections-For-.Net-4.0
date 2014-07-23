namespace System.Collections.Immutable
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public interface IImmutableStack<T> : IEnumerable<T>, IEnumerable
    {
        IImmutableStack<T> Clear();
        T Peek();
        IImmutableStack<T> Pop();
        IImmutableStack<T> Push(T value);

        bool IsEmpty { get; }
    }
}

