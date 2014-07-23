namespace System.Collections.Immutable
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.InteropServices;
    using Validation;

    [DebuggerTypeProxy(typeof(ImmutableQueue<>.DebuggerProxy)), DebuggerDisplay("IsEmpty = {IsEmpty}")]
    public sealed class ImmutableQueue<T> : IImmutableQueue<T>, IEnumerable<T>, IEnumerable
    {
        private readonly ImmutableStack<T> backwards;
        private ImmutableStack<T> backwardsReversed;
        private static readonly ImmutableQueue<T> EmptyField;
        private readonly ImmutableStack<T> forwards;

        static ImmutableQueue()
        {
            ImmutableQueue<T>.EmptyField = new ImmutableQueue<T>(ImmutableStack<T>.Empty, ImmutableStack<T>.Empty);
        }

        private ImmutableQueue(ImmutableStack<T> forward, ImmutableStack<T> backward)
        {
            Requires.NotNull<ImmutableStack<T>>(forward, "forward");
            Requires.NotNull<ImmutableStack<T>>(backward, "backward");
            this.forwards = forward;
            this.backwards = backward;
            this.backwardsReversed = null;
        }

        public ImmutableQueue<T> Clear()
        {
            return ImmutableQueue<T>.Empty;
        }

        public ImmutableQueue<T> Dequeue()
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException(Strings.InvalidEmptyOperation);
            }
            ImmutableStack<T> forward = this.forwards.Pop();
            if (!forward.IsEmpty)
            {
                return new ImmutableQueue<T>(forward, this.backwards);
            }
            if (this.backwards.IsEmpty)
            {
                return ImmutableQueue<T>.Empty;
            }
            return new ImmutableQueue<T>(this.BackwardsReversed, ImmutableStack<T>.Empty);
        }

        public ImmutableQueue<T> Dequeue(out T value)
        {
            value = this.Peek();
            return this.Dequeue();
        }

        public ImmutableQueue<T> Enqueue(T value)
        {
            if (this.IsEmpty)
            {
                return new ImmutableQueue<T>(ImmutableStack<T>.Empty.Push(value), ImmutableStack<T>.Empty);
            }
            return new ImmutableQueue<T>(this.forwards, this.backwards.Push(value));
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator((ImmutableQueue<T>) this);
        }

        public T Peek()
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException(Strings.InvalidEmptyOperation);
            }
            return this.forwards.Peek();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new EnumeratorObject((ImmutableQueue<T>) this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EnumeratorObject((ImmutableQueue<T>) this);
        }

        IImmutableQueue<T> IImmutableQueue<T>.Clear()
        {
            return this.Clear();
        }

        IImmutableQueue<T> IImmutableQueue<T>.Dequeue()
        {
            return this.Dequeue();
        }

        IImmutableQueue<T> IImmutableQueue<T>.Enqueue(T value)
        {
            return this.Enqueue(value);
        }

        private ImmutableStack<T> BackwardsReversed
        {
            get
            {
                if (this.backwardsReversed == null)
                {
                    this.backwardsReversed = this.backwards.Reverse();
                }
                return this.backwardsReversed;
            }
        }

        public static ImmutableQueue<T> Empty
        {
            get
            {
                return ImmutableQueue<T>.EmptyField;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.forwards.IsEmpty && this.backwards.IsEmpty);
            }
        }

        //[ExcludeFromCodeCoverage]
        private class DebuggerProxy
        {
            private T[] contents;
            private readonly ImmutableQueue<T> queue;

            public DebuggerProxy(ImmutableQueue<T> queue)
            {
                this.queue = queue;
            }

            [DebuggerBrowsable((DebuggerBrowsableState) DebuggerBrowsableState.RootHidden)]
            public T[] Contents
            {
                get
                {
                    if (this.contents == null)
                    {
                        this.contents = Enumerable.ToArray<T>(this.queue);
                    }
                    return this.contents;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Advanced)]
        public struct Enumerator
        {
            private readonly ImmutableQueue<T> originalQueue;
            private ImmutableStack<T> remainingForwardsStack;
            private ImmutableStack<T> remainingBackwardsStack;
            internal Enumerator(ImmutableQueue<T> queue)
            {
                this.originalQueue = queue;
                this.remainingForwardsStack = null;
                this.remainingBackwardsStack = null;
            }

            public T Current
            {
                get
                {
                    if (this.remainingForwardsStack == null)
                    {
                        throw new InvalidOperationException();
                    }
                    if (!this.remainingForwardsStack.IsEmpty)
                    {
                        return this.remainingForwardsStack.Peek();
                    }
                    if (this.remainingBackwardsStack.IsEmpty)
                    {
                        throw new InvalidOperationException();
                    }
                    return this.remainingBackwardsStack.Peek();
                }
            }
            public bool MoveNext()
            {
                if (this.remainingForwardsStack == null)
                {
                    this.remainingForwardsStack = this.originalQueue.forwards;
                    this.remainingBackwardsStack = this.originalQueue.BackwardsReversed;
                }
                else if (!this.remainingForwardsStack.IsEmpty)
                {
                    this.remainingForwardsStack = this.remainingForwardsStack.Pop();
                }
                else if (!this.remainingBackwardsStack.IsEmpty)
                {
                    this.remainingBackwardsStack = this.remainingBackwardsStack.Pop();
                }
                if (this.remainingForwardsStack.IsEmpty)
                {
                    return !this.remainingBackwardsStack.IsEmpty;
                }
                return true;
            }
        }

        private class EnumeratorObject : IEnumerator<T>, IEnumerator, IDisposable
        {
            private bool disposed;
            private readonly ImmutableQueue<T> originalQueue;
            private ImmutableStack<T> remainingBackwardsStack;
            private ImmutableStack<T> remainingForwardsStack;

            internal EnumeratorObject(ImmutableQueue<T> queue)
            {
                this.originalQueue = queue;
            }

            public void Dispose()
            {
                this.disposed = true;
            }

            public bool MoveNext()
            {
                this.ThrowIfDisposed();
                if (this.remainingForwardsStack == null)
                {
                    this.remainingForwardsStack = this.originalQueue.forwards;
                    this.remainingBackwardsStack = this.originalQueue.BackwardsReversed;
                }
                else if (!this.remainingForwardsStack.IsEmpty)
                {
                    this.remainingForwardsStack = this.remainingForwardsStack.Pop();
                }
                else if (!this.remainingBackwardsStack.IsEmpty)
                {
                    this.remainingBackwardsStack = this.remainingBackwardsStack.Pop();
                }
                if (this.remainingForwardsStack.IsEmpty)
                {
                    return !this.remainingBackwardsStack.IsEmpty;
                }
                return true;
            }

            public void Reset()
            {
                this.ThrowIfDisposed();
                this.remainingBackwardsStack = null;
                this.remainingForwardsStack = null;
            }

            private void ThrowIfDisposed()
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().FullName);
                }
            }

            public T Current
            {
                get
                {
                    this.ThrowIfDisposed();
                    if (this.remainingForwardsStack == null)
                    {
                        throw new InvalidOperationException();
                    }
                    if (!this.remainingForwardsStack.IsEmpty)
                    {
                        return this.remainingForwardsStack.Peek();
                    }
                    if (this.remainingBackwardsStack.IsEmpty)
                    {
                        throw new InvalidOperationException();
                    }
                    return this.remainingBackwardsStack.Peek();
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }
        }
    }
}

