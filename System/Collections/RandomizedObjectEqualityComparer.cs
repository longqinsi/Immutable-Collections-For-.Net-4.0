using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace System.Collections
{
    internal sealed class RandomizedObjectEqualityComparer : IEqualityComparer, IWellKnownStringEqualityComparer
    {
        // Fields
        private long _entropy = HashHelpers.GetEntropy();

        // Methods
        public override bool Equals(object obj)
        {
            RandomizedObjectEqualityComparer comparer = obj as RandomizedObjectEqualityComparer;
            return ((comparer != null) && (this._entropy == comparer._entropy));
        }

        public bool Equals(object x, object y)
        {
            if (x != null)
            {
                return ((y != null) && x.Equals(y));
            }
            if (y != null)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (base.GetType().Name.GetHashCode() ^ ((int)(this._entropy & 0x7fffffffL)));
        }

        [SecuritySafeCritical]
        public int GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            string s = obj as string;
            //if (s != null)
            //{
            //    return ExternalInvoke.InternalMarvin32HashString(s, s.Length, this._entropy);
            //}
            return obj.GetHashCode();
        }

        IEqualityComparer IWellKnownStringEqualityComparer.GetEqualityComparerForSerialization()
        {
            return null;
        }

        IEqualityComparer IWellKnownStringEqualityComparer.GetRandomizedEqualityComparer()
        {
            return new RandomizedObjectEqualityComparer();
        }
    }


}
