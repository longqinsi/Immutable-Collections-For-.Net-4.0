using System;
using System.Collections.Generic;
using System.Text;

namespace System.Collections.Generic
{
    [Serializable]
    internal class HashSetEqualityComparer<T> : IEqualityComparer<HashSetV20<T>>
    {
        // Fields
        private IEqualityComparer<T> m_comparer;

        // Methods
        public HashSetEqualityComparer()
        {
            this.m_comparer = EqualityComparer<T>.Default;
        }

        public HashSetEqualityComparer(IEqualityComparer<T> comparer)
        {
            if (this.m_comparer == null)
            {
                this.m_comparer = EqualityComparer<T>.Default;
            }
            else
            {
                this.m_comparer = comparer;
            }
        }

        public override bool Equals(object obj)
        {
            HashSetEqualityComparer<T> comparer = obj as HashSetEqualityComparer<T>;
            if (comparer == null)
            {
                return false;
            }
            return (this.m_comparer == comparer.m_comparer);
        }

        public bool Equals(HashSetV20<T> x, HashSetV20<T> y)
        {
            return HashSetV20<T>.HashSetEquals(x, y, this.m_comparer);
        }

        public override int GetHashCode()
        {
            return this.m_comparer.GetHashCode();
        }

        public int GetHashCode(HashSetV20<T> obj)
        {
            int num = 0;
            if (obj != null)
            {
                foreach (T local in obj)
                {
                    num ^= this.m_comparer.GetHashCode(local) & 0x7fffffff;
                }
            }
            return num;
        }
    }


}
