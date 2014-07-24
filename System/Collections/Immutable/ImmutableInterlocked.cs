namespace System.Collections.Immutable
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Validation;

    public static class ImmutableInterlocked
    {
        public static TValue AddOrUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue local;
            bool flag;
            Requires.NotNull<Func<TKey, TValue>>(addValueFactory, "addValueFactory");
            Requires.NotNull<Func<TKey, TValue, TValue>>(updateValueFactory, "updateValueFactory");
            ImmutableDictionary<TKey, TValue> dictionary = VolatileV40.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            do
            {
                TValue local2;
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionary, "location");
                if (dictionary.TryGetValue(key, out local2))
                {
                    local = updateValueFactory(key, local2);
                }
                else
                {
                    local = addValueFactory(key);
                }
                ImmutableDictionary<TKey, TValue> dictionary2 = dictionary.SetItem(key, local);
                ImmutableDictionary<TKey, TValue> objB = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, dictionary2, dictionary);
                flag = object.ReferenceEquals(dictionary, objB);
                dictionary = objB;
            }
            while (!flag);
            return local;
        }

        public static TValue AddOrUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            TValue local;
            bool flag;
            Requires.NotNull<Func<TKey, TValue, TValue>>(updateValueFactory, "updateValueFactory");
            ImmutableDictionary<TKey, TValue> dictionary = VolatileV40.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            do
            {
                TValue local2;
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionary, "location");
                if (dictionary.TryGetValue(key, out local2))
                {
                    local = updateValueFactory(key, local2);
                }
                else
                {
                    local = addValue;
                }
                ImmutableDictionary<TKey, TValue> dictionary2 = dictionary.SetItem(key, local);
                ImmutableDictionary<TKey, TValue> objB = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, dictionary2, dictionary);
                flag = object.ReferenceEquals(dictionary, objB);
                dictionary = objB;
            }
            while (!flag);
            return local;
        }

        public static void Enqueue<T>(ref ImmutableQueue<T> location, T value)
        {
            bool flag;
            ImmutableQueue<T> queue = VolatileV40.Read<ImmutableQueue<T>>(ref location);
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

        public static TValue GetOrAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue local;
            Requires.NotNull<Func<TKey, TValue>>(valueFactory, "valueFactory");
            ImmutableDictionary<TKey, TValue> dictionary = VolatileV40.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionary, "location");
            if (dictionary.TryGetValue(key, out local))
            {
                return local;
            }
            local = valueFactory(key);
            return GetOrAdd<TKey, TValue>(ref location, key, local);
        }

        public static TValue GetOrAdd<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue value)
        {
            bool flag;
            ImmutableDictionary<TKey, TValue> dictionary = VolatileV40.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            do
            {
                TValue local;
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionary, "location");
                if (dictionary.TryGetValue(key, out local))
                {
                    return local;
                }
                ImmutableDictionary<TKey, TValue> dictionary2 = dictionary.Add(key, value);
                ImmutableDictionary<TKey, TValue> objB = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, dictionary2, dictionary);
                flag = object.ReferenceEquals(dictionary, objB);
                dictionary = objB;
            }
            while (!flag);
            return value;
        }

        public static TValue GetOrAdd<TKey, TValue, TArg>(ref ImmutableDictionary<TKey, TValue> location, TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
        {
            TValue local;
            Requires.NotNull<Func<TKey, TArg, TValue>>(valueFactory, "valueFactory");
            ImmutableDictionary<TKey, TValue> dictionary = VolatileV40.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionary, "location");
            if (dictionary.TryGetValue(key, out local))
            {
                return local;
            }
            local = valueFactory(key, factoryArgument);
            return GetOrAdd<TKey, TValue>(ref location, key, local);
        }

        public static void Push<T>(ref ImmutableStack<T> location, T value)
        {
            bool flag;
            ImmutableStack<T> stack = VolatileV40.Read<ImmutableStack<T>>(ref location);
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
            ImmutableDictionary<TKey, TValue> dictionary = VolatileV40.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            do
            {
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionary, "location");
                if (dictionary.ContainsKey(key))
                {
                    return false;
                }
                ImmutableDictionary<TKey, TValue> dictionary2 = dictionary.Add(key, value);
                ImmutableDictionary<TKey, TValue> objB = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, dictionary2, dictionary);
                flag = object.ReferenceEquals(dictionary, objB);
                dictionary = objB;
            }
            while (!flag);
            return true;
        }

        public static bool TryDequeue<T>(ref ImmutableQueue<T> location, out T value)
        {
            bool flag;
            ImmutableQueue<T> queue = VolatileV40.Read<ImmutableQueue<T>>(ref location);
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

        public static bool TryPop<T>(ref ImmutableStack<T> location, out T value)
        {
            bool flag;
            ImmutableStack<T> stack = VolatileV40.Read<ImmutableStack<T>>(ref location);
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
            ImmutableDictionary<TKey, TValue> dictionary = VolatileV40.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            do
            {
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionary, "location");
                if (!dictionary.TryGetValue(key, out value))
                {
                    return false;
                }
                ImmutableDictionary<TKey, TValue> dictionary2 = dictionary.Remove(key);
                ImmutableDictionary<TKey, TValue> objB = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, dictionary2, dictionary);
                flag = object.ReferenceEquals(dictionary, objB);
                dictionary = objB;
            }
            while (!flag);
            return true;
        }

        public static bool TryUpdate<TKey, TValue>(ref ImmutableDictionary<TKey, TValue> location, TKey key, TValue newValue, TValue comparisonValue)
        {
            bool flag;
            EqualityComparer<TValue> comparer = EqualityComparer<TValue>.Default;
            ImmutableDictionary<TKey, TValue> dictionary = VolatileV40.Read<ImmutableDictionary<TKey, TValue>>(ref location);
            do
            {
                TValue local;
                Requires.NotNull<ImmutableDictionary<TKey, TValue>>(dictionary, "location");
                if (!dictionary.TryGetValue(key, out local) || !comparer.Equals(local, comparisonValue))
                {
                    return false;
                }
                ImmutableDictionary<TKey, TValue> dictionary2 = dictionary.SetItem(key, newValue);
                ImmutableDictionary<TKey, TValue> objB = Interlocked.CompareExchange<ImmutableDictionary<TKey, TValue>>(ref location, dictionary2, dictionary);
                flag = object.ReferenceEquals(dictionary, objB);
                dictionary = objB;
            }
            while (!flag);
            return true;
        }
    }
}

