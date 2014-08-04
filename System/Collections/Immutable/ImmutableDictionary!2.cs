using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic.V40;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Validation;

namespace System.Collections.Immutable
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ImmutableDictionary<,>.DebuggerProxy))]
    public sealed class ImmutableDictionary<TKey, TValue> : IImmutableDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IImmutableDictionaryInternal<TKey, TValue>, IHashKeyCollection<TKey>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
    {
        public readonly static ImmutableDictionary<TKey, TValue> Empty;

        private readonly static Action<KeyValuePair<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>> FreezeBucketAction;

        private readonly Int32 count;

        private readonly ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root;

        private readonly ImmutableDictionary<TKey, TValue>.Comparers comparers;

        public Int32 Count
        {
            get
            {
                return this.count;
            }
        }

        public Boolean IsEmpty
        {
            get
            {
                return this.Count == 0;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                TValue tValue;
                Requires.NotNullAllowStructs<TKey>(key, "key");
                if (!this.TryGetValue(key, out tValue))
                {
                    throw new KeyNotFoundException();
                }
                return tValue;
            }
        }

        public IEqualityComparer<TKey> KeyComparer
        {
            get
            {
                return this.comparers.KeyComparer;
            }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                foreach (KeyValuePair<Int32, ImmutableDictionary<TKey, TValue>.HashBucket> keyValuePair in this.root)
                {
                    foreach (KeyValuePair<TKey, TValue> value in keyValuePair.Value)
                    {
                        yield return value.Key;
                    }
                }
            }
        }

        private ImmutableDictionary<TKey, TValue>.MutationInput Origin
        {
            get
            {
                return new ImmutableDictionary<TKey, TValue>.MutationInput(this);
            }
        }

        Boolean System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        TValue System.Collections.Generic.IDictionary<TKey, TValue>.this[TKey key]
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

        ICollection<TKey> System.Collections.Generic.IDictionary<TKey, TValue>.Keys
        {
            get
            {
                return new KeysCollectionAccessor<TKey, TValue>(this);
            }
        }

        ICollection<TValue> System.Collections.Generic.IDictionary<TKey, TValue>.Values
        {
            get
            {
                return new ValuesCollectionAccessor<TKey, TValue>(this);
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Boolean System.Collections.ICollection.IsSynchronized
        {
            get
            {
                return true;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        Object System.Collections.ICollection.SyncRoot
        {
            get
            {
                return this;
            }
        }

        Boolean System.Collections.IDictionary.IsFixedSize
        {
            get
            {
                return true;
            }
        }

        Boolean System.Collections.IDictionary.IsReadOnly
        {
            get
            {
                return true;
            }
        }

        Object System.Collections.IDictionary.this[Object key]
        {
            get
            {
                return this[(TKey)key];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        ICollection System.Collections.IDictionary.Keys
        {
            get
            {
                return new KeysCollectionAccessor<TKey, TValue>(this);
            }
        }

        ICollection System.Collections.IDictionary.Values
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
                return this.comparers.ValueComparer;
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                foreach (KeyValuePair<Int32, ImmutableDictionary<TKey, TValue>.HashBucket> keyValuePair in this.root)
                {
                    foreach (KeyValuePair<TKey, TValue> value in keyValuePair.Value)
                    {
                        yield return value.Value;
                    }
                }
            }
        }

        static ImmutableDictionary()
        {
            ImmutableDictionary<TKey, TValue>.Empty = new ImmutableDictionary<TKey, TValue>(null);
            ImmutableDictionary<TKey, TValue>.FreezeBucketAction = (KeyValuePair<Int32, ImmutableDictionary<TKey, TValue>.HashBucket> kv) => kv.Value.Freeze();
        }

        private ImmutableDictionary(ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, ImmutableDictionary<TKey, TValue>.Comparers comparers, Int32 count)
            : this(Requires.NotNull<ImmutableDictionary<TKey, TValue>.Comparers>(comparers, "comparers"))
        {
            Requires.NotNull<ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node>(root, "root");
            root.Freeze(ImmutableDictionary<TKey, TValue>.FreezeBucketAction);
            this.root = root;
            this.count = count;
        }

        private ImmutableDictionary(ImmutableDictionary<TKey, TValue>.Comparers comparers = null)
        {
            this.comparers = comparers ?? ImmutableDictionary<TKey, TValue>.Comparers.Get(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);
            this.root = ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node.EmptyNode;
        }

        public ImmutableDictionary<TKey, TValue> Add(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            ImmutableDictionary<TKey, TValue>.MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.Add(key, value, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent, new ImmutableDictionary<TKey, TValue>.MutationInput(this));
            return mutationResult.Finalize(this);
        }

        private static ImmutableDictionary<TKey, TValue>.MutationResult Add(TKey key, TValue value, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior behavior, ImmutableDictionary<TKey, TValue>.MutationInput origin)
        {
            ImmutableDictionary<TKey, TValue>.OperationResult operationResult;
            Requires.NotNullAllowStructs<TKey>(key, "key");
            Int32 hashCode = origin.KeyComparer.GetHashCode(key);
            ImmutableDictionary<TKey, TValue>.HashBucket valueOrDefault = origin.Root.GetValueOrDefault(hashCode, Comparer<Int32>.Default);
            ImmutableDictionary<TKey, TValue>.HashBucket tKeys = valueOrDefault.Add(key, value, origin.KeyOnlyComparer, origin.ValueComparer, behavior, out operationResult);
            if (operationResult == ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired)
            {
                return new ImmutableDictionary<TKey, TValue>.MutationResult(origin);
            }
            ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node nums = ImmutableDictionary<TKey, TValue>.UpdateRoot(origin.Root, hashCode, tKeys, origin.HashBucketComparer);
            return new ImmutableDictionary<TKey, TValue>.MutationResult(nums, (operationResult == ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged ? 1 : 0));
        }

        public ImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(pairs, "pairs");
            return this.AddRange(pairs, false);
        }

        private static ImmutableDictionary<TKey, TValue>.MutationResult AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items, ImmutableDictionary<TKey, TValue>.MutationInput origin, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior collisionBehavior = (ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior)2)
        {
            ImmutableDictionary<TKey, TValue>.OperationResult operationResult;
            Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
            Int32 num = 0;
            ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root = origin.Root;
            foreach (KeyValuePair<TKey, TValue> item in items)
            {
                Int32 hashCode = origin.KeyComparer.GetHashCode(item.Key);
                ImmutableDictionary<TKey, TValue>.HashBucket valueOrDefault = root.GetValueOrDefault(hashCode, Comparer<Int32>.Default);
                ImmutableDictionary<TKey, TValue>.HashBucket tKeys = valueOrDefault.Add(item.Key, item.Value, origin.KeyOnlyComparer, origin.ValueComparer, collisionBehavior, out operationResult);
                root = ImmutableDictionary<TKey, TValue>.UpdateRoot(root, hashCode, tKeys, origin.HashBucketComparer);
                if (operationResult != ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged)
                {
                    continue;
                }
                num++;
            }
            return new ImmutableDictionary<TKey, TValue>.MutationResult(root, num);
        }

        private ImmutableDictionary<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs, Boolean avoidToHashMap)
        {
            ImmutableDictionary<TKey, TValue> tKeys;
            Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(pairs, "pairs");
            if (this.IsEmpty && !avoidToHashMap && ImmutableDictionary<TKey, TValue>.TryCastToImmutableMap(pairs, out tKeys))
            {
                return tKeys.WithComparers(this.KeyComparer, this.ValueComparer);
            }
            ImmutableDictionary<TKey, TValue>.MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.AddRange(pairs, this.Origin, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent);
            return mutationResult.Finalize(this);
        }

        public ImmutableDictionary<TKey, TValue> Clear()
        {
            if (this.IsEmpty)
            {
                return this;
            }
            return ImmutableDictionary<TKey, TValue>.EmptyWithComparers(this.comparers);
        }

        public Boolean Contains(KeyValuePair<TKey, TValue> pair)
        {
            return ImmutableDictionary<TKey, TValue>.Contains(pair, this.Origin);
        }

        private static Boolean Contains(KeyValuePair<TKey, TValue> keyValuePair, ImmutableDictionary<TKey, TValue>.MutationInput origin)
        {
            ImmutableDictionary<TKey, TValue>.HashBucket tKeys;
            TValue tValue;
            Int32 hashCode = origin.KeyComparer.GetHashCode(keyValuePair.Key);
            if (!origin.Root.TryGetValue(hashCode, Comparer<Int32>.Default, out tKeys))
            {
                return false;
            }
            if (!tKeys.TryGetValue(keyValuePair.Key, origin.KeyOnlyComparer, out tValue))
            {
                return false;
            }
            return origin.ValueComparer.Equals(tValue, keyValuePair.Value);
        }

        public Boolean ContainsKey(TKey key)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            return ImmutableDictionary<TKey, TValue>.ContainsKey(key, new ImmutableDictionary<TKey, TValue>.MutationInput(this));
        }

        private static Boolean ContainsKey(TKey key, ImmutableDictionary<TKey, TValue>.MutationInput origin)
        {
            ImmutableDictionary<TKey, TValue>.HashBucket tKeys;
            TValue tValue;
            Int32 hashCode = origin.KeyComparer.GetHashCode(key);
            if (!origin.Root.TryGetValue(hashCode, Comparer<Int32>.Default, out tKeys))
            {
                return false;
            }
            return tKeys.TryGetValue(key, origin.KeyOnlyComparer, out tValue);
        }

        public Boolean ContainsValue(TValue value)
        {
            return this.Values.Contains<TValue>(value, this.ValueComparer);
        }

        private static ImmutableDictionary<TKey, TValue> EmptyWithComparers(ImmutableDictionary<TKey, TValue>.Comparers comparers)
        {
            Requires.NotNull<ImmutableDictionary<TKey, TValue>.Comparers>(comparers, "comparers");
            if (ImmutableDictionary<TKey, TValue>.Empty.comparers == comparers)
            {
                return ImmutableDictionary<TKey, TValue>.Empty;
            }
            return new ImmutableDictionary<TKey, TValue>(comparers);
        }

        public ImmutableDictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return new ImmutableDictionary<TKey, TValue>.Enumerator(this.root, null);
        }

        public ImmutableDictionary<TKey, TValue> Remove(TKey key)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            ImmutableDictionary<TKey, TValue>.MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.Remove(key, new ImmutableDictionary<TKey, TValue>.MutationInput(this));
            return mutationResult.Finalize(this);
        }

        private static ImmutableDictionary<TKey, TValue>.MutationResult Remove(TKey key, ImmutableDictionary<TKey, TValue>.MutationInput origin)
        {
            ImmutableDictionary<TKey, TValue>.HashBucket tKeys;
            ImmutableDictionary<TKey, TValue>.OperationResult operationResult;
            Int32 hashCode = origin.KeyComparer.GetHashCode(key);
            if (!origin.Root.TryGetValue(hashCode, Comparer<Int32>.Default, out tKeys))
            {
                return new ImmutableDictionary<TKey, TValue>.MutationResult(origin);
            }
            ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node nums = ImmutableDictionary<TKey, TValue>.UpdateRoot(origin.Root, hashCode, tKeys.Remove(key, origin.KeyOnlyComparer, out operationResult), origin.HashBucketComparer);
            return new ImmutableDictionary<TKey, TValue>.MutationResult(nums, (operationResult == ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged ? -1 : 0));
        }

        public ImmutableDictionary<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
        {
            ImmutableDictionary<TKey, TValue>.HashBucket tKeys;
            ImmutableDictionary<TKey, TValue>.OperationResult operationResult;
            Requires.NotNull<IEnumerable<TKey>>(keys, "keys");
            Int32 num = this.count;
            ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node nums = this.root;
            foreach (TKey key in keys)
            {
                Int32 hashCode = this.KeyComparer.GetHashCode(key);
                if (!nums.TryGetValue(hashCode, Comparer<Int32>.Default, out tKeys))
                {
                    continue;
                }
                ImmutableDictionary<TKey, TValue>.HashBucket tKeys1 = tKeys.Remove(key, this.comparers.KeyOnlyComparer, out operationResult);
                nums = ImmutableDictionary<TKey, TValue>.UpdateRoot(nums, hashCode, tKeys1, this.comparers.HashBucketEqualityComparer);
                if (operationResult != ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged)
                {
                    continue;
                }
                num--;
            }
            return this.Wrap(nums, num);
        }

        public ImmutableDictionary<TKey, TValue> SetItem(TKey key, TValue value)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            ImmutableDictionary<TKey, TValue>.MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.Add(key, value, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.SetValue, new ImmutableDictionary<TKey, TValue>.MutationInput(this));
            return mutationResult.Finalize(this);
        }

        public ImmutableDictionary<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Requires.NotNull<IEnumerable<KeyValuePair<TKey, TValue>>>(items, "items");
            ImmutableDictionary<TKey, TValue>.MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.AddRange(items, this.Origin, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.SetValue);
            return mutationResult.Finalize(this);
        }

        void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.Clear()
        {
            throw new NotSupportedException();
        }

        void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, Int32 arrayIndex)
        {
            Requires.NotNull<KeyValuePair<TKey, TValue>[]>(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex", null);
            Requires.Range((Int32)array.Length >= arrayIndex + this.Count, "arrayIndex", null);
            foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
            {
                Int32 num = arrayIndex;
                arrayIndex = num + 1;
                array[num] = keyValuePair;
            }
        }

        Boolean System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        void System.Collections.Generic.IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw new NotSupportedException();
        }

        Boolean System.Collections.Generic.IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        void System.Collections.ICollection.CopyTo(Array array, Int32 arrayIndex)
        {
            Requires.NotNull<Array>(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex", null);
            Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
            foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
            {
                Object dictionaryEntry = new DictionaryEntry((Object)keyValuePair.Key, (Object)keyValuePair.Value);
                Int32[] numArray = new Int32[1];
                Int32 num = arrayIndex;
                arrayIndex = num + 1;
                numArray[0] = num;
                array.SetValue(dictionaryEntry, numArray);
            }
        }

        void System.Collections.IDictionary.Add(Object key, Object value)
        {
            throw new NotSupportedException();
        }

        void System.Collections.IDictionary.Clear()
        {
            throw new NotSupportedException();
        }

        Boolean System.Collections.IDictionary.Contains(Object key)
        {
            return this.ContainsKey((TKey)key);
        }

        IDictionaryEnumerator System.Collections.IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator<TKey, TValue>(this.GetEnumerator());

        }

        void System.Collections.IDictionary.Remove(Object key)
        {
            throw new NotSupportedException();
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IImmutableDictionary<TKey, TValue> System.Collections.Immutable.IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            return this.Add(key, value);
        }

        IImmutableDictionary<TKey, TValue> System.Collections.Immutable.IImmutableDictionary<TKey, TValue>.AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
        {
            return this.AddRange(pairs);
        }

        IImmutableDictionary<TKey, TValue> System.Collections.Immutable.IImmutableDictionary<TKey, TValue>.Clear()
        {
            return this.Clear();
        }

        IImmutableDictionary<TKey, TValue> System.Collections.Immutable.IImmutableDictionary<TKey, TValue>.Remove(TKey key)
        {
            return this.Remove(key);
        }

        IImmutableDictionary<TKey, TValue> System.Collections.Immutable.IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys)
        {
            return this.RemoveRange(keys);
        }

        IImmutableDictionary<TKey, TValue> System.Collections.Immutable.IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value)
        {
            return this.SetItem(key, value);
        }

        IImmutableDictionary<TKey, TValue> System.Collections.Immutable.IImmutableDictionary<TKey, TValue>.SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return this.SetItems(items);
        }

        public ImmutableDictionary<TKey, TValue>.Builder ToBuilder()
        {
            return new ImmutableDictionary<TKey, TValue>.Builder(this);
        }

        private static Boolean TryCastToImmutableMap(IEnumerable<KeyValuePair<TKey, TValue>> sequence, out ImmutableDictionary<TKey, TValue> other)
        {
            other = sequence as ImmutableDictionary<TKey, TValue>;
            if (other != null)
            {
                return true;
            }
            ImmutableDictionary<TKey, TValue>.Builder tKeys = sequence as ImmutableDictionary<TKey, TValue>.Builder;
            if (tKeys == null)
            {
                return false;
            }
            other = tKeys.ToImmutable();
            return true;
        }

        public Boolean TryGetKey(TKey equalKey, out TKey actualKey)
        {
            Requires.NotNullAllowStructs<TKey>(equalKey, "equalKey");
            return ImmutableDictionary<TKey, TValue>.TryGetKey(equalKey, this.Origin, out actualKey);
        }

        private static Boolean TryGetKey(TKey equalKey, ImmutableDictionary<TKey, TValue>.MutationInput origin, out TKey actualKey)
        {
            ImmutableDictionary<TKey, TValue>.HashBucket tKeys;
            Int32 hashCode = origin.KeyComparer.GetHashCode(equalKey);
            if (!origin.Root.TryGetValue(hashCode, Comparer<Int32>.Default, out tKeys))
            {
                actualKey = equalKey;
                return false;
            }
            return tKeys.TryGetKey(equalKey, origin.KeyOnlyComparer, out actualKey);
        }

        public Boolean TryGetValue(TKey key, out TValue value)
        {
            Requires.NotNullAllowStructs<TKey>(key, "key");
            return ImmutableDictionary<TKey, TValue>.TryGetValue(key, this.Origin, out value);
        }

        private static Boolean TryGetValue(TKey key, ImmutableDictionary<TKey, TValue>.MutationInput origin, out TValue value)
        {
            ImmutableDictionary<TKey, TValue>.HashBucket tKeys;
            Int32 hashCode = origin.KeyComparer.GetHashCode(key);
            if (!origin.Root.TryGetValue(hashCode, Comparer<Int32>.Default, out tKeys))
            {
                value = default(TValue);
                return false;
            }
            return tKeys.TryGetValue(key, origin.KeyOnlyComparer, out value);
        }

        private static ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node UpdateRoot(ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, Int32 hashCode, ImmutableDictionary<TKey, TValue>.HashBucket newBucket, IEqualityComparer<ImmutableDictionary<TKey, TValue>.HashBucket> hashBucketComparer)
        {
            Boolean flag;
            Boolean flag1;
            if (newBucket.IsEmpty)
            {
                return root.Remove(hashCode, Comparer<Int32>.Default, out flag);
            }
            return root.SetItem(hashCode, newBucket, Comparer<Int32>.Default, hashBucketComparer, out flag1, out flag);
        }

        public ImmutableDictionary<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            if (keyComparer == null)
            {
                keyComparer = EqualityComparer<TKey>.Default;
            }
            if (valueComparer == null)
            {
                valueComparer = EqualityComparer<TValue>.Default;
            }
            if (this.KeyComparer != keyComparer)
            {
                ImmutableDictionary<TKey, TValue>.Comparers comparer = ImmutableDictionary<TKey, TValue>.Comparers.Get(keyComparer, valueComparer);
                return (new ImmutableDictionary<TKey, TValue>(comparer)).AddRange(this, true);
            }
            if (this.ValueComparer == valueComparer)
            {
                return this;
            }
            ImmutableDictionary<TKey, TValue>.Comparers comparer1 = this.comparers.WithValueComparer(valueComparer);
            return new ImmutableDictionary<TKey, TValue>(this.root, comparer1, this.count);
        }

        public ImmutableDictionary<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer)
        {
            return this.WithComparers(keyComparer, this.comparers.ValueComparer);
        }

        private static ImmutableDictionary<TKey, TValue> Wrap(ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, ImmutableDictionary<TKey, TValue>.Comparers comparers, Int32 count)
        {
            Requires.NotNull<ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node>(root, "root");
            Requires.NotNull<ImmutableDictionary<TKey, TValue>.Comparers>(comparers, "comparers");
            Requires.Range(count >= 0, "count", null);
            return new ImmutableDictionary<TKey, TValue>(root, comparers, count);
        }

        private ImmutableDictionary<TKey, TValue> Wrap(ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, Int32 adjustedCountIfDifferentRoot)
        {
            if (root == null)
            {
                return this.Clear();
            }
            if (this.root == root)
            {
                return this;
            }
            if (root.IsEmpty)
            {
                return this.Clear();
            }
            return new ImmutableDictionary<TKey, TValue>(root, this.comparers, adjustedCountIfDifferentRoot);
        }

        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy(typeof(ImmutableDictionary<,>.Builder.DebuggerProxy))]
        public sealed class Builder : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IDictionary, ICollection, IEnumerable
        {
            private ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root;

            private ImmutableDictionary<TKey, TValue>.Comparers comparers;

            private Int32 count;

            private ImmutableDictionary<TKey, TValue> immutable;

            private Int32 version;

            private Object syncRoot;

            public Int32 Count
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
                    TValue tValue;
                    if (!this.TryGetValue(key, out tValue))
                    {
                        throw new KeyNotFoundException();
                    }
                    return tValue;
                }
                set
                {
                    ImmutableDictionary<TKey, TValue>.MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.Add(key, value, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.SetValue, this.Origin);
                    this.Apply(mutationResult);
                }
            }

            public IEqualityComparer<TKey> KeyComparer
            {
                get
                {
                    return this.comparers.KeyComparer;
                }
                set
                {
                    Requires.NotNull<IEqualityComparer<TKey>>(value, "value");
                    if (value != this.KeyComparer)
                    {
                        ImmutableDictionary<TKey, TValue>.Comparers comparer = ImmutableDictionary<TKey, TValue>.Comparers.Get(value, this.ValueComparer);
                        ImmutableDictionary<TKey, TValue>.MutationInput mutationInput = new ImmutableDictionary<TKey, TValue>.MutationInput(ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node.EmptyNode, comparer, 0);
                        ImmutableDictionary<TKey, TValue>.MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.AddRange(this, mutationInput, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent);
                        this.immutable = null;
                        this.comparers = comparer;
                        this.count = mutationResult.CountAdjustment;
                        this.Root = mutationResult.Root;
                    }
                }
            }

            public IEnumerable<TKey> Keys
            {
                get
                {
                    return this.root.Values.SelectMany<ImmutableDictionary<TKey, TValue>.HashBucket, KeyValuePair<TKey, TValue>>((ImmutableDictionary<TKey, TValue>.HashBucket b) => b).Select<KeyValuePair<TKey, TValue>, TKey>((KeyValuePair<TKey, TValue> kv) => kv.Key);
                }
            }

            private ImmutableDictionary<TKey, TValue>.MutationInput Origin
            {
                get
                {
                    return new ImmutableDictionary<TKey, TValue>.MutationInput(this.Root, this.comparers, this.count);
                }
            }

            private ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node Root
            {
                get
                {
                    return this.root;
                }
                set
                {
                    ImmutableDictionary<TKey, TValue>.Builder builder = this;
                    builder.version = builder.version + 1;
                    if (this.root != value)
                    {
                        this.root = value;
                        this.immutable = null;
                    }
                }
            }

            Boolean System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            ICollection<TKey> System.Collections.Generic.IDictionary<TKey, TValue>.Keys
            {
                get
                {
                    return this.Keys.ToArray<TKey>(this.Count);
                }
            }

            ICollection<TValue> System.Collections.Generic.IDictionary<TKey, TValue>.Values
            {
                get
                {
                    return this.Values.ToArray<TValue>(this.Count);
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Boolean System.Collections.ICollection.IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            Object System.Collections.ICollection.SyncRoot
            {
                get
                {
                    if (this.syncRoot == null)
                    {
                        Interlocked.CompareExchange<Object>(ref this.syncRoot, new Object(), null);
                    }
                    return this.syncRoot;
                }
            }

            Boolean System.Collections.IDictionary.IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            Boolean System.Collections.IDictionary.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            Object System.Collections.IDictionary.this[Object key]
            {
                get
                {
                    return this[(TKey)key];
                }
                set
                {
                    this[(TKey)key] = (TValue)value;
                }
            }

            ICollection System.Collections.IDictionary.Keys
            {
                get
                {
                    return this.Keys.ToArray<TKey>(this.Count);
                }
            }

            ICollection System.Collections.IDictionary.Values
            {
                get
                {
                    return this.Values.ToArray<TValue>(this.Count);
                }
            }

            public IEqualityComparer<TValue> ValueComparer
            {
                get
                {
                    return this.comparers.ValueComparer;
                }
                set
                {
                    Requires.NotNull<IEqualityComparer<TValue>>(value, "value");
                    if (value != this.ValueComparer)
                    {
                        this.comparers = this.comparers.WithValueComparer(value);
                        this.immutable = null;
                    }
                }
            }

            public IEnumerable<TValue> Values
            {
                get
                {
                    return this.root.Values.SelectMany<ImmutableDictionary<TKey, TValue>.HashBucket, KeyValuePair<TKey, TValue>>((ImmutableDictionary<TKey, TValue>.HashBucket b) => b).Select<KeyValuePair<TKey, TValue>, TValue>((KeyValuePair<TKey, TValue> kv) => kv.Value).ToArray<TValue>(this.Count);
                }
            }

            internal Int32 Version
            {
                get
                {
                    return this.version;
                }
            }

            internal Builder(ImmutableDictionary<TKey, TValue> map)
            {
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(map, "map");
                this.root = map.root;
                this.count = map.count;
                this.comparers = map.comparers;
                this.immutable = map;
            }

            public void Add(TKey key, TValue value)
            {
                ImmutableDictionary<TKey, TValue>.MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.Add(key, value, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent, this.Origin);
                this.Apply(mutationResult);
            }

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                this.Add(item.Key, item.Value);
            }

            public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
            {
                ImmutableDictionary<TKey, TValue>.MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.AddRange(items, this.Origin, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent);
                this.Apply(mutationResult);
            }

            private Boolean Apply(ImmutableDictionary<TKey, TValue>.MutationResult result)
            {
                this.Root = result.Root;
                ImmutableDictionary<TKey, TValue>.Builder countAdjustment = this;
                countAdjustment.count = countAdjustment.count + result.CountAdjustment;
                return result.CountAdjustment != 0;
            }

            public void Clear()
            {
                this.Root = ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node.EmptyNode;
                this.count = 0;
            }

            public Boolean Contains(KeyValuePair<TKey, TValue> item)
            {
                return ImmutableDictionary<TKey, TValue>.Contains(item, this.Origin);
            }

            public Boolean ContainsKey(TKey key)
            {
                return ImmutableDictionary<TKey, TValue>.ContainsKey(key, this.Origin);
            }

            public Boolean ContainsValue(TValue value)
            {
                return this.Values.Contains<TValue>(value, this.ValueComparer);
            }

            public ImmutableDictionary<TKey, TValue>.Enumerator GetEnumerator()
            {
                return new ImmutableDictionary<TKey, TValue>.Enumerator(this.root, this);
            }

            public TValue GetValueOrDefault(TKey key)
            {
                return this.GetValueOrDefault(key, default(TValue));
            }

            public TValue GetValueOrDefault(TKey key, TValue defaultValue)
            {
                TValue tValue;
                Requires.NotNullAllowStructs<TKey>(key, "key");
                if (this.TryGetValue(key, out tValue))
                {
                    return tValue;
                }
                return defaultValue;
            }

            public Boolean Remove(TKey key)
            {
                ImmutableDictionary<TKey, TValue>.MutationResult mutationResult = ImmutableDictionary<TKey, TValue>.Remove(key, this.Origin);
                return this.Apply(mutationResult);
            }

            public Boolean Remove(KeyValuePair<TKey, TValue> item)
            {
                if (!this.Contains(item))
                {
                    return false;
                }
                return this.Remove(item.Key);
            }

            public void RemoveRange(IEnumerable<TKey> keys)
            {
                Requires.NotNull<IEnumerable<TKey>>(keys, "keys");
                foreach (TKey key in keys)
                {
                    this.Remove(key);
                }
            }

            void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, Int32 arrayIndex)
            {
                Requires.NotNull<KeyValuePair<TKey, TValue>[]>(array, "array");
                foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
                {
                    Int32 num = arrayIndex;
                    arrayIndex = num + 1;
                    array[num] = keyValuePair;
                }
            }

            IEnumerator<KeyValuePair<TKey, TValue>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            void System.Collections.ICollection.CopyTo(Array array, Int32 arrayIndex)
            {
                Requires.NotNull<Array>(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex", null);
                Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
                foreach (KeyValuePair<TKey, TValue> keyValuePair in this)
                {
                    Object dictionaryEntry = new DictionaryEntry((Object)keyValuePair.Key, (Object)keyValuePair.Value);
                    Int32[] numArray = new Int32[1];
                    Int32 num = arrayIndex;
                    arrayIndex = num + 1;
                    numArray[0] = num;
                    array.SetValue(dictionaryEntry, numArray);
                }
            }

            void System.Collections.IDictionary.Add(Object key, Object value)
            {
                this.Add((TKey)key, (TValue)value);
            }

            Boolean System.Collections.IDictionary.Contains(Object key)
            {
                return this.ContainsKey((TKey)key);
            }

            IDictionaryEnumerator System.Collections.IDictionary.GetEnumerator()
            {
                return new DictionaryEnumerator<TKey, TValue>(this.GetEnumerator());
            }

            void System.Collections.IDictionary.Remove(Object key)
            {
                this.Remove((TKey)key);
            }

            IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public ImmutableDictionary<TKey, TValue> ToImmutable()
            {
                if (this.immutable == null)
                {
                    this.immutable = ImmutableDictionary<TKey, TValue>.Wrap(this.root, this.comparers, this.count);
                }
                return this.immutable;
            }

            public Boolean TryGetKey(TKey equalKey, out TKey actualKey)
            {
                return ImmutableDictionary<TKey, TValue>.TryGetKey(equalKey, this.Origin, out actualKey);
            }

            public Boolean TryGetValue(TKey key, out TValue value)
            {
                return ImmutableDictionary<TKey, TValue>.TryGetValue(key, this.Origin, out value);
            }

            private class DebuggerProxy
            {
                private readonly ImmutableDictionary<TKey, TValue>.Builder map;

                private KeyValuePair<TKey, TValue>[] contents;

                [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
                public KeyValuePair<TKey, TValue>[] Contents
                {
                    get
                    {
                        if (this.contents == null)
                        {
                            this.contents = this.map.ToArray<KeyValuePair<TKey, TValue>>(this.map.Count);
                        }
                        return this.contents;
                    }
                }

                public DebuggerProxy(ImmutableDictionary<TKey, TValue>.Builder map)
                {
                    Requires.NotNull<ImmutableDictionary<TKey, TValue>.Builder>(map, "map");
                    this.map = map;
                }
            }
        }

        internal class Comparers : IEqualityComparer<ImmutableDictionary<TKey, TValue>.HashBucket>, IEqualityComparer<KeyValuePair<TKey, TValue>>
        {
            internal readonly static ImmutableDictionary<TKey, TValue>.Comparers Default;

            private readonly IEqualityComparer<TKey> keyComparer;

            private readonly IEqualityComparer<TValue> valueComparer;

            internal IEqualityComparer<ImmutableDictionary<TKey, TValue>.HashBucket> HashBucketEqualityComparer
            {
                get
                {
                    return this;
                }
            }

            internal IEqualityComparer<TKey> KeyComparer
            {
                get
                {
                    return this.keyComparer;
                }
            }

            internal IEqualityComparer<KeyValuePair<TKey, TValue>> KeyOnlyComparer
            {
                get
                {
                    return this;
                }
            }

            internal IEqualityComparer<TValue> ValueComparer
            {
                get
                {
                    return this.valueComparer;
                }
            }

            static Comparers()
            {
                ImmutableDictionary<TKey, TValue>.Comparers.Default = new ImmutableDictionary<TKey, TValue>.Comparers(EqualityComparer<TKey>.Default, EqualityComparer<TValue>.Default);
            }

            internal Comparers(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNull<IEqualityComparer<TKey>>(keyComparer, "keyComparer");
                Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
                this.keyComparer = keyComparer;
                this.valueComparer = valueComparer;
            }

            public Boolean Equals(ImmutableDictionary<TKey, TValue>.HashBucket x, ImmutableDictionary<TKey, TValue>.HashBucket y)
            {
                if (!Object.ReferenceEquals(x.AdditionalElements, y.AdditionalElements) || !this.KeyComparer.Equals(x.FirstValue.Key, y.FirstValue.Key))
                {
                    return false;
                }
                IEqualityComparer<TValue> valueComparer = this.ValueComparer;
                KeyValuePair<TKey, TValue> firstValue = x.FirstValue;
                return valueComparer.Equals(firstValue.Value, y.FirstValue.Value);
            }

            internal static ImmutableDictionary<TKey, TValue>.Comparers Get(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNull<IEqualityComparer<TKey>>(keyComparer, "keyComparer");
                Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
                if (keyComparer == ImmutableDictionary<TKey, TValue>.Comparers.Default.KeyComparer && valueComparer == ImmutableDictionary<TKey, TValue>.Comparers.Default.ValueComparer)
                {
                    return ImmutableDictionary<TKey, TValue>.Comparers.Default;
                }
                return new ImmutableDictionary<TKey, TValue>.Comparers(keyComparer, valueComparer);
            }

            public Int32 GetHashCode(ImmutableDictionary<TKey, TValue>.HashBucket obj)
            {
                return this.KeyComparer.GetHashCode(obj.FirstValue.Key);
            }

            Boolean System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<TKey, TValue>>.Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return this.keyComparer.Equals(x.Key, y.Key);
            }

            Int32 System.Collections.Generic.IEqualityComparer<System.Collections.Generic.KeyValuePair<TKey, TValue>>.GetHashCode(KeyValuePair<TKey, TValue> obj)
            {
                return this.keyComparer.GetHashCode(obj.Key);
            }

            internal ImmutableDictionary<TKey, TValue>.Comparers WithValueComparer(IEqualityComparer<TValue> valueComparer)
            {
                Requires.NotNull<IEqualityComparer<TValue>>(valueComparer, "valueComparer");
                if (this.valueComparer == valueComparer)
                {
                    return this;
                }
                return ImmutableDictionary<TKey, TValue>.Comparers.Get(this.KeyComparer, valueComparer);
            }
        }

        private class DebuggerProxy
        {
            private readonly ImmutableDictionary<TKey, TValue> map;

            private KeyValuePair<TKey, TValue>[] contents;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public KeyValuePair<TKey, TValue>[] Contents
            {
                get
                {
                    if (this.contents == null)
                    {
                        this.contents = this.map.ToArray<KeyValuePair<TKey, TValue>>(this.map.Count);
                    }
                    return this.contents;
                }
            }

            public DebuggerProxy(ImmutableDictionary<TKey, TValue> map)
            {
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(map, "map");
                this.map = map;
            }
        }

        public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable
        {
            private readonly ImmutableDictionary<TKey, TValue>.Builder builder;

            private ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Enumerator mapEnumerator;

            private ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator bucketEnumerator;

            private Int32 enumeratingBuilderVersion;

            public KeyValuePair<TKey, TValue> Current
            {
                get
                {
                    this.mapEnumerator.ThrowIfDisposed();
                    return this.bucketEnumerator.Current;
                }
            }

            Object System.Collections.IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            internal Enumerator(ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, ImmutableDictionary<TKey, TValue>.Builder builder = null)
            {
                this.builder = builder;
                this.mapEnumerator = new ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Enumerator(root, null);
                this.bucketEnumerator = new ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator();
                this.enumeratingBuilderVersion = (builder != null ? builder.Version : -1);
            }

            public void Dispose()
            {
                this.mapEnumerator.Dispose();
                this.bucketEnumerator.Dispose();
            }

            public Boolean MoveNext()
            {
                this.ThrowIfChanged();
                if (this.bucketEnumerator.MoveNext())
                {
                    return true;
                }
                if (!this.mapEnumerator.MoveNext())
                {
                    return false;
                }
                this.bucketEnumerator = new ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator(this.mapEnumerator.Current.Value);
                return this.bucketEnumerator.MoveNext();
            }

            public void Reset()
            {
                this.enumeratingBuilderVersion = (this.builder != null ? this.builder.Version : -1);
                this.mapEnumerator.Reset();
                this.bucketEnumerator.Dispose();
                this.bucketEnumerator = new ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator();
            }

            private void ThrowIfChanged()
            {
                if (this.builder != null && this.builder.Version != this.enumeratingBuilderVersion)
                {
                    throw new InvalidOperationException(Strings.CollectionModifiedDuringEnumeration);
                }
            }
        }

        internal struct HashBucket : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IEquatable<ImmutableDictionary<TKey, TValue>.HashBucket>
        {
            private readonly KeyValuePair<TKey, TValue> firstValue;

            private readonly ImmutableList<KeyValuePair<TKey, TValue>>.Node additionalElements;

            internal ImmutableList<KeyValuePair<TKey, TValue>>.Node AdditionalElements
            {
                get
                {
                    return this.additionalElements;
                }
            }

            internal KeyValuePair<TKey, TValue> FirstValue
            {
                get
                {
                    if (this.IsEmpty)
                    {
                        throw new InvalidOperationException();
                    }
                    return this.firstValue;
                }
            }

            internal Boolean IsEmpty
            {
                get
                {
                    return this.additionalElements == null;
                }
            }

            private HashBucket(KeyValuePair<TKey, TValue> firstElement, ImmutableList<KeyValuePair<TKey, TValue>>.Node additionalElements = null)
            {
                this.firstValue = firstElement;
                Object emptyNode = additionalElements;
                if (emptyNode == null)
                {
                    emptyNode = ImmutableList<KeyValuePair<TKey, TValue>>.Node.EmptyNode;
                }
                this.additionalElements = (ImmutableList<KeyValuePair<TKey, TValue>>.Node)emptyNode;
            }

            internal ImmutableDictionary<TKey, TValue>.HashBucket Add(TKey key, TValue value, IEqualityComparer<KeyValuePair<TKey, TValue>> keyOnlyComparer, IEqualityComparer<TValue> valueComparer, ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior behavior, out ImmutableDictionary<TKey, TValue>.OperationResult result)
            {
                KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, value);
                if (this.IsEmpty)
                {
                    result = ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged;
                    return new ImmutableDictionary<TKey, TValue>.HashBucket(keyValuePair, null);
                }
                if (keyOnlyComparer.Equals(keyValuePair, this.firstValue))
                {
                    switch (behavior)
                    {
                        case (ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior)ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.SetValue:
                            {
                                result = ImmutableDictionary<TKey, TValue>.OperationResult.AppliedWithoutSizeChange;
                                return new ImmutableDictionary<TKey, TValue>.HashBucket(keyValuePair, this.additionalElements);
                            }
                        case (ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior)ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.Skip:
                            {
                                result = ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired;
                                return this;
                            }
                        case (ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior)ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent:
                            {
                                if (!valueComparer.Equals(this.firstValue.Value, value))
                                {
                                    throw new ArgumentException(Strings.DuplicateKey);
                                }
                                result = ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired;
                                return this;
                            }
                        case (ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior)ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowAlways:
                            {
                                throw new ArgumentException(Strings.DuplicateKey);
                            }
                    }
                    throw new InvalidOperationException();
                }
                Int32 num = this.additionalElements.IndexOf(keyValuePair, keyOnlyComparer);
                if (num < 0)
                {
                    result = ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged;
                    return new ImmutableDictionary<TKey, TValue>.HashBucket(this.firstValue, this.additionalElements.Add(keyValuePair));
                }
                switch (behavior)
                {
                    case (ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior)ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.SetValue:
                        {
                            result = ImmutableDictionary<TKey, TValue>.OperationResult.AppliedWithoutSizeChange;
                            return new ImmutableDictionary<TKey, TValue>.HashBucket(this.firstValue, this.additionalElements.ReplaceAt(num, keyValuePair));
                        }
                    case (ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior)ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.Skip:
                        {
                            result = ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired;
                            return this;
                        }
                    case (ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior)ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowIfValueDifferent:
                        {
                            if (!valueComparer.Equals(this.additionalElements[num].Value, value))
                            {
                                throw new ArgumentException(Strings.DuplicateKey);
                            }
                            result = ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired;
                            return this;
                        }
                    case (ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior)ImmutableDictionary<TKey, TValue>.KeyCollisionBehavior.ThrowAlways:
                        {
                            throw new ArgumentException(Strings.DuplicateKey);
                        }
                }
                throw new InvalidOperationException();
            }

            internal void Freeze()
            {
                if (this.additionalElements != null)
                {
                    this.additionalElements.Freeze();
                }
            }

            public ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator GetEnumerator()
            {
                return new ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator(this);
            }

            internal ImmutableDictionary<TKey, TValue>.HashBucket Remove(TKey key, IEqualityComparer<KeyValuePair<TKey, TValue>> keyOnlyComparer, out ImmutableDictionary<TKey, TValue>.OperationResult result)
            {
                if (this.IsEmpty)
                {
                    result = ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired;
                    return this;
                }
                KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, default(TValue));
                if (keyOnlyComparer.Equals(this.firstValue, keyValuePair))
                {
                    if (this.additionalElements.IsEmpty)
                    {
                        result = ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged;
                        return new ImmutableDictionary<TKey, TValue>.HashBucket();
                    }
                    Int32 count = ((IBinaryTree<KeyValuePair<TKey, TValue>>)this.additionalElements).Left.Count;
                    result = ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged;
                    return new ImmutableDictionary<TKey, TValue>.HashBucket(this.additionalElements.Key, this.additionalElements.RemoveAt(count));
                }
                Int32 num = this.additionalElements.IndexOf(keyValuePair, keyOnlyComparer);
                if (num < 0)
                {
                    result = ImmutableDictionary<TKey, TValue>.OperationResult.NoChangeRequired;
                    return this;
                }
                result = ImmutableDictionary<TKey, TValue>.OperationResult.SizeChanged;
                return new ImmutableDictionary<TKey, TValue>.HashBucket(this.firstValue, this.additionalElements.RemoveAt(num));
            }

            IEnumerator<KeyValuePair<TKey, TValue>> System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            Boolean System.IEquatable<System.Collections.Immutable.ImmutableDictionary<TKey, TValue>.HashBucket>.Equals(ImmutableDictionary<TKey, TValue>.HashBucket other)
            {
                throw new Exception();
            }

            internal Boolean TryGetKey(TKey equalKey, IEqualityComparer<KeyValuePair<TKey, TValue>> keyOnlyComparer, out TKey actualKey)
            {
                if (this.IsEmpty)
                {
                    actualKey = equalKey;
                    return false;
                }
                KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(equalKey, default(TValue));
                if (keyOnlyComparer.Equals(this.firstValue, keyValuePair))
                {
                    actualKey = this.firstValue.Key;
                    return true;
                }
                Int32 num = this.additionalElements.IndexOf(keyValuePair, keyOnlyComparer);
                if (num < 0)
                {
                    actualKey = equalKey;
                    return false;
                }
                actualKey = this.additionalElements[num].Key;
                return true;
            }

            internal Boolean TryGetValue(TKey key, IEqualityComparer<KeyValuePair<TKey, TValue>> keyOnlyComparer, out TValue value)
            {
                if (this.IsEmpty)
                {
                    value = default(TValue);
                    return false;
                }
                KeyValuePair<TKey, TValue> keyValuePair = new KeyValuePair<TKey, TValue>(key, default(TValue));
                if (keyOnlyComparer.Equals(this.firstValue, keyValuePair))
                {
                    value = this.firstValue.Value;
                    return true;
                }
                Int32 num = this.additionalElements.IndexOf(keyValuePair, keyOnlyComparer);
                if (num < 0)
                {
                    value = default(TValue);
                    return false;
                }
                value = this.additionalElements[num].Value;
                return true;
            }

            internal struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable
            {
                private readonly ImmutableDictionary<TKey, TValue>.HashBucket bucket;

                private ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position currentPosition;

                private ImmutableList<KeyValuePair<TKey, TValue>>.Enumerator additionalEnumerator;

                public KeyValuePair<TKey, TValue> Current
                {
                    get
                    {
                        switch (this.currentPosition)
                        {
                            case (ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position)ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.First:
                                {
                                    return this.bucket.firstValue;
                                }
                            case (ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position)ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.Additional:
                                {
                                    return this.additionalEnumerator.Current;
                                }
                        }
                        throw new InvalidOperationException();
                    }
                }

                Object System.Collections.IEnumerator.Current
                {
                    get
                    {
                        return this.Current;
                    }
                }

                internal Enumerator(ImmutableDictionary<TKey, TValue>.HashBucket bucket)
                {
                    this.bucket = bucket;
                    this.currentPosition = ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.BeforeFirst;
                    this.additionalEnumerator = new ImmutableList<KeyValuePair<TKey, TValue>>.Enumerator();
                }

                public void Dispose()
                {
                    this.additionalEnumerator.Dispose();
                }

                public Boolean MoveNext()
                {
                    if (this.bucket.IsEmpty)
                    {
                        this.currentPosition = ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.End;
                        return false;
                    }
                    switch (this.currentPosition)
                    {
                        case (ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position)ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.BeforeFirst:
                            {
                                this.currentPosition = ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.First;
                                return true;
                            }
                        case (ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position)ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.First:
                            {
                                if (this.bucket.additionalElements.IsEmpty)
                                {
                                    this.currentPosition = ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.End;
                                    return false;
                                }
                                this.currentPosition = ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.Additional;
                                this.additionalEnumerator = new ImmutableList<KeyValuePair<TKey, TValue>>.Enumerator(this.bucket.additionalElements, null, -1, -1, false);
                                return this.additionalEnumerator.MoveNext();
                            }
                        case (ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position)ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.Additional:
                            {
                                return this.additionalEnumerator.MoveNext();
                            }
                        case (ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position)ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.End:
                            {
                                return false;
                            }
                    }
                    throw new InvalidOperationException();
                }

                public void Reset()
                {
                    this.additionalEnumerator.Dispose();
                    this.currentPosition = ImmutableDictionary<TKey, TValue>.HashBucket.Enumerator.Position.BeforeFirst;
                }

                private enum Position
                {
                    BeforeFirst,
                    First,
                    Additional,
                    End
                }
            }
        }

        internal enum KeyCollisionBehavior
        {
            SetValue,
            Skip,
            ThrowIfValueDifferent,
            ThrowAlways
        }

        private struct MutationInput
        {
            private readonly ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root;

            private readonly ImmutableDictionary<TKey, TValue>.Comparers comparers;

            private readonly Int32 count;

            internal Int32 Count
            {
                get
                {
                    return this.count;
                }
            }

            internal IEqualityComparer<ImmutableDictionary<TKey, TValue>.HashBucket> HashBucketComparer
            {
                get
                {
                    return this.comparers.HashBucketEqualityComparer;
                }
            }

            internal IEqualityComparer<TKey> KeyComparer
            {
                get
                {
                    return this.comparers.KeyComparer;
                }
            }

            internal IEqualityComparer<KeyValuePair<TKey, TValue>> KeyOnlyComparer
            {
                get
                {
                    return this.comparers.KeyOnlyComparer;
                }
            }

            internal ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node Root
            {
                get
                {
                    return this.root;
                }
            }

            internal IEqualityComparer<TValue> ValueComparer
            {
                get
                {
                    return this.comparers.ValueComparer;
                }
            }

            internal MutationInput(ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, ImmutableDictionary<TKey, TValue>.Comparers comparers, Int32 count)
            {
                this.root = root;
                this.comparers = comparers;
                this.count = count;
            }

            internal MutationInput(ImmutableDictionary<TKey, TValue> map)
            {
                this.root = map.root;
                this.comparers = map.comparers;
                this.count = map.count;
            }
        }

        private struct MutationResult
        {
            private readonly ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root;

            private readonly Int32 countAdjustment;

            internal Int32 CountAdjustment
            {
                get
                {
                    return this.countAdjustment;
                }
            }

            internal ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node Root
            {
                get
                {
                    return this.root;
                }
            }

            internal MutationResult(ImmutableDictionary<TKey, TValue>.MutationInput unchangedInput)
            {
                this.root = unchangedInput.Root;
                this.countAdjustment = 0;
            }

            internal MutationResult(ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node root, Int32 countAdjustment)
            {
                Requires.NotNull<ImmutableSortedDictionary<Int32, ImmutableDictionary<TKey, TValue>.HashBucket>.Node>(root, "root");
                this.root = root;
                this.countAdjustment = countAdjustment;
            }

            internal ImmutableDictionary<TKey, TValue> Finalize(ImmutableDictionary<TKey, TValue> priorMap)
            {
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(priorMap, "priorMap");
                return priorMap.Wrap(this.Root, priorMap.count + this.CountAdjustment);
            }
        }

        internal enum OperationResult
        {
            AppliedWithoutSizeChange,
            SizeChanged,
            NoChangeRequired
        }
    }
}