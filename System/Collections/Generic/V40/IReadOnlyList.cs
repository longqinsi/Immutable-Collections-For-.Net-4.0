// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Interface:  IReadOnlyListV40<T>
** 
** <OWNER>[....]</OWNER>
**
** Purpose: Base interface for read-only generic lists.
** 
===========================================================*/
using System;

using System.Runtime.CompilerServices;

namespace System.Collections.Generic.V40
{

    // Provides a read-only, covariant view of a generic list.

    // Note that T[] : IReadOnlyListV40<T>, and we want to ensure that if you use
    // IList<YourValueType>, we ensure a YourValueType[] can be used 
    // without jitting.  Hence the TypeDependencyAttribute on SZArrayHelper.
    // This is a special hack internally though - see VM\compile.cpp.
    // The same attribute is on IList<T>, IEnumerable<T>, ICollection<T> and IReadOnlyCollectionV40<T>.
#if CONTRACTS_FULL
    [ContractClass(typeof(IReadOnlyListContract<>))]
#endif
    // If we ever implement more interfaces on IReadOnlyListV40, we should also update RuntimeTypeCache.PopulateInterfaces() in rttype.cs
    public interface IReadOnlyList<T> : IReadOnlyCollection<T>
    {
        T this[int index] { get; }
    }

#if CONTRACTS_FULL
    [ContractClassFor(typeof(IReadOnlyListV40<>))]
    internal abstract class IReadOnlyListContract<T> : IReadOnlyListV40<T>
    {
        T IReadOnlyListV40<T>.this[int index] {
            get {
                //Contract.Requires(index >= 0);
                //Contract.Requires(index < ((ICollection<T>)this).Count);
                return default(T);
            }
        }

        int IReadOnlyCollectionV40<T>.Count {
            get {
                return default(int);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return default(IEnumerator<T>);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return default(IEnumerator);
        }
    }
#endif
}
