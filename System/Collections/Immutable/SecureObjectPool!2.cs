namespace System.Collections.Immutable
{
    using System;
    using System.Runtime.InteropServices;
    using Validation;

    internal class SecureObjectPool<T, TCaller> where TCaller: ISecurePooledObjectUser
    {
        private AllocFreeConcurrentStack<SecurePooledObject<T>> pool;

        public SecureObjectPool()
        {
            this.pool = new AllocFreeConcurrentStack<SecurePooledObject<T>>();
        }

        public SecurePooledObject<T> PrepNew(TCaller caller, T newValue)
        {
            Requires.NotNullAllowStructs<T>(newValue, "newValue");
            return new SecurePooledObject<T>(newValue) { Owner = caller.PoolUserId };
        }

        public void TryAdd(TCaller caller, SecurePooledObject<T> item)
        {
            lock (item)
            {
                if (caller.PoolUserId == item.Owner)
                {
                    item.Owner = Guid.Empty;
                    this.pool.TryAdd(item);
                }
            }
        }

        public bool TryTake(TCaller caller, out SecurePooledObject<T> item)
        {
            if ((caller.PoolUserId != Guid.Empty) && this.pool.TryTake(out item))
            {
                item.Owner = caller.PoolUserId;
                return true;
            }
            item = null;
            return false;
        }
    }
}

