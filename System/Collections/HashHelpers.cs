using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace System.Collections
{
    public static class HashHelpers
    {
        // Fields
        private const int bufferSize = 0x400;
        private static int currentIndex = 0x400;
        private static byte[] data;
        public const int HashCollisionThreshold = 100;
        private static readonly object lockObj = new object();
        public const int MaxPrimeArrayLength = 0x7feffffd;
        public static readonly int[] primes = new int[] { 
        3, 7, 11, 0x11, 0x17, 0x1d, 0x25, 0x2f, 0x3b, 0x47, 0x59, 0x6b, 0x83, 0xa3, 0xc5, 0xef, 
        0x125, 0x161, 0x1af, 0x209, 0x277, 0x2f9, 0x397, 0x44f, 0x52f, 0x63d, 0x78b, 0x91d, 0xaf1, 0xd2b, 0xfd1, 0x12fd, 
        0x16cf, 0x1b65, 0x20e3, 0x2777, 0x2f6f, 0x38ff, 0x446f, 0x521f, 0x628d, 0x7655, 0x8e01, 0xaa6b, 0xcc89, 0xf583, 0x126a7, 0x1619b, 
        0x1a857, 0x1fd3b, 0x26315, 0x2dd67, 0x3701b, 0x42023, 0x4f361, 0x5f0ed, 0x72125, 0x88e31, 0xa443b, 0xc51eb, 0xec8c1, 0x11bdbf, 0x154a3f, 0x198c4f, 
        0x1ea867, 0x24ca19, 0x2c25c1, 0x34fa1b, 0x3f928f, 0x4c4987, 0x5b8b6f, 0x6dda89
     };
        private static RandomNumberGenerator rng;
        private static ConditionalWeakTable<object, SerializationInfo> s_SerializationInfoTable;
        //public static bool s_UseRandomizedStringHashing = ExternalInvoke.UseRandomizedHashing();

        // Methods
        public static int ExpandPrime(int oldSize)
        {
            int min = 2 * oldSize;
            if ((min > 0x7feffffd) && (0x7feffffd > oldSize))
            {
                return 0x7feffffd;
            }
            return GetPrime(min);
        }

        internal static long GetEntropy()
        {
            lock (lockObj)
            {
                if (currentIndex == 0x400)
                {
                    if (rng == null)
                    {
                        rng = RandomNumberGenerator.Create();
                        data = new byte[0x400];
                    }
                    rng.GetBytes(data);
                    currentIndex = 0;
                }
                long num = BitConverter.ToInt64(data, currentIndex);
                currentIndex += 8;
                return num;
            }
        }

        public static object GetEqualityComparerForSerialization(object comparer)
        {
            if (comparer == null)
            {
                return null;
            }
            IWellKnownStringEqualityComparer comparer2 = comparer as IWellKnownStringEqualityComparer;
            if (comparer2 != null)
            {
                return comparer2.GetEqualityComparerForSerialization();
            }
            return comparer;
        }

        public static int GetMinPrime()
        {
            return primes[0];
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static int GetPrime(int min)
        {
            if (min < 0)
            {
                throw new ArgumentException("Arg_HTCapacityOverflow");
            }
            for (int i = 0; i < primes.Length; i++)
            {
                int num2 = primes[i];
                if (num2 >= min)
                {
                    return num2;
                }
            }
            for (int j = min | 1; j < 0x7fffffff; j += 2)
            {
                if (IsPrime(j) && (((j - 1) % 0x65) != 0))
                {
                    return j;
                }
            }
            return min;
        }

        public static IEqualityComparer GetRandomizedEqualityComparer(object comparer)
        {
            if (comparer == null)
            {
                return new RandomizedObjectEqualityComparer();
            }
            if (comparer == EqualityComparer<string>.Default)
            {
                return new RandomizedStringEqualityComparer();
            }
            IWellKnownStringEqualityComparer comparer2 = comparer as IWellKnownStringEqualityComparer;
            if (comparer2 != null)
            {
                return comparer2.GetRandomizedEqualityComparer();
            }
            return null;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static bool IsPrime(int candidate)
        {
            if ((candidate & 1) == 0)
            {
                return (candidate == 2);
            }
            int num = (int)Math.Sqrt((double)candidate);
            for (int i = 3; i <= num; i += 2)
            {
                if ((candidate % i) == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsWellKnownEqualityComparer(object comparer)
        {
            if ((comparer != null) && (comparer != EqualityComparer<string>.Default))
            {
                return (comparer is IWellKnownStringEqualityComparer);
            }
            return true;
        }

        // Properties
        internal static ConditionalWeakTable<object, SerializationInfo> SerializationInfoTable
        {
            get
            {
                if (s_SerializationInfoTable == null)
                {
                    ConditionalWeakTable<object, SerializationInfo> table = new ConditionalWeakTable<object, SerializationInfo>();
                    Interlocked.CompareExchange<ConditionalWeakTable<object, SerializationInfo>>(ref s_SerializationInfoTable, table, null);
                }
                return s_SerializationInfoTable;
            }
        }
    }

}
