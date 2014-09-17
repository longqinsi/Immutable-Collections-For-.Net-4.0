namespace System.Collections.Immutable
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Validation;

    public static class ImmutableStack
    {
        public static ImmutableStack<T> Create<T>()
        {
            return ImmutableStack<T>.Empty;
        }

        public static ImmutableStack<T> Create<T>(T item)
        {
            return ImmutableStack<T>.Empty.Push(item);
        }

        public static ImmutableStack<T> Create<T>(params T[] items)
        {
            Requires.NotNull<T[]>(items, "items");
            ImmutableStack<T> empty = ImmutableStack<T>.Empty;
            foreach (T local in items)
            {
                empty = empty.Push(local);
            }
            return empty;
        }

        public static ImmutableStack<T> CreateRange<T>(IEnumerable<T> items)
        {
            Requires.NotNull<IEnumerable<T>>(items, "items");
            ImmutableStack<T> empty = ImmutableStack<T>.Empty;
            foreach (T local in items)
            {
                empty = empty.Push(local);
            }
            return empty;
        }

        public static IImmutableStack<T> Pop<T>(IImmutableStack<T> stack, T value)
        {
            Requires.NotNull<IImmutableStack<T>>(stack, "stack");
            value = stack.Peek();
            return stack.Pop();
        }
    }
}

