namespace System.Collections.Immutable
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Validation;

    [DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableList<>.DebuggerProxy))]
    public sealed class ImmutableList<T> : IImmutableList<T>, IList<T>, ICollection<T>, IList, ICollection, IOrderedCollection<T>, IImmutableListQueries<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
    {
        public static readonly ImmutableList<T> Empty;
        private readonly Node root;

        static ImmutableList()
        {
            ImmutableList<T>.Empty = new ImmutableList<T>();
        }

        internal ImmutableList()
        {
            this.root = Node.EmptyNode;
        }

        private ImmutableList(Node root)
        {
            Requires.NotNull<Node>(root, "root");
            root.Freeze();
            this.root = root;
        }

        public ImmutableList<T> Add(T value)
        {
            Node root = this.root.Add(value);
            return this.Wrap(root);
        }

        public ImmutableList<T> AddRange(IEnumerable<T> items)
        {
            Requires.NotNull<IEnumerable<T>>(items, "items");
            if (this.IsEmpty)
            {
                return this.FillFromEmpty(items);
            }
            Node root = this.root;
            foreach (T local in items)
            {
                root = root.Add(local);
            }
            return this.Wrap(root);
        }

        public int BinarySearch(T item)
        {
            return this.BinarySearch(item, null);
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return this.BinarySearch(0, this.Count, item, comparer);
        }

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            return this.root.BinarySearch(index, count, item, comparer);
        }

        public ImmutableList<T> Clear()
        {
            return ImmutableList<T>.Empty;
        }

        public bool Contains(T value)
        {
            return (this.IndexOf(value) >= 0);
        }

        public ImmutableList<TOutput> ConvertAll<TOutput>(Func<T, TOutput> converter)
        {
            Requires.NotNull<Func<T, TOutput>>(converter, "converter");
            return ImmutableList<TOutput>.WrapNode(this.root.ConvertAll<TOutput>(converter));
        }

        public void CopyTo(T[] array)
        {
            Requires.NotNull<T[]>(array, "array");
            Requires.Range(array.Length >= this.Count, "array", null);
            this.root.CopyTo(array);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Requires.NotNull<T[]>(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex", null);
            Requires.Range(array.Length >= (arrayIndex + this.Count), "arrayIndex", null);
            this.root.CopyTo(array, arrayIndex);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            this.root.CopyTo(index, array, arrayIndex, count);
        }

        public bool Exists(Predicate<T> match)
        {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.Exists(match);
        }

        private ImmutableList<T> FillFromEmpty(IEnumerable<T> items)
        {
            ImmutableList<T> list;
            if (ImmutableList<T>.TryCastToImmutableList(items, out list))
            {
                return list;
            }
            IOrderedCollection<T> ordereds = items.AsOrderedCollection<T>();
            if (ordereds.Count == 0)
            {
                return (ImmutableList<T>) this;
            }
            return new ImmutableList<T>(Node.NodeTreeFromList(ordereds, 0, ordereds.Count));
        }

        public T Find(Predicate<T> match)
        {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.Find(match);
        }

        public ImmutableList<T> FindAll(Predicate<T> match)
        {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.FindAll(match);
        }

        public int FindIndex(Predicate<T> match)
        {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.FindIndex(match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            Requires.NotNull<Predicate<T>>(match, "match");
            Requires.Range(startIndex >= 0, "startIndex", null);
            Requires.Range(startIndex <= this.Count, "startIndex", null);
            return this.root.FindIndex(startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            Requires.NotNull<Predicate<T>>(match, "match");
            Requires.Range(startIndex >= 0, "startIndex", null);
            Requires.Range(count >= 0, "count", null);
            Requires.Range((startIndex + count) <= this.Count, "count", null);
            return this.root.FindIndex(startIndex, count, match);
        }

        public T FindLast(Predicate<T> match)
        {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.FindLast(match);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.FindLastIndex(match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            Requires.NotNull<Predicate<T>>(match, "match");
            Requires.Range(startIndex >= 0, "startIndex", null);
            Requires.Range((startIndex == 0) || (startIndex < this.Count), "startIndex", null);
            return this.root.FindLastIndex(startIndex, match);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            Requires.NotNull<Predicate<T>>(match, "match");
            Requires.Range(startIndex >= 0, "startIndex", null);
            Requires.Range(count <= this.Count, "count", null);
            Requires.Range(((startIndex - count) + 1) >= 0, "startIndex", null);
            return this.root.FindLastIndex(startIndex, count, match);
        }

        public void ForEach(Action<T> action)
        {
            Requires.NotNull<Action<T>>(action, "action");
            foreach (T local in this)
            {
                action(local);
            }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this.root, null, -1, -1, false);
        }

        public ImmutableList<T> GetRange(int index, int count)
        {
            Requires.Range(index >= 0, "index", null);
            Requires.Range(count >= 0, "count", null);
            Requires.Range((index + count) <= this.Count, "count", null);
            return this.Wrap(Node.NodeTreeFromList(this, index, count));
        }

        public int IndexOf(T value)
        {
            return this.IndexOf<T>(value, ((IEqualityComparer<T>) EqualityComparer<T>.Default));
        }

        public int IndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer)
        {
            return this.root.IndexOf(item, index, count, equalityComparer);
        }

        public ImmutableList<T> Insert(int index, T item)
        {
            Requires.Range((index >= 0) && (index <= this.Count), "index", null);
            return this.Wrap(this.root.Insert(index, item));
        }

        public ImmutableList<T> InsertRange(int index, IEnumerable<T> items)
        {
            Requires.Range((index >= 0) && (index <= this.Count), "index", null);
            Requires.NotNull<IEnumerable<T>>(items, "items");
            Node root = this.root;
            foreach (T local in items)
            {
                root = root.Insert(index++, local);
            }
            return this.Wrap(root);
        }

        public int LastIndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer)
        {
            return this.root.LastIndexOf(item, index, count, equalityComparer);
        }

        public ImmutableList<T> Remove(T value)
        {
            return this.Remove(value, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public ImmutableList<T> Remove(T value, IEqualityComparer<T> equalityComparer)
        {
            int index = this.IndexOf<T>(value, equalityComparer);
            if (index >= 0)
            {
                return this.RemoveAt(index);
            }
            return (ImmutableList<T>) this;
        }

        public ImmutableList<T> RemoveAll(Predicate<T> match)
        {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.Wrap(this.root.RemoveAll(match));
        }

        public ImmutableList<T> RemoveAt(int index)
        {
            Requires.Range((index >= 0) && (index < this.Count), "index", null);
            Node root = this.root.RemoveAt(index);
            return this.Wrap(root);
        }

        public ImmutableList<T> RemoveRange(IEnumerable<T> items)
        {
            return this.RemoveRange(items, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public ImmutableList<T> RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer)
        {
            Requires.NotNull<IEnumerable<T>>(items, "items");
            Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
            if (this.IsEmpty)
            {
                return (ImmutableList<T>) this;
            }
            Node root = this.root;
            foreach (T local in items)
            {
                int index = root.IndexOf(local, equalityComparer);
                if (index >= 0)
                {
                    root = root.RemoveAt(index);
                }
            }
            return this.Wrap(root);
        }

        public ImmutableList<T> RemoveRange(int index, int count)
        {
            Requires.Range((index >= 0) && ((index < this.Count) || ((index == this.Count) && (count == 0))), "index", null);
            Requires.Range((count >= 0) && ((index + count) <= this.Count), "count", null);
            Node root = this.root;
            int num = count;
            while (num-- > 0)
            {
                root = root.RemoveAt(index);
            }
            return this.Wrap(root);
        }

        public ImmutableList<T> Replace(T oldValue, T newValue)
        {
            return this.Replace(oldValue, newValue, (IEqualityComparer<T>) EqualityComparer<T>.Default);
        }

        public ImmutableList<T> Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer)
        {
            Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
            int index = this.IndexOf<T>(oldValue, equalityComparer);
            if (index < 0)
            {
                throw new ArgumentException(Strings.CannotFindOldValue, "oldValue");
            }
            return this.SetItem(index, newValue);
        }

        public ImmutableList<T> Reverse()
        {
            return this.Wrap(this.root.Reverse());
        }

        public ImmutableList<T> Reverse(int index, int count)
        {
            return this.Wrap(this.root.Reverse(index, count));
        }

        public ImmutableList<T> SetItem(int index, T value)
        {
            return this.Wrap(this.root.ReplaceAt(index, value));
        }

        public ImmutableList<T> Sort()
        {
            return this.Wrap(this.root.Sort());
        }

        public ImmutableList<T> Sort(IComparer<T> comparer)
        {
            Requires.NotNull<IComparer<T>>(comparer, "comparer");
            return this.Wrap(this.root.Sort(comparer));
        }

        public ImmutableList<T> Sort(Comparison<T> comparison)
        {
            Requires.NotNull<Comparison<T>>(comparison, "comparison");
            return this.Wrap(this.root.Sort(comparison));
        }

        public ImmutableList<T> Sort(int index, int count, IComparer<T> comparer)
        {
            Requires.Range(index >= 0, "index", null);
            Requires.Range(count >= 0, "count", null);
            Requires.Range((index + count) <= this.Count, "count", null);
            Requires.NotNull<IComparer<T>>(comparer, "comparer");
            return this.Wrap(this.root.Sort(index, count, comparer));
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            this.root.CopyTo(array, arrayIndex);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        int IList.Add(object value)
        {
            throw new NotSupportedException();
        }

        void IList.Clear()
        {
            throw new NotSupportedException();
        }

        bool IList.Contains(object value)
        {
            return this.Contains((T) value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((T) value);
        }

        void IList.Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        void IList.Remove(object value)
        {
            throw new NotSupportedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        //[ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.Add(T value)
        {
            return this.Add(value);
        }

        //[ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.AddRange(IEnumerable<T> items)
        {
            return this.AddRange(items);
        }

        IImmutableList<T> IImmutableList<T>.Clear()
        {
            return this.Clear();
        }

        //[ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.Insert(int index, T item)
        {
            return this.Insert(index, item);
        }

        //[ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.InsertRange(int index, IEnumerable<T> items)
        {
            return this.InsertRange(index, items);
        }

        //[ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.Remove(T value, IEqualityComparer<T> equalityComparer)
        {
            return this.Remove(value, equalityComparer);
        }

        //[ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.RemoveAll(Predicate<T> match)
        {
            return this.RemoveAll(match);
        }

        //[ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.RemoveAt(int index)
        {
            return this.RemoveAt(index);
        }

        //[ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.RemoveRange(IEnumerable<T> items, IEqualityComparer<T> equalityComparer)
        {
            return this.RemoveRange(items, equalityComparer);
        }

        //[ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.RemoveRange(int index, int count)
        {
            return this.RemoveRange(index, count);
        }

        //[ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.Replace(T oldValue, T newValue, IEqualityComparer<T> equalityComparer)
        {
            return this.Replace(oldValue, newValue, equalityComparer);
        }

        //[ExcludeFromCodeCoverage]
        IImmutableList<T> IImmutableList<T>.SetItem(int index, T value)
        {
            return this.SetItem(index, value);
        }

        public Builder ToBuilder()
        {
            return new Builder((ImmutableList<T>) this);
        }

        public bool TrueForAll(Predicate<T> match)
        {
            Requires.NotNull<Predicate<T>>(match, "match");
            return this.root.TrueForAll(match);
        }

        private static bool TryCastToImmutableList(IEnumerable<T> sequence, out ImmutableList<T> other)
        {
            other = sequence as ImmutableList<T>;
            if (other != null)
            {
                return true;
            }
            Builder builder = sequence as Builder;
            if (builder != null)
            {
                other = builder.ToImmutable();
                return true;
            }
            return false;
        }

        private ImmutableList<T> Wrap(Node root)
        {
            if (root == this.root)
            {
                return (ImmutableList<T>) this;
            }
            if (!root.IsEmpty)
            {
                return new ImmutableList<T>(root);
            }
            return this.Clear();
        }

        private static ImmutableList<T> WrapNode(Node root)
        {
            if (!root.IsEmpty)
            {
                return new ImmutableList<T>(root);
            }
            return ImmutableList<T>.Empty;
        }

        public int Count
        {
            get
            {
                return this.root.Count;
            }
        }

        [DebuggerBrowsable((DebuggerBrowsableState) DebuggerBrowsableState.Never)]
        public bool IsEmpty
        {
            get
            {
                return this.root.IsEmpty;
            }
        }

        public T this[int index]
        {
            get
            {
                return this.root[index];
            }
        }

        bool ICollection<T>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        T IList<T>.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        [DebuggerBrowsable((DebuggerBrowsableState) DebuggerBrowsableState.Never)]
        bool ICollection.IsSynchronized
        {
            get
            {
                return true;
            }
        }

        [DebuggerBrowsable((DebuggerBrowsableState) DebuggerBrowsableState.Never)]
        object ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        bool IList.IsFixedSize
        {
            get
            {
                return true;
            }
        }

        bool IList.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        T IOrderedCollection<T>.this[int index]
        {
            get
            {
                return this[index];
            }
        }

        [DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableList<>.DebuggerProxy))]
        public sealed class Builder : IList<T>, ICollection<T>, IList, ICollection, IOrderedCollection<T>, IImmutableListQueries<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable
        {
            private ImmutableList<T> immutable;
            private ImmutableList<T>.Node root;
            private object syncRoot;
            private int version;

            internal Builder(ImmutableList<T> list)
            {
                this.root = ImmutableList<T>.Node.EmptyNode;
                Requires.NotNull<ImmutableList<T>>(list, "list");
                this.root = list.root;
                this.immutable = list;
            }

            public void Add(T item)
            {
                this.Root = this.Root.Add(item);
            }

            public int BinarySearch(T item)
            {
                return this.BinarySearch(item, null);
            }

            public int BinarySearch(T item, IComparer<T> comparer)
            {
                return this.BinarySearch(0, this.Count, item, comparer);
            }

            public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
            {
                return this.Root.BinarySearch(index, count, item, comparer);
            }

            public void Clear()
            {
                this.Root = ImmutableList<T>.Node.EmptyNode;
            }

            public bool Contains(T item)
            {
                return (this.IndexOf(item) >= 0);
            }

            public ImmutableList<TOutput> ConvertAll<TOutput>(Func<T, TOutput> converter)
            {
                Requires.NotNull<Func<T, TOutput>>(converter, "converter");
                return ImmutableList<TOutput>.WrapNode(this.root.ConvertAll<TOutput>(converter));
            }

            public void CopyTo(T[] array)
            {
                Requires.NotNull<T[]>(array, "array");
                Requires.Range(array.Length >= this.Count, "array", null);
                this.root.CopyTo(array);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                Requires.NotNull<T[]>(array, "array");
                Requires.Range(array.Length >= (arrayIndex + this.Count), "arrayIndex", null);
                this.root.CopyTo(array, arrayIndex);
            }

            public void CopyTo(int index, T[] array, int arrayIndex, int count)
            {
                this.root.CopyTo(index, array, arrayIndex, count);
            }

            public bool Exists(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.Exists(match);
            }

            public T Find(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.Find(match);
            }

            public ImmutableList<T> FindAll(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.FindAll(match);
            }

            public int FindIndex(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.FindIndex(match);
            }

            public int FindIndex(int startIndex, Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(startIndex <= this.Count, "startIndex", null);
                return this.root.FindIndex(startIndex, match);
            }

            public int FindIndex(int startIndex, int count, Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range((startIndex + count) <= this.Count, "count", null);
                return this.root.FindIndex(startIndex, count, match);
            }

            public T FindLast(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.FindLast(match);
            }

            public int FindLastIndex(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.FindLastIndex(match);
            }

            public int FindLastIndex(int startIndex, Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range((startIndex == 0) || (startIndex < this.Count), "startIndex", null);
                return this.root.FindLastIndex(startIndex, match);
            }

            public int FindLastIndex(int startIndex, int count, Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(count <= this.Count, "count", null);
                Requires.Range(((startIndex - count) + 1) >= 0, "startIndex", null);
                return this.root.FindLastIndex(startIndex, count, match);
            }

            public void ForEach(Action<T> action)
            {
                Requires.NotNull<Action<T>>(action, "action");
                foreach (T local in this)
                {
                    action(local);
                }
            }

            public ImmutableList<T>.Enumerator GetEnumerator()
            {
                return this.Root.GetEnumerator((ImmutableList<T>.Builder) this);
            }

            public ImmutableList<T> GetRange(int index, int count)
            {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range((index + count) <= this.Count, "count", null);
                return ImmutableList<T>.WrapNode(ImmutableList<T>.Node.NodeTreeFromList(this, index, count));
            }

            public int IndexOf(T item)
            {
                return this.Root.IndexOf(item, (IEqualityComparer<T>) EqualityComparer<T>.Default);
            }

            public int IndexOf(T item, int index)
            {
                return this.root.IndexOf(item, index, this.Count - index, (IEqualityComparer<T>) EqualityComparer<T>.Default);
            }

            public int IndexOf(T item, int index, int count)
            {
                return this.root.IndexOf(item, index, count, (IEqualityComparer<T>) EqualityComparer<T>.Default);
            }

            public int IndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer)
            {
                Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
                return this.root.IndexOf(item, index, count, equalityComparer);
            }

            public void Insert(int index, T item)
            {
                this.Root = this.Root.Insert(index, item);
            }

            public void InsertRange(int index, IEnumerable<T> items)
            {
                Requires.Range((index >= 0) && (index <= this.Count), "index", null);
                Requires.NotNull<IEnumerable<T>>(items, "items");
                foreach (T local in items)
                {
                    this.Root = this.Root.Insert(index++, local);
                }
            }

            public int LastIndexOf(T item)
            {
                if (this.Count == 0)
                {
                    return -1;
                }
                return this.root.LastIndexOf(item, this.Count - 1, this.Count, (IEqualityComparer<T>) EqualityComparer<T>.Default);
            }

            public int LastIndexOf(T item, int startIndex)
            {
                if ((this.Count == 0) && (startIndex == 0))
                {
                    return -1;
                }
                return this.root.LastIndexOf(item, startIndex, startIndex + 1, (IEqualityComparer<T>) EqualityComparer<T>.Default);
            }

            public int LastIndexOf(T item, int startIndex, int count)
            {
                return this.root.LastIndexOf(item, startIndex, count, (IEqualityComparer<T>) EqualityComparer<T>.Default);
            }

            public int LastIndexOf(T item, int startIndex, int count, IEqualityComparer<T> equalityComparer)
            {
                return this.root.LastIndexOf(item, startIndex, count, equalityComparer);
            }

            public bool Remove(T item)
            {
                int index = this.IndexOf(item);
                if (index < 0)
                {
                    return false;
                }
                this.Root = this.Root.RemoveAt(index);
                return true;
            }

            public int RemoveAll(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                int count = this.Count;
                this.Root = this.Root.RemoveAll(match);
                return (count - this.Count);
            }

            public void RemoveAt(int index)
            {
                this.Root = this.Root.RemoveAt(index);
            }

            public void Reverse()
            {
                this.Reverse(0, this.Count);
            }

            public void Reverse(int index, int count)
            {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range((index + count) <= this.Count, "count", null);
                this.Root = this.Root.Reverse(index, count);
            }

            public void Sort()
            {
                this.Root = this.Root.Sort();
            }

            public void Sort(IComparer<T> comparer)
            {
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                this.Root = this.Root.Sort(comparer);
            }

            public void Sort(Comparison<T> comparison)
            {
                Requires.NotNull<Comparison<T>>(comparison, "comparison");
                this.Root = this.Root.Sort(comparison);
            }

            public void Sort(int index, int count, IComparer<T> comparer)
            {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range((index + count) <= this.Count, "count", null);
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                this.Root = this.Root.Sort(index, count, comparer);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            void ICollection.CopyTo(Array array, int arrayIndex)
            {
                this.Root.CopyTo(array, arrayIndex);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            int IList.Add(object value)
            {
                this.Add((T) value);
                return (this.Count - 1);
            }

            void IList.Clear()
            {
                this.Clear();
            }

            bool IList.Contains(object value)
            {
                return this.Contains((T) value);
            }

            int IList.IndexOf(object value)
            {
                return this.IndexOf((T) value);
            }

            void IList.Insert(int index, object value)
            {
                this.Insert(index, (T) value);
            }

            void IList.Remove(object value)
            {
                this.Remove((T) value);
            }

            public ImmutableList<T> ToImmutable()
            {
                if (this.immutable == null)
                {
                    this.immutable = ImmutableList<T>.WrapNode(this.Root);
                }
                return this.immutable;
            }

            public bool TrueForAll(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.root.TrueForAll(match);
            }

            public int Count
            {
                get
                {
                    return this.Root.Count;
                }
            }

            public T this[int index]
            {
                get
                {
                    return this.Root[index];
                }
                set
                {
                    this.Root = this.Root.ReplaceAt(index, value);
                }
            }

            internal ImmutableList<T>.Node Root
            {
                get
                {
                    return this.root;
                }
                private set
                {
                    this.version++;
                    if (this.root != value)
                    {
                        this.root = value;
                        this.immutable = null;
                    }
                }
            }

            bool ICollection<T>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            [DebuggerBrowsable((DebuggerBrowsableState) DebuggerBrowsableState.Never)]
            bool ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            [DebuggerBrowsable((DebuggerBrowsableState) DebuggerBrowsableState.Never)]
            object ICollection.SyncRoot
            {
                get
                {
                    if (this.syncRoot == null)
                    {
                        Interlocked.CompareExchange<object>(ref this.syncRoot, new object(), null);
                    }
                    return this.syncRoot;
                }
            }

            bool IList.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            bool IList.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            object IList.this[int index]
            {
                get
                {
                    return this[index];
                }
                set
                {
                    this[index] = (T) value;
                }
            }

            T IOrderedCollection<T>.this[int index]
            {
                get
                {
                    return this[index];
                }
            }

            internal int Version
            {
                get
                {
                    return this.version;
                }
            }
        }

        //[ExcludeFromCodeCoverage]
        private class DebuggerProxy
        {
            private T[] cachedContents;
            private readonly ImmutableList<T>.Node list;

            public DebuggerProxy(ImmutableList<T> list)
            {
                Requires.NotNull<ImmutableList<T>>(list, "list");
                this.list = list.root;
            }

            public DebuggerProxy(ImmutableList<T>.Builder builder)
            {
                Requires.NotNull<ImmutableList<T>.Builder>(builder, "builder");
                this.list = builder.Root;
            }

            [DebuggerBrowsable((DebuggerBrowsableState) DebuggerBrowsableState.RootHidden)]
            public T[] Contents
            {
                get
                {
                    if (this.cachedContents == null)
                    {
                        this.cachedContents = this.list.ToArray<T>(this.list.Count);
                    }
                    return this.cachedContents;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Advanced)]
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable, ISecurePooledObjectUser
        {
            private static readonly SecureObjectPool<Stack<RefAsValueType<IBinaryTree<T>>>, ImmutableList<T>.Enumerator> EnumeratingStacks;
            private readonly ImmutableList<T>.Builder builder;
            private readonly Guid poolUserId;
            private readonly int startIndex;
            private readonly int count;
            private int remainingCount;
            private bool reversed;
            private IBinaryTree<T> root;
            private SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>> stack;
            private IBinaryTree<T> current;
            private int enumeratingBuilderVersion;
            internal Enumerator(IBinaryTree<T> root, ImmutableList<T>.Builder builder = null, Int32 startIndex = -1, Int32 count = -1, Boolean reversed = false)
            {
                Int32 num;
                Requires.NotNull<IBinaryTree<T>>(root, "root");
                Requires.Range(startIndex >= -1, "startIndex", null);
                Requires.Range(count >= -1, "count", null);
                Requires.Argument((reversed || count == -1 ? true : (startIndex == -1 ? 0 : startIndex) + count <= root.Count));
                Requires.Argument((!reversed || count == -1 ? true : (startIndex == -1 ? root.Count - 1 : startIndex) - count + 1 >= 0));
                this.root = root;
                this.builder = builder;
                this.current = null;
                if (startIndex >= 0)
                {
                    num = startIndex;
                }
                else
                {
                    num = (reversed ? root.Count - 1 : 0);
                }
                this.startIndex = num;
                this.count = (count == -1 ? root.Count : count);
                this.remainingCount = this.count;
                this.reversed = reversed;
                this.enumeratingBuilderVersion = (builder != null ? builder.Version : -1);
                this.poolUserId = Guid.NewGuid();
                if (this.count <= 0)
                {
                    this.stack = null;
                }
                else
                {
                    this.stack = null;
                    if (!ImmutableList<T>.Enumerator.EnumeratingStacks.TryTake(this, out this.stack))
                    {
                        this.stack = ImmutableList<T>.Enumerator.EnumeratingStacks.PrepNew(this, new Stack<RefAsValueType<IBinaryTree<T>>>(root.Height));
                    }
                }
                this.Reset();
            }

            Guid ISecurePooledObjectUser.PoolUserId
            {
                get
                {
                    return this.poolUserId;
                }
            }
            public T Current
            {
                get
                {
                    this.ThrowIfDisposed();
                    if (this.current == null)
                    {
                        throw new InvalidOperationException();
                    }
                    return this.current.Value;
                }
            }
            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
            public void Dispose()
            {
                this.root = null;
                this.current = null;
                if (this.stack != null && this.stack.Owner == this.poolUserId)
                {
                    SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableList<T>.Enumerator>(this);
                    try
                    {
                        securePooledObjectUser.Value.Clear();
                    }
                    finally
                    {
                        ((IDisposable)securePooledObjectUser).Dispose();
                    }
                    ImmutableList<T>.Enumerator.EnumeratingStacks.TryAdd(this, this.stack);
                }
                this.stack = null;
            }

            public Boolean MoveNext()
            {
                Boolean flag;
                this.ThrowIfDisposed();
                this.ThrowIfChanged();
                if (this.stack != null)
                {
                    SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableList<T>.Enumerator>(this);
                    try
                    {
                        if (this.remainingCount <= 0 || securePooledObjectUser.Value.Count <= 0)
                        {
                            this.current = null;
                            return false;
                        }
                        else
                        {
                            IBinaryTree<T> value = securePooledObjectUser.Value.Pop().Value;
                            this.current = value;
                            this.PushNext(this.NextBranch(value));
                            ImmutableList<T>.Enumerator enumerator = this;
                            enumerator.remainingCount = enumerator.remainingCount - 1;
                            flag = true;
                        }
                    }
                    finally
                    {
                        ((IDisposable)securePooledObjectUser).Dispose();
                    }
                    return flag;
                }
                this.current = null;
                return false;
            }

            public void Reset()
            {
                this.ThrowIfDisposed();
                this.enumeratingBuilderVersion = (this.builder != null ? this.builder.Version : -1);
                this.remainingCount = this.count;
                if (this.stack != null)
                {
                    SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableList<T>.Enumerator>(this);
                    try
                    {
                        securePooledObjectUser.Value.Clear();
                        IBinaryTree<T> binaryTree = this.root;
                        Int32 count = (this.reversed ? this.root.Count - this.startIndex - 1 : this.startIndex);
                        while (!binaryTree.IsEmpty && count != this.PreviousBranch(binaryTree).Count)
                        {
                            if (count >= this.PreviousBranch(binaryTree).Count)
                            {
                                count = count - (this.PreviousBranch(binaryTree).Count + 1);
                                binaryTree = this.NextBranch(binaryTree);
                            }
                            else
                            {
                                securePooledObjectUser.Value.Push(new RefAsValueType<IBinaryTree<T>>(binaryTree));
                                binaryTree = this.PreviousBranch(binaryTree);
                            }
                        }
                        if (!binaryTree.IsEmpty)
                        {
                            securePooledObjectUser.Value.Push(new RefAsValueType<IBinaryTree<T>>(binaryTree));
                        }
                    }
                    finally
                    {
                        ((IDisposable)securePooledObjectUser).Dispose();
                    }
                }
            }

            private IBinaryTree<T> NextBranch(IBinaryTree<T> node)
            {
                if (!this.reversed)
                {
                    return node.Right;
                }
                return node.Left;
            }

            private IBinaryTree<T> PreviousBranch(IBinaryTree<T> node)
            {
                if (!this.reversed)
                {
                    return node.Left;
                }
                return node.Right;
            }

            private void ThrowIfDisposed()
            {
                if (this.root == null)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this.stack != null)
                {
                    this.stack.ThrowDisposedIfNotOwned<ImmutableList<T>.Enumerator>(this);
                }
            }

            private void ThrowIfChanged()
            {
                if ((this.builder != null) && (this.builder.Version != this.enumeratingBuilderVersion))
                {
                    throw new InvalidOperationException(Strings.CollectionModifiedDuringEnumeration);
                }
            }

            private void PushNext(IBinaryTree<T> node)
            {
                Requires.NotNull<IBinaryTree<T>>(node, "node");
                if (!node.IsEmpty)
                {
                    SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableList<T>.Enumerator>(this);
                    try
                    {
                        while (!node.IsEmpty)
                        {
                            securePooledObjectUser.Value.Push(new RefAsValueType<IBinaryTree<T>>(node));
                            node = this.PreviousBranch(node);
                        }
                    }
                    finally
                    {
                        ((IDisposable)securePooledObjectUser).Dispose();
                    }
                }
            }

            static Enumerator()
            {
                ImmutableList<T>.Enumerator.EnumeratingStacks = new SecureObjectPool<Stack<RefAsValueType<IBinaryTree<T>>>, ImmutableList<T>.Enumerator>();
            }
        }

        [DebuggerDisplay("{key}")]
        internal sealed class Node : IBinaryTree<T>, IEnumerable<T>, IEnumerable
        {
            private int count;
            internal static readonly ImmutableList<T>.Node EmptyNode;
            private bool frozen;
            private int height;
            private T key;
            private ImmutableList<T>.Node left;
            private ImmutableList<T>.Node right;

            static Node()
            {
                ImmutableList<T>.Node.EmptyNode = new ImmutableList<T>.Node();
            }

            private Node()
            {
                this.frozen = true;
            }

            private Node(T key, ImmutableList<T>.Node left, ImmutableList<T>.Node right, bool frozen = false)
            {
                Requires.NotNull<ImmutableList<T>.Node>(left, "left");
                Requires.NotNull<ImmutableList<T>.Node>(right, "right");
                this.key = key;
                this.left = left;
                this.right = right;
                this.height = 1 + Math.Max(left.height, right.height);
                this.count = (1 + left.count) + right.count;
                this.frozen = frozen;
            }

            internal ImmutableList<T>.Node Add(T key)
            {
                return this.Insert(this.count, key);
            }

            private static int Balance(ImmutableList<T>.Node tree)
            {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                return (tree.right.height - tree.left.height);
            }

            internal int BinarySearch(int index, int count, T item, IComparer<T> comparer)
            {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                comparer = (IComparer<T>) (comparer ?? Comparer<T>.Default);
                if (this.IsEmpty || (count <= 0))
                {
                    return ~index;
                }
                int num = this.left.Count;
                if ((index + count) > num)
                {
                    if (index > num)
                    {
                        int num2 = this.right.BinarySearch((index - num) - 1, count, item, comparer);
                        int num3 = num + 1;
                        if (num2 >= 0)
                        {
                            return (num2 + num3);
                        }
                        return (num2 - num3);
                    }
                    int num4 = comparer.Compare(item, this.key);
                    if (num4 == 0)
                    {
                        return num;
                    }
                    if (num4 > 0)
                    {
                        int num5 = (count - (num - index)) - 1;
                        int num6 = (num5 < 0) ? -1 : this.right.BinarySearch(0, num5, item, comparer);
                        int num7 = num + 1;
                        if (num6 >= 0)
                        {
                            return (num6 + num7);
                        }
                        return (num6 - num7);
                    }
                    if (index == num)
                    {
                        return ~index;
                    }
                }
                return this.left.BinarySearch(index, count, item, comparer);
            }

            internal ImmutableList<TOutput>.Node ConvertAll<TOutput>(Func<T, TOutput> converter)
            {
                ImmutableList<TOutput>.Node emptyNode = ImmutableList<TOutput>.Node.EmptyNode;
                foreach (T local in this)
                {
                    emptyNode = emptyNode.Add(converter(local));
                }
                return emptyNode;
            }

            internal void CopyTo(T[] array)
            {
                Requires.NotNull<T[]>(array, "array");
                Requires.Argument(array.Length >= this.Count);
                int num = 0;
                foreach (T local in this)
                {
                    array[num++] = local;
                }
            }

            internal void CopyTo(T[] array, int arrayIndex)
            {
                Requires.NotNull<T[]>(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex", null);
                Requires.Range(arrayIndex <= array.Length, "arrayIndex", null);
                Requires.Argument((arrayIndex + this.Count) <= array.Length);
                foreach (T local in this)
                {
                    array[arrayIndex++] = local;
                }
            }

            internal void CopyTo(Array array, int arrayIndex)
            {
                Requires.NotNull<Array>(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex", null);
                Requires.Range(array.Length >= (arrayIndex + this.Count), "arrayIndex", null);
                foreach (T local in this)
                {
                    array.SetValue(local, new int[] { arrayIndex++ });
                }
            }

            internal void CopyTo(int index, T[] array, int arrayIndex, int count)
            {
                Requires.NotNull<T[]>(array, "array");
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range((index + count) <= this.Count, "count", null);
                Requires.Range(arrayIndex >= 0, "arrayIndex", null);
                Requires.Range((arrayIndex + count) <= array.Length, "arrayIndex", null);
                using (ImmutableList<T>.Enumerator enumerator = new ImmutableList<T>.Enumerator(this, null, index, count, false))
                {
                    while (enumerator.MoveNext())
                    {
                        array[arrayIndex++] = enumerator.Current;
                    }
                }
            }

            private static ImmutableList<T>.Node DoubleLeft(ImmutableList<T>.Node tree)
            {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                if (tree.right.IsEmpty)
                {
                    return tree;
                }
                return ImmutableList<T>.Node.RotateLeft(tree.Mutate(null, ImmutableList<T>.Node.RotateRight(tree.right)));
            }

            private static ImmutableList<T>.Node DoubleRight(ImmutableList<T>.Node tree)
            {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                if (tree.left.IsEmpty)
                {
                    return tree;
                }
                return ImmutableList<T>.Node.RotateRight(tree.Mutate(ImmutableList<T>.Node.RotateLeft(tree.left), null));
            }

            internal bool Exists(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                foreach (T local in this)
                {
                    if (match(local))
                    {
                        return true;
                    }
                }
                return false;
            }

            internal T Find(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                foreach (T local in this)
                {
                    if (match(local))
                    {
                        return local;
                    }
                }
                return default(T);
            }

            internal ImmutableList<T> FindAll(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                ImmutableList<T>.Builder builder = ImmutableList<T>.Empty.ToBuilder();
                foreach (T local in this)
                {
                    if (match(local))
                    {
                        builder.Add(local);
                    }
                }
                return builder.ToImmutable();
            }

            internal int FindIndex(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.FindIndex(0, this.count, match);
            }

            internal int FindIndex(int startIndex, Predicate<T> match)
            {
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(startIndex <= this.Count, "startIndex", null);
                Requires.NotNull<Predicate<T>>(match, "match");
                return this.FindIndex(startIndex, this.Count - startIndex, match);
            }

            internal int FindIndex(int startIndex, int count, Predicate<T> match)
            {
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Argument((startIndex + count) <= this.Count);
                Requires.NotNull<Predicate<T>>(match, "match");
                using (ImmutableList<T>.Enumerator enumerator = new ImmutableList<T>.Enumerator(this, null, startIndex, count, false))
                {
                    for (int i = startIndex; enumerator.MoveNext(); i++)
                    {
                        if (match(enumerator.Current))
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }

            internal T FindLast(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                using (ImmutableList<T>.Enumerator enumerator = new ImmutableList<T>.Enumerator(this, null, -1, -1, true))
                {
                    while (enumerator.MoveNext())
                    {
                        if (match(enumerator.Current))
                        {
                            return enumerator.Current;
                        }
                    }
                }
                return default(T);
            }

            internal int FindLastIndex(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                if (this.IsEmpty)
                {
                    return -1;
                }
                return this.FindLastIndex(this.Count - 1, this.Count, match);
            }

            internal int FindLastIndex(int startIndex, Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range((startIndex == 0) || (startIndex < this.Count), "startIndex", null);
                if (this.IsEmpty)
                {
                    return -1;
                }
                return this.FindLastIndex(startIndex, startIndex + 1, match);
            }

            internal int FindLastIndex(int startIndex, int count, Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                Requires.Range(startIndex >= 0, "startIndex", null);
                Requires.Range(count <= this.Count, "count", null);
                Requires.Argument(((startIndex - count) + 1) >= 0);
                using (ImmutableList<T>.Enumerator enumerator = new ImmutableList<T>.Enumerator(this, null, startIndex, count, true))
                {
                    for (int i = startIndex; enumerator.MoveNext(); i--)
                    {
                        if (match(enumerator.Current))
                        {
                            return i;
                        }
                    }
                }
                return -1;
            }

            internal void Freeze()
            {
                if (!this.frozen)
                {
                    this.left.Freeze();
                    this.right.Freeze();
                    this.frozen = true;
                }
            }

            public ImmutableList<T>.Enumerator GetEnumerator()
            {
                return new ImmutableList<T>.Enumerator(this, null, -1, -1, false);
            }

            internal ImmutableList<T>.Enumerator GetEnumerator(ImmutableList<T>.Builder builder)
            {
                return new ImmutableList<T>.Enumerator(this, builder, -1, -1, false);
            }

            internal int IndexOf(T item, IEqualityComparer<T> equalityComparer)
            {
                return this.IndexOf(item, 0, this.Count, equalityComparer);
            }

            internal int IndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer)
            {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range(count <= this.Count, "count", null);
                Requires.Range((index + count) <= this.Count, "count", null);
                Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
                using (ImmutableList<T>.Enumerator enumerator = new ImmutableList<T>.Enumerator(this, null, index, count, false))
                {
                    while (enumerator.MoveNext())
                    {
                        if (equalityComparer.Equals(item, enumerator.Current))
                        {
                            return index;
                        }
                        index++;
                    }
                }
                return -1;
            }

            internal ImmutableList<T>.Node Insert(int index, T key)
            {
                ImmutableList<T>.Node node;
                Requires.Range((index >= 0) && (index <= this.Count), "index", null);
                if (this.IsEmpty)
                {
                    return new ImmutableList<T>.Node(key, (ImmutableList<T>.Node) this, (ImmutableList<T>.Node) this, false);
                }
                if (index <= this.left.count)
                {
                    ImmutableList<T>.Node left = this.left.Insert(index, key);
                    node = this.Mutate(left, null);
                }
                else
                {
                    ImmutableList<T>.Node right = this.right.Insert((index - this.left.count) - 1, key);
                    node = this.Mutate(null, right);
                }
                return ImmutableList<T>.Node.MakeBalanced(node);
            }

            private static bool IsLeftHeavy(ImmutableList<T>.Node tree)
            {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                return (ImmutableList<T>.Node.Balance(tree) <= -2);
            }

            private static bool IsRightHeavy(ImmutableList<T>.Node tree)
            {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                return (ImmutableList<T>.Node.Balance(tree) >= 2);
            }

            internal int LastIndexOf(T item, int index, int count, IEqualityComparer<T> equalityComparer)
            {
                Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "ValueComparer");
                Requires.Range(index >= 0, "index", null);
                Requires.Range((count >= 0) && (count <= this.Count), "count", null);
                Requires.Argument(((index - count) + 1) >= 0);
                using (ImmutableList<T>.Enumerator enumerator = new ImmutableList<T>.Enumerator(this, null, index, count, true))
                {
                    while (enumerator.MoveNext())
                    {
                        if (equalityComparer.Equals(item, enumerator.Current))
                        {
                            return index;
                        }
                        index--;
                    }
                }
                return -1;
            }

            private static ImmutableList<T>.Node MakeBalanced(ImmutableList<T>.Node tree)
            {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                if (ImmutableList<T>.Node.IsRightHeavy(tree))
                {
                    if (!ImmutableList<T>.Node.IsLeftHeavy(tree.right))
                    {
                        return ImmutableList<T>.Node.RotateLeft(tree);
                    }
                    return ImmutableList<T>.Node.DoubleLeft(tree);
                }
                if (!ImmutableList<T>.Node.IsLeftHeavy(tree))
                {
                    return tree;
                }
                if (!ImmutableList<T>.Node.IsRightHeavy(tree.left))
                {
                    return ImmutableList<T>.Node.RotateRight(tree);
                }
                return ImmutableList<T>.Node.DoubleRight(tree);
            }

            private ImmutableList<T>.Node Mutate(T value)
            {
                if (this.frozen)
                {
                    return new ImmutableList<T>.Node(value, this.left, this.right, false);
                }
                this.key = value;
                return (ImmutableList<T>.Node) this;
            }

            private ImmutableList<T>.Node Mutate(ImmutableList<T>.Node left = null, ImmutableList<T>.Node right = null)
            {
                if (this.frozen)
                {
                    return new ImmutableList<T>.Node(this.key, left ?? this.left, right ?? this.right, false);
                }
                if (left != null)
                {
                    this.left = left;
                }
                if (right != null)
                {
                    this.right = right;
                }
                this.height = 1 + Math.Max(this.left.height, this.right.height);
                this.count = (1 + this.left.count) + this.right.count;
                return (ImmutableList<T>.Node) this;
            }

            internal static ImmutableList<T>.Node NodeTreeFromList(IOrderedCollection<T> items, int start, int length)
            {
                Requires.NotNull<IOrderedCollection<T>>(items, "items");
                Requires.Range(start >= 0, "start", null);
                Requires.Range(length >= 0, "length", null);
                if (length == 0)
                {
                    return ImmutableList<T>.Node.EmptyNode;
                }
                int num = (length - 1) / 2;
                int num2 = (length - 1) - num;
                ImmutableList<T>.Node left = ImmutableList<T>.Node.NodeTreeFromList(items, start, num2);
                return new ImmutableList<T>.Node(items[start + num2], left, ImmutableList<T>.Node.NodeTreeFromList(items, (start + num2) + 1, num), true);
            }

            internal ImmutableList<T>.Node RemoveAll(Predicate<T> match)
            {
                Requires.NotNull<Predicate<T>>(match, "match");
                ImmutableList<T>.Node ts = this;
                Int32 num = 0;
                foreach (T t in this)
                {
                    if (!match(t))
                    {
                        num++;
                    }
                    else
                    {
                        ts = ts.RemoveAt(num);
                    }
                }
                return ts;
            }

            internal ImmutableList<T>.Node RemoveAt(Int32 index)
            {
                Requires.Range((index < 0 ? false : index < this.Count), "index", null);
                ImmutableList<T>.Node emptyNode = this;
                if (index != this.left.count)
                {
                    if (index >= this.left.count)
                    {
                        ImmutableList<T>.Node ts = this.right.RemoveAt(index - this.left.count - 1);
                        emptyNode = this.Mutate(null, ts);
                    }
                    else
                    {
                        ImmutableList<T>.Node ts1 = this.left.RemoveAt(index);
                        emptyNode = this.Mutate(ts1, null);
                    }
                }
                else if (this.right.IsEmpty && this.left.IsEmpty)
                {
                    emptyNode = ImmutableList<T>.Node.EmptyNode;
                }
                else if (this.right.IsEmpty && !this.left.IsEmpty)
                {
                    emptyNode = this.left;
                }
                else if (this.right.IsEmpty || !this.left.IsEmpty)
                {
                    ImmutableList<T>.Node ts2 = this.right;
                    while (!ts2.left.IsEmpty)
                    {
                        ts2 = ts2.left;
                    }
                    ImmutableList<T>.Node ts3 = this.right.RemoveAt(0);
                    emptyNode = ts2.Mutate(this.left, ts3);
                }
                else
                {
                    emptyNode = this.right;
                }
                if (emptyNode.IsEmpty)
                {
                    return emptyNode;
                }
                return ImmutableList<T>.Node.MakeBalanced(emptyNode);
            }
            internal ImmutableList<T>.Node ReplaceAt(int index, T value)
            {
                Requires.Range((index >= 0) && (index < this.Count), "index", null);
                if (index == this.left.count)
                {
                    return this.Mutate(value);
                }
                if (index < this.left.count)
                {
                    ImmutableList<T>.Node left = this.left.ReplaceAt(index, value);
                    return this.Mutate(left, null);
                }
                ImmutableList<T>.Node right = this.right.ReplaceAt((index - this.left.count) - 1, value);
                return this.Mutate(null, right);
            }

            internal ImmutableList<T>.Node Reverse()
            {
                return this.Reverse(0, this.Count);
            }

            internal ImmutableList<T>.Node Reverse(Int32 index, Int32 count)
            {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Range(index + count <= this.Count, "index", null);
                ImmutableList<T>.Node ts = this;
                Int32 num = index;
                for (Int32 i = index + count - 1; num < i; i--)
                {
                    T item = ts[num];
                    T t = ts[i];
                    ts = ts.ReplaceAt(i, item).ReplaceAt(num, t);
                    num++;
                }
                return ts;
            }

            private static ImmutableList<T>.Node RotateLeft(ImmutableList<T>.Node tree)
            {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                if (tree.right.IsEmpty)
                {
                    return tree;
                }
                ImmutableList<T>.Node right = tree.right;
                return right.Mutate(tree.Mutate(null, right.left), null);
            }

            private static ImmutableList<T>.Node RotateRight(ImmutableList<T>.Node tree)
            {
                Requires.NotNull<ImmutableList<T>.Node>(tree, "tree");
                if (tree.left.IsEmpty)
                {
                    return tree;
                }
                ImmutableList<T>.Node left = tree.left;
                return left.Mutate(null, tree.Mutate(left.right, null));
            }

            internal ImmutableList<T>.Node Sort()
            {
                return this.Sort((IComparer<T>) Comparer<T>.Default);
            }

            internal ImmutableList<T>.Node Sort(IComparer<T> comparer)
            {
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                return this.Sort(0, this.Count, comparer);
            }

            internal ImmutableList<T>.Node Sort(Comparison<T> comparison)
            {
                Requires.NotNull<Comparison<T>>(comparison, "comparison");
                T[] array = new T[this.Count];
                this.CopyTo(array);
                Array.Sort<T>(array, comparison);
                return ImmutableList<T>.Node.NodeTreeFromList(array.AsOrderedCollection<T>(), 0, this.Count);
            }

            internal ImmutableList<T>.Node Sort(int index, int count, IComparer<T> comparer)
            {
                Requires.Range(index >= 0, "index", null);
                Requires.Range(count >= 0, "count", null);
                Requires.Argument((index + count) <= this.Count);
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                T[] array = new T[this.Count];
                this.CopyTo(array);
                Array.Sort<T>(array, index, count, comparer);
                return ImmutableList<T>.Node.NodeTreeFromList(array.AsOrderedCollection<T>(), 0, this.Count);
            }

            //[ExcludeFromCodeCoverage]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            //[ExcludeFromCodeCoverage]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            internal bool TrueForAll(Predicate<T> match)
            {
                foreach (T local in this)
                {
                    if (!match(local))
                    {
                        return false;
                    }
                }
                return true;
            }

            public int Count
            {
                get
                {
                    return this.count;
                }
            }

            public bool IsEmpty
            {
                get
                {
                    return (this.left == null);
                }
            }

            internal T this[int index]
            {
                get
                {
                    Requires.Range((index >= 0) && (index < this.Count), "index", null);
                    if (index < this.left.count)
                    {
                        return this.left[index];
                    }
                    if (index > this.left.count)
                    {
                        return this.right[(index - this.left.count) - 1];
                    }
                    return this.key;
                }
            }

            internal T Key
            {
                get
                {
                    return this.key;
                }
            }

            int IBinaryTree<T>.Height
            {
                get
                {
                    return this.height;
                }
            }

            IBinaryTree<T> IBinaryTree<T>.Left
            {
                get
                {
                    return this.left;
                }
            }

            IBinaryTree<T> IBinaryTree<T>.Right
            {
                get
                {
                    return this.right;
                }
            }

            T IBinaryTree<T>.Value
            {
                get
                {
                    return this.key;
                }
            }
        }
    }
}

