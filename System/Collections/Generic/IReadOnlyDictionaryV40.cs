// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Interface:  IReadOnlyDictionaryV40<TKey, TValue>
** 
** <OWNER>[....]</OWNER>
**
** Purpose: Base interface for read-only generic dictionaries.
** 
===========================================================*/
using System;
using System.Diagnostics.Contracts;

namespace System.Collections.Generic
{
    // Provides a read-only view of a generic dictionaryV40.
#if CONTRACTS_FULL
    [ContractClass(typeof(IReadOnlyDictionaryContract<,>))]
#endif
    public interface IReadOnlyDictionaryV40<TKey, TValue> : IReadOnlyCollectionV40<KeyValuePair<TKey, TValue>>
    {
        bool ContainsKey(TKey key);
        bool TryGetValue(TKey key, out TValue value);

        TValue this[TKey key] { get; }
        IEnumerable<TKey> Keys { get; }
        IEnumerable<TValue> Values { get; }
    }

#if CONTRACTS_FULL
    [ContractClassFor(typeof(IReadOnlyDictionaryV40<,>))]
    internal abstract class IReadOnlyDictionaryContract<TKey, TValue> : IReadOnlyDictionaryV40<TKey, TValue>
    {
        bool IReadOnlyDictionaryV40<TKey, TValue>.ContainsKey(TKey key)
        {
            return default(bool);
        }

        bool IReadOnlyDictionaryV40<TKey, TValue>.TryGetValue(TKey key, out TValue value)
        {
            value = default(TValue);
            return default(bool);
        }

        TValue IReadOnlyDictionaryV40<TKey, TValue>.this[TKey key]
        {
            get { return default(TValue); }
        }

        IEnumerable<TKey> IReadOnlyDictionaryV40<TKey, TValue>.Keys {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<TKey>>() != null);
                return default(IEnumerable<TKey>);
            }
        }

        IEnumerable<TValue> IReadOnlyDictionaryV40<TKey, TValue>.Values {
            get {
                Contract.Ensures(Contract.Result<IEnumerable<TValue>>() != null);
                return default(IEnumerable<TValue>);
            }
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return default(IEnumerator<KeyValuePair<TKey, TValue>>);
        }

        int IReadOnlyCollectionV40<KeyValuePair<TKey, TValue>>.Count {
            get {
                return default(int);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return default(IEnumerator);
        }
    }
#endif
}
