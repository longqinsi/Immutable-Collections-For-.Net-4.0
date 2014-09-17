namespace System.Collections.Immutable
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Validation;

    public static class ImmutableInterlocked
    {
        public static TValue AddOrUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, FuncV20<TKey, TValue> addValueFactory, FuncV20<TKey, TValue, TValue> updateValueFactory)
        {
            TValue local;
            bool flag;
            Requires.NotNull<FuncV20<TKey, TValue>>(addValueFactory, "addValueFactory");
            Requires.NotNull<FuncV20<TKey, TValue, TValue>>(updateValueFactory, "updateValueFactory");
            ImmutableDictionary<TKey, TValue> dictionaryV40 = VolatileV20.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            do
            {
                TValue local2;
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionaryV40, "location");
                if (dictionaryV40.TryGetValue(key, out local2))
                {
                    local = updateValueFactory(key, local2);
                }
                else
                {
                    local = addValueFactory(key);
                }
                ImmutableDictionary<TKey, TValue> dictionary2 = dictionaryV40.SetItem(key, local);
                ImmutableDictionary<TKey, TValue> objB = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, dictionary2, dictionaryV40);
                flag = object.ReferenceEquals(dictionaryV40, objB);
                dictionaryV40 = objB;
            }
            while (!flag);
            return local;
        }

        public static TValue AddOrUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue addValue, FuncV20<TKey, TValue, TValue> updateValueFactory)
        {
            TValue local;
            bool flag;
            Requires.NotNull<FuncV20<TKey, TValue, TValue>>(updateValueFactory, "updateValueFactory");
            ImmutableDictionary<TKey, TValue> dictionaryV40 = VolatileV20.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            do
            {
                TValue local2;
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionaryV40, "location");
                if (dictionaryV40.TryGetValue(key, out local2))
                {
                    local = updateValueFactory(key, local2);
                }
                else
                {
                    local = addValue;
                }
                ImmutableDictionary<TKey, TValue> dictionary2 = dictionaryV40.SetItem(key, local);
                ImmutableDictionary<TKey, TValue> objB = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, dictionary2, dictionaryV40);
                flag = object.ReferenceEquals(dictionaryV40, objB);
                dictionaryV40 = objB;
            }
            while (!flag);
            return local;
        }

        public static void Enqueue<T>(ref ImmutableQueue<T> location, T value)
        {
            bool flag;
            ImmutableQueue<T> queue = VolatileV20.Read<ImmutableQueue<T>>(ref location);
            do
            {
                Requires.NotNull<ImmutableQueue<T>>(queue, "location");
                ImmutableQueue<T> queue2 = queue.Enqueue(value);
                ImmutableQueue<T> objB = Interlocked.CompareExchange<ImmutableQueue<T>>(ref location, queue2, queue);
                flag = object.ReferenceEquals(queue, objB);
                queue = objB;
            }
            while (!flag);
        }

        public static TValue GetOrAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, FuncV20<TKey, TValue> valueFactory)
        {
            TValue local;
            Requires.NotNull<FuncV20<TKey, TValue>>(valueFactory, "valueFactory");
            ImmutableDictionary<TKey, TValue> dictionaryV40 = VolatileV20.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionaryV40, "location");
            if (dictionaryV40.TryGetValue(key, out local))
            {
                return local;
            }
            local = valueFactory(key);
            return GetOrAdd<TKey, TValue>(ref location, key, local);
        }

        public static TValue GetOrAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue value)
        {
            bool flag;
            ImmutableDictionary<TKey, TValue> dictionaryV40 = VolatileV20.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            do
            {
                TValue local;
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionaryV40, "location");
                if (dictionaryV40.TryGetValue(key, out local))
                {
                    return local;
                }
                ImmutableDictionary<TKey, TValue> dictionary2 = dictionaryV40.Add(key, value);
                ImmutableDictionary<TKey, TValue> objB = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, dictionary2, dictionaryV40);
                flag = object.ReferenceEquals(dictionaryV40, objB);
                dictionaryV40 = objB;
            }
            while (!flag);
            return value;
        }

        public static TValue GetOrAdd<TKey, TValue, TArg>(ref ImmutableDictionary<TKey, TValue> location, TKey key, FuncV20<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
        {
            TValue local;
            Requires.NotNull<FuncV20<TKey, TArg, TValue>>(valueFactory, "valueFactory");
            ImmutableDictionary<TKey, TValue> dictionaryV40 = VolatileV20.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionaryV40, "location");
            if (dictionaryV40.TryGetValue(key, out local))
            {
                return local;
            }
            local = valueFactory(key, factoryArgument);
            return GetOrAdd<TKey, TValue>(ref location, key, local);
        }

        public static void Push<T>(ref ImmutableStack<T> location, T value)
        {
            bool flag;
            ImmutableStack<T> stack = VolatileV20.Read<ImmutableStack<T>>(ref location);
            do
            {
                Requires.NotNull<ImmutableStack<T>>(stack, "location");
                ImmutableStack<T> stack2 = stack.Push(value);
                ImmutableStack<T> objB = Interlocked.CompareExchange<ImmutableStack<T>>(ref location, stack2, stack);
                flag = object.ReferenceEquals(stack, objB);
                stack = objB;
            }
            while (!flag);
        }

        public static bool TryAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue value)
        {
            bool flag;
            ImmutableDictionary<TKey, TValue> dictionaryV40 = VolatileV20.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            do
            {
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionaryV40, "location");
                if (dictionaryV40.ContainsKey(key))
                {
                    return false;
                }
                ImmutableDictionary<TKey, TValue> dictionary2 = dictionaryV40.Add(key, value);
                ImmutableDictionary<TKey, TValue> objB = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, dictionary2, dictionaryV40);
                flag = object.ReferenceEquals(dictionaryV40, objB);
                dictionaryV40 = objB;
            }
            while (!flag);
            return true;
        }

        public static bool TryDequeue<T>(ref ImmutableQueue<T> location, T value)
        {
            bool flag;
            ImmutableQueue<T> queue = VolatileV20.Read<ImmutableQueue<T>>(ref location);
            do
            {
                Requires.NotNull<ImmutableQueue<T>>(queue, "location");
                if (queue.IsEmpty)
                {
                    value = default(T);
                    return false;
                }
                ImmutableQueue<T> queue2 = queue.Dequeue(out value);
                ImmutableQueue<T> objB = Interlocked.CompareExchange<ImmutableQueue<T>>(ref location, queue2, queue);
                flag = object.ReferenceEquals(queue, objB);
                queue = objB;
            }
            while (!flag);
            return true;
        }

        public static bool TryPop<T>(ref ImmutableStack<T> location, T value)
        {
            bool flag;
            ImmutableStack<T> stack = VolatileV20.Read<ImmutableStack<T>>(ref location);
            do
            {
                Requires.NotNull<ImmutableStack<T>>(stack, "location");
                if (stack.IsEmpty)
                {
                    value = default(T);
                    return false;
                }
                ImmutableStack<T> stack2 = stack.Pop(out value);
                ImmutableStack<T> objB = Interlocked.CompareExchange<ImmutableStack<T>>(ref location, stack2, stack);
                flag = object.ReferenceEquals(stack, objB);
                stack = objB;
            }
            while (!flag);
            return true;
        }

        public static bool TryRemove<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, out TValue value)
        {
            bool flag;
            ImmutableDictionary<TKey, TValue> dictionaryV40 = VolatileV20.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            do
            {
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionaryV40, "location");
                if (!dictionaryV40.TryGetValue(key, out value))
                {
                    return false;
                }
                ImmutableDictionary<TKey, TValue> dictionary2 = dictionaryV40.Remove(key);
                ImmutableDictionary<TKey, TValue> objB = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, dictionary2, dictionaryV40);
                flag = object.ReferenceEquals(dictionaryV40, objB);
                dictionaryV40 = objB;
            }
            while (!flag);
            return true;
        }

        public static bool TryUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue newValue, TValue comparisonValue)
        {
            bool flag;
            EqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;
            ImmutableDictionary<TKey, TValue> dictionaryV40 = VolatileV20.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            do
            {
                TValue local;
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionaryV40, "location");
                if (!dictionaryV40.TryGetValue(key, out local) || !comparer.Equals(local, comparisonValue))
                {
                    return false;
                }
                ImmutableDictionary<TKey, TValue> dictionary2 = dictionaryV40.SetItem(key, newValue);
                ImmutableDictionary<TKey, TValue> objB = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, dictionary2, dictionaryV40);
                flag = object.ReferenceEquals(dictionaryV40, objB);
                dictionaryV40 = objB;
            }
            while (!flag);
            return true;
        }
    }
}

