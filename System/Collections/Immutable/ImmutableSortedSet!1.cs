using System.Collections.Generic.V40;

namespace System.Collections.Immutable
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Validation;

    [DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableSortedSet<>.DebuggerProxy))]
    public sealed class ImmutableSortedSet<T> : IImmutableSet<T>, ISortKeyCollection<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IList<T>, ISetV20<T>, ICollection<T>, IEnumerable<T>, IList, ICollection, IEnumerable
    {
        private readonly IComparer<T> comparer;
        public static readonly ImmutableSortedSet<T> Empty;
        private const float RefillOverIncrementalThreshold = 0.15f;
        private readonly Node root;

        static ImmutableSortedSet()
        {
            ImmutableSortedSet<T>.Empty = new ImmutableSortedSet<T>(null);
        }

        internal ImmutableSortedSet(IComparer<T> comparer = null)
        {
            this.root = Node.EmptyNode;
            this.comparer = (IComparer<T>) (comparer ?? Comparer<T>.Default);
        }

        private ImmutableSortedSet(Node root, IComparer<T> comparer)
        {
            Requires.NotNull<Node>(root, "root");
            Requires.NotNull<IComparer<T>>(comparer, "comparer");
            root.Freeze();
            this.root = root;
            this.comparer = comparer;
        }

        public ImmutableSortedSet<T> Add(T value)
        {
            bool flag;
            Requires.NotNullAllowStructs<T>(value, "value");
            return this.Wrap(this.root.Add(value, this.comparer, out flag));
        }

        public ImmutableSortedSet<T> Clear()
        {
            if (!this.root.IsEmpty)
            {
                return ImmutableSortedSet<T>.Empty.WithComparer(this.comparer);
            }
            return (ImmutableSortedSet<T>) this;
        }

        public bool Contains(T value)
        {
            Requires.NotNullAllowStructs<T>(value, "value");
            return this.root.Contains(value, this.comparer);
        }

        public ImmutableSortedSet<T> Except(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            Node root = this.root;
            foreach (T local in other)
            {
                bool flag;
                root = root.Remove(local, this.comparer, out flag);
            }
            return this.Wrap(root);
        }

        public Enumerator GetEnumerator()
        {
            return this.root.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            Requires.NotNullAllowStructs<T>(item, "item");
            return this.root.IndexOf(item, this.comparer);
        }

        public ImmutableSortedSet<T> Intersect(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            ImmutableSortedSet<T> set = this.Clear();
            foreach (T local in other)
            {
                if (this.Contains(local))
                {
                    set = set.Add(local);
                }
            }
            return set;
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            if (this.IsEmpty)
            {
                return EnumerableV20.Any<T>(other);
            }
            SortedSetV20<T> set = new SortedSetV20<T>(other, this.KeyComparer);
            if (this.Count < set.Count)
            {
                int num = 0;
                bool flag = false;
                foreach (T local in set)
                {
                    if (this.Contains(local))
                    {
                        num++;
                    }
                    else
                    {
                        flag = true;
                    }
                    if ((num == this.Count) && flag)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            if (this.IsEmpty)
            {
                return false;
            }
            int num = 0;
            foreach (T local in other)
            {
                num++;
                if (!this.Contains(local))
                {
                    return false;
                }
            }
            return (this.Count > num);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            if (this.IsEmpty)
            {
                return true;
            }
            SortedSetV20<T> set = new SortedSetV20<T>(other, this.KeyComparer);
            int num = 0;
            foreach (T local in set)
            {
                if (this.Contains(local))
                {
                    num++;
                }
            }
            return (num == this.Count);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            foreach (T local in other)
            {
                if (!this.Contains(local))
                {
                    return false;
                }
            }
            return true;
        }

        private ImmutableSortedSet<T> LeafToRootRefill(IEnumerable<T> addedItems)
        {
            Requires.NotNull<IEnumerable<T>>(addedItems, "addedItems");
            SortedSetV20<T> collection = new SortedSetV20<T>(EnumerableV20.Concat<T>(this, addedItems), this.KeyComparer);
            Node root = Node.NodeTreeFromSortedSet(collection);
            return this.Wrap(root);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            if (!this.IsEmpty)
            {
                foreach (T local in other)
                {
                    if (this.Contains(local))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public ImmutableSortedSet<T> Remove(T value)
        {
            bool flag;
            Requires.NotNullAllowStructs<T>(value, "value");
            return this.Wrap(this.root.Remove(value, this.comparer, out flag));
        }

        public IEnumerable<T> Reverse()
        {
            return new ImmutableSortedSet<T>.ReverseEnumerable(this.root);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            SortedSetV20<T> set = new SortedSetV20<T>(other, this.KeyComparer);
            if (this.Count != set.Count)
            {
                return false;
            }
            int num = 0;
            foreach (T local in set)
            {
                if (!this.Contains(local))
                {
                    return false;
                }
                num++;
            }
            return (num == this.Count);
        }

        public ImmutableSortedSet<T> SymmetricExcept(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            ImmutableSortedSet<T> set = ImmutableSortedSet<T>.Empty.Union(other);
            ImmutableSortedSet<T> set2 = this.Clear();
            foreach (T local in this)
            {
                if (!set.Contains(local))
                {
                    set2 = set2.Add(local);
                }
            }
            foreach (T local2 in set)
            {
                if (!this.Contains(local2))
                {
                    set2 = set2.Add(local2);
                }
            }
            return set2;
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            this.root.CopyTo(array, arrayIndex);
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

        bool ISetV20<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void ISetV20<T>.ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void ISetV20<T>.IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void ISetV20<T>.SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void ISetV20<T>.UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.root.CopyTo(array, index);
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

        IImmutableSet<T> IImmutableSet<T>.Add(T value)
        {
            return this.Add(value);
        }

        IImmutableSet<T> IImmutableSet<T>.Clear()
        {
            return this.Clear();
        }

        IImmutableSet<T> IImmutableSet<T>.Except(IEnumerable<T> other)
        {
            return this.Except(other);
        }

        IImmutableSet<T> IImmutableSet<T>.Intersect(IEnumerable<T> other)
        {
            return this.Intersect(other);
        }

        IImmutableSet<T> IImmutableSet<T>.Remove(T value)
        {
            return this.Remove(value);
        }

        IImmutableSet<T> IImmutableSet<T>.SymmetricExcept(IEnumerable<T> other)
        {
            return this.SymmetricExcept(other);
        }

        IImmutableSet<T> IImmutableSet<T>.Union(IEnumerable<T> other)
        {
            return this.Union(other);
        }

        public Builder ToBuilder()
        {
            return new Builder((ImmutableSortedSet<T>) this);
        }

        private static bool TryCastToImmutableSortedSet(IEnumerable<T> sequence, out ImmutableSortedSet<T> other)
        {
            other = sequence as ImmutableSortedSet<T>;
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

        public bool TryGetValue(T equalValue, T actualValue)
        {
            Requires.NotNullAllowStructs<T>(equalValue, "equalValue");
            Node node = this.root.Search(equalValue, this.comparer);
            if (node.IsEmpty)
            {
                actualValue = equalValue;
                return false;
            }
            actualValue = node.Key;
            return true;
        }

        public ImmutableSortedSet<T> Union(IEnumerable<T> other)
        {
            ImmutableSortedSet<T> set;
            int num;
            Requires.NotNull<IEnumerable<T>>(other, "other");
            if (ImmutableSortedSet<T>.TryCastToImmutableSortedSet(other, out set) && (set.KeyComparer == this.KeyComparer))
            {
                if (set.IsEmpty)
                {
                    return (ImmutableSortedSet<T>) this;
                }
                if (this.IsEmpty)
                {
                    return set;
                }
                if (set.Count > this.Count)
                {
                    return set.Union(this);
                }
            }
            if (!this.IsEmpty && (!ImmutableExtensions.TryGetCount<T>(other, out num) || (((this.Count + num) * 0.15f) <= this.Count)))
            {
                return this.UnionIncremental(other);
            }
            return this.LeafToRootRefill(other);
        }

        private ImmutableSortedSet<T> UnionIncremental(IEnumerable<T> items)
        {
            Requires.NotNull<IEnumerable<T>>(items, "items");
            Node root = this.root;
            foreach (T local in items)
            {
                bool flag;
                root = root.Add(local, this.comparer, out flag);
            }
            return this.Wrap(root);
        }

        public ImmutableSortedSet<T> WithComparer(IComparer<T> comparer)
        {
            if (comparer == null)
            {
                comparer = (IComparer<T>) Comparer<T>.Default;
            }
            if (comparer == this.comparer)
            {
                return (ImmutableSortedSet<T>) this;
            }
            ImmutableSortedSet<T> set = new ImmutableSortedSet<T>(Node.EmptyNode, comparer);
            return set.Union(this);
        }

        private ImmutableSortedSet<T> Wrap(Node root)
        {
            if (root == this.root)
            {
                return (ImmutableSortedSet<T>) this;
            }
            if (!root.IsEmpty)
            {
                return new ImmutableSortedSet<T>(root, this.comparer);
            }
            return this.Clear();
        }

        private static ImmutableSortedSet<T> Wrap(Node root, IComparer<T> comparer)
        {
            if (!root.IsEmpty)
            {
                return new ImmutableSortedSet<T>(root, comparer);
            }
            return ImmutableSortedSet<T>.Empty.WithComparer(comparer);
        }

        public int Count
        {
            get
            {
                return this.root.Count;
            }
        }

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

        public IComparer<T> KeyComparer
        {
            get
            {
                return this.comparer;
            }
        }

        public T Max
        {
            get
            {
                return this.root.Max;
            }
        }

        public T Min
        {
            get
            {
                return this.root.Min;
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

        [DebuggerDisplay("Count = {Count}"), DebuggerTypeProxy(typeof(ImmutableSortedSet<>.Builder.DebuggerProxy))]
        public sealed class Builder : ISortKeyCollection<T>, IReadOnlyCollection<T>, ISetV20<T>, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
        {
            private IComparer<T> comparer;
            private ImmutableSortedSet<T> immutable;
            private ImmutableSortedSet<T>.Node root;
            private object syncRoot;
            private int version;

            internal Builder(ImmutableSortedSet<T> set)
            {
                this.root = ImmutableSortedSet<T>.Node.EmptyNode;
                this.comparer = (IComparer<T>) Comparer<T>.Default;
                Requires.NotNull<ImmutableSortedSet<T>>(set, "set");
                this.root = set.root;
                this.comparer = set.KeyComparer;
                this.immutable = set;
            }

            public bool Add(T item)
            {
                bool flag;
                this.Root = this.Root.Add(item, this.comparer, out flag);
                return flag;
            }

            public void Clear()
            {
                this.Root = ImmutableSortedSet<T>.Node.EmptyNode;
            }

            public bool Contains(T item)
            {
                return this.Root.Contains(item, this.comparer);
            }

            public void ExceptWith(IEnumerable<T> other)
            {
                Requires.NotNull<IEnumerable<T>>(other, "other");
                foreach (T local in other)
                {
                    bool flag;
                    this.Root = this.Root.Remove(local, this.comparer, out flag);
                }
            }

            public ImmutableSortedSet<T>.Enumerator GetEnumerator()
            {
                return this.Root.GetEnumerator((ImmutableSortedSet<T>.Builder) this);
            }

            public void IntersectWith(IEnumerable<T> other)
            {
                Requires.NotNull<IEnumerable<T>>(other, "other");
                ImmutableSortedSet<T>.Node emptyNode = ImmutableSortedSet<T>.Node.EmptyNode;
                foreach (T local in other)
                {
                    if (this.Contains(local))
                    {
                        bool flag;
                        emptyNode = emptyNode.Add(local, this.comparer, out flag);
                    }
                }
                this.Root = emptyNode;
            }

            public bool IsProperSubsetOf(IEnumerable<T> other)
            {
                return this.ToImmutable().IsProperSubsetOf(other);
            }

            public bool IsProperSupersetOf(IEnumerable<T> other)
            {
                return this.ToImmutable().IsProperSupersetOf(other);
            }

            public bool IsSubsetOf(IEnumerable<T> other)
            {
                return this.ToImmutable().IsSubsetOf(other);
            }

            public bool IsSupersetOf(IEnumerable<T> other)
            {
                return this.ToImmutable().IsSupersetOf(other);
            }

            public bool Overlaps(IEnumerable<T> other)
            {
                return this.ToImmutable().Overlaps(other);
            }

            public bool Remove(T item)
            {
                bool flag;
                this.Root = this.Root.Remove(item, this.comparer, out flag);
                return flag;
            }

            public IEnumerable<T> Reverse()
            {
                return new ImmutableSortedSet<T>.ReverseEnumerable(this.root);
            }

            public bool SetEquals(IEnumerable<T> other)
            {
                return this.ToImmutable().SetEquals(other);
            }

            public void SymmetricExceptWith(IEnumerable<T> other)
            {
                this.Root = this.ToImmutable().SymmetricExcept(other).root;
            }

            void ICollection<T>.Add(T item)
            {
                this.Add(item);
            }

            void ICollection<T>.CopyTo(T[] array, int arrayIndex)
            {
                this.root.CopyTo(array, arrayIndex);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return this.Root.GetEnumerator();
            }

            void ICollection.CopyTo(Array array, int arrayIndex)
            {
                this.Root.CopyTo(array, arrayIndex);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public ImmutableSortedSet<T> ToImmutable()
            {
                if (this.immutable == null)
                {
                    this.immutable = ImmutableSortedSet<T>.Wrap(this.Root, this.comparer);
                }
                return this.immutable;
            }

            public void UnionWith(IEnumerable<T> other)
            {
                Requires.NotNull<IEnumerable<T>>(other, "other");
                foreach (T local in other)
                {
                    bool flag;
                    this.Root = this.Root.Add(local, this.comparer, out flag);
                }
            }

            public int Count
            {
                get
                {
                    return this.Root.Count;
                }
            }

            public IComparer<T> KeyComparer
            {
                get
                {
                    return this.comparer;
                }
                set
                {
                    Requires.NotNull<IComparer<T>>(value, "value");
                    if (value != this.comparer)
                    {
                        ImmutableSortedSet<T>.Node emptyNode = ImmutableSortedSet<T>.Node.EmptyNode;
                        foreach (T local in this)
                        {
                            bool flag;
                            emptyNode = emptyNode.Add(local, value, out flag);
                        }
                        this.immutable = null;
                        this.comparer = value;
                        this.Root = emptyNode;
                    }
                }
            }

            public T Max
            {
                get
                {
                    return this.root.Max;
                }
            }

            public T Min
            {
                get
                {
                    return this.root.Min;
                }
            }

            private ImmutableSortedSet<T>.Node Root
            {
                get
                {
                    return this.root;
                }
                set
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

            internal int Version
            {
                get
                {
                    return this.version;
                }
            }

            private class DebuggerProxy
            {
                private T[] contents;
                private readonly ImmutableSortedSet<T>.Node set;

                public DebuggerProxy(ImmutableSortedSet<T>.Builder builder)
                {
                    Requires.NotNull<ImmutableSortedSet<T>.Builder>(builder, "builder");
                    this.set = builder.Root;
                }

                [DebuggerBrowsable((DebuggerBrowsableState) DebuggerBrowsableState.RootHidden)]
                public T[] Contents
                {
                    get
                    {
                        if (this.contents == null)
                        {
                            this.contents = ImmutableExtensions.ToArray<T>(this.set, this.set.Count);
                        }
                        return this.contents;
                    }
                }
            }
        }

        private class DebuggerProxy
        {
            private T[] contents;
            private readonly ImmutableSortedSet<T> set;

            public DebuggerProxy(ImmutableSortedSet<T> set)
            {
                Requires.NotNull<ImmutableSortedSet<T>>(set, "set");
                this.set = set;
            }

            [DebuggerBrowsable((DebuggerBrowsableState) DebuggerBrowsableState.RootHidden)]
            public T[] Contents
            {
                get
                {
                    if (this.contents == null)
                    {
                        this.contents = ImmutableExtensions.ToArray<T>(this.set, this.set.Count);
                    }
                    return this.contents;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Advanced)]
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable, ISecurePooledObjectUser
        {
            private static readonly SecureObjectPool<Stack<RefAsValueType<IBinaryTree<T>>>, ImmutableSortedSet<T>.Enumerator> enumeratingStacks;
            private readonly ImmutableSortedSet<T>.Builder builder;
            private readonly Guid poolUserId;
            private readonly bool reverse;
            private IBinaryTree<T> root;
            private SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>> stack;
            private IBinaryTree<T> current;
            private int enumeratingBuilderVersion;
            internal Enumerator(IBinaryTree<T> root, ImmutableSortedSet<T>.Builder builder = null, Boolean reverse = false)
            {
                Requires.NotNull<IBinaryTree<T>>(root, "root");
                this.root = root;
                this.builder = builder;
                this.current = null;
                this.reverse = reverse;
                this.enumeratingBuilderVersion = (builder != null ? builder.Version : -1);
                this.poolUserId = Guid.NewGuid();
                this.stack = null;
                if (!ImmutableSortedSet<T>.Enumerator.enumeratingStacks.TryTake(this, out this.stack))
                {
                    this.stack = ImmutableSortedSet<T>.Enumerator.enumeratingStacks.PrepNew(this, new Stack<RefAsValueType<IBinaryTree<T>>>(root.Height));
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
                    SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedSet<T>.Enumerator>(this);
                    try
                    {
                        securePooledObjectUser.Value.Clear();
                    }
                    finally
                    {
                        ((IDisposable)securePooledObjectUser).Dispose();
                    }
                    ImmutableSortedSet<T>.Enumerator.enumeratingStacks.TryAdd(this, this.stack);
                    this.stack = null;
                }
            }

            public Boolean MoveNext()
            {
                Boolean flag;
                this.ThrowIfDisposed();
                this.ThrowIfChanged();
                SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedSet<T>.Enumerator>(this);
                try
                {
                    if (securePooledObjectUser.Value.Count <= 0)
                    {
                        this.current = null;
                        flag = false;
                    }
                    else
                    {
                        IBinaryTree<T> value = securePooledObjectUser.Value.Pop().Value;
                        this.current = value;
                        this.PushNext((this.reverse ? value.Left : value.Right));
                        flag = true;
                    }
                }
                finally
                {
                    ((IDisposable)securePooledObjectUser).Dispose();
                }
                return flag;
            }

            private void PushNext(IBinaryTree<T> node)
            {
                Requires.NotNull<IBinaryTree<T>>(node, "node");
                SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedSet<T>.Enumerator>(this);
                try
                {
                    while (!node.IsEmpty)
                    {
                        securePooledObjectUser.Value.Push(new RefAsValueType<IBinaryTree<T>>(node));
                        node = (this.reverse ? node.Right : node.Left);
                    }
                }
                finally
                {
                    ((IDisposable)securePooledObjectUser).Dispose();
                }
            }

            public void Reset()
            {
                this.ThrowIfDisposed();
                this.enumeratingBuilderVersion = (this.builder != null ? this.builder.Version : -1);
                this.current = null;
                SecurePooledObject<Stack<RefAsValueType<IBinaryTree<T>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedSet<T>.Enumerator>(this);
                try
                {
                    securePooledObjectUser.Value.Clear();
                }
                finally
                {
                    ((IDisposable)securePooledObjectUser).Dispose();
                }
                this.PushNext(this.root);
            }

            private void ThrowIfDisposed()
            {
                if (this.root == null)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this.stack != null)
                {
                    this.stack.ThrowDisposedIfNotOwned<ImmutableSortedSet<T>.Enumerator>(this);
                }
            }
  

            private void ThrowIfChanged()
            {
                if ((this.builder != null) && (this.builder.Version != this.enumeratingBuilderVersion))
                {
                    throw new InvalidOperationException(Strings.CollectionModifiedDuringEnumeration);
                }
            }


            static Enumerator()
            {
                ImmutableSortedSet<T>.Enumerator.enumeratingStacks = new SecureObjectPool<Stack<RefAsValueType<IBinaryTree<T>>>, ImmutableSortedSet<T>.Enumerator>();
            }
        }

        [DebuggerDisplay("{key}")]
        private sealed class Node : IBinaryTree<T>, IEnumerable<T>, IEnumerable
        {
            private int count;
            internal static readonly ImmutableSortedSet<T>.Node EmptyNode;
            private bool frozen;
            private int height;
            private readonly T key;
            private ImmutableSortedSet<T>.Node left;
            private ImmutableSortedSet<T>.Node right;

            static Node()
            {
                ImmutableSortedSet<T>.Node.EmptyNode = new ImmutableSortedSet<T>.Node();
            }

            private Node()
            {
                this.frozen = true;
            }

            private Node(T key, ImmutableSortedSet<T>.Node left, ImmutableSortedSet<T>.Node right, bool frozen = false)
            {
                Requires.NotNullAllowStructs<T>(key, "key");
                Requires.NotNull<ImmutableSortedSet<T>.Node>(left, "left");
                Requires.NotNull<ImmutableSortedSet<T>.Node>(right, "right");
                this.key = key;
                this.left = left;
                this.right = right;
                this.height = 1 + Math.Max(left.height, right.height);
                this.count = (1 + left.count) + right.count;
                this.frozen = frozen;
            }

            internal ImmutableSortedSet<T>.Node Add(T key, IComparer<T> comparer, out Boolean mutated)
            {
                Requires.NotNullAllowStructs<T>(key, "key");
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                if (this.IsEmpty)
                {
                    mutated = true;
                    return new ImmutableSortedSet<T>.Node(key, this, this, false);
                }
                ImmutableSortedSet<T>.Node ts = this;
                Int32 num = comparer.Compare(key, this.key);
                if (num <= 0)
                {
                    if (num >= 0)
                    {
                        mutated = false;
                        return this;
                    }
                    ImmutableSortedSet<T>.Node ts1 = this.left.Add(key, comparer, out mutated);
                    if (mutated)
                    {
                        ts = this.Mutate(ts1, null);
                    }
                }
                else
                {
                    ImmutableSortedSet<T>.Node ts2 = this.right.Add(key, comparer, out mutated);
                    if (mutated)
                    {
                        ts = this.Mutate(null, ts2);
                    }
                }
                if (!mutated)
                {
                    return ts;
                }
                return ImmutableSortedSet<T>.Node.MakeBalanced(ts);
            }
            private static int Balance(ImmutableSortedSet<T>.Node tree)
            {
                Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
                return (tree.right.height - tree.left.height);
            }

            internal bool Contains(T key, IComparer<T> comparer)
            {
                Requires.NotNullAllowStructs<T>(key, "key");
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                return !this.Search(key, comparer).IsEmpty;
            }

            internal void CopyTo(T[] array, int arrayIndex)
            {
                Requires.NotNull<T[]>(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex", null);
                Requires.Range(array.Length >= (arrayIndex + this.Count), "arrayIndex", null);
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

            private static ImmutableSortedSet<T>.Node DoubleLeft(ImmutableSortedSet<T>.Node tree)
            {
                Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
                if (tree.right.IsEmpty)
                {
                    return tree;
                }
                return ImmutableSortedSet<T>.Node.RotateLeft(tree.Mutate(null, ImmutableSortedSet<T>.Node.RotateRight(tree.right)));
            }

            private static ImmutableSortedSet<T>.Node DoubleRight(ImmutableSortedSet<T>.Node tree)
            {
                Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
                if (tree.left.IsEmpty)
                {
                    return tree;
                }
                return ImmutableSortedSet<T>.Node.RotateRight(tree.Mutate(ImmutableSortedSet<T>.Node.RotateLeft(tree.left), null));
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

            public ImmutableSortedSet<T>.Enumerator GetEnumerator()
            {
                return new ImmutableSortedSet<T>.Enumerator(this, null, false);
            }

            internal ImmutableSortedSet<T>.Enumerator GetEnumerator(ImmutableSortedSet<T>.Builder builder)
            {
                return new ImmutableSortedSet<T>.Enumerator(this, builder, false);
            }

            internal int IndexOf(T key, IComparer<T> comparer)
            {
                Requires.NotNullAllowStructs<T>(key, "key");
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                if (this.IsEmpty)
                {
                    return -1;
                }
                int num = comparer.Compare(key, this.key);
                if (num == 0)
                {
                    return this.left.Count;
                }
                if (num <= 0)
                {
                    return this.left.IndexOf(key, comparer);
                }
                int index = this.right.IndexOf(key, comparer);
                bool flag = index < 0;
                if (flag)
                {
                    index = ~index;
                }
                index = (this.left.Count + 1) + index;
                if (flag)
                {
                    index = ~index;
                }
                return index;
            }

            private static bool IsLeftHeavy(ImmutableSortedSet<T>.Node tree)
            {
                Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
                return (ImmutableSortedSet<T>.Node.Balance(tree) <= -2);
            }

            private static bool IsRightHeavy(ImmutableSortedSet<T>.Node tree)
            {
                Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
                return (ImmutableSortedSet<T>.Node.Balance(tree) >= 2);
            }

            private static ImmutableSortedSet<T>.Node MakeBalanced(ImmutableSortedSet<T>.Node tree)
            {
                Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
                if (ImmutableSortedSet<T>.Node.IsRightHeavy(tree))
                {
                    if (!ImmutableSortedSet<T>.Node.IsLeftHeavy(tree.right))
                    {
                        return ImmutableSortedSet<T>.Node.RotateLeft(tree);
                    }
                    return ImmutableSortedSet<T>.Node.DoubleLeft(tree);
                }
                if (!ImmutableSortedSet<T>.Node.IsLeftHeavy(tree))
                {
                    return tree;
                }
                if (!ImmutableSortedSet<T>.Node.IsRightHeavy(tree.left))
                {
                    return ImmutableSortedSet<T>.Node.RotateRight(tree);
                }
                return ImmutableSortedSet<T>.Node.DoubleRight(tree);
            }

            private ImmutableSortedSet<T>.Node Mutate(ImmutableSortedSet<T>.Node left = null, ImmutableSortedSet<T>.Node right = null)
            {
                if (this.frozen)
                {
                    return new ImmutableSortedSet<T>.Node(this.key, left ?? this.left, right ?? this.right, false);
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
                return (ImmutableSortedSet<T>.Node) this;
            }

            private static ImmutableSortedSet<T>.Node NodeTreeFromList(IOrderedCollection<T> items, int start, int length)
            {
                Requires.NotNull<IOrderedCollection<T>>(items, "items");
                if (length == 0)
                {
                    return ImmutableSortedSet<T>.Node.EmptyNode;
                }
                int num = (length - 1) / 2;
                int num2 = (length - 1) - num;
                ImmutableSortedSet<T>.Node left = ImmutableSortedSet<T>.Node.NodeTreeFromList(items, start, num2);
                return new ImmutableSortedSet<T>.Node(items[start + num2], left, ImmutableSortedSet<T>.Node.NodeTreeFromList(items, (start + num2) + 1, num), true);
            }

            internal static ImmutableSortedSet<T>.Node NodeTreeFromSortedSet(SortedSetV20<T> collection)
            {
                Requires.NotNull<SortedSetV20<T>>(collection, "collection");
                if (collection.Count == 0)
                {
                    return ImmutableSortedSet<T>.Node.EmptyNode;
                }
                IOrderedCollection<T> items = ImmutableExtensions.AsOrderedCollection<T>(collection);
                return ImmutableSortedSet<T>.Node.NodeTreeFromList(items, 0, items.Count);
            }

            internal ImmutableSortedSet<T>.Node Remove(T key, IComparer<T> comparer, out Boolean mutated)
            {
                Boolean flag;
                Requires.NotNullAllowStructs<T>(key, "key");
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                if (this.IsEmpty)
                {
                    mutated = false;
                    return this;
                }
                ImmutableSortedSet<T>.Node emptyNode = this;
                Int32 num = comparer.Compare(key, this.key);
                if (num == 0)
                {
                    mutated = true;
                    if (this.right.IsEmpty && this.left.IsEmpty)
                    {
                        emptyNode = ImmutableSortedSet<T>.Node.EmptyNode;
                    }
                    else if (this.right.IsEmpty && !this.left.IsEmpty)
                    {
                        emptyNode = this.left;
                    }
                    else if (this.right.IsEmpty || !this.left.IsEmpty)
                    {
                        ImmutableSortedSet<T>.Node ts = this.right;
                        while (!ts.left.IsEmpty)
                        {
                            ts = ts.left;
                        }
                        ImmutableSortedSet<T>.Node ts1 = this.right.Remove(ts.key, comparer, out flag);
                        emptyNode = ts.Mutate(this.left, ts1);
                    }
                    else
                    {
                        emptyNode = this.right;
                    }
                }
                else if (num >= 0)
                {
                    ImmutableSortedSet<T>.Node ts2 = this.right.Remove(key, comparer, out mutated);
                    if (mutated)
                    {
                        emptyNode = this.Mutate(null, ts2);
                    }
                }
                else
                {
                    ImmutableSortedSet<T>.Node ts3 = this.left.Remove(key, comparer, out mutated);
                    if (mutated)
                    {
                        emptyNode = this.Mutate(ts3, null);
                    }
                }
                if (emptyNode.IsEmpty)
                {
                    return emptyNode;
                }
                return ImmutableSortedSet<T>.Node.MakeBalanced(emptyNode);
            }

            internal IEnumerator<T> Reverse()
            {
                return new ImmutableSortedSet<T>.Enumerator(this, null, true);
            }

            private static ImmutableSortedSet<T>.Node RotateLeft(ImmutableSortedSet<T>.Node tree)
            {
                Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
                if (tree.right.IsEmpty)
                {
                    return tree;
                }
                ImmutableSortedSet<T>.Node right = tree.right;
                return right.Mutate(tree.Mutate(null, right.left), null);
            }

            private static ImmutableSortedSet<T>.Node RotateRight(ImmutableSortedSet<T>.Node tree)
            {
                Requires.NotNull<ImmutableSortedSet<T>.Node>(tree, "tree");
                if (tree.left.IsEmpty)
                {
                    return tree;
                }
                ImmutableSortedSet<T>.Node left = tree.left;
                return left.Mutate(null, tree.Mutate(left.right, null));
            }

            internal ImmutableSortedSet<T>.Node Search(T key, IComparer<T> comparer)
            {
                Requires.NotNullAllowStructs<T>(key, "key");
                Requires.NotNull<IComparer<T>>(comparer, "comparer");
                if (this.IsEmpty)
                {
                    return (ImmutableSortedSet<T>.Node) this;
                }
                int num = comparer.Compare(key, this.key);
                if (num == 0)
                {
                    return (ImmutableSortedSet<T>.Node) this;
                }
                if (num > 0)
                {
                    return this.right.Search(key, comparer);
                }
                return this.left.Search(key, comparer);
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
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

            internal T Max
            {
                get
                {
                    if (this.IsEmpty)
                    {
                        return default(T);
                    }
                    while (!right.right.IsEmpty)
                    {
                        right = right.right;
                    }
                    return right.key;
                }
            }

            internal T Min
            {
                get
                {
                    if (this.IsEmpty)
                    {
                        return default(T);
                    }
                    while (!left.left.IsEmpty)
                    {
                        left = left.left;
                    }
                    return left.key;
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

        private class ReverseEnumerable : IEnumerable<T>, IEnumerable
        {
            private readonly ImmutableSortedSet<T>.Node root;

            internal ReverseEnumerable(ImmutableSortedSet<T>.Node root)
            {
                Requires.NotNull<ImmutableSortedSet<T>.Node>(root, "root");
                this.root = root;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this.root.Reverse();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return (IEnumerator) this.GetEnumerator();
            }
        }
    }
}

