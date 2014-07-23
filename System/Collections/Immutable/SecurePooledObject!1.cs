namespace System.Collections.Immutable
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using Validation;

    internal class SecurePooledObject<T>
    {
        private Guid owner;
        private readonly T value;

        internal SecurePooledObject(T newValue)
        {
            Requires.NotNullAllowStructs<T>(newValue, "newValue");
            this.value = newValue;
        }

        internal void ThrowDisposedIfNotOwned<TCaller>(TCaller caller) where TCaller: ISecurePooledObjectUser
        {
            if (caller.PoolUserId != this.owner)
            {
                throw new ObjectDisposedException(caller.GetType().FullName);
            }
        }

        internal SecurePooledObject<T>.SecurePooledObjectUser Use<TCaller>(TCaller caller)
        where TCaller : ISecurePooledObjectUser
        {
            this.ThrowDisposedIfNotOwned<TCaller>(caller);
            return new SecurePooledObject<T>.SecurePooledObjectUser(this);
        }

        internal Guid Owner
        {
            get
            {
                lock (((SecurePooledObject<T>) this))
                {
                    return this.owner;
                }
            }
            set
            {
                lock (((SecurePooledObject<T>) this))
                {
                    this.owner = value;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SecurePooledObjectUser : IDisposable
        {
            private readonly SecurePooledObject<T> value;
            internal SecurePooledObjectUser(SecurePooledObject<T> value)
            {
                this.value = value;
                Monitor.Enter(value);
            }

            internal T Value
            {
                get
                {
                    return this.value.value;
                }
            }
            public void Dispose()
            {
                Monitor.Exit(this.value);
            }
        }
    }
}

