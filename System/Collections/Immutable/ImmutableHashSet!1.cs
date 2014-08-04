using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Generic.V40;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Validation;

namespace System.Collections.Immutable
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof(ImmutableHashSet<>.DebuggerProxy))]
    public sealed class ImmutableHashSet<T> : IImmutableSet<T>, IHashKeyCollection<T>, IReadOnlyCollection<T>, ISet<T>, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable
    {
        public readonly static ImmutableHashSet<T> Empty;

        private readonly static Action<KeyValuePair<Int32, ImmutableHashSet<T>.HashBucket>> FreezeBucketAction;

        private readonly IEqualityComparer<T> equalityComparer;

        private readonly Int32 count;

        private readonly ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root;

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

        public IEqualityComparer<T> KeyComparer
        {
            get
            {
                return this.equalityComparer;
            }
        }

        private ImmutableHashSet<T>.MutationInput Origin
        {
            get
            {
                return new ImmutableHashSet<T>.MutationInput(this);
            }
        }

        Boolean System.Collections.Generic.ICollection<T>.IsReadOnly
        {
            get
            {
                return true;
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

        static ImmutableHashSet()
        {
            ImmutableHashSet<T>.Empty = new ImmutableHashSet<T>(ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode, EqualityComparer<T>.Default, 0);
            ImmutableHashSet<T>.FreezeBucketAction = (KeyValuePair<Int32, ImmutableHashSet<T>.HashBucket> kv) => kv.Value.Freeze();
        }

        internal ImmutableHashSet(IEqualityComparer<T> equalityComparer)
            : this(ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode, equalityComparer, 0)
        {
        }

        private ImmutableHashSet(ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root, IEqualityComparer<T> equalityComparer, Int32 count)
        {
            Requires.NotNull<ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node>(root, "root");
            Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
            root.Freeze(ImmutableHashSet<T>.FreezeBucketAction);
            this.root = root;
            this.count = count;
            this.equalityComparer = equalityComparer;
        }

        public ImmutableHashSet<T> Add(T item)
        {
            Requires.NotNullAllowStructs<T>(item, "item");
            ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Add(item, this.Origin);
            return mutationResult.Finalize(this);
        }

        private static ImmutableHashSet<T>.MutationResult Add(T item, ImmutableHashSet<T>.MutationInput origin)
        {
            ImmutableHashSet<T>.OperationResult operationResult;
            Requires.NotNullAllowStructs<T>(item, "item");
            Int32 hashCode = origin.EqualityComparer.GetHashCode(item);
            ImmutableHashSet<T>.HashBucket valueOrDefault = origin.Root.GetValueOrDefault(hashCode, Comparer<Int32>.Default);
            ImmutableHashSet<T>.HashBucket hashBucket = valueOrDefault.Add(item, origin.EqualityComparer, out operationResult);
            if (operationResult == ImmutableHashSet<T>.OperationResult.NoChangeRequired)
            {
                return new ImmutableHashSet<T>.MutationResult(origin.Root, 0, ImmutableHashSet<T>.CountType.Adjustment);
            }
            ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node nums = ImmutableHashSet<T>.UpdateRoot(origin.Root, hashCode, hashBucket);
            return new ImmutableHashSet<T>.MutationResult(nums, 1, ImmutableHashSet<T>.CountType.Adjustment);
        }

        public ImmutableHashSet<T> Clear()
        {
            if (this.IsEmpty)
            {
                return this;
            }
            return ImmutableHashSet<T>.Empty.WithComparer(this.equalityComparer);
        }

        public Boolean Contains(T item)
        {
            Requires.NotNullAllowStructs<T>(item, "item");
            return ImmutableHashSet<T>.Contains(item, this.Origin);
        }

        private static Boolean Contains(T item, ImmutableHashSet<T>.MutationInput origin)
        {
            ImmutableHashSet<T>.HashBucket hashBucket;
            Int32 hashCode = origin.EqualityComparer.GetHashCode(item);
            if (!origin.Root.TryGetValue(hashCode, Comparer<Int32>.Default, out hashBucket))
            {
                return false;
            }
            return hashBucket.Contains(item, origin.EqualityComparer);
        }

        public ImmutableHashSet<T> Except(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Except(other, this.equalityComparer, this.root);
            return mutationResult.Finalize(this);
        }

        private static ImmutableHashSet<T>.MutationResult Except(IEnumerable<T> other, IEqualityComparer<T> equalityComparer, ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root)
        {
            ImmutableHashSet<T>.HashBucket hashBucket;
            ImmutableHashSet<T>.OperationResult operationResult;
            Requires.NotNull<IEnumerable<T>>(other, "other");
            Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
            Requires.NotNull<ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node>(root, "root");
            Int32 num = 0;
            ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node nums = root;
            foreach (T t in other)
            {
                Int32 hashCode = equalityComparer.GetHashCode(t);
                if (!nums.TryGetValue(hashCode, Comparer<Int32>.Default, out hashBucket))
                {
                    continue;
                }
                ImmutableHashSet<T>.HashBucket hashBucket1 = hashBucket.Remove(t, equalityComparer, out operationResult);
                if (operationResult != ImmutableHashSet<T>.OperationResult.SizeChanged)
                {
                    continue;
                }
                num--;
                nums = ImmutableHashSet<T>.UpdateRoot(nums, hashCode, hashBucket1);
            }
            return new ImmutableHashSet<T>.MutationResult(nums, num, ImmutableHashSet<T>.CountType.Adjustment);
        }

        public ImmutableHashSet<T>.Enumerator GetEnumerator()
        {
            return new ImmutableHashSet<T>.Enumerator(this.root, null);
        }

        public ImmutableHashSet<T> Intersect(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Intersect(other, this.Origin);
            return mutationResult.Finalize(this);
        }

        private static ImmutableHashSet<T>.MutationResult Intersect(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node emptyNode = ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode;
            Int32 count = 0;
            foreach (T t in other)
            {
                if (!ImmutableHashSet<T>.Contains(t, origin))
                {
                    continue;
                }
                ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Add(t, new ImmutableHashSet<T>.MutationInput(emptyNode, origin.EqualityComparer, count));
                emptyNode = mutationResult.Root;
                count = count + mutationResult.Count;
            }
            return new ImmutableHashSet<T>.MutationResult(emptyNode, count, ImmutableHashSet<T>.CountType.FinalValue);
        }

        public Boolean IsProperSubsetOf(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            return ImmutableHashSet<T>.IsProperSubsetOf(other, this.Origin);
        }

        private static Boolean IsProperSubsetOf(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
        {
            Boolean flag;
            Requires.NotNull<IEnumerable<T>>(other, "other");
            if (origin.Root.IsEmpty)
            {
                return other.Any<T>();
            }
            HashSet<T> ts = new HashSet<T>(other, origin.EqualityComparer);
            if (origin.Count >= ts.Count)
            {
                return false;
            }
            Int32 num = 0;
            Boolean flag1 = false;
            HashSet<T>.Enumerator enumerator = ts.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    if (!ImmutableHashSet<T>.Contains(enumerator.Current, origin))
                    {
                        flag1 = true;
                    }
                    else
                    {
                        num++;
                    }
                    if (num != origin.Count || !flag1)
                    {
                        continue;
                    }
                    flag = true;
                    return flag;
                }
                return false;
            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
            //return flag;
        }

        public Boolean IsProperSupersetOf(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            return ImmutableHashSet<T>.IsProperSupersetOf(other, this.Origin);
        }

        private static Boolean IsProperSupersetOf(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
        {
            Boolean flag;
            Requires.NotNull<IEnumerable<T>>(other, "other");
            if (origin.Root.IsEmpty)
            {
                return false;
            }
            Int32 num = 0;
            using (IEnumerator<T> enumerator = other.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    num++;
                    if (ImmutableHashSet<T>.Contains(current, origin))
                    {
                        continue;
                    }
                    flag = false;
                    return flag;
                }
                return origin.Count > num;
            }
            //return flag;
        }

        public Boolean IsSubsetOf(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            return ImmutableHashSet<T>.IsSubsetOf(other, this.Origin);
        }

        private static Boolean IsSubsetOf(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            if (origin.Root.IsEmpty)
            {
                return true;
            }
            HashSet<T> ts = new HashSet<T>(other, origin.EqualityComparer);
            Int32 num = 0;
            foreach (T t in ts)
            {
                if (!ImmutableHashSet<T>.Contains(t, origin))
                {
                    continue;
                }
                num++;
            }
            return num == origin.Count;
        }

        public Boolean IsSupersetOf(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            return ImmutableHashSet<T>.IsSupersetOf(other, this.Origin);
        }

        private static Boolean IsSupersetOf(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
        {
            Boolean flag;
            Requires.NotNull<IEnumerable<T>>(other, "other");
            using (IEnumerator<T> enumerator = other.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (ImmutableHashSet<T>.Contains(enumerator.Current, origin))
                    {
                        continue;
                    }
                    flag = false;
                    return flag;
                }
                return true;
            }
            //return flag;
        }

        public Boolean Overlaps(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            return ImmutableHashSet<T>.Overlaps(other, this.Origin);
        }

        private static Boolean Overlaps(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
        {
            Boolean flag;
            Requires.NotNull<IEnumerable<T>>(other, "other");
            if (origin.Root.IsEmpty)
            {
                return false;
            }
            using (IEnumerator<T> enumerator = other.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!ImmutableHashSet<T>.Contains(enumerator.Current, origin))
                    {
                        continue;
                    }
                    flag = true;
                    return flag;
                }
                return false;
            }
            //return flag;
        }

        public ImmutableHashSet<T> Remove(T item)
        {
            Requires.NotNullAllowStructs<T>(item, "item");
            ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Remove(item, this.Origin);
            return mutationResult.Finalize(this);
        }

        private static ImmutableHashSet<T>.MutationResult Remove(T item, ImmutableHashSet<T>.MutationInput origin)
        {
            ImmutableHashSet<T>.HashBucket hashBucket;
            Requires.NotNullAllowStructs<T>(item, "item");
            ImmutableHashSet<T>.OperationResult operationResult = ImmutableHashSet<T>.OperationResult.NoChangeRequired;
            Int32 hashCode = origin.EqualityComparer.GetHashCode(item);
            ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root = origin.Root;
            if (origin.Root.TryGetValue(hashCode, Comparer<Int32>.Default, out hashBucket))
            {
                ImmutableHashSet<T>.HashBucket hashBucket1 = hashBucket.Remove(item, origin.EqualityComparer, out operationResult);
                if (operationResult == ImmutableHashSet<T>.OperationResult.NoChangeRequired)
                {
                    return new ImmutableHashSet<T>.MutationResult(origin.Root, 0, ImmutableHashSet<T>.CountType.Adjustment);
                }
                root = ImmutableHashSet<T>.UpdateRoot(origin.Root, hashCode, hashBucket1);
            }
            return new ImmutableHashSet<T>.MutationResult(root, (operationResult == ImmutableHashSet<T>.OperationResult.SizeChanged ? -1 : 0), ImmutableHashSet<T>.CountType.Adjustment);
        }

        public Boolean SetEquals(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            return ImmutableHashSet<T>.SetEquals(other, this.Origin);
        }

        private static Boolean SetEquals(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
        {
            Boolean flag;
            Requires.NotNull<IEnumerable<T>>(other, "other");
            HashSet<T> ts = new HashSet<T>(other, origin.EqualityComparer);
            if (origin.Count != ts.Count)
            {
                return false;
            }
            Int32 num = 0;
            HashSet<T>.Enumerator enumerator = ts.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    if (ImmutableHashSet<T>.Contains(enumerator.Current, origin))
                    {
                        num++;
                    }
                    else
                    {
                        flag = false;
                        return flag;
                    }
                }
                return num == origin.Count;
            }
            finally
            {
                ((IDisposable)enumerator).Dispose();
            }
            //return flag;
        }

        public ImmutableHashSet<T> SymmetricExcept(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.SymmetricExcept(other, this.Origin);
            return mutationResult.Finalize(this);
        }

        private static ImmutableHashSet<T>.MutationResult SymmetricExcept(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            ImmutableHashSet<T> ts = ImmutableHashSet<T>.Empty.Union(other);
            Int32 count = 0;
            ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node emptyNode = ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode;
            foreach (T t in new ImmutableHashSet<T>.NodeEnumerable(origin.Root))
            {
                if (ts.Contains(t))
                {
                    continue;
                }
                ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Add(t, new ImmutableHashSet<T>.MutationInput(emptyNode, origin.EqualityComparer, count));
                emptyNode = mutationResult.Root;
                count = count + mutationResult.Count;
            }
            foreach (T t1 in ts)
            {
                if (ImmutableHashSet<T>.Contains(t1, origin))
                {
                    continue;
                }
                ImmutableHashSet<T>.MutationResult mutationResult1 = ImmutableHashSet<T>.Add(t1, new ImmutableHashSet<T>.MutationInput(emptyNode, origin.EqualityComparer, count));
                emptyNode = mutationResult1.Root;
                count = count + mutationResult1.Count;
            }
            return new ImmutableHashSet<T>.MutationResult(emptyNode, count, ImmutableHashSet<T>.CountType.FinalValue);
        }

        void System.Collections.Generic.ICollection<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void System.Collections.Generic.ICollection<T>.Clear()
        {
            throw new NotSupportedException();
        }

        void System.Collections.Generic.ICollection<T>.CopyTo(T[] array, Int32 arrayIndex)
        {
            Requires.NotNull<T[]>(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex", null);
            Requires.Range((Int32)array.Length >= arrayIndex + this.Count, "arrayIndex", null);
            foreach (T t in this)
            {
                Int32 num = arrayIndex;
                arrayIndex = num + 1;
                array[num] = t;
            }
        }

        Boolean System.Collections.Generic.ICollection<T>.Remove(T item)
        {
            throw new NotSupportedException();
        }

        IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        Boolean System.Collections.Generic.ISet<T>.Add(T item)
        {
            throw new NotSupportedException();
        }

        void System.Collections.Generic.ISet<T>.ExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void System.Collections.Generic.ISet<T>.IntersectWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void System.Collections.Generic.ISet<T>.SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void System.Collections.Generic.ISet<T>.UnionWith(IEnumerable<T> other)
        {
            throw new NotSupportedException();
        }

        void System.Collections.ICollection.CopyTo(Array array, Int32 arrayIndex)
        {
            Requires.NotNull<Array>(array, "array");
            Requires.Range(arrayIndex >= 0, "arrayIndex", null);
            Requires.Range(array.Length >= arrayIndex + this.Count, "arrayIndex", null);
            foreach (T t in this)
            {
                Object obj = t;
                Int32[] numArray = new Int32[1];
                Int32 num = arrayIndex;
                arrayIndex = num + 1;
                numArray[0] = num;
                array.SetValue(obj, numArray);
            }
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        IImmutableSet<T> System.Collections.Immutable.IImmutableSet<T>.Add(T item)
        {
            return this.Add(item);
        }

        IImmutableSet<T> System.Collections.Immutable.IImmutableSet<T>.Clear()
        {
            return this.Clear();
        }

        IImmutableSet<T> System.Collections.Immutable.IImmutableSet<T>.Except(IEnumerable<T> other)
        {
            return this.Except(other);
        }

        IImmutableSet<T> System.Collections.Immutable.IImmutableSet<T>.Intersect(IEnumerable<T> other)
        {
            return this.Intersect(other);
        }

        IImmutableSet<T> System.Collections.Immutable.IImmutableSet<T>.Remove(T item)
        {
            return this.Remove(item);
        }

        IImmutableSet<T> System.Collections.Immutable.IImmutableSet<T>.SymmetricExcept(IEnumerable<T> other)
        {
            return this.SymmetricExcept(other);
        }

        IImmutableSet<T> System.Collections.Immutable.IImmutableSet<T>.Union(IEnumerable<T> other)
        {
            return this.Union(other);
        }

        public ImmutableHashSet<T>.Builder ToBuilder()
        {
            return new ImmutableHashSet<T>.Builder(this);
        }

        public Boolean TryGetValue(T equalValue, out T actualValue)
        {
            ImmutableHashSet<T>.HashBucket hashBucket;
            Requires.NotNullAllowStructs<T>(equalValue, "value");
            Int32 hashCode = this.equalityComparer.GetHashCode(equalValue);
            if (!this.root.TryGetValue(hashCode, Comparer<Int32>.Default, out hashBucket))
            {
                actualValue = equalValue;
                return false;
            }
            return hashBucket.TryExchange(equalValue, this.equalityComparer, out actualValue);
        }

        public ImmutableHashSet<T> Union(IEnumerable<T> other)
        {
            Requires.NotNull<IEnumerable<T>>(other, "other");
            return this.Union(other, false);
        }

        private static ImmutableHashSet<T>.MutationResult Union(IEnumerable<T> other, ImmutableHashSet<T>.MutationInput origin)
        {
            ImmutableHashSet<T>.OperationResult operationResult;
            Requires.NotNull<IEnumerable<T>>(other, "other");
            Int32 num = 0;
            ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root = origin.Root;
            foreach (T t in other)
            {
                Int32 hashCode = origin.EqualityComparer.GetHashCode(t);
                ImmutableHashSet<T>.HashBucket valueOrDefault = root.GetValueOrDefault(hashCode, Comparer<Int32>.Default);
                ImmutableHashSet<T>.HashBucket hashBucket = valueOrDefault.Add(t, origin.EqualityComparer, out operationResult);
                if (operationResult != ImmutableHashSet<T>.OperationResult.SizeChanged)
                {
                    continue;
                }
                root = ImmutableHashSet<T>.UpdateRoot(root, hashCode, hashBucket);
                num++;
            }
            return new ImmutableHashSet<T>.MutationResult(root, num, ImmutableHashSet<T>.CountType.Adjustment);
        }

        private ImmutableHashSet<T> Union(IEnumerable<T> items, Boolean avoidWithComparer)
        {
            Requires.NotNull<IEnumerable<T>>(items, "items");
            if (this.IsEmpty && !avoidWithComparer)
            {
                ImmutableHashSet<T> ts = items as ImmutableHashSet<T>;
                if (ts != null)
                {
                    return ts.WithComparer(this.KeyComparer);
                }
            }
            ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Union(items, this.Origin);
            return mutationResult.Finalize(this);
        }

        private static ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node UpdateRoot(ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root, Int32 hashCode, ImmutableHashSet<T>.HashBucket newBucket)
        {
            Boolean flag;
            Boolean flag1;
            if (newBucket.IsEmpty)
            {
                return root.Remove(hashCode, Comparer<Int32>.Default, out flag);
            }
            return root.SetItem(hashCode, newBucket, Comparer<Int32>.Default, EqualityComparer<ImmutableHashSet<T>.HashBucket>.Default, out flag1, out flag);
        }

        public ImmutableHashSet<T> WithComparer(IEqualityComparer<T> equalityComparer)
        {
            if (equalityComparer == null)
            {
                equalityComparer = EqualityComparer<T>.Default;
            }
            if (equalityComparer == this.equalityComparer)
            {
                return this;
            }
            return (new ImmutableHashSet<T>(equalityComparer)).Union(this, true);
        }

        private static ImmutableHashSet<T> Wrap(ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root, IEqualityComparer<T> equalityComparer, Int32 count)
        {
            Requires.NotNull<ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node>(root, "root");
            Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
            Requires.Range(count >= 0, "count", null);
            return new ImmutableHashSet<T>(root, equalityComparer, count);
        }

        private ImmutableHashSet<T> Wrap(ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root, Int32 adjustedCountIfDifferentRoot)
        {
            if (root == this.root)
            {
                return this;
            }
            return new ImmutableHashSet<T>(root, this.equalityComparer, adjustedCountIfDifferentRoot);
        }

        [DebuggerDisplay("Count = {Count}")]
        public sealed class Builder : IReadOnlyCollection<T>, ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable
        {
            private ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root;

            private IEqualityComparer<T> equalityComparer;

            private Int32 count;

            private ImmutableHashSet<T> immutable;

            private Int32 version;

            public Int32 Count
            {
                get
                {
                    return this.count;
                }
            }

            public IEqualityComparer<T> KeyComparer
            {
                get
                {
                    return this.equalityComparer;
                }
                set
                {
                    Requires.NotNull<IEqualityComparer<T>>(value, "value");
                    if (value != this.equalityComparer)
                    {
                        ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Union(this, new ImmutableHashSet<T>.MutationInput(ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode, value, 0));
                        this.immutable = null;
                        this.equalityComparer = value;
                        this.Root = mutationResult.Root;
                        this.count = mutationResult.Count;
                    }
                }
            }

            private ImmutableHashSet<T>.MutationInput Origin
            {
                get
                {
                    return new ImmutableHashSet<T>.MutationInput(this.Root, this.equalityComparer, this.count);
                }
            }

            private ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node Root
            {
                get
                {
                    return this.root;
                }
                set
                {
                    ImmutableHashSet<T>.Builder builder = this;
                    builder.version = builder.version + 1;
                    if (this.root != value)
                    {
                        this.root = value;
                        this.immutable = null;
                    }
                }
            }

            Boolean System.Collections.Generic.ICollection<T>.IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            internal Int32 Version
            {
                get
                {
                    return this.version;
                }
            }

            internal Builder(ImmutableHashSet<T> set)
            {
                Requires.NotNull<ImmutableHashSet<T>>(set, "set");
                this.root = set.root;
                this.count = set.count;
                this.equalityComparer = set.equalityComparer;
                this.immutable = set;
            }

            public Boolean Add(T item)
            {
                ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Add(item, this.Origin);
                this.Apply(mutationResult);
                return mutationResult.Count != 0;
            }

            private void Apply(ImmutableHashSet<T>.MutationResult result)
            {
                this.Root = result.Root;
                if (result.CountType != ImmutableHashSet<T>.CountType.Adjustment)
                {
                    this.count = result.Count;
                    return;
                }
                ImmutableHashSet<T>.Builder count = this;
                count.count = count.count + result.Count;
            }

            public void Clear()
            {
                this.count = 0;
                this.Root = ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node.EmptyNode;
            }

            public Boolean Contains(T item)
            {
                return ImmutableHashSet<T>.Contains(item, this.Origin);
            }

            public void ExceptWith(IEnumerable<T> other)
            {
                ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Except(other, this.equalityComparer, this.root);
                this.Apply(mutationResult);
            }

            public ImmutableHashSet<T>.Enumerator GetEnumerator()
            {
                return new ImmutableHashSet<T>.Enumerator(this.root, this);
            }

            public void IntersectWith(IEnumerable<T> other)
            {
                this.Apply(ImmutableHashSet<T>.Intersect(other, this.Origin));
            }

            public Boolean IsProperSubsetOf(IEnumerable<T> other)
            {
                return ImmutableHashSet<T>.IsProperSubsetOf(other, this.Origin);
            }

            public Boolean IsProperSupersetOf(IEnumerable<T> other)
            {
                return ImmutableHashSet<T>.IsProperSupersetOf(other, this.Origin);
            }

            public Boolean IsSubsetOf(IEnumerable<T> other)
            {
                return ImmutableHashSet<T>.IsSubsetOf(other, this.Origin);
            }

            public Boolean IsSupersetOf(IEnumerable<T> other)
            {
                return ImmutableHashSet<T>.IsSupersetOf(other, this.Origin);
            }

            public Boolean Overlaps(IEnumerable<T> other)
            {
                return ImmutableHashSet<T>.Overlaps(other, this.Origin);
            }

            public Boolean Remove(T item)
            {
                ImmutableHashSet<T>.MutationResult mutationResult = ImmutableHashSet<T>.Remove(item, this.Origin);
                this.Apply(mutationResult);
                return mutationResult.Count != 0;
            }

            public Boolean SetEquals(IEnumerable<T> other)
            {
                return ImmutableHashSet<T>.SetEquals(other, this.Origin);
            }

            public void SymmetricExceptWith(IEnumerable<T> other)
            {
                this.Apply(ImmutableHashSet<T>.SymmetricExcept(other, this.Origin));
            }

            void System.Collections.Generic.ICollection<T>.Add(T item)
            {
                this.Add(item);
            }

            void System.Collections.Generic.ICollection<T>.CopyTo(T[] array, Int32 arrayIndex)
            {
                Requires.NotNull<T[]>(array, "array");
                Requires.Range(arrayIndex >= 0, "arrayIndex", null);
                Requires.Range((Int32)array.Length >= arrayIndex + this.Count, "arrayIndex", null);
                foreach (T t in this)
                {
                    Int32 num = arrayIndex;
                    arrayIndex = num + 1;
                    array[num] = t;
                }
            }

            IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            public ImmutableHashSet<T> ToImmutable()
            {
                if (this.immutable == null)
                {
                    this.immutable = ImmutableHashSet<T>.Wrap(this.root, this.equalityComparer, this.count);
                }
                return this.immutable;
            }

            public void UnionWith(IEnumerable<T> other)
            {
                this.Apply(ImmutableHashSet<T>.Union(other, this.Origin));
            }
        }

        private enum CountType
        {
            Adjustment,
            FinalValue
        }

        private class DebuggerProxy
        {
            private readonly ImmutableHashSet<T> @set;

            private T[] contents;

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] Contents
            {
                get
                {
                    if (this.contents == null)
                    {
                        this.contents = this.@set.ToArray<T>(this.@set.Count);
                    }
                    return this.contents;
                }
            }

            public DebuggerProxy(ImmutableHashSet<T> set)
            {
                Requires.NotNull<ImmutableHashSet<T>>(set, "set");
                this.@set = set;
            }
        }

        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            private readonly ImmutableHashSet<T>.Builder builder;

            private ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Enumerator mapEnumerator;

            private ImmutableHashSet<T>.HashBucket.Enumerator bucketEnumerator;

            private Int32 enumeratingBuilderVersion;

            public T Current
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

            internal Enumerator(ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root, ImmutableHashSet<T>.Builder builder = null)
            {
                this.builder = builder;
                this.mapEnumerator = new ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Enumerator(root, null);
                this.bucketEnumerator = new ImmutableHashSet<T>.HashBucket.Enumerator();
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
                this.bucketEnumerator = new ImmutableHashSet<T>.HashBucket.Enumerator(this.mapEnumerator.Current.Value);
                return this.bucketEnumerator.MoveNext();
            }

            public void Reset()
            {
                this.enumeratingBuilderVersion = (this.builder != null ? this.builder.Version : -1);
                this.mapEnumerator.Reset();
                this.bucketEnumerator.Dispose();
                this.bucketEnumerator = new ImmutableHashSet<T>.HashBucket.Enumerator();
            }

            private void ThrowIfChanged()
            {
                if (this.builder != null && this.builder.Version != this.enumeratingBuilderVersion)
                {
                    throw new InvalidOperationException(Strings.CollectionModifiedDuringEnumeration);
                }
            }
        }

        internal struct HashBucket
        {
            private readonly T firstValue;

            private readonly ImmutableList<T>.Node additionalElements;

            internal Boolean IsEmpty
            {
                get
                {
                    return this.additionalElements == null;
                }
            }

            private HashBucket(T firstElement, ImmutableList<T>.Node additionalElements = null)
            {
                this.firstValue = firstElement;
                this.additionalElements = additionalElements ?? ImmutableList<T>.Node.EmptyNode;
            }

            internal ImmutableHashSet<T>.HashBucket Add(T value, IEqualityComparer<T> valueComparer, out ImmutableHashSet<T>.OperationResult result)
            {
                if (this.IsEmpty)
                {
                    result = ImmutableHashSet<T>.OperationResult.SizeChanged;
                    return new ImmutableHashSet<T>.HashBucket(value, null);
                }
                if (valueComparer.Equals(value, this.firstValue) || this.additionalElements.IndexOf(value, valueComparer) >= 0)
                {
                    result = ImmutableHashSet<T>.OperationResult.NoChangeRequired;
                    return this;
                }
                result = ImmutableHashSet<T>.OperationResult.SizeChanged;
                return new ImmutableHashSet<T>.HashBucket(this.firstValue, this.additionalElements.Add(value));
            }

            internal Boolean Contains(T value, IEqualityComparer<T> valueComparer)
            {
                if (this.IsEmpty)
                {
                    return false;
                }
                if (valueComparer.Equals(value, this.firstValue))
                {
                    return true;
                }
                return this.additionalElements.IndexOf(value, valueComparer) >= 0;
            }

            internal void Freeze()
            {
                if (this.additionalElements != null)
                {
                    this.additionalElements.Freeze();
                }
            }

            public ImmutableHashSet<T>.HashBucket.Enumerator GetEnumerator()
            {
                return new ImmutableHashSet<T>.HashBucket.Enumerator(this);
            }

            internal ImmutableHashSet<T>.HashBucket Remove(T value, IEqualityComparer<T> equalityComparer, out ImmutableHashSet<T>.OperationResult result)
            {
                if (this.IsEmpty)
                {
                    result = ImmutableHashSet<T>.OperationResult.NoChangeRequired;
                    return this;
                }
                if (equalityComparer.Equals(this.firstValue, value))
                {
                    if (this.additionalElements.IsEmpty)
                    {
                        result = ImmutableHashSet<T>.OperationResult.SizeChanged;
                        return new ImmutableHashSet<T>.HashBucket();
                    }
                    Int32 count = ((IBinaryTree<T>)this.additionalElements).Left.Count;
                    result = ImmutableHashSet<T>.OperationResult.SizeChanged;
                    return new ImmutableHashSet<T>.HashBucket(this.additionalElements.Key, this.additionalElements.RemoveAt(count));
                }
                Int32 num = this.additionalElements.IndexOf(value, equalityComparer);
                if (num < 0)
                {
                    result = ImmutableHashSet<T>.OperationResult.NoChangeRequired;
                    return this;
                }
                result = ImmutableHashSet<T>.OperationResult.SizeChanged;
                return new ImmutableHashSet<T>.HashBucket(this.firstValue, this.additionalElements.RemoveAt(num));
            }

            internal Boolean TryExchange(T value, IEqualityComparer<T> valueComparer, out T existingValue)
            {
                if (!this.IsEmpty)
                {
                    if (valueComparer.Equals(value, this.firstValue))
                    {
                        existingValue = this.firstValue;
                        return true;
                    }
                    Int32 num = this.additionalElements.IndexOf(value, valueComparer);
                    if (num >= 0)
                    {
                        existingValue = this.additionalElements[num];
                        return true;
                    }
                }
                existingValue = value;
                return false;
            }

            internal struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
            {
                private readonly ImmutableHashSet<T>.HashBucket bucket;

                private Boolean disposed;

                private ImmutableHashSet<T>.HashBucket.Enumerator.Position currentPosition;

                private ImmutableList<T>.Enumerator additionalEnumerator;

                public T Current
                {
                    get
                    {
                        this.ThrowIfDisposed();
                        switch (this.currentPosition)
                        {
                            case (ImmutableHashSet<T>.HashBucket.Enumerator.Position)ImmutableHashSet<T>.HashBucket.Enumerator.Position.First:
                                {
                                    return this.bucket.firstValue;
                                }
                            case (ImmutableHashSet<T>.HashBucket.Enumerator.Position)ImmutableHashSet<T>.HashBucket.Enumerator.Position.Additional:
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

                internal Enumerator(ImmutableHashSet<T>.HashBucket bucket)
                {
                    this.disposed = false;
                    this.bucket = bucket;
                    this.currentPosition = ImmutableHashSet<T>.HashBucket.Enumerator.Position.BeforeFirst;
                    this.additionalEnumerator = new ImmutableList<T>.Enumerator();
                }

                public void Dispose()
                {
                    this.disposed = true;
                    this.additionalEnumerator.Dispose();
                }

                public Boolean MoveNext()
                {
                    this.ThrowIfDisposed();
                    if (this.bucket.IsEmpty)
                    {
                        this.currentPosition = ImmutableHashSet<T>.HashBucket.Enumerator.Position.End;
                        return false;
                    }
                    switch (this.currentPosition)
                    {
                        case (ImmutableHashSet<T>.HashBucket.Enumerator.Position)ImmutableHashSet<T>.HashBucket.Enumerator.Position.BeforeFirst:
                            {
                                this.currentPosition = ImmutableHashSet<T>.HashBucket.Enumerator.Position.First;
                                return true;
                            }
                        case (ImmutableHashSet<T>.HashBucket.Enumerator.Position)ImmutableHashSet<T>.HashBucket.Enumerator.Position.First:
                            {
                                if (this.bucket.additionalElements.IsEmpty)
                                {
                                    this.currentPosition = ImmutableHashSet<T>.HashBucket.Enumerator.Position.End;
                                    return false;
                                }
                                this.currentPosition = ImmutableHashSet<T>.HashBucket.Enumerator.Position.Additional;
                                this.additionalEnumerator = new ImmutableList<T>.Enumerator(this.bucket.additionalElements, null, -1, -1, false);
                                return this.additionalEnumerator.MoveNext();
                            }
                        case (ImmutableHashSet<T>.HashBucket.Enumerator.Position)ImmutableHashSet<T>.HashBucket.Enumerator.Position.Additional:
                            {
                                return this.additionalEnumerator.MoveNext();
                            }
                        case (ImmutableHashSet<T>.HashBucket.Enumerator.Position)ImmutableHashSet<T>.HashBucket.Enumerator.Position.End:
                            {
                                return false;
                            }
                    }
                    throw new InvalidOperationException();
                }

                public void Reset()
                {
                    this.ThrowIfDisposed();
                    this.additionalEnumerator.Dispose();
                    this.currentPosition = ImmutableHashSet<T>.HashBucket.Enumerator.Position.BeforeFirst;
                }

                private void ThrowIfDisposed()
                {
                    if (this.disposed)
                    {
                        throw new ObjectDisposedException(base.GetType().FullName);
                    }
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

        private struct MutationInput
        {
            private readonly ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root;

            private readonly IEqualityComparer<T> equalityComparer;

            private readonly Int32 count;

            internal Int32 Count
            {
                get
                {
                    return this.count;
                }
            }

            internal IEqualityComparer<T> EqualityComparer
            {
                get
                {
                    return this.equalityComparer;
                }
            }

            internal ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node Root
            {
                get
                {
                    return this.root;
                }
            }

            internal MutationInput(ImmutableHashSet<T> set)
            {
                Requires.NotNull<ImmutableHashSet<T>>(set, "set");
                this.root = set.root;
                this.equalityComparer = set.equalityComparer;
                this.count = set.count;
            }

            internal MutationInput(ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root, IEqualityComparer<T> equalityComparer, Int32 count)
            {
                Requires.NotNull<ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node>(root, "root");
                Requires.NotNull<IEqualityComparer<T>>(equalityComparer, "equalityComparer");
                Requires.Range(count >= 0, "count", null);
                this.root = root;
                this.equalityComparer = equalityComparer;
                this.count = count;
            }
        }

        private struct MutationResult
        {
            private readonly ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root;

            private readonly Int32 count;

            private readonly ImmutableHashSet<T>.CountType countType;

            internal Int32 Count
            {
                get
                {
                    return this.count;
                }
            }

            internal ImmutableHashSet<T>.CountType CountType
            {
                get
                {
                    return (ImmutableHashSet<T>.CountType)this.countType;
                }
            }

            internal ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node Root
            {
                get
                {
                    return this.root;
                }
            }

            internal MutationResult(ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root, Int32 count, ImmutableHashSet<T>.CountType countType = 0)
            {
                Requires.NotNull<ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node>(root, "root");
                this.root = root;
                this.count = count;
                this.countType = countType;
            }

            internal ImmutableHashSet<T> Finalize(ImmutableHashSet<T> priorSet)
            {
                Requires.NotNull<ImmutableHashSet<T>>(priorSet, "priorSet");
                Int32 count = this.Count;
                if (this.CountType == ImmutableHashSet<T>.CountType.Adjustment)
                {
                    count = count + priorSet.count;
                }
                return priorSet.Wrap(this.Root, count);
            }
        }

        private struct NodeEnumerable : IEnumerable<T>, IEnumerable
        {
            private readonly ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root;

            internal NodeEnumerable(ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node root)
            {
                Requires.NotNull<ImmutableSortedDictionary<Int32, ImmutableHashSet<T>.HashBucket>.Node>(root, "root");
                this.root = root;
            }

            public ImmutableHashSet<T>.Enumerator GetEnumerator()
            {
                return new ImmutableHashSet<T>.Enumerator(this.root, null);
            }

            IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        internal enum OperationResult
        {
            SizeChanged,
            NoChangeRequired
        }
    }
}