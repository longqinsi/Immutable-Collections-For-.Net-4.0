using System;
using System.Collections.Generic;
using System.Security;
using System.Text;

namespace System.Collections.Generic
{
    internal sealed class RandomizedStringEqualityComparer : IEqualityComparer<string>, IEqualityComparer, IWellKnownStringEqualityComparer
    {
        // Fields
        private long _entropy = HashHelpers.GetEntropy();

        // Methods
        public override bool Equals(object obj)
        {
            RandomizedStringEqualityComparer comparer = obj as RandomizedStringEqualityComparer;
            return ((comparer != null) && (this._entropy == comparer._entropy));
        }

        public bool Equals(object x, object y)
        {
            if (x == y)
            {
                return true;
            }
            if ((x != null) && (y != null))
            {
                if ((x is string) && (y is string))
                {
                    return this.Equals((string)x, (string)y);
                }
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArgumentForComparison);
            }
            return false;
        }

        public bool Equals(string x, string y)
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
            if (s != null)
            {
                return ExternalInvoke.InternalMarvin32HashString(s, s.Length, this._entropy);
            }
            return obj.GetHashCode();
        }

        [SecuritySafeCritical]
        public int GetHashCode(string obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return ExternalInvoke.InternalMarvin32HashString(obj, obj.Length, this._entropy);
        }

        IEqualityComparer IWellKnownStringEqualityComparer.GetEqualityComparerForSerialization()
        {
            return EqualityComparer<string>.Default;
        }

        IEqualityComparer IWellKnownStringEqualityComparer.GetRandomizedEqualityComparer()
        {
            return new RandomizedStringEqualityComparer();
        }
    }


}
