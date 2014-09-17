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

    [DebuggerTypeProxy(typeof(ImmutableSortedDictionary<, >.DebuggerProxy)), DebuggerDisplay("Count = {Count}")]
    public sealed class ImmutableSortedDictionary<TKey, TValue> : IImmutableDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, ISortKeyCollection<TKey>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
    {
        private readonly int count;
        public static readonly ImmutableSortedDictionary<TKey, TValue> Empty;
        private readonly IComparer<TKey> keyComparer;
        private readonly Node root;
        private readonly IEqualityComparer<TValue> valueComparer;

        static ImmutableSortedDictionary()
        {
            ImmutableSortedDictionary<TKey, TValue>.Empty = new ImmutableSortedDictionary<TKey, TValue>(null, null);
        }

        internal ImmutableSortedDictionary(IComparer<TKey> keyComparer = null, IEqualityComparer<TValue> valueComparer = null)
        {
            this.keyComparer = (IComparer<TKey>) (keyComparer ?? Comparer<TKey>.Default);
            this.valueComparer = (IEqualityComparer<TValue>) (valueComparer ?? EqualityComparer<TValue>.Default);
            this.root = Node.EmptyNode;
        }

        private ImmutableSortedDictionary(Node root, int count, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            Requires.NotNull<Node>(root, "root");
            Requires.Range(count >= 0, "count", null);
            Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
            Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
            root.Freeze(null);
            this.root = root;
            this.count = count;
            this.keyComparer = keyComparer;
            this.valueComparer = valueComparer;
        }

        public ImmutableSortedDictionary<TKey, TValue> Add(TKey key, TValue value)
        {
            bool flag;
            Requires.NotNullAllowStructs<TKey>(key, "key");
            Node root = this.root.Add(key, value, this.keyComparer, this.valueComparer, out flag);
            return this.Wrap(root, this.count + 1);
        }

        public ImmutableSortedDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
            return this.AddRange(items, false, false);
        }

        private ImmutableSortedDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items, bool overwriteOnCollision, bool avoidToSortedMap)
        {
            Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
            if (this.IsEmpty && !avoidToSortedMap)
            {
                return this.FillFromEmpty(items, overwriteOnCollision);
            }
            Node root = this.root;
            int count = this.count;
            foreach (KeyValuePair<TKey, TValue> pair in items)
            {
                bool flag;
                bool replacedExistingValue = false;
                Node node2 = overwriteOnCollision ? root.SetItem(pair.Key, pair.Value, this.keyComparer, this.valueComparer, out replacedExistingValue, out flag) : root.Add(pair.Key, pair.Value, this.keyComparer, this.valueComparer, out flag);
                if (flag)
                {
                    root = node2;
                    if (!replacedExistingValue)
                    {
                        count++;
                    }
                }
            }
            return this.Wrap(root, count);
        }

        public ImmutableSortedDictionary<TKey, TValue> Clear()
        {
            if (!this.root.IsEmpty)
            {
                return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(this.keyComparer, this.valueComparer);
            }
            return (ImmutableSortedDictionary<TKey, TValue>) this;
        }

        public bool Contains(KeyValuePair<TKey, TValue> pair)
        {
            return this.root.Contains(pair, this.keyComparer, this.valueComparer);
        }

        public bool ContainsKey(TKey key)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            return this.root.ContainsKey(key, this.keyComparer);
        }

        public bool ContainsValue(TValue value)
        {
            return this.root.ContainsValue(value, this.valueComparer);
        }

        private ImmutableSortedDictionary<TKey, TValue> FillFromEmpty(IEnumerable<KeyValuePair<TKey, TValue>> items, Boolean overwriteOnCollision)
        {
            ImmutableSortedDictionary<TKey, TValue> tKeys;
            SortedDictionary<TKey, TValue> value;
            TValue tValue;
            Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
            if (ImmutableSortedDictionary<TKey, TValue>.TryCastToImmutableMap(items, out tKeys))
            {
                return tKeys.WithComparers(this.KeyComparer, this.ValueComparer);
            }
            IDictionary<TKey, TValue> tKeys1 = items as IDictionary<TKey, TValue>;
            if (tKeys1 == null)
            {
                value = new SortedDictionary<TKey, TValue>(this.KeyComparer);
                foreach (KeyValuePair<TKey, TValue> item in items)
                {
                    if (overwriteOnCollision)
                    {
                        value[item.Key] = item.Value;
                    }
                    else if (!value.TryGetValue(item.Key, out tValue))
                    {
                        value.Add(item.Key, item.Value);
                    }
                    else
                    {
                        if (this.valueComparer.Equals(tValue, item.Value))
                        {
                            continue;
                        }
                        throw new ArgumentException(Strings.DuplicateKey);
                    }
                }
            }
            else
            {
                value = new SortedDictionary<TKey, TValue>(tKeys1, this.KeyComparer);
            }
            if (value.Count == 0)
            {
                return this;
            }
            ImmutableSortedDictionary<TKey, TValue>.Node tKeys2 = ImmutableSortedDictionary<TKey, TValue>.Node.NodeTreeFromSortedDictionary(value);
            return new ImmutableSortedDictionary<TKey, TValue>(tKeys2, value.Count, this.KeyComparer, this.ValueComparer);
        }
        public Enumerator GetEnumerator()
        {
            return this.root.GetEnumerator();
        }

        public ImmutableSortedDictionary<TKey, TValue> Remove(TKey value)
        {
            bool flag;
            Requires.NotNullAllowStructs<TKey>(value, "value");
            Node root = this.root.Remove(value, this.keyComparer, out flag);
            return this.Wrap(root, this.count - 1);
        }

        public ImmutableSortedDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
        {
            Requires.NotNull<IEnumerable<TKey>>(keys, "keys");
            Node root = this.root;
            int count = this.count;
            foreach (TKey local in keys)
            {
                bool flag;
                Node node2 = root.Remove(local, this.keyComparer, out flag);
                if (flag)
                {
                    root = node2;
                    count--;
                }
            }
            return this.Wrap(root, count);
        }

        public ImmutableSortedDictionary<TKey, TValue> SetItem(TKey key, TValue value)
        {
            bool flag;
            bool flag2;
            Requires.NotNullAllowStructs<TKey>(key, "key");
            Node root = this.root.SetItem(key, value, this.keyComparer, this.valueComparer, out flag, out flag2);
            return this.Wrap(root, flag ? this.count : (this.count + 1));
        }

        public ImmutableSortedDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
            return this.AddRange(items, true, false);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            Requires.NotNull<KeyValuePair<TKey, TValue>[]>(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex", null);
            Requires.Range(array.Length >= (arrayIndex + this.Count), "arrayIndex", null);
            foreach (KeyValuePair<TKey, TValue> pair in this)
            {
                array[arrayIndex++] = pair;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.root.CopyTo(array, index, this.Count);
        }

        void IDictionary.Add(object key, object value)
        {
            throw new NotSupportedException();
        }

        void IDictionary.Clear()
        {
            throw new NotSupportedException();
        }

        bool IDictionary.Contains(object key)
        {
            return this.ContainsKey((TKey) key);
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator<TKey, TValue>(this.GetEnumerator());
        }

        void IDictionary.Remove(object key)
        {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            return this.Add(key, value);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            return this.AddRange(pairs);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear()
        {
            return this.Clear();
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key)
        {
            return this.Remove(key);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys)
        {
            return this.RemoveRange(keys);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value)
        {
            return this.SetItem(key, value);
        }

        IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return this.SetItems(items);
        }

        public Builder ToBuilder()
        {
            return new Builder((ImmutableSortedDictionary<TKey, TValue>) this);
        }

        private static bool TryCastToImmutableMap(IEnumerable<KeyValuePair<TKey, TValue>> sequence, out ImmutableSortedDictionary<TKey, TValue> other)
        {
            other = sequence as ImmutableSortedDictionary<TKey, TValue>;
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

        public bool TryGetKey(TKey equalKey, out TKey actualKey)
        {
            Requires.NotNullAllowStructs<TKey>(equalKey, "equalKey");
            return this.root.TryGetKey(equalKey, this.keyComparer, out actualKey);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            return this.root.TryGetValue(key, this.keyComparer, out value);
        }

        public ImmutableSortedDictionary<TKey, TValue> WithComparers(IComparer<TKey> keyComparer)
        {
            return this.WithComparers(keyComparer, this.valueComparer);
        }

        public ImmutableSortedDictionary<TKey, TValue> WithComparers(IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            if (keyComparer == null)
            {
                keyComparer = (IComparer<TKey>) Comparer<TKey>.Default;
            }
            if (valueComparer == null)
            {
                valueComparer = (IEqualityComparer<TValue>) EqualityComparer<TValue>.Default;
            }
            if (keyComparer == this.keyComparer)
            {
                if (valueComparer == this.valueComparer)
                {
                    return (ImmutableSortedDictionary<TKey, TValue>) this;
                }
                return new ImmutableSortedDictionary<TKey, TValue>(this.root, this.count, this.keyComparer, valueComparer);
            }
            ImmutableSortedDictionary<TKey, TValue> dictionaryV40 = new ImmutableSortedDictionary<TKey, TValue>(Node.EmptyNode, 0, keyComparer, valueComparer);
            return dictionaryV40.AddRange(this, false, true);
        }

        private ImmutableSortedDictionary<TKey, TValue> Wrap(Node root, int adjustedCountIfDifferentRoot)
        {
            if (this.root == root)
            {
                return (ImmutableSortedDictionary<TKey, TValue>) this;
            }
            if (!root.IsEmpty)
            {
                return new ImmutableSortedDictionary<TKey, TValue>(root, adjustedCountIfDifferentRoot, this.keyComparer, this.valueComparer);
            }
            return this.Clear();
        }

        private static ImmutableSortedDictionary<TKey, TValue> Wrap(Node root, int count, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            if (!root.IsEmpty)
            {
                return new ImmutableSortedDictionary<TKey, TValue>(root, count, keyComparer, valueComparer);
            }
            return ImmutableSortedDictionary<TKey, TValue>.Empty.WithComparers(keyComparer, valueComparer);
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
                return this.root.IsEmpty;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue local;
                Requires.NotNullAllowStructs<TKey>(key, "key");
                if (!this.TryGetValue(key, out local))
                {
                    throw new KeyNotFoundException();
                }
                return local;
            }
        }

        public IComparer<TKey> KeyComparer
        {
            get
            {
                return this.keyComparer;
            }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                return this.root.Keys;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return this[key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return new KeysCollectionAccessor<TKey, TValue>(this);
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return new ValuesCollectionAccessor<TKey, TValue>(this);
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

        bool IDictionary.IsFixedSize
        {
            get
            {
                return true;
            }
        }

        bool IDictionary.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                return this[(TKey) key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                return new KeysCollectionAccessor<TKey, TValue>(this);
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                return new ValuesCollectionAccessor<TKey, TValue>(this);
            }
        }

        public IEqualityComparer<TValue> ValueComparer
        {
            get
            {
                return this.valueComparer;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                return this.root.Values;
            }
        }

        [DebuggerTypeProxy(typeof(ImmutableSortedDictionary<, >.Builder.DebuggerProxy)), DebuggerDisplay("Count = {Count}")]
        public sealed class Builder : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
        {
            private int count;
            private ImmutableSortedDictionary<TKey, TValue> immutable;
            private IComparer<TKey> keyComparer;
            private ImmutableSortedDictionary<TKey, TValue>.Node root;
            private object syncRoot;
            private IEqualityComparer<TValue> valueComparer;
            private int version;

            internal Builder(ImmutableSortedDictionary<TKey, TValue> map)
            {
                this.root = ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode;
                this.keyComparer = (IComparer<TKey>) Comparer<TKey>.Default;
                this.valueComparer = (IEqualityComparer<TValue>) EqualityComparer<TValue>.Default;
                Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>>(map, "map");
                this.root = map.root;
                this.keyComparer = map.KeyComparer;
                this.valueComparer = map.ValueComparer;
                this.count = map.Count;
                this.immutable = map;
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                this.Add(item.Key, item.Value);
            }

            public void Add(TKey key, TValue value)
            {
                bool flag;
                this.Root = this.Root.Add(key, value, this.keyComparer, this.valueComparer, out flag);
                if (flag)
                {
                    this.count++;
                }
            }

            public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
            {
                Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
                foreach (KeyValuePair<TKey, TValue> pair in items)
                {
                    this.Add(pair);
                }
            }

            public void Clear()
            {
                this.Root = ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode;
                this.count = 0;
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                return this.Root.Contains(item, this.keyComparer, this.valueComparer);
            }

            public bool ContainsKey(TKey key)
            {
                return this.Root.ContainsKey(key, this.keyComparer);
            }

            public bool ContainsValue(TValue value)
            {
                return this.root.ContainsValue(value, this.valueComparer);
            }

            public ImmutableSortedDictionary<TKey, TValue>.Enumerator GetEnumerator()
            {
                return this.Root.GetEnumerator((ImmutableSortedDictionary<TKey, TValue>.Builder) this);
            }

            public TValue GetValueOrDefault(TKey key)
            {
                return this.GetValueOrDefault(key, default(TValue));
            }

            public TValue GetValueOrDefault(TKey key, TValue defaultValue)
            {
                TValue local;
                Requires.NotNullAllowStructs<TKey>(key, "key");
                if (this.TryGetValue(key, out local))
                {
                    return local;
                }
                return defaultValue;
            }

            public bool Remove(TKey key)
            {
                bool flag;
                this.Root = this.Root.Remove(key, this.keyComparer, out flag);
                if (flag)
                {
                    this.count--;
                }
                return flag;
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                return (this.Contains(item) && this.Remove(item.Key));
            }

            public void RemoveRange(IEnumerable<TKey> keys)
            {
                Requires.NotNull<IEnumerable<TKey>>(keys, "keys");
                foreach (TKey local in keys)
                {
                    this.Remove(local);
                }
            }

            void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                this.Root.CopyTo(array, arrayIndex, this.Count);
            }

            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            void ICollection.CopyTo(Array array, int index)
            {
                this.Root.CopyTo(array, index, this.Count);
            }

            void IDictionary.Add(object key, object value)
            {
                this.Add((TKey) key, (TValue) value);
            }

            bool IDictionary.Contains(object key)
            {
                return this.ContainsKey((TKey) key);
            }

            IDictionaryEnumerator IDictionary.GetEnumerator()
            {
                return new DictionaryEnumerator<TKey, TValue>(this.GetEnumerator());
            }

            void IDictionary.Remove(object key)
            {
                this.Remove((TKey) key);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public ImmutableSortedDictionary<TKey, TValue> ToImmutable()
            {
                if (this.immutable == null)
                {
                    this.immutable = ImmutableSortedDictionary<TKey, TValue>.Wrap(this.Root, this.count, this.keyComparer, this.valueComparer);
                }
                return this.immutable;
            }

            public bool TryGetKey(TKey equalKey, out TKey actualKey)
            {
                Requires.NotNullAllowStructs<TKey>(equalKey, "equalKey");
                return this.Root.TryGetKey(equalKey, this.keyComparer, out actualKey);
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                return this.Root.TryGetValue(key, this.keyComparer, out value);
            }

            public int Count
            {
                get
                {
                    return this.count;
                }
            }

            public TValue this[TKey key]
            {
                get
                {
                    TValue local;
                    if (!this.TryGetValue(key, out local))
                    {
                        throw new KeyNotFoundException();
                    }
                    return local;
                }
                set
                {
                    bool flag;
                    bool flag2;
                    this.Root = this.root.SetItem(key, value, this.keyComparer, this.valueComparer, out flag, out flag2);
                    if (flag2 && !flag)
                    {
                        this.count++;
                    }
                }
            }

            public IComparer<TKey> KeyComparer
            {
                get
                {
                    return this.keyComparer;
                }
                set
                {
                    Requires.NotNull<IComparer<TKey>>(value, "value");
                    if (value != this.keyComparer)
                    {
                        ImmutableSortedDictionary<TKey, TValue>.Node emptyNode = ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode;
                        int num = 0;
                        foreach (KeyValuePair<TKey, TValue> pair in this)
                        {
                            bool flag;
                            TKey key = pair.Key;
                            emptyNode = emptyNode.Add(key, pair.Value, value, this.valueComparer, out flag);
                            if (flag)
                            {
                                num++;
                            }
                        }
                        this.keyComparer = value;
                        this.Root = emptyNode;
                        this.count = num;
                    }
                }
            }

            public IEnumerable<TKey> Keys
            {
                get
                {
                    return this.Root.Keys;
                }
            }

            private ImmutableSortedDictionary<TKey, TValue>.Node Root
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

            bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            ICollection<TKey> IDictionary<TKey, TValue>.Keys
            {
                get
                {
                    return ImmutableExtensions.ToArray<TKey>(this.Root.Keys, this.Count);
                }
            }

            ICollection<TValue> IDictionary<TKey, TValue>.Values
            {
                get
                {
                    return ImmutableExtensions.ToArray<TValue>(this.Root.Values, this.Count);
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

            bool IDictionary.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            bool IDictionary.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            object IDictionary.this[object key]
            {
                get
                {
                    return this[(TKey) key];
                }
                set
                {
                    this[(TKey) key] = (TValue) value;
                }
            }

            ICollection IDictionary.Keys
            {
                get
                {
                    return ImmutableExtensions.ToArray<TKey>(this.Keys, this.Count);
                }
            }

            ICollection IDictionary.Values
            {
                get
                {
                    return ImmutableExtensions.ToArray<TValue>(this.Values, this.Count);
                }
            }

            public IEqualityComparer<TValue> ValueComparer
            {
                get
                {
                    return this.valueComparer;
                }
                set
                {
                    Requires.NotNull<IEqualityComparer<TValue>>(value, "value");
                    if (value != this.valueComparer)
                    {
                        this.valueComparer = value;
                        this.immutable = null;
                    }
                }
            }

            public IEnumerable<TValue> Values
            {
                get
                {
                    return this.Root.Values;
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
                private KeyValuePair<TKey, TValue>[] contents;
                private readonly ImmutableSortedDictionary<TKey, TValue>.Builder map;

                public DebuggerProxy(ImmutableSortedDictionary<TKey, TValue>.Builder map)
                {
                    Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Builder>(map, "map");
                    this.map = map;
                }

                [DebuggerBrowsable((DebuggerBrowsableState) DebuggerBrowsableState.RootHidden)]
                public KeyValuePair<TKey, TValue>[] Contents
                {
                    get
                    {
                        if (this.contents == null)
                        {
                            this.contents = ImmutableExtensions.ToArray<KeyValuePair<TKey, TValue>>(this.map, this.map.Count);
                        }
                        return this.contents;
                    }
                }
            }
        }

        private class DebuggerProxy
        {
            private KeyValuePair<TKey, TValue>[] contents;
            private readonly ImmutableSortedDictionary<TKey, TValue> map;

            public DebuggerProxy(ImmutableSortedDictionary<TKey, TValue> map)
            {
                Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>>(map, "map");
                this.map = map;
            }

            [DebuggerBrowsable((DebuggerBrowsableState) DebuggerBrowsableState.RootHidden)]
            public KeyValuePair<TKey, TValue>[] Contents
            {
                get
                {
                    if (this.contents == null)
                    {
                        this.contents = ImmutableExtensions.ToArray<KeyValuePair<TKey, TValue>>(this.map, this.map.Count);
                    }
                    return this.contents;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Advanced)]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable, ISecurePooledObjectUser
        {
            private static readonly SecureObjectPool<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>, ImmutableSortedDictionary<TKey, TValue>.Enumerator> enumeratingStacks;
            private readonly ImmutableSortedDictionary<TKey, TValue>.Builder builder;
            private readonly Guid poolUserId;
            private IBinaryTree<KeyValuePair<TKey, TValue>> root;
            private SecurePooledObject<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>> stack;
            private IBinaryTree<KeyValuePair<TKey, TValue>> current;
            private int enumeratingBuilderVersion;
            internal Enumerator(IBinaryTree<KeyValuePair<TKey, TValue>> root, ImmutableSortedDictionary<TKey, TValue>.Builder builder = null)
            {
                Requires.NotNull<IBinaryTree<KeyValuePair<TKey, TValue>>>(root, "root");
                this.root = root;
                this.builder = builder;
                this.current = null;
                this.enumeratingBuilderVersion = (builder != null ? builder.Version : -1);
                this.poolUserId = Guid.NewGuid();
                this.stack = null;
                if (!ImmutableSortedDictionary<TKey, TValue>.Enumerator.enumeratingStacks.TryTake(this, out this.stack))
                {
                    this.stack = ImmutableSortedDictionary<TKey, TValue>.Enumerator.enumeratingStacks.PrepNew(this, new Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>(root.Height));
                }
                this.Reset();
            }

            public KeyValuePair<TKey, TValue> Current
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
            Guid ISecurePooledObjectUser.PoolUserId
            {
                get
                {
                    return this.poolUserId;
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
                    SecurePooledObject<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedDictionary<TKey, TValue>.Enumerator>(this);
                    try
                    {
                        securePooledObjectUser.Value.Clear();
                    }
                    finally
                    {
                        ((IDisposable)securePooledObjectUser).Dispose();
                    }
                    ImmutableSortedDictionary<TKey, TValue>.Enumerator.enumeratingStacks.TryAdd(this, this.stack);
                }
                this.stack = null;
            }
            public Boolean MoveNext()
            {
                Boolean flag;
                this.ThrowIfDisposed();
                this.ThrowIfChanged();
                SecurePooledObject<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedDictionary<TKey, TValue>.Enumerator>(this);
                try
                {
                    if (securePooledObjectUser.Value.Count <= 0)
                    {
                        this.current = null;
                        flag = false;
                    }
                    else
                    {
                        IBinaryTree<KeyValuePair<TKey, TValue>> value = securePooledObjectUser.Value.Pop().Value;
                        this.current = value;
                        this.PushLeft(value.Right);
                        flag = true;
                    }
                }
                finally
                {
                    ((IDisposable)securePooledObjectUser).Dispose();
                }
                return flag;
            }

            public void Reset()
            {
                this.ThrowIfDisposed();
                this.enumeratingBuilderVersion = (this.builder != null ? this.builder.Version : -1);
                this.current = null;
                SecurePooledObject<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedDictionary<TKey, TValue>.Enumerator>(this);
                try
                {
                    securePooledObjectUser.Value.Clear();
                }
                finally
                {
                    ((IDisposable)securePooledObjectUser).Dispose();
                }
                this.PushLeft(this.root);
            }

            internal void ThrowIfDisposed()
            {
                if (this.root == null)
                {
                    throw new ObjectDisposedException(this.GetType().FullName);
                }
                if (this.stack != null)
                {
                    this.stack.ThrowDisposedIfNotOwned<ImmutableSortedDictionary<TKey, TValue>.Enumerator>(this);
                }
            }
            private void ThrowIfChanged()
            {
                if ((this.builder != null) && (this.builder.Version != this.enumeratingBuilderVersion))
                {
                    throw new InvalidOperationException(Strings.CollectionModifiedDuringEnumeration);
                }
            }

            private void PushLeft(IBinaryTree<KeyValuePair<TKey, TValue>> node)
            {
                Requires.NotNull<IBinaryTree<KeyValuePair<TKey, TValue>>>(node, "node");
                SecurePooledObject<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>>.SecurePooledObjectUser securePooledObjectUser = this.stack.Use<ImmutableSortedDictionary<TKey, TValue>.Enumerator>(this);
                try
                {
                    while (!node.IsEmpty)
                    {
                        securePooledObjectUser.Value.Push(new RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>(node));
                        node = node.Left;
                    }
                }
                finally
                {
                    ((IDisposable)securePooledObjectUser).Dispose();
                }
            }


            static Enumerator()
            {
                ImmutableSortedDictionary<TKey, TValue>.Enumerator.enumeratingStacks = new SecureObjectPool<Stack<RefAsValueType<IBinaryTree<KeyValuePair<TKey, TValue>>>>, ImmutableSortedDictionary<TKey, TValue>.Enumerator>();
            }
        }

        [DebuggerDisplay("{key} = {value}")]
        internal sealed class Node : IBinaryTree<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
        {
            internal static readonly ImmutableSortedDictionary<TKey, TValue>.Node EmptyNode;
            private bool frozen;
            private int height;
            private readonly TKey key;
            private ImmutableSortedDictionary<TKey, TValue>.Node left;
            private ImmutableSortedDictionary<TKey, TValue>.Node right;
            private TValue value;

            static Node()
            {
                ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode = new ImmutableSortedDictionary<TKey, TValue>.Node();
            }

            private Node()
            {
                this.frozen = true;
            }

            private Node(TKey key, TValue value, ImmutableSortedDictionary<TKey, TValue>.Node left, ImmutableSortedDictionary<TKey, TValue>.Node right, bool frozen = false)
            {
                Requires.NotNullAllowStructs<TKey>(key, "key");
                Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(left, "left");
                Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(right, "right");
                this.key = key;
                this.value = value;
                this.left = left;
                this.right = right;
                this.height = 1 + Math.Max(left.height, right.height);
                this.frozen = frozen;
            }

            internal ImmutableSortedDictionary<TKey, TValue>.Node Add(TKey key, TValue value, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, out bool mutated)
            {
                bool flag;
                Requires.NotNullAllowStructs<TKey>(key, "key");
                Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
                Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
                return this.SetOrAdd(key, value, keyComparer, valueComparer, false, out flag, out mutated);
            }

            private static int Balance(ImmutableSortedDictionary<TKey, TValue>.Node tree)
            {
                Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
                return (tree.right.height - tree.left.height);
            }

            internal bool Contains(KeyValuePair<TKey, TValue> pair, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNullAllowStructs<bool>(pair.Key != null, "key");
                Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
                Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
                ImmutableSortedDictionary<TKey, TValue>.Node node = this.Search(pair.Key, keyComparer);
                if (node.IsEmpty)
                {
                    return false;
                }
                return valueComparer.Equals(node.value, pair.Value);
            }

            internal bool ContainsKey(TKey key, IComparer<TKey> keyComparer)
            {
                Requires.NotNullAllowStructs<TKey>(key, "key");
                Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
                return !this.Search(key, keyComparer).IsEmpty;
            }

            internal bool ContainsValue(TValue value, IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
                return Enumerable.Contains<TValue>(this.Values, value, valueComparer);
            }

            internal void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex, int dictionarySize)
            {
                Requires.NotNull<KeyValuePair<TKey, TValue>[]>(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex", null);
                Requires.Range(array.Length >= (arrayIndex + dictionarySize), "arrayIndex", null);
                foreach (KeyValuePair<TKey, TValue> pair in this)
                {
                    array[arrayIndex++] = pair;
                }
            }

            internal void CopyTo(Array array, int arrayIndex, int dictionarySize)
            {
                Requires.NotNull<Array>(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex", null);
                Requires.Range(array.Length >= (arrayIndex + dictionarySize), "arrayIndex", null);
                foreach (KeyValuePair<TKey, TValue> pair in this)
                {
                    array.SetValue(new DictionaryEntry(pair.Key, pair.Value), new int[] { arrayIndex++ });
                }
            }

            private static ImmutableSortedDictionary<TKey, TValue>.Node DoubleLeft(ImmutableSortedDictionary<TKey, TValue>.Node tree)
            {
                Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
                if (tree.right.IsEmpty)
                {
                    return tree;
                }
                return ImmutableSortedDictionary<TKey, TValue>.Node.RotateLeft(tree.Mutate(null, ImmutableSortedDictionary<TKey, TValue>.Node.RotateRight(tree.right)));
            }

            private static ImmutableSortedDictionary<TKey, TValue>.Node DoubleRight(ImmutableSortedDictionary<TKey, TValue>.Node tree)
            {
                Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
                if (tree.left.IsEmpty)
                {
                    return tree;
                }
                return ImmutableSortedDictionary<TKey, TValue>.Node.RotateRight(tree.Mutate(ImmutableSortedDictionary<TKey, TValue>.Node.RotateLeft(tree.left), null));
            }

            internal void Freeze(Action<KeyValuePair<TKey, TValue>> freezeAction = null)
            {
                if (!this.frozen)
                {
                    if (freezeAction != null)
                    {
                        freezeAction(new KeyValuePair<TKey, TValue>(this.key, this.value));
                    }
                    this.left.Freeze(freezeAction);
                    this.right.Freeze(freezeAction);
                    this.frozen = true;
                }
            }

            public ImmutableSortedDictionary<TKey, TValue>.Enumerator GetEnumerator()
            {
                return new ImmutableSortedDictionary<TKey, TValue>.Enumerator(this, null);
            }

            internal ImmutableSortedDictionary<TKey, TValue>.Enumerator GetEnumerator(ImmutableSortedDictionary<TKey, TValue>.Builder builder)
            {
                return new ImmutableSortedDictionary<TKey, TValue>.Enumerator(this, builder);
            }

            internal TValue GetValueOrDefault(TKey key, IComparer<TKey> keyComparer)
            {
                Requires.NotNullAllowStructs<TKey>(key, "key");
                Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
                ImmutableSortedDictionary<TKey, TValue>.Node node = this.Search(key, keyComparer);
                if (!node.IsEmpty)
                {
                    return node.value;
                }
                return default(TValue);
            }

            private static bool IsLeftHeavy(ImmutableSortedDictionary<TKey, TValue>.Node tree)
            {
                Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
                return (ImmutableSortedDictionary<TKey, TValue>.Node.Balance(tree) <= -2);
            }

            private static bool IsRightHeavy(ImmutableSortedDictionary<TKey, TValue>.Node tree)
            {
                Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
                return (ImmutableSortedDictionary<TKey, TValue>.Node.Balance(tree) >= 2);
            }

            private static ImmutableSortedDictionary<TKey, TValue>.Node MakeBalanced(ImmutableSortedDictionary<TKey, TValue>.Node tree)
            {
                Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
                if (ImmutableSortedDictionary<TKey, TValue>.Node.IsRightHeavy(tree))
                {
                    if (!ImmutableSortedDictionary<TKey, TValue>.Node.IsLeftHeavy(tree.right))
                    {
                        return ImmutableSortedDictionary<TKey, TValue>.Node.RotateLeft(tree);
                    }
                    return ImmutableSortedDictionary<TKey, TValue>.Node.DoubleLeft(tree);
                }
                if (!ImmutableSortedDictionary<TKey, TValue>.Node.IsLeftHeavy(tree))
                {
                    return tree;
                }
                if (!ImmutableSortedDictionary<TKey, TValue>.Node.IsRightHeavy(tree.left))
                {
                    return ImmutableSortedDictionary<TKey, TValue>.Node.RotateRight(tree);
                }
                return ImmutableSortedDictionary<TKey, TValue>.Node.DoubleRight(tree);
            }

            private ImmutableSortedDictionary<TKey, TValue>.Node Mutate(ImmutableSortedDictionary<TKey, TValue>.Node left = null, ImmutableSortedDictionary<TKey, TValue>.Node right = null)
            {
                if (this.frozen)
                {
                    return new ImmutableSortedDictionary<TKey, TValue>.Node(this.key, this.value, left ?? this.left, right ?? this.right, false);
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
                return (ImmutableSortedDictionary<TKey, TValue>.Node) this;
            }

            private static ImmutableSortedDictionary<TKey, TValue>.Node NodeTreeFromList(IOrderedCollection<KeyValuePair<TKey, TValue>> items, int start, int length)
            {
                Requires.NotNull<IOrderedCollection<KeyValuePair<TKey, TValue>>>(items, "items");
                Requires.Range(start >= 0, "start", null);
                Requires.Range(length >= 0, "length", null);
                if (length == 0)
                {
                    return ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode;
                }
                int num = (length - 1) / 2;
                int num2 = (length - 1) - num;
                ImmutableSortedDictionary<TKey, TValue>.Node left = ImmutableSortedDictionary<TKey, TValue>.Node.NodeTreeFromList(items, start, num2);
                ImmutableSortedDictionary<TKey, TValue>.Node right = ImmutableSortedDictionary<TKey, TValue>.Node.NodeTreeFromList(items, (start + num2) + 1, num);
                KeyValuePair<TKey, TValue> pair = items[start + num2];
                TKey key = pair.Key;
                return new ImmutableSortedDictionary<TKey, TValue>.Node(key, pair.Value, left, right, true);
            }

            internal static ImmutableSortedDictionary<TKey, TValue>.Node NodeTreeFromSortedDictionary(SortedDictionary<TKey, TValue> dictionary)
            {
                Requires.NotNull<SortedDictionary<TKey, TValue>>(dictionary, "dictionaryV40");
                IOrderedCollection<KeyValuePair<TKey, TValue>> items = ImmutableExtensions.AsOrderedCollection<KeyValuePair<TKey, TValue>>(((IEnumerable<KeyValuePair<TKey, TValue>>)dictionary));
                return ImmutableSortedDictionary<TKey, TValue>.Node.NodeTreeFromList(items, 0, items.Count);
            }

            internal ImmutableSortedDictionary<TKey, TValue>.Node Remove(TKey key, IComparer<TKey> keyComparer, out bool mutated)
            {
                Requires.NotNullAllowStructs<TKey>(key, "key");
                Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
                return this.RemoveRecursive(key, keyComparer, out mutated);
            }

            private ImmutableSortedDictionary<TKey, TValue>.Node RemoveRecursive(TKey key, IComparer<TKey> keyComparer, out Boolean mutated)
            {
                Boolean flag;
                if (this.IsEmpty)
                {
                    mutated = false;
                    return this;
                }
                ImmutableSortedDictionary<TKey, TValue>.Node emptyNode = this;
                Int32 num = keyComparer.Compare(key, this.key);
                if (num == 0)
                {
                    mutated = true;
                    if (this.right.IsEmpty && this.left.IsEmpty)
                    {
                        emptyNode = ImmutableSortedDictionary<TKey, TValue>.Node.EmptyNode;
                    }
                    else if (this.right.IsEmpty && !this.left.IsEmpty)
                    {
                        emptyNode = this.left;
                    }
                    else if (this.right.IsEmpty || !this.left.IsEmpty)
                    {
                        ImmutableSortedDictionary<TKey, TValue>.Node tKeys = this.right;
                        while (!tKeys.left.IsEmpty)
                        {
                            tKeys = tKeys.left;
                        }
                        ImmutableSortedDictionary<TKey, TValue>.Node tKeys1 = this.right.Remove(tKeys.key, keyComparer, out flag);
                        emptyNode = tKeys.Mutate(this.left, tKeys1);
                    }
                    else
                    {
                        emptyNode = this.right;
                    }
                }
                else if (num >= 0)
                {
                    ImmutableSortedDictionary<TKey, TValue>.Node tKeys2 = this.right.Remove(key, keyComparer, out mutated);
                    if (mutated)
                    {
                        emptyNode = this.Mutate(null, tKeys2);
                    }
                }
                else
                {
                    ImmutableSortedDictionary<TKey, TValue>.Node tKeys3 = this.left.Remove(key, keyComparer, out mutated);
                    if (mutated)
                    {
                        emptyNode = this.Mutate(tKeys3, null);
                    }
                }
                if (emptyNode.IsEmpty)
                {
                    return emptyNode;
                }
                return ImmutableSortedDictionary<TKey, TValue>.Node.MakeBalanced(emptyNode);
            }

            private static ImmutableSortedDictionary<TKey, TValue>.Node RotateLeft(ImmutableSortedDictionary<TKey, TValue>.Node tree)
            {
                Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
                if (tree.right.IsEmpty)
                {
                    return tree;
                }
                ImmutableSortedDictionary<TKey, TValue>.Node right = tree.right;
                return right.Mutate(tree.Mutate(null, right.left), null);
            }

            private static ImmutableSortedDictionary<TKey, TValue>.Node RotateRight(ImmutableSortedDictionary<TKey, TValue>.Node tree)
            {
                Requires.NotNull<ImmutableSortedDictionary<TKey, TValue>.Node>(tree, "tree");
                if (tree.left.IsEmpty)
                {
                    return tree;
                }
                ImmutableSortedDictionary<TKey, TValue>.Node left = tree.left;
                return left.Mutate(null, tree.Mutate(left.right, null));
            }

            private ImmutableSortedDictionary<TKey, TValue>.Node Search(TKey key, IComparer<TKey> keyComparer)
            {
                if (this.left == null)
                {
                    return (ImmutableSortedDictionary<TKey, TValue>.Node) this;
                }
                int num = keyComparer.Compare(key, this.key);
                if (num == 0)
                {
                    return (ImmutableSortedDictionary<TKey, TValue>.Node) this;
                }
                if (num > 0)
                {
                    return this.right.Search(key, keyComparer);
                }
                return this.left.Search(key, keyComparer);
            }

            internal ImmutableSortedDictionary<TKey, TValue>.Node SetItem(TKey key, TValue value, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, out bool replacedExistingValue, out bool mutated)
            {
                Requires.NotNullAllowStructs<TKey>(key, "key");
                Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
                Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
                return this.SetOrAdd(key, value, keyComparer, valueComparer, true, out replacedExistingValue, out mutated);
            }

            private ImmutableSortedDictionary<TKey, TValue>.Node SetOrAdd(TKey key, TValue value, IComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, Boolean overwriteExistingValue, out Boolean replacedExistingValue, out Boolean mutated)
            {
                replacedExistingValue = false;
                if (this.IsEmpty)
                {
                    mutated = true;
                    return new ImmutableSortedDictionary<TKey, TValue>.Node(key, value, this, this, false);
                }
                ImmutableSortedDictionary<TKey, TValue>.Node tKeys = this;
                Int32 num = keyComparer.Compare(key, this.key);
                if (num > 0)
                {
                    ImmutableSortedDictionary<TKey, TValue>.Node tKeys1 = this.right.SetOrAdd(key, value, keyComparer, valueComparer, overwriteExistingValue, out replacedExistingValue, out mutated);
                    if (mutated)
                    {
                        tKeys = this.Mutate(null, tKeys1);
                    }
                }
                else if (num >= 0)
                {
                    if (valueComparer.Equals(this.@value, value))
                    {
                        mutated = false;
                        return this;
                    }
                    if (!overwriteExistingValue)
                    {
                        throw new ArgumentException(Strings.DuplicateKey);
                    }
                    mutated = true;
                    replacedExistingValue = true;
                    tKeys = new ImmutableSortedDictionary<TKey, TValue>.Node(key, value, this.left, this.right, false);
                }
                else
                {
                    ImmutableSortedDictionary<TKey, TValue>.Node tKeys2 = this.left.SetOrAdd(key, value, keyComparer, valueComparer, overwriteExistingValue, out replacedExistingValue, out mutated);
                    if (mutated)
                    {
                        tKeys = this.Mutate(tKeys2, null);
                    }
                }
                if (!mutated)
                {
                    return tKeys;
                }
                return ImmutableSortedDictionary<TKey, TValue>.Node.MakeBalanced(tKeys);
            }

            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            internal bool TryGetKey(TKey equalKey, IComparer<TKey> keyComparer, out TKey actualKey)
            {
                Requires.NotNullAllowStructs<TKey>(equalKey, "equalKey");
                Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
                ImmutableSortedDictionary<TKey, TValue>.Node node = this.Search(equalKey, keyComparer);
                if (node.IsEmpty)
                {
                    actualKey = equalKey;
                    return false;
                }
                actualKey = node.key;
                return true;
            }

            internal bool TryGetValue(TKey key, IComparer<TKey> keyComparer, out TValue value)
            {
                Requires.NotNullAllowStructs<TKey>(key, "key");
                Requires.NotNull<IComparer<TKey>>(keyComparer, "keyComparer");
                ImmutableSortedDictionary<TKey, TValue>.Node node = this.Search(key, keyComparer);
                if (node.IsEmpty)
                {
                    value = default(TValue);
                    return false;
                }
                value = node.value;
                return true;
            }

            public bool IsEmpty
            {
                get
                {
                    return (this.left == null);
                }
            }

            internal IEnumerable<TKey> Keys
            {
                get
                {
                    return Enumerable.Select<KeyValuePair<TKey, TValue>, TKey>(this, delegate (KeyValuePair<TKey, TValue> p) {
                        return p.Key;
                    });
                }
            }

            int IBinaryTree<KeyValuePair<TKey, TValue>>.Count
            {
                get
                {
                    throw new NotSupportedException();
                }
            }

            int IBinaryTree<KeyValuePair<TKey, TValue>>.Height
            {
                get
                {
                    return this.height;
                }
            }

            IBinaryTree<KeyValuePair<TKey, TValue>> IBinaryTree<KeyValuePair<TKey, TValue>>.Left
            {
                get
                {
                    return this.left;
                }
            }

            IBinaryTree<KeyValuePair<TKey, TValue>> IBinaryTree<KeyValuePair<TKey, TValue>>.Right
            {
                get
                {
                    return this.right;
                }
            }

            KeyValuePair<TKey, TValue> IBinaryTree<KeyValuePair<TKey, TValue>>.Value
            {
                get
                {
                    return new KeyValuePair<TKey, TValue>(this.key, this.value);
                }
            }

            internal IEnumerable<TValue> Values
            {
                get
                {
                    return Enumerable.Select<KeyValuePair<TKey, TValue>, TValue>(this, delegate (KeyValuePair<TKey, TValue> p) {
                        return p.Value;
                    });
                }
            }
        }
    }
}

