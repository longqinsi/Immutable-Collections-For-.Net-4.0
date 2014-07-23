namespace System.Collections.Immutable
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Validation;

    internal class DictionaryEnumerator<TKey, TValue> : IDictionaryEnumerator, IEnumerator
    {
        private readonly IEnumerator<KeyValuePair<TKey, TValue>> inner;

        internal DictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> inner)
        {
            Requires.NotNull<IEnumerator<KeyValuePair<TKey, TValue>>>(inner, "inner");
            this.inner = inner;
        }

        public bool MoveNext()
        {
            return this.inner.MoveNext();
        }

        public void Reset()
        {
            this.inner.Reset();
        }

        public object Current
        {
            get
            {
                return this.Entry;
            }
        }

        public DictionaryEntry Entry
        {
            get
            {
                return new DictionaryEntry(this.inner.Current.Key, this.inner.Current.Value);
            }
        }

        public object Key
        {
            get
            {
                return this.inner.Current.Key;
            }
        }

        public object Value
        {
            get
            {
                return this.inner.Current.Value;
            }
        }
    }
}

