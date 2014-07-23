namespace System.Collections.Immutable
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Validation;

    internal static class ImmutableExtensions
    {
        internal static IOrderedCollection<T> AsOrderedCollection<T>(this IEnumerable<T> sequence)
        {
            Requires.NotNull<IEnumerable<T>>(sequence, "sequence");
            IOrderedCollection<T> ordereds = sequence as IOrderedCollection<T>;
            if (ordereds != null)
            {
                return ordereds;
            }
            IList<T> collection = sequence as IList<T>;
            if (collection != null)
            {
                return new ListOfTWrapper<T>(collection);
            }
            return new FallbackWrapper<T>(sequence);
        }

        internal static int GetCount<T>(ref IEnumerable<T> sequence)
        {
            int num;
            if (!sequence.TryGetCount<T>(out num))
            {
                List<T> list = Enumerable.ToList<T>(sequence);
                num = list.Count;
                sequence = (IEnumerable<T>) list;
            }
            return num;
        }

        internal static T[] ToArray<T>(this IEnumerable<T> sequence, int count)
        {
            Requires.NotNull<IEnumerable<T>>(sequence, "sequence");
            Requires.Range(count >= 0, "count", null);
            T[] localArray = new T[count];
            int num = 0;
            foreach (T local in sequence)
            {
                Requires.Argument(num < count);
                localArray[num++] = local;
            }
            Requires.Argument(num == count);
            return localArray;
        }

        internal static bool TryGetCount<T>(this IEnumerable sequence, out int count)
        {
            ICollection is2 = sequence as ICollection;
            if (is2 != null)
            {
                count = is2.Count;
                return true;
            }
            ICollection<T> is3 = sequence as ICollection<T>;
            if (is3 != null)
            {
                count = is3.Count;
                return true;
            }
            IReadOnlyCollection<T> onlys = sequence as IReadOnlyCollection<T>;
            if (onlys != null)
            {
                count = onlys.Count;
                return true;
            }
            count = 0;
            return false;
        }

        internal static bool TryGetCount<T>(this IEnumerable<T> sequence, out int count)
        {
            return ((IEnumerable) sequence).TryGetCount<T>(out count);
        }

        private class FallbackWrapper<T> : IOrderedCollection<T>, IEnumerable<T>, IEnumerable
        {
            private IList<T> collection;
            private readonly IEnumerable<T> sequence;

            internal FallbackWrapper(IEnumerable<T> sequence)
            {
                Requires.NotNull<IEnumerable<T>>(sequence, "sequence");
                this.sequence = sequence;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this.sequence.GetEnumerator();
            }

            //[ExcludeFromCodeCoverage]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator) this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    if (this.collection == null)
                    {
                        int num;
                        if (this.sequence.TryGetCount<T>(out num))
                        {
                            return num;
                        }
                        this.collection = Enumerable.ToArray<T>(this.sequence);
                    }
                    return this.collection.Count;
                }
            }

            public T this[int index]
            {
                get
                {
                    if (this.collection == null)
                    {
                        this.collection = Enumerable.ToArray<T>(this.sequence);
                    }
                    return this.collection[index];
                }
            }
        }

        private class ListOfTWrapper<T> : IOrderedCollection<T>, IEnumerable<T>, IEnumerable
        {
            private readonly IList<T> collection;

            internal ListOfTWrapper(IList<T> collection)
            {
                Requires.NotNull<IList<T>>(collection, "collection");
                this.collection = collection;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this.collection.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator) this.GetEnumerator();
            }

            public int Count
            {
                get
                {
                    return this.collection.Count;
                }
            }

            public T this[int index]
            {
                get
                {
                    return this.collection[index];
                }
            }
        }
    }
}

