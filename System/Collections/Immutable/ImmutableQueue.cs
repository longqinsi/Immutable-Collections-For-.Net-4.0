namespace System.Collections.Immutable
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Validation;

    public static class ImmutableQueue
    {
        public static ImmutableQueue<T> Create<T>()
        {
            return ImmutableQueue<T>.Empty;
        }

        public static ImmutableQueue<T> Create<T>(T item)
        {
            return ImmutableQueue<T>.Empty.Enqueue(item);
        }

        public static ImmutableQueue<T> Create<T>(params T[] items)
        {
            Requires.NotNull<T[]>(items, "items");
            ImmutableQueue<T> empty = ImmutableQueue<T>.Empty;
            foreach (T local in items)
            {
                empty = empty.Enqueue(local);
            }
            return empty;
        }

        public static ImmutableQueue<T> CreateRange<T>(IEnumerable<T> items)
        {
            Requires.NotNull<IEnumerable<T>>(items, "items");
            ImmutableQueue<T> empty = ImmutableQueue<T>.Empty;
            foreach (T local in items)
            {
                empty = empty.Enqueue(local);
            }
            return empty;
        }

        public static IImmutableQueue<T> Dequeue<T>(IImmutableQueue<T> queue, T value)
        {
            Requires.NotNull<IImmutableQueue<T>>(queue, "queue");
            value = queue.Peek();
            return queue.Dequeue();
        }
    }
}

