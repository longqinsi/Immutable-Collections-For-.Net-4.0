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

    [DebuggerDisplay("IsEmpty = {IsEmpty}; Top = {head}"), DebuggerTypeProxy(typeof(ImmutableStack<>.DebuggerProxy))]
    public sealed class ImmutableStack<T> : IImmutableStack<T>, IEnumerable<T>, IEnumerable
    {
        private static readonly ImmutableStack<T> EmptyField;
        private readonly T head;
        private readonly ImmutableStack<T> tail;

        static ImmutableStack()
        {
            ImmutableStack<T>.EmptyField = new ImmutableStack<T>();
        }

        private ImmutableStack()
        {
        }

        private ImmutableStack(T head, ImmutableStack<T> tail)
        {
            Requires.NotNull<ImmutableStack<T>>(tail, "tail");
            this.head = head;
            this.tail = tail;
        }

        public ImmutableStack<T> Clear()
        {
            return ImmutableStack<T>.Empty;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator((ImmutableStack<T>) this);
        }

        public T Peek()
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException(Strings.InvalidEmptyOperation);
            }
            return this.head;
        }

        public ImmutableStack<T> Pop()
        {
            if (this.IsEmpty)
            {
                throw new InvalidOperationException(Strings.InvalidEmptyOperation);
            }
            return this.tail;
        }

        public ImmutableStack<T> Pop(out T value)
        {
            value = this.Peek();
            return this.Pop();
        }

        public ImmutableStack<T> Push(T value)
        {
            return new ImmutableStack<T>(value, (ImmutableStack<T>) this);
        }

        internal ImmutableStack<T> Reverse()
        {
            ImmutableStack<T> stack = this.Clear();
            for (ImmutableStack<T> stack2 = (ImmutableStack<T>) this; !stack2.IsEmpty; stack2 = stack2.Pop())
            {
                stack = stack.Push(stack2.Peek());
            }
            return stack;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new EnumeratorObject((ImmutableStack<T>) this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new EnumeratorObject((ImmutableStack<T>) this);
        }

        IImmutableStack<T> IImmutableStack<T>.Clear()
        {
            return this.Clear();
        }

        IImmutableStack<T> IImmutableStack<T>.Pop()
        {
            return this.Pop();
        }

        IImmutableStack<T> IImmutableStack<T>.Push(T value)
        {
            return this.Push(value);
        }

        public static ImmutableStack<T> Empty
        {
            get
            {
                return ImmutableStack<T>.EmptyField;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return (this.tail == null);
            }
        }

        private class DebuggerProxy
        {
            private T[] contents;
            private readonly ImmutableStack<T> stack;

            public DebuggerProxy(ImmutableStack<T> stack)
            {
                Requires.NotNull<ImmutableStack<T>>(stack, "stack");
                this.stack = stack;
            }

            [DebuggerBrowsable((DebuggerBrowsableState) DebuggerBrowsableState.RootHidden)]
            public T[] Contents
            {
                get
                {
                    if (this.contents == null)
                    {
                        this.contents = Enumerable.ToArray<T>(this.stack);
                    }
                    return this.contents;
                }
            }
        }

        [StructLayout(LayoutKind.Sequential), EditorBrowsable((EditorBrowsableState) EditorBrowsableState.Advanced)]
        public struct Enumerator
        {
            private readonly ImmutableStack<T> originalStack;
            private ImmutableStack<T> remainingStack;
            internal Enumerator(ImmutableStack<T> stack)
            {
                Requires.NotNull<ImmutableStack<T>>(stack, "stack");
                this.originalStack = stack;
                this.remainingStack = null;
            }

            public T Current
            {
                get
                {
                    if ((this.remainingStack == null) || this.remainingStack.IsEmpty)
                    {
                        throw new InvalidOperationException();
                    }
                    return this.remainingStack.Peek();
                }
            }
            public bool MoveNext()
            {
                if (this.remainingStack == null)
                {
                    this.remainingStack = this.originalStack;
                }
                else if (!this.remainingStack.IsEmpty)
                {
                    this.remainingStack = this.remainingStack.Pop();
                }
                return !this.remainingStack.IsEmpty;
            }
        }

        private class EnumeratorObject : IEnumerator<T>, IEnumerator, IDisposable
        {
            private bool disposed;
            private readonly ImmutableStack<T> originalStack;
            private ImmutableStack<T> remainingStack;

            internal EnumeratorObject(ImmutableStack<T> stack)
            {
                Requires.NotNull<ImmutableStack<T>>(stack, "stack");
                this.originalStack = stack;
            }

            public void Dispose()
            {
                this.disposed = true;
            }

            public bool MoveNext()
            {
                this.ThrowIfDisposed();
                if (this.remainingStack == null)
                {
                    this.remainingStack = this.originalStack;
                }
                else if (!this.remainingStack.IsEmpty)
                {
                    this.remainingStack = this.remainingStack.Pop();
                }
                return !this.remainingStack.IsEmpty;
            }

            public void Reset()
            {
                this.ThrowIfDisposed();
                this.remainingStack = null;
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
                    if ((this.remainingStack == null) || this.remainingStack.IsEmpty)
                    {
                        throw new InvalidOperationException();
                    }
                    return this.remainingStack.Peek();
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

