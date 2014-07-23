namespace System.Collections.Immutable
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    [DebuggerDisplay("Count = {stack.Count}")]
    internal class AllocFreeConcurrentStack<T>
    {
        private readonly Stack<RefAsValueType<T>> stack;

        public AllocFreeConcurrentStack()
        {
            this.stack = new Stack<RefAsValueType<T>>();
        }

        public void TryAdd(T item)
        {
            lock (this.stack)
            {
                this.stack.Push(new RefAsValueType<T>(item));
            }
        }

        public bool TryTake(out T item)
        {
            lock (this.stack)
            {
                if (this.stack.Count > 0)
                {
                    item = this.stack.Pop().Value;
                    return true;
                }
            }
            item = default(T);
            return false;
        }
    }
}

