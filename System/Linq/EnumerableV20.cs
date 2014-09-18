using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

// Include Silverlight's managed resources
#if SILVERLIGHT
using System.Core;
#endif //SILVERLIGHT

namespace System.Linq
{
    internal class Error
    {
        public static ArgumentNullException ArgumentNull(string paramName)
        {
            return new ArgumentNullException(paramName);
        }

        public static Exception NoElements()
        {
            return new Exception();
        }

        public static Exception NoMatch()
        {
            return new Exception();
        }

        public static Exception MoreThanOneElement()
        {
            return new Exception();
        }

        public static Exception MoreThanOneMatch()
        {
            return new Exception();
        }

        public static ArgumentOutOfRangeException ArgumentOutOfRange(string paramName)
        {
            return new ArgumentOutOfRangeException(paramName);
        }

        public static NotSupportedException NotSupported()
        {
            throw new NotImplementedException();
        }
    }
    public static class EnumerableV20
    {
        public static IEnumerable<TSource> Where<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            if (source is Iterator<TSource>) return ((Iterator<TSource>)source).Where(predicate);
            if (source is TSource[]) return new WhereArrayIterator<TSource>((TSource[])source, predicate);
            if (source is List<TSource>) return new WhereListIterator<TSource>((List<TSource>)source, predicate);
            return new WhereEnumerableIterator<TSource>(source, predicate);
        }



        public static IEnumerable<TSource> Where<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            return WhereIterator<TSource>(source, predicate);
        }

        static IEnumerable<TSource> WhereIterator<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int, bool> predicate) {
            int index = -1;
            foreach (TSource element in source) {
                checked { index++; }
                if (predicate(element, index)) yield return element;
            }
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, FuncV20<TSource, TResult> selector) {
            if (source == null) throw Error.ArgumentNull("source");
            if (selector == null) throw Error.ArgumentNull("selector");
            if (source is Iterator<TSource>) return ((Iterator<TSource>)source).Select(selector);
            if (source is TSource[]) return new WhereSelectArrayIterator<TSource, TResult>((TSource[])source, null, selector);
            if (source is List<TSource>) return new WhereSelectListIterator<TSource, TResult>((List<TSource>)source, null, selector);
            return new WhereSelectEnumerableIterator<TSource, TResult>(source, null, selector);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, FuncV20<TSource, int, TResult> selector) {
            if (source == null) throw Error.ArgumentNull("source");
            if (selector == null) throw Error.ArgumentNull("selector");
            return SelectIterator<TSource, TResult>(source, selector);
        }

        static IEnumerable<TResult> SelectIterator<TSource, TResult>(IEnumerable<TSource> source, FuncV20<TSource, int, TResult> selector) {
            int index = -1;
            foreach (TSource element in source) {
                checked { index++; }
                yield return selector(element, index);
            }
        }

        static FuncV20<TSource, bool> CombinePredicates<TSource>(FuncV20<TSource, bool> predicate1, FuncV20<TSource, bool> predicate2) {
            return x => predicate1(x) && predicate2(x);
        }

        static FuncV20<TSource, TResult> CombineSelectors<TSource, TMiddle, TResult>(FuncV20<TSource, TMiddle> selector1, FuncV20<TMiddle, TResult> selector2) {
            return x => selector2(selector1(x));
        }

        abstract class Iterator<TSource> : IEnumerable<TSource>, IEnumerator<TSource>
        {
            int threadId;
            internal int state;
            internal TSource current;

            public Iterator() {
                threadId = Thread.CurrentThread.ManagedThreadId;
            }

            public TSource Current {
                get { return current; }
            }

            public abstract Iterator<TSource> Clone();

            public virtual void Dispose() {
                current = default(TSource);
                state = -1;
            }

            public IEnumerator<TSource> GetEnumerator() {
                if (threadId == Thread.CurrentThread.ManagedThreadId && state == 0) {
                    state = 1;
                    return this;
                }
                Iterator<TSource> duplicate = Clone();
                duplicate.state = 1;
                return duplicate;
            }

            public abstract bool MoveNext();

            public abstract IEnumerable<TResult> Select<TResult>(FuncV20<TSource, TResult> selector);

            public abstract IEnumerable<TSource> Where(FuncV20<TSource, bool> predicate);

            object IEnumerator.Current {
                get { return Current; }
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            void IEnumerator.Reset() {
                throw new NotImplementedException();
            }
        }

        class WhereEnumerableIterator<TSource> : Iterator<TSource>
        {
            IEnumerable<TSource> source;
            FuncV20<TSource, bool> predicate;
            IEnumerator<TSource> enumerator;

            public WhereEnumerableIterator(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
                this.source = source;
                this.predicate = predicate;
            }

            public override Iterator<TSource> Clone() {
                return new WhereEnumerableIterator<TSource>(source, predicate);
            }

            public override void Dispose() {
                if (enumerator is IDisposable) ((IDisposable)enumerator).Dispose();
                enumerator = null;
                base.Dispose();
            }

            public override bool MoveNext() {
                switch (state) {
                    case 1:
                        enumerator = source.GetEnumerator();
                        state = 2;
                        goto case 2;
                    case 2:
                        while (enumerator.MoveNext()) {
                            TSource item = enumerator.Current;
                            if (predicate(item)) {
                                current = item;
                                return true;
                            }
                        }
                        Dispose();
                        break;
                }
                return false;
            }

            public override IEnumerable<TResult> Select<TResult>(FuncV20<TSource, TResult> selector) {
                return new WhereSelectEnumerableIterator<TSource, TResult>(source, predicate, selector);
            }

            public override IEnumerable<TSource> Where(FuncV20<TSource, bool> predicate) {
                return new WhereEnumerableIterator<TSource>(source, CombinePredicates(this.predicate, predicate));
            }
        }

        class WhereArrayIterator<TSource> : Iterator<TSource>
        {
            TSource[] source;
            FuncV20<TSource, bool> predicate;
            int index;

            public WhereArrayIterator(TSource[] source, FuncV20<TSource, bool> predicate) {
                this.source = source;
                this.predicate = predicate;
            }

            public override Iterator<TSource> Clone() {
                return new WhereArrayIterator<TSource>(source, predicate);
            }

            public override bool MoveNext() {
                if (state == 1) {
                    while (index < source.Length) {
                        TSource item = source[index];
                        index++;
                        if (predicate(item)) {
                            current = item;
                            return true;
                        }
                    }
                    Dispose();
                }
                return false;
            }

            public override IEnumerable<TResult> Select<TResult>(FuncV20<TSource, TResult> selector) {
                return new WhereSelectArrayIterator<TSource, TResult>(source, predicate, selector);
            }

            public override IEnumerable<TSource> Where(FuncV20<TSource, bool> predicate) {
                return new WhereArrayIterator<TSource>(source, CombinePredicates(this.predicate, predicate));
            }
        }

        class WhereListIterator<TSource> : Iterator<TSource>
        {
            List<TSource> source;
            FuncV20<TSource, bool> predicate;
            List<TSource>.Enumerator enumerator;

            public WhereListIterator(List<TSource> source, FuncV20<TSource, bool> predicate) {
                this.source = source;
                this.predicate = predicate;
            }

            public override Iterator<TSource> Clone() {
                return new WhereListIterator<TSource>(source, predicate);
            }

            public override bool MoveNext() {
                switch (state) {
                    case 1:
                        enumerator = source.GetEnumerator();
                        state = 2;
                        goto case 2;
                    case 2:
                        while (enumerator.MoveNext()) {
                            TSource item = enumerator.Current;
                            if (predicate(item)) {
                                current = item;
                                return true;
                            }
                        }
                        Dispose();
                        break;
                }
                return false;
            }

            public override IEnumerable<TResult> Select<TResult>(FuncV20<TSource, TResult> selector) {
                return new WhereSelectListIterator<TSource, TResult>(source, predicate, selector);
            }

            public override IEnumerable<TSource> Where(FuncV20<TSource, bool> predicate) {
                return new WhereListIterator<TSource>(source, CombinePredicates(this.predicate, predicate));
            }
        }

        class WhereSelectEnumerableIterator<TSource, TResult> : Iterator<TResult>
        {
            IEnumerable<TSource> source;
            FuncV20<TSource, bool> predicate;
            FuncV20<TSource, TResult> selector;
            IEnumerator<TSource> enumerator;

            public WhereSelectEnumerableIterator(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate, FuncV20<TSource, TResult> selector) {
                this.source = source;
                this.predicate = predicate;
                this.selector = selector;
            }

            public override Iterator<TResult> Clone() {
                return new WhereSelectEnumerableIterator<TSource, TResult>(source, predicate, selector);
            }

            public override void Dispose() {
                if (enumerator is IDisposable) ((IDisposable)enumerator).Dispose();
                enumerator = null;
                base.Dispose();
            }

            public override bool MoveNext() {
                switch (state) {
                    case 1:
                        enumerator = source.GetEnumerator();
                        state = 2;
                        goto case 2;
                    case 2:
                        while (enumerator.MoveNext()) {
                            TSource item = enumerator.Current;
                            if (predicate == null || predicate(item)) {
                                current = selector(item);
                                return true;
                            }
                        }
                        Dispose();
                        break;
                }
                return false;
            }

            public override IEnumerable<TResult2> Select<TResult2>(FuncV20<TResult, TResult2> selector) {
                return new WhereSelectEnumerableIterator<TSource, TResult2>(source, predicate, CombineSelectors(this.selector, selector));
            }

            public override IEnumerable<TResult> Where(FuncV20<TResult, bool> predicate) {
                return new WhereEnumerableIterator<TResult>(this, predicate);
            }
        }

        class WhereSelectArrayIterator<TSource, TResult> : Iterator<TResult>
        {
            TSource[] source;
            FuncV20<TSource, bool> predicate;
            FuncV20<TSource, TResult> selector;
            int index;

            public WhereSelectArrayIterator(TSource[] source, FuncV20<TSource, bool> predicate, FuncV20<TSource, TResult> selector) {
                this.source = source;
                this.predicate = predicate;
                this.selector = selector;
            }

            public override Iterator<TResult> Clone() {
                return new WhereSelectArrayIterator<TSource, TResult>(source, predicate, selector);
            }

            public override bool MoveNext() {
                if (state == 1) {
                    while (index < source.Length) {
                        TSource item = source[index];
                        index++;
                        if (predicate == null || predicate(item)) {
                            current = selector(item);
                            return true;
                        }
                    }
                    Dispose();
                }
                return false;
            }

            public override IEnumerable<TResult2> Select<TResult2>(FuncV20<TResult, TResult2> selector) {
                return new WhereSelectArrayIterator<TSource, TResult2>(source, predicate, CombineSelectors(this.selector, selector));
            }

            public override IEnumerable<TResult> Where(FuncV20<TResult, bool> predicate) {
                return new WhereEnumerableIterator<TResult>(this, predicate);
            }
        }

        class WhereSelectListIterator<TSource, TResult> : Iterator<TResult>
        {
            List<TSource> source;
            FuncV20<TSource, bool> predicate;
            FuncV20<TSource, TResult> selector;
            List<TSource>.Enumerator enumerator;

            public WhereSelectListIterator(List<TSource> source, FuncV20<TSource, bool> predicate, FuncV20<TSource, TResult> selector) {
                this.source = source;
                this.predicate = predicate;
                this.selector = selector;
            }

            public override Iterator<TResult> Clone() {
                return new WhereSelectListIterator<TSource, TResult>(source, predicate, selector);
            }

            public override bool MoveNext() {
                switch (state) {
                    case 1:
                        enumerator = source.GetEnumerator();
                        state = 2;
                        goto case 2;
                    case 2:
                        while (enumerator.MoveNext()) {
                            TSource item = enumerator.Current;
                            if (predicate == null || predicate(item)) {
                                current = selector(item);
                                return true;
                            }
                        }
                        Dispose();
                        break;
                }
                return false;
            }

            public override IEnumerable<TResult2> Select<TResult2>(FuncV20<TResult, TResult2> selector) {
                return new WhereSelectListIterator<TSource, TResult2>(source, predicate, CombineSelectors(this.selector, selector));
            }

            public override IEnumerable<TResult> Where(FuncV20<TResult, bool> predicate) {
                return new WhereEnumerableIterator<TResult>(this, predicate);
            }
        }

        //public static IEnumerable<TSource> Where<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
        //    if (source == null) throw Error.ArgumentNull("source");
        //    if (predicate == null) throw Error.ArgumentNull("predicate");
        //    return WhereIterator<TSource>(source, predicate);
        //}

        //static IEnumerable<TSource> WhereIterator<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate) {
        //    foreach (TSource element in source) {
        //        if (predicate(element)) yield return element;
        //    }
        //}

        //public static IEnumerable<TResult> Select<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector) {
        //    if (source == null) throw Error.ArgumentNull("source");
        //    if (selector == null) throw Error.ArgumentNull("selector");
        //    return SelectIterator<TSource, TResult>(source, selector);
        //}

        //static IEnumerable<TResult> SelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector) {
        //    foreach (TSource element in source) {
        //        yield return selector(element);
        //    }
        //}

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(IEnumerable<TSource> source, FuncV20<TSource, IEnumerable<TResult>> selector) {
            if (source == null) throw Error.ArgumentNull("source");
            if (selector == null) throw Error.ArgumentNull("selector");
            return SelectManyIterator<TSource, TResult>(source, selector);
        }

        static IEnumerable<TResult> SelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, FuncV20<TSource, IEnumerable<TResult>> selector) {
            foreach (TSource element in source) {
                foreach (TResult subElement in selector(element)) {
                    yield return subElement;
                }
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TResult>(IEnumerable<TSource> source, FuncV20<TSource, int, IEnumerable<TResult>> selector) {
            if (source == null) throw Error.ArgumentNull("source");
            if (selector == null) throw Error.ArgumentNull("selector");
            return SelectManyIterator<TSource, TResult>(source, selector);
        }

        static IEnumerable<TResult> SelectManyIterator<TSource, TResult>(IEnumerable<TSource> source, FuncV20<TSource, int, IEnumerable<TResult>> selector) {
            int index = -1;
            foreach (TSource element in source) {
                checked { index++; }
                foreach (TResult subElement in selector(element, index)) {
                    yield return subElement;
                }
            }
        }
        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(IEnumerable<TSource> source, FuncV20<TSource, int, IEnumerable<TCollection>> collectionSelector, FuncV20<TSource, TCollection, TResult> resultSelector)
        {
            if (source == null) throw Error.ArgumentNull("source");
            if (collectionSelector == null) throw Error.ArgumentNull("collectionSelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return SelectManyIterator<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        }

        static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, FuncV20<TSource, int, IEnumerable<TCollection>> collectionSelector, FuncV20<TSource, TCollection, TResult> resultSelector){
            int index = -1;
            foreach (TSource element in source){
                checked { index++; }
                foreach (TCollection subElement in collectionSelector(element, index)){
                    yield return resultSelector(element, subElement);
                }
            }
        }

        public static IEnumerable<TResult> SelectMany<TSource, TCollection, TResult>(IEnumerable<TSource> source, FuncV20<TSource, IEnumerable<TCollection>> collectionSelector, FuncV20<TSource, TCollection, TResult> resultSelector) {
            if (source == null) throw Error.ArgumentNull("source");
            if (collectionSelector == null) throw Error.ArgumentNull("collectionSelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return SelectManyIterator<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        }

        static IEnumerable<TResult> SelectManyIterator<TSource, TCollection, TResult>(IEnumerable<TSource> source, FuncV20<TSource, IEnumerable<TCollection>> collectionSelector, FuncV20<TSource, TCollection, TResult> resultSelector) {
            foreach (TSource element in source) {
                foreach (TCollection subElement in collectionSelector(element)) {
                    yield return resultSelector(element, subElement);
                }
            }
        }

        public static IEnumerable<TSource> Take<TSource>(IEnumerable<TSource> source, int count) {
            if (source == null) throw Error.ArgumentNull("source");
            return TakeIterator<TSource>(source, count);
        }

        static IEnumerable<TSource> TakeIterator<TSource>(IEnumerable<TSource> source, int count) {
            if (count > 0) {
                foreach (TSource element in source) {
                    yield return element;
                    if (--count == 0) break;
                }
            }
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            return TakeWhileIterator<TSource>(source, predicate);
        }

        static IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            foreach (TSource element in source) {
                if (!predicate(element)) break;
                yield return element;
            }
        }

        public static IEnumerable<TSource> TakeWhile<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            return TakeWhileIterator<TSource>(source, predicate);
        }

        static IEnumerable<TSource> TakeWhileIterator<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int, bool> predicate) {
            int index = -1;
            foreach (TSource element in source) {
                checked { index++; }
                if (!predicate(element, index)) break;
                yield return element;
            }
        }

        public static IEnumerable<TSource> Skip<TSource>(IEnumerable<TSource> source, int count) {
            if (source == null) throw Error.ArgumentNull("source");
            return SkipIterator<TSource>(source, count);
        }

        static IEnumerable<TSource> SkipIterator<TSource>(IEnumerable<TSource> source, int count) {
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                while (count > 0 && e.MoveNext()) count--;
                if (count <= 0) {
                    while (e.MoveNext()) yield return e.Current;
                }
            }
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            return SkipWhileIterator<TSource>(source, predicate);
        }

        static IEnumerable<TSource> SkipWhileIterator<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            bool yielding = false;
            foreach (TSource element in source) {
                if (!yielding && !predicate(element)) yielding = true;
                if (yielding) yield return element;
            }
        }

        public static IEnumerable<TSource> SkipWhile<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            return SkipWhileIterator<TSource>(source, predicate);
        }

        static IEnumerable<TSource> SkipWhileIterator<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int, bool> predicate) {
            int index = -1;
            bool yielding = false;
            foreach (TSource element in source) {
                checked { index++; }
                if (!yielding && !predicate(element, index)) yielding = true;
                if (yielding) yield return element;
            }
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, FuncV20<TOuter, TKey> outerKeySelector, FuncV20<TInner, TKey> innerKeySelector, FuncV20<TOuter, TInner, TResult> resultSelector) {
            if (outer == null) throw Error.ArgumentNull("outer");
            if (inner == null) throw Error.ArgumentNull("inner");
            if (outerKeySelector == null) throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null) throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return JoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> Join<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, FuncV20<TOuter, TKey> outerKeySelector, FuncV20<TInner, TKey> innerKeySelector, FuncV20<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer) {
            if (outer == null) throw Error.ArgumentNull("outer");
            if (inner == null) throw Error.ArgumentNull("inner");
            if (outerKeySelector == null) throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null) throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return JoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        static IEnumerable<TResult> JoinIterator<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, FuncV20<TOuter, TKey> outerKeySelector, FuncV20<TInner, TKey> innerKeySelector, FuncV20<TOuter, TInner, TResult> resultSelector, IEqualityComparer<TKey> comparer) {
            Lookup<TKey, TInner> lookup = Lookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
            foreach (TOuter item in outer) {
                Lookup<TKey, TInner>.Grouping g = lookup.GetGrouping(outerKeySelector(item), false);
                if (g != null) {
                    for (int i = 0; i < g.count; i++) {
                        yield return resultSelector(item, g.elements[i]);
                    }
                }
            }
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, FuncV20<TOuter, TKey> outerKeySelector, FuncV20<TInner, TKey> innerKeySelector, FuncV20<TOuter, IEnumerable<TInner>, TResult> resultSelector) {
            if (outer == null) throw Error.ArgumentNull("outer");
            if (inner == null) throw Error.ArgumentNull("inner");
            if (outerKeySelector == null) throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null) throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return GroupJoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupJoin<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, FuncV20<TOuter, TKey> outerKeySelector, FuncV20<TInner, TKey> innerKeySelector, FuncV20<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer) {
            if (outer == null) throw Error.ArgumentNull("outer");
            if (inner == null) throw Error.ArgumentNull("inner");
            if (outerKeySelector == null) throw Error.ArgumentNull("outerKeySelector");
            if (innerKeySelector == null) throw Error.ArgumentNull("innerKeySelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return GroupJoinIterator<TOuter, TInner, TKey, TResult>(outer, inner, outerKeySelector, innerKeySelector, resultSelector, comparer);
        }

        static IEnumerable<TResult> GroupJoinIterator<TOuter, TInner, TKey, TResult>(IEnumerable<TOuter> outer, IEnumerable<TInner> inner, FuncV20<TOuter, TKey> outerKeySelector, FuncV20<TInner, TKey> innerKeySelector, FuncV20<TOuter, IEnumerable<TInner>, TResult> resultSelector, IEqualityComparer<TKey> comparer) {
            Lookup<TKey, TInner> lookup = Lookup<TKey, TInner>.CreateForJoin(inner, innerKeySelector, comparer);
            foreach (TOuter item in outer) {
                yield return resultSelector(item, lookup[outerKeySelector(item)]);
            }
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector) {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, null, false);
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, false);
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector) {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, null, true);
        }

        public static IOrderedEnumerable<TSource> OrderByDescending<TSource, TKey>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            return new OrderedEnumerable<TSource, TKey>(source, keySelector, comparer, true);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(IOrderedEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector) {
            if (source == null) throw Error.ArgumentNull("source");
            return source.CreateOrderedEnumerable<TKey>(keySelector, null, false);
        }

        public static IOrderedEnumerable<TSource> ThenBy<TSource, TKey>(IOrderedEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source == null) throw Error.ArgumentNull("source");
            return source.CreateOrderedEnumerable<TKey>(keySelector, comparer, false);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(IOrderedEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector) {
            if (source == null) throw Error.ArgumentNull("source");
            return source.CreateOrderedEnumerable<TKey>(keySelector, null, true);
        }

        public static IOrderedEnumerable<TSource> ThenByDescending<TSource, TKey>(IOrderedEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, IComparer<TKey> comparer) {
            if (source == null) throw Error.ArgumentNull("source");
            return source.CreateOrderedEnumerable<TKey>(keySelector, comparer, true);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector) {
            return new GroupedEnumerable<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public static IEnumerable<IGrouping<TKey, TSource>> GroupBy<TSource, TKey>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            return new GroupedEnumerable<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TElement> elementSelector) {
            return new GroupedEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
        }

        public static IEnumerable<IGrouping<TKey, TElement>> GroupBy<TSource, TKey, TElement>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            return new GroupedEnumerable<TSource, TKey, TElement>(source, keySelector, elementSelector, comparer);
        }

       public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TKey, IEnumerable<TSource>, TResult> resultSelector){
           return  new GroupedEnumerable<TSource, TKey, TSource, TResult>(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TElement> elementSelector, FuncV20<TKey, IEnumerable<TElement>, TResult> resultSelector){
           return new GroupedEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, null);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TResult>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TKey, IEnumerable<TSource>, TResult> resultSelector, IEqualityComparer<TKey> comparer){
            return  new GroupedEnumerable<TSource, TKey, TSource, TResult>(source, keySelector, IdentityFunction<TSource>.Instance, resultSelector, comparer);
        }

        public static IEnumerable<TResult> GroupBy<TSource, TKey, TElement, TResult>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TElement> elementSelector, FuncV20<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer){
            return  new GroupedEnumerable<TSource, TKey, TElement, TResult>(source, keySelector, elementSelector, resultSelector, comparer);
        }

        public static IEnumerable<TSource> Concat<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second) {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return ConcatIterator<TSource>(first, second);
        }

        static IEnumerable<TSource> ConcatIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second) {
            foreach (TSource element in first) yield return element;
            foreach (TSource element in second) yield return element;
        }

        public static IEnumerable<TResult> Zip<TFirst, TSecond, TResult>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, FuncV20<TFirst, TSecond, TResult> resultSelector) {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            return ZipIterator(first, second, resultSelector);
        }

        static IEnumerable<TResult> ZipIterator<TFirst, TSecond, TResult>(IEnumerable<TFirst> first, IEnumerable<TSecond> second, FuncV20<TFirst, TSecond, TResult> resultSelector) {
            using (IEnumerator<TFirst> e1 = first.GetEnumerator())
                using (IEnumerator<TSecond> e2 = second.GetEnumerator())
                    while (e1.MoveNext() && e2.MoveNext())
                        yield return resultSelector(e1.Current, e2.Current);
        }


        public static IEnumerable<TSource> Distinct<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            return DistinctIterator<TSource>(source, null);
        }

        public static IEnumerable<TSource> Distinct<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) {
            if (source == null) throw Error.ArgumentNull("source");
            return DistinctIterator<TSource>(source, comparer);
        }

        static IEnumerable<TSource> DistinctIterator<TSource>(IEnumerable<TSource> source, IEqualityComparer<TSource> comparer) {
            Set<TSource> set = new Set<TSource>(comparer);
            foreach (TSource element in source)
                if (set.Add(element)) yield return element;
        }

        public static IEnumerable<TSource> Union<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second) {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return UnionIterator<TSource>(first, second, null);
        }

        public static IEnumerable<TSource> Union<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return UnionIterator<TSource>(first, second, comparer);
        }

        static IEnumerable<TSource> UnionIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            Set<TSource> set = new Set<TSource>(comparer);
            foreach (TSource element in first)
                if (set.Add(element)) yield return element;
            foreach (TSource element in second)
                if (set.Add(element)) yield return element;
        }

        public static IEnumerable<TSource> Intersect<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second) {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return IntersectIterator<TSource>(first, second, null);
        }

        public static IEnumerable<TSource> Intersect<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return IntersectIterator<TSource>(first, second, comparer);
        }

        static IEnumerable<TSource> IntersectIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            Set<TSource> set = new Set<TSource>(comparer);
            foreach (TSource element in second) set.Add(element);
            foreach (TSource element in first)
                if (set.Remove(element)) yield return element;
        }

        public static IEnumerable<TSource> Except<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return ExceptIterator<TSource>(first, second, null);
        }

        public static IEnumerable<TSource> Except<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            return ExceptIterator<TSource>(first, second, comparer);
        }

        static IEnumerable<TSource> ExceptIterator<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer) {
            Set<TSource> set = new Set<TSource>(comparer);
            foreach (TSource element in second) set.Add(element);
            foreach (TSource element in first)
                if (set.Add(element)) yield return element;
        }

        public static IEnumerable<TSource> Reverse<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            return ReverseIterator<TSource>(source);
        }

        static IEnumerable<TSource> ReverseIterator<TSource>(IEnumerable<TSource> source) {
            Buffer<TSource> buffer = new Buffer<TSource>(source);
            for (int i = buffer.count - 1; i >= 0; i--) yield return buffer.items[i];
        }

        public static bool SequenceEqual<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second) {
            return SequenceEqual<TSource>(first, second, null);
        }

        public static bool SequenceEqual<TSource>(IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (comparer == null) comparer = EqualityComparer<TSource>.Default;
            if (first == null) throw Error.ArgumentNull("first");
            if (second == null) throw Error.ArgumentNull("second");
            using (IEnumerator<TSource> e1 = first.GetEnumerator())
            using (IEnumerator<TSource> e2 = second.GetEnumerator())
            {
                while (e1.MoveNext())
                {
                    if (!(e2.MoveNext() && comparer.Equals(e1.Current, e2.Current))) return false;
                }
                if (e2.MoveNext()) return false;
            }
            return true;
        }

        public static IEnumerable<TSource> AsEnumerable<TSource>(IEnumerable<TSource> source)
        {
            return source;
        }

        public static TSource[] ToArray<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            return new Buffer<TSource>(source).ToArray();
        }

        public static List<TSource> ToList<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            return new List<TSource>(source);
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector) {
            return ToDictionary<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            return ToDictionary<TSource, TKey, TSource>(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TElement> elementSelector) {
            return ToDictionary<TSource, TKey, TElement>(source, keySelector, elementSelector, null);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            if (source == null) throw Error.ArgumentNull("source");
            if (keySelector == null) throw Error.ArgumentNull("keySelector");
            if (elementSelector == null) throw Error.ArgumentNull("elementSelector");
            Dictionary<TKey, TElement> d = new Dictionary<TKey, TElement>(comparer);
            foreach (TSource element in source) d.Add(keySelector(element), elementSelector(element));
            return d;
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector) {
            return Lookup<TKey, TSource>.Create(source, keySelector, IdentityFunction<TSource>.Instance, null);
        }

        public static ILookup<TKey, TSource> ToLookup<TSource, TKey>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            return Lookup<TKey, TSource>.Create(source, keySelector, IdentityFunction<TSource>.Instance, comparer);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TElement> elementSelector) {
            return Lookup<TKey, TElement>.Create(source, keySelector, elementSelector, null);
        }

        public static ILookup<TKey, TElement> ToLookup<TSource, TKey, TElement>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            return Lookup<TKey, TElement>.Create(source, keySelector, elementSelector, comparer);
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(IEnumerable<TSource> source) {
            return DefaultIfEmpty(source, default(TSource));
        }

        public static IEnumerable<TSource> DefaultIfEmpty<TSource>(IEnumerable<TSource> source, TSource defaultValue) {
            if (source == null) throw Error.ArgumentNull("source");
            return DefaultIfEmptyIterator<TSource>(source, defaultValue);
        }

        static IEnumerable<TSource> DefaultIfEmptyIterator<TSource>(IEnumerable<TSource> source, TSource defaultValue) {
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                if (e.MoveNext()) {
                    do {
                        yield return e.Current;
                    } while (e.MoveNext());
                }
                else {
                    yield return defaultValue;
                }
            }
        }

        public static IEnumerable<TResult> OfType<TResult>(IEnumerable source) {
            if (source == null) throw Error.ArgumentNull("source");
            return OfTypeIterator<TResult>(source);
        }

        static IEnumerable<TResult> OfTypeIterator<TResult>(IEnumerable source) {
            foreach (object obj in source) {
                if (obj is TResult) yield return (TResult)obj;
            }
        }

        public static IEnumerable<TResult> Cast<TResult>(IEnumerable source) {
            IEnumerable<TResult> typedSource = source as IEnumerable<TResult>;
            if (typedSource != null) return typedSource;
            if (source == null) throw Error.ArgumentNull("source");
            return CastIterator<TResult>(source);
        }

        static IEnumerable<TResult> CastIterator<TResult>(IEnumerable source) {
            foreach (object obj in source) yield return (TResult)obj;
        }

        public static TSource First<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) {
                if (list.Count > 0) return list[0];
            }
            else {
                using (IEnumerator<TSource> e = source.GetEnumerator()) {
                    if (e.MoveNext()) return e.Current;
                }
            }
            throw Error.NoElements();
        }

        public static TSource First<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            foreach (TSource element in source) {
                if (predicate(element)) return element;
            }
            throw Error.NoMatch();
        }

        public static TSource FirstOrDefault<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) {
                if (list.Count > 0) return list[0];
            }
            else {
                using (IEnumerator<TSource> e = source.GetEnumerator()) {
                    if (e.MoveNext()) return e.Current;
                }
            }
            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            foreach (TSource element in source) {
                if (predicate(element)) return element;
            }
            return default(TSource);
        }

        public static TSource Last<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) {
                int count = list.Count;
                if (count > 0) return list[count - 1];
            }
            else {
                using (IEnumerator<TSource> e = source.GetEnumerator()) {
                    if (e.MoveNext()) {
                        TSource result;
                        do {
                            result = e.Current;
                        } while (e.MoveNext());
                        return result;
                    }
                }
            }
            throw Error.NoElements();
        }

        public static TSource Last<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            TSource result = default(TSource);
            bool found = false;
            foreach (TSource element in source) {
                if (predicate(element)) {
                    result = element;
                    found = true;
                }
            }
            if (found) return result;
            throw Error.NoMatch();
        }

        public static TSource LastOrDefault<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) {
                int count = list.Count;
                if (count > 0) return list[count - 1];
            }
            else {
                using (IEnumerator<TSource> e = source.GetEnumerator()) {
                    if (e.MoveNext()) {
                        TSource result;
                        do {
                            result = e.Current;
                        } while (e.MoveNext());
                        return result;
                    }
                }
            }
            return default(TSource);
        }

        public static TSource LastOrDefault<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            TSource result = default(TSource);
            foreach (TSource element in source) {
                if (predicate(element)) {
                    result = element;
                }
            }
            return result;
        }

        public static TSource Single<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) {
                switch (list.Count) {
                    case 0: throw Error.NoElements();
                    case 1: return list[0];
                }
            }
            else {
                using (IEnumerator<TSource> e = source.GetEnumerator()) {
                    if (!e.MoveNext()) throw Error.NoElements();
                    TSource result = e.Current;
                    if (!e.MoveNext()) return result;
                }
            }
            throw Error.MoreThanOneElement();
        }

        public static TSource Single<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            TSource result = default(TSource);
            long count = 0;
            foreach (TSource element in source) {
                if (predicate(element)) {
                    result = element;
                    checked { count++; }
                }
            }
            switch (count) {
                case 0: throw Error.NoMatch();
                case 1: return result;
            }
            throw Error.MoreThanOneMatch();
        }

        public static TSource SingleOrDefault<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) {
                switch (list.Count) {
                    case 0: return default(TSource);
                    case 1: return list[0];
                }
            }
            else {
                using (IEnumerator<TSource> e = source.GetEnumerator()) {
                    if (!e.MoveNext()) return default(TSource);
                    TSource result = e.Current;
                    if (!e.MoveNext()) return result;
                }
            }
            throw Error.MoreThanOneElement();
        }

        public static TSource SingleOrDefault<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            TSource result = default(TSource);
            long count = 0;
            foreach (TSource element in source) {
                if (predicate(element)) {
                    result = element;
                    checked { count++; }
                }
            }
            switch (count) {
                case 0: return default(TSource);
                case 1: return result;
            }
            throw Error.MoreThanOneMatch();
        }

        public static TSource ElementAt<TSource>(IEnumerable<TSource> source, int index) {
            if (source == null) throw Error.ArgumentNull("source");
            IList<TSource> list = source as IList<TSource>;
            if (list != null) return list[index];
            if (index < 0) throw Error.ArgumentOutOfRange("index");
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                while (true) {
                    if (!e.MoveNext()) throw Error.ArgumentOutOfRange("index");
                    if (index == 0) return e.Current;
                    index--;
                }
            }
        }

        public static TSource ElementAtOrDefault<TSource>(IEnumerable<TSource> source, int index) {
            if (source == null) throw Error.ArgumentNull("source");
            if (index >= 0) {
                IList<TSource> list = source as IList<TSource>;
                if (list != null) {
                    if (index < list.Count) return list[index];
                }
                else {
                    using (IEnumerator<TSource> e = source.GetEnumerator()) {
                        while (true) {
                            if (!e.MoveNext()) break;
                            if (index == 0) return e.Current;
                            index--;
                        }
                    }
                }
            }
            return default(TSource);
        }

        public static IEnumerable<int> Range(int start, int count) {
            long max = ((long)start) + count - 1;
            if (count < 0 || max > Int32.MaxValue) throw Error.ArgumentOutOfRange("count");
            return RangeIterator(start, count);
        }

        static IEnumerable<int> RangeIterator(int start, int count) {
            for (int i = 0; i < count; i++) yield return start + i;
        }

        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count) {
            if (count < 0) throw Error.ArgumentOutOfRange("count");
            return RepeatIterator<TResult>(element, count);
        }

        static IEnumerable<TResult> RepeatIterator<TResult>(TResult element, int count) {
            for (int i = 0; i < count; i++) yield return element;
        }

        public static IEnumerable<TResult> Empty<TResult>() {
            return EmptyEnumerable<TResult>.Instance;
        }

        public static bool Any<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                if (e.MoveNext()) return true;
            }
            return false;
        }

        public static bool Any<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            foreach (TSource element in source) {
                if (predicate(element)) return true;
            }
            return false;
        }

        public static bool All<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            foreach (TSource element in source) {
                if (!predicate(element)) return false;
            }
            return true;
        }

        public static int Count<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            ICollection<TSource> collectionoft = source as ICollection<TSource>;
            if (collectionoft != null) return collectionoft.Count;
            ICollection collection = source as ICollection;
            if (collection != null) return collection.Count;
            int count = 0;
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                checked {
                    while (e.MoveNext()) count++;
                }
            }
            return count;
        }

        public static int Count<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            int count = 0;
            foreach (TSource element in source) {
                checked {
                    if (predicate(element)) count++;
                }
            }
            return count;
        }

        public static long LongCount<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long count = 0;
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                checked {
                    while (e.MoveNext()) count++;
                }
            }
            return count;
        }

        public static long LongCount<TSource>(IEnumerable<TSource> source, FuncV20<TSource, bool> predicate) {
            if (source == null) throw Error.ArgumentNull("source");
            if (predicate == null) throw Error.ArgumentNull("predicate");
            long count = 0;
            foreach (TSource element in source) {
                checked {
                    if (predicate(element)) count++;
                }
            }
            return count;
        }

        public static bool Contains<TSource>(IEnumerable<TSource> source, TSource value) {
            ICollection<TSource> collection = source as ICollection<TSource>;
            if (collection != null) return collection.Contains(value);
            return Contains<TSource>(source, value, null);
        }

        public static bool Contains<TSource>(IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            if (comparer == null) comparer = EqualityComparer<TSource>.Default;
            if (source == null) throw Error.ArgumentNull("source");
            foreach (TSource element in source)
                if (comparer.Equals(element, value)) return true;
            return false;
        }

        public static TSource Aggregate<TSource>(IEnumerable<TSource> source, FuncV20<TSource, TSource, TSource> func)
        {
            if (source == null) throw Error.ArgumentNull("source");
            if (func == null) throw Error.ArgumentNull("func");
            using (IEnumerator<TSource> e = source.GetEnumerator()) {
                if (!e.MoveNext()) throw Error.NoElements();
                TSource result = e.Current;
                while (e.MoveNext()) result = func(result, e.Current);
                return result;
            }
        }

        public static TAccumulate Aggregate<TSource, TAccumulate>(IEnumerable<TSource> source, TAccumulate seed, FuncV20<TAccumulate, TSource, TAccumulate> func) {
            if (source == null) throw Error.ArgumentNull("source");
            if (func == null) throw Error.ArgumentNull("func");
            TAccumulate result = seed;
            foreach (TSource element in source) result = func(result, element);
            return result;
        }

        public static TResult Aggregate<TSource, TAccumulate, TResult>(IEnumerable<TSource> source, TAccumulate seed, FuncV20<TAccumulate, TSource, TAccumulate> func, FuncV20<TAccumulate, TResult> resultSelector) {
            if (source == null) throw Error.ArgumentNull("source");
            if (func == null) throw Error.ArgumentNull("func");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            TAccumulate result = seed;
            foreach (TSource element in source) result = func(result, element);
            return resultSelector(result);
        }

        public static int Sum(IEnumerable<int> source) {
            if (source == null) throw Error.ArgumentNull("source");
            int sum = 0;
            checked {
                foreach (int v in source) sum += v;
            }
            return sum;
        }

        public static int? Sum(IEnumerable<int?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            int sum = 0;
            checked {
                foreach (int? v in source) {
                    if (v != null) sum += v.GetValueOrDefault();
                }
            }
            return sum;
        }

        public static long Sum(IEnumerable<long> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long sum = 0;
            checked {
                foreach (long v in source) sum += v;
            }
            return sum;
        }

        public static long? Sum(IEnumerable<long?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long sum = 0;
            checked {
                foreach (long? v in source) {
                    if (v != null) sum += v.GetValueOrDefault();
                }
            }
            return sum;
        }

        public static float Sum(IEnumerable<float> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            foreach (float v in source) sum += v;
            return (float)sum;
        }

        public static float? Sum(IEnumerable<float?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            foreach (float? v in source) {
                if (v != null) sum += v.GetValueOrDefault();
            }
            return (float)sum;
        }

        public static double Sum(IEnumerable<double> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            foreach (double v in source) sum += v;
            return sum;
        }

        public static double? Sum(IEnumerable<double?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            foreach (double? v in source) {
                if (v != null) sum += v.GetValueOrDefault();
            }
            return sum;
        }

        public static decimal Sum(IEnumerable<decimal> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal sum = 0;
            foreach (decimal v in source) sum += v;
            return sum;
        }

        public static decimal? Sum(IEnumerable<decimal?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal sum = 0;
            foreach (decimal? v in source) {
                if (v != null) sum += v.GetValueOrDefault();
            }
            return sum;
        }

        public static int Sum<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int> selector) {
            return EnumerableV20.Sum(EnumerableV20.Select(source, selector));
        }

        public static int? Sum<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int?> selector) {
            return EnumerableV20.Sum(EnumerableV20.Select(source, selector));
        }

        public static long Sum<TSource>(IEnumerable<TSource> source, FuncV20<TSource, long> selector) {
            return EnumerableV20.Sum(EnumerableV20.Select(source, selector));
        }

        public static long? Sum<TSource>(IEnumerable<TSource> source, FuncV20<TSource, long?> selector) {
            return EnumerableV20.Sum(EnumerableV20.Select(source, selector));
        }

        public static float Sum<TSource>(IEnumerable<TSource> source, FuncV20<TSource, float> selector) {
            return EnumerableV20.Sum(EnumerableV20.Select(source, selector));
        }

        public static float? Sum<TSource>(IEnumerable<TSource> source, FuncV20<TSource, float?> selector) {
            return EnumerableV20.Sum(EnumerableV20.Select(source, selector));
        }

        public static double Sum<TSource>(IEnumerable<TSource> source, FuncV20<TSource, double> selector) {
            return EnumerableV20.Sum(EnumerableV20.Select(source, selector));
        }

        public static double? Sum<TSource>(IEnumerable<TSource> source, FuncV20<TSource, double?> selector) {
            return EnumerableV20.Sum(EnumerableV20.Select(source, selector));
        }

        public static decimal Sum<TSource>(IEnumerable<TSource> source, FuncV20<TSource, decimal> selector) {
            return EnumerableV20.Sum(EnumerableV20.Select(source, selector));
        }

        public static decimal? Sum<TSource>(IEnumerable<TSource> source, FuncV20<TSource, decimal?> selector) {
            return EnumerableV20.Sum(EnumerableV20.Select(source, selector));
        }

        public static int Min(IEnumerable<int> source) {
            if (source == null) throw Error.ArgumentNull("source");
            int value = 0;
            bool hasValue = false;
            foreach (int x in source) {
                if (hasValue) {
                    if (x < value) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static int? Min(IEnumerable<int?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            int? value = null;
            foreach (int? x in source) {
                if (value == null || x < value)
                    value = x;
            }
            return value;
        }

        public static long Min(IEnumerable<long> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long value = 0;
            bool hasValue = false;
            foreach (long x in source) {
                if (hasValue) {
                    if (x < value) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static long? Min(IEnumerable<long?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long? value = null;
            foreach (long? x in source) {
                if (value == null || x < value) value = x;
            }
            return value;
        }

        public static float Min(IEnumerable<float> source) {
            if (source == null) throw Error.ArgumentNull("source");
            float value = 0;
            bool hasValue = false;
            foreach (float x in source) {
                if (hasValue) {
                    // Normally NaN < anything is false, as is anything < NaN
                    // However, this leads to some irksome outcomes in Min and Max.
                    // If we use those semantics then Min(NaN, 5.0) is NaN, but
                    // Min(5.0, NaN) is 5.0!  To fix this, we impose a total
                    // ordering where NaN is smaller than every value, including
                    // negative infinity.
                    if (x < value || System.Single.IsNaN(x)) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static float? Min(IEnumerable<float?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            float? value = null;
            foreach (float? x in source) {
                if (x == null) continue;
                if (value == null || x < value || System.Single.IsNaN((float)x)) value = x;
            }
            return value;
        }

        public static double Min(IEnumerable<double> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double value = 0;
            bool hasValue = false;
            foreach (double x in source) {
                if (hasValue) {
                    if (x < value || System.Double.IsNaN(x)) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static double? Min(IEnumerable<double?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double? value = null;
            foreach (double? x in source) {
                if (x == null) continue;
                if (value == null || x < value || System.Double.IsNaN((double)x)) value = x;
            }
            return value;
        }

        public static decimal Min(IEnumerable<decimal> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal value = 0;
            bool hasValue = false;
            foreach (decimal x in source) {
                if (hasValue) {
                    if (x < value) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static decimal? Min(IEnumerable<decimal?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal? value = null;
            foreach (decimal? x in source) {
                if (value == null || x < value) value = x;
            }
            return value;
        }

        public static TSource Min<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            Comparer<TSource> comparer = Comparer<TSource>.Default;
            TSource value = default(TSource);
            if (value == null) {
                foreach (TSource x in source) {
                    if (x != null && (value == null || comparer.Compare(x, value) < 0))
                        value = x;
                }
                return value;
            }
            else {
                bool hasValue = false;
                foreach (TSource x in source) {
                    if (hasValue) {
                        if (comparer.Compare(x, value) < 0)
                            value = x;
                    }
                    else {
                        value = x;
                        hasValue = true;
                    }
                }
                if (hasValue) return value;
                throw Error.NoElements();
            }
        }

        public static int Min<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int> selector) {
            return EnumerableV20.Min(EnumerableV20.Select(source, selector));
        }

        public static int? Min<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int?> selector) {
            return EnumerableV20.Min(EnumerableV20.Select(source, selector));
        }

        public static long Min<TSource>(IEnumerable<TSource> source, FuncV20<TSource, long> selector) {
            return EnumerableV20.Min(EnumerableV20.Select(source, selector));
        }

        public static long? Min<TSource>(IEnumerable<TSource> source, FuncV20<TSource, long?> selector) {
            return EnumerableV20.Min(EnumerableV20.Select(source, selector));
        }

        public static float Min<TSource>(IEnumerable<TSource> source, FuncV20<TSource, float> selector) {
            return EnumerableV20.Min(EnumerableV20.Select(source, selector));
        }

        public static float? Min<TSource>(IEnumerable<TSource> source, FuncV20<TSource, float?> selector) {
            return EnumerableV20.Min(EnumerableV20.Select(source, selector));
        }

        public static double Min<TSource>(IEnumerable<TSource> source, FuncV20<TSource, double> selector) {
            return EnumerableV20.Min(EnumerableV20.Select(source, selector));
        }

        public static double? Min<TSource>(IEnumerable<TSource> source, FuncV20<TSource, double?> selector) {
            return EnumerableV20.Min(EnumerableV20.Select(source, selector));
        }

        public static decimal Min<TSource>(IEnumerable<TSource> source, FuncV20<TSource, decimal> selector) {
            return EnumerableV20.Min(EnumerableV20.Select(source, selector));
        }

        public static decimal? Min<TSource>(IEnumerable<TSource> source, FuncV20<TSource, decimal?> selector) {
            return EnumerableV20.Min(EnumerableV20.Select(source, selector));
        }

        public static TResult Min<TSource, TResult>(IEnumerable<TSource> source, FuncV20<TSource, TResult> selector) {
            return EnumerableV20.Min(EnumerableV20.Select(source, selector));
        }

        public static int Max(IEnumerable<int> source) {
            if (source == null) throw Error.ArgumentNull("source");
            int value = 0;
            bool hasValue = false;
            foreach (int x in source) {
                if (hasValue) {
                    if (x > value) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static int? Max(IEnumerable<int?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            int? value = null;
            foreach (int? x in source) {
                if (value == null || x > value) value = x;
            }
            return value;
        }

        public static long Max(IEnumerable<long> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long value = 0;
            bool hasValue = false;
            foreach (long x in source) {
                if (hasValue) {
                    if (x > value) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static long? Max(IEnumerable<long?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long? value = null;
            foreach (long? x in source) {
                if (value == null || x > value) value = x;
            }
            return value;
        }

        public static double Max(IEnumerable<double> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double value = 0;
            bool hasValue = false;
            foreach (double x in source) {
                if (hasValue) {
                    if (x > value || System.Double.IsNaN(value)) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static double? Max(IEnumerable<double?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double? value = null;
            foreach (double? x in source) {
                if (x == null) continue;
                if (value == null || x > value || System.Double.IsNaN((double)value)) value = x;
            }
            return value;
        }

        public static float Max(IEnumerable<float> source) {
            if (source == null) throw Error.ArgumentNull("source");
            float value = 0;
            bool hasValue = false;
            foreach (float x in source) {
                if (hasValue) {
                    if (x > value || System.Double.IsNaN(value)) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static float? Max(IEnumerable<float?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            float? value = null;
            foreach (float? x in source) {
                if (x == null) continue;
                if (value == null || x > value || System.Single.IsNaN((float)value)) value = x;
            }
            return value;
        }

        public static decimal Max(IEnumerable<decimal> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal value = 0;
            bool hasValue = false;
            foreach (decimal x in source) {
                if (hasValue) {
                    if (x > value) value = x;
                }
                else {
                    value = x;
                    hasValue = true;
                }
            }
            if (hasValue) return value;
            throw Error.NoElements();
        }

        public static decimal? Max(IEnumerable<decimal?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal? value = null;
            foreach (decimal? x in source) {
                if (value == null || x > value) value = x;
            }
            return value;
        }

        public static TSource Max<TSource>(IEnumerable<TSource> source) {
            if (source == null) throw Error.ArgumentNull("source");
            Comparer<TSource> comparer = Comparer<TSource>.Default;
            TSource value = default(TSource);
            if (value == null) {
                foreach (TSource x in source) {
                    if (x != null && (value == null || comparer.Compare(x, value) > 0))
                        value = x;
                }
                return value;
            }
            else {
                bool hasValue = false;
                foreach (TSource x in source) {
                    if (hasValue) {
                        if (comparer.Compare(x, value) > 0)
                            value = x;
                    }
                    else {
                        value = x;
                        hasValue = true;
                    }
                }
                if (hasValue) return value;
                throw Error.NoElements();
            }
        }

        public static int Max<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int> selector) {
            return EnumerableV20.Max(EnumerableV20.Select(source, selector));
        }

        public static int? Max<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int?> selector) {
            return EnumerableV20.Max(EnumerableV20.Select(source, selector));
        }

        public static long Max<TSource>(IEnumerable<TSource> source, FuncV20<TSource, long> selector) {
            return EnumerableV20.Max(EnumerableV20.Select(source, selector));
        }

        public static long? Max<TSource>(IEnumerable<TSource> source, FuncV20<TSource, long?> selector) {
            return EnumerableV20.Max(EnumerableV20.Select(source, selector));
        }

        public static float Max<TSource>(IEnumerable<TSource> source, FuncV20<TSource, float> selector) {
            return EnumerableV20.Max(EnumerableV20.Select(source, selector));
        }

        public static float? Max<TSource>(IEnumerable<TSource> source, FuncV20<TSource, float?> selector) {
            return EnumerableV20.Max(EnumerableV20.Select(source, selector));
        }

        public static double Max<TSource>(IEnumerable<TSource> source, FuncV20<TSource, double> selector) {
            return EnumerableV20.Max(EnumerableV20.Select(source, selector));
        }

        public static double? Max<TSource>(IEnumerable<TSource> source, FuncV20<TSource, double?> selector) {
            return EnumerableV20.Max(EnumerableV20.Select(source, selector));
        }

        public static decimal Max<TSource>(IEnumerable<TSource> source, FuncV20<TSource, decimal> selector) {
            return EnumerableV20.Max(EnumerableV20.Select(source, selector));
        }

        public static decimal? Max<TSource>(IEnumerable<TSource> source, FuncV20<TSource, decimal?> selector) {
            return EnumerableV20.Max(EnumerableV20.Select(source, selector));
        }

        public static TResult Max<TSource, TResult>(IEnumerable<TSource> source, FuncV20<TSource, TResult> selector) {
            return EnumerableV20.Max(EnumerableV20.Select(source, selector));
        }

        public static double Average(IEnumerable<int> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long sum = 0;
            long count = 0;
            checked {
                foreach (int v in source) {
                    sum += v;
                    count++;
                }
            }
            if (count > 0) return (double)sum / count;
            throw Error.NoElements();
        }

        public static double? Average(IEnumerable<int?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long sum = 0;
            long count = 0;
            checked {
                foreach (int? v in source) {
                    if (v != null) {
                        sum += v.GetValueOrDefault();
                        count++;
                    }
                }
            }
            if (count > 0) return (double)sum / count;
            return null;
        }

        public static double Average(IEnumerable<long> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long sum = 0;
            long count = 0;
            checked {
                foreach (long v in source) {
                    sum += v;
                    count++;
                }
            }
            if (count > 0) return (double)sum / count;
            throw Error.NoElements();
        }

        public static double? Average(IEnumerable<long?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            long sum = 0;
            long count = 0;
            checked {
                foreach (long? v in source) {
                    if (v != null) {
                        sum += v.GetValueOrDefault();
                        count++;
                    }
                }
            }
            if (count > 0) return (double)sum / count;
            return null;
        }

        public static float Average(IEnumerable<float> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            long count = 0;
            checked {
                foreach (float v in source) {
                    sum += v;
                    count++;
                }
            }
            if (count > 0) return (float)(sum / count);
            throw Error.NoElements();
        }

        public static float? Average(IEnumerable<float?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            long count = 0;
            checked {
                foreach (float? v in source) {
                    if (v != null) {
                        sum += v.GetValueOrDefault();
                        count++;
                    }
                }
            }
            if (count > 0) return (float)(sum / count);
            return null;
        }

        public static double Average(IEnumerable<double> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            long count = 0;
            checked {
                foreach (double v in source) {
                    sum += v;
                    count++;
                }
            }
            if (count > 0) return sum / count;
            throw Error.NoElements();
        }

        public static double? Average(IEnumerable<double?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            double sum = 0;
            long count = 0;
            checked {
                foreach (double? v in source) {
                    if (v != null) {
                        sum += v.GetValueOrDefault();
                        count++;
                    }
                }
            }
            if (count > 0) return sum / count;
            return null;
        }

        public static decimal Average(IEnumerable<decimal> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal sum = 0;
            long count = 0;
            checked {
                foreach (decimal v in source) {
                    sum += v;
                    count++;
                }
            }
            if (count > 0) return sum / count;
            throw Error.NoElements();
        }

        public static decimal? Average(IEnumerable<decimal?> source) {
            if (source == null) throw Error.ArgumentNull("source");
            decimal sum = 0;
            long count = 0;
            checked {
                foreach (decimal? v in source) {
                    if (v != null) {
                        sum += v.GetValueOrDefault();
                        count++;
                    }
                }
            }
            if (count > 0) return sum / count;
            return null;
        }

        public static double Average<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int> selector) {
            return EnumerableV20.Average(EnumerableV20.Select(source, selector));
        }

        public static double? Average<TSource>(IEnumerable<TSource> source, FuncV20<TSource, int?> selector) {
            return EnumerableV20.Average(EnumerableV20.Select(source, selector));
        }

        public static double Average<TSource>(IEnumerable<TSource> source, FuncV20<TSource, long> selector) {
            return EnumerableV20.Average(EnumerableV20.Select(source, selector));
        }

        public static double? Average<TSource>(IEnumerable<TSource> source, FuncV20<TSource, long?> selector) {
            return EnumerableV20.Average(EnumerableV20.Select(source, selector));
        }

        public static float Average<TSource>(IEnumerable<TSource> source, FuncV20<TSource, float> selector) {
            return EnumerableV20.Average(EnumerableV20.Select(source, selector));
        }

        public static float? Average<TSource>(IEnumerable<TSource> source, FuncV20<TSource, float?> selector) {
            return EnumerableV20.Average(EnumerableV20.Select(source, selector));
        }

        public static double Average<TSource>(IEnumerable<TSource> source, FuncV20<TSource, double> selector) {
            return EnumerableV20.Average(EnumerableV20.Select(source, selector));
        }

        public static double? Average<TSource>(IEnumerable<TSource> source, FuncV20<TSource, double?> selector) {
            return EnumerableV20.Average(EnumerableV20.Select(source, selector));
        }

        public static decimal Average<TSource>(IEnumerable<TSource> source, FuncV20<TSource, decimal> selector) {
            return EnumerableV20.Average(EnumerableV20.Select(source, selector));
        }

        public static decimal? Average<TSource>(IEnumerable<TSource> source, FuncV20<TSource, decimal?> selector) {
            return EnumerableV20.Average(EnumerableV20.Select(source, selector));
        }

        #region Added by longqinsi 2014/09/18
        /// <summary>对 IEnumerable&lt;T&gt;的每个元素执行指定操作</summary>
        /// <typeparam name="T">IEnumerable&lt;T&gt;序列的元素类型</typeparam>
        /// <param name="enumerable">IEnumerable&lt;T&gt;序列</param>
        /// <param name="action">要对IEnumerable&lt;T&gt;的每个元素执行的 Action&lt;T&gt;委托。</param>
        /// <exception cref="ArgumentNullException"><paramref name="enumerable"/>或<paramref name="action"/>的值为null。</exception>
        public static void ForEach<T>(IEnumerable<T> enumerable, Action<T> action)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        /// <summary>
        /// 对两个枚举数进行比较。
        /// </summary>
        /// <remarks>
        /// 将对两个枚举数的元素挨个比较。如果任意一次比较不相等，就以该次比较的结果作为最终比较结果。
        /// 如果如此比较到其中一个枚举数的元素已经枚举完毕，那么：
        /// 1.如果另一个枚举数也已枚举完毕，则两者相等；
        /// 2.否则，则以仍有剩余元素者为大。
        /// </remarks>
        /// <typeparam name="TSource">枚举的对象的类型</typeparam>
        /// <param name="x">要比较的第一个枚举数</param>
        /// <param name="y">要比较的第二个枚举数</param>
        /// <param name="comparison">比较元素时要使用的 <see cref="System.Comparison&lt;T&gt;"/></param>
        /// <returns>一个有符号整数，指示 <paramref name="x"/> 与 <paramref name="y"/> 的相对值，如下表所示。
        /// <list type="table">
        ///    <listheader>
        ///        <term>值</term>
        ///        <description>含义</description>
        ///    </listheader>
        ///    <item>
        ///        <term>小于0</term>
        ///        <description><paramref name="x"/>小于<paramref name="y"/></description>
        ///    </item>
        ///    <item>
        ///        <term>0</term>
        ///        <description><paramref name="x"/>等于<paramref name="y"/></description>
        ///    </item>
        ///    <item>
        ///        <term>大于0</term>
        ///        <description><paramref name="x"/>大于<paramref name="y"/></description>
        ///    </item>
        ///</list>
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="x"/>, <paramref name="y"/>或<paramref name="comparison"/>等于null。</exception>
        /// <exception cref="ArgumentException"><paramref name="comparison"/> 的实现导致比较时出现错误。 例如，将某个项与其自身进行比较时，<paramref name="comparison"/> 可能不返回 0。</exception>
        public static int CompareTo<TSource>(IEnumerable<TSource> x, IEnumerable<TSource> y, Comparison<TSource> comparison)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }
            if (comparison == null)
            {
                throw new ArgumentNullException("comparison");
            }
            var enumeratorX = x.GetEnumerator();
            var enumeratorY = y.GetEnumerator();
            while (true)
            {
                if (!enumeratorX.MoveNext())
                {
                    if (!enumeratorY.MoveNext())
                    {
                        return 0;
                    }
                    return -1;
                }
                if (!enumeratorY.MoveNext())
                {
                    return 1;
                }
                var currentCompareResult = comparison(enumeratorX.Current, enumeratorY.Current);
                if (currentCompareResult != 0)
                {
                    return currentCompareResult;
                }
            }
        }


        /// <summary>
        /// 使用指定的比较器对两个枚举数进行比较。
        /// </summary>
        /// <remarks>
        /// 将对两个枚举数的元素挨个比较。如果任意一次比较不相等，就以该次比较的结果作为最终比较结果。
        /// 如果如此比较到其中一个枚举数的元素已经枚举完毕，那么：
        /// 1.如果另一个枚举数也已枚举完毕，则两者相等；
        /// 2.否则，则以仍有剩余元素者为大。
        /// </remarks>
        /// <typeparam name="TSource">枚举的对象的类型</typeparam>
        /// <param name="x">要比较的第一个枚举数</param>
        /// <param name="y">要比较的第二个枚举数</param>
        /// <param name="comparer">比较元素时要使用的 <see cref="IComparer&lt;T&gt;"/> 实现，或者为null，表示使用默认比较器 <see cref="Comparer&lt;T&gt;.Default"/></param>
        /// <returns>一个有符号整数，指示 <paramref name="x"/> 与 <paramref name="y"/> 的相对值，如下表所示。
        /// <list type="table">
        ///    <listheader>
        ///        <term>值</term>
        ///        <description>含义</description>
        ///    </listheader>
        ///    <item>
        ///        <term>小于0</term>
        ///        <description><paramref name="x"/>小于<paramref name="y"/></description>
        ///    </item>
        ///    <item>
        ///        <term>0</term>
        ///        <description><paramref name="x"/>等于<paramref name="y"/></description>
        ///    </item>
        ///    <item>
        ///        <term>大于0</term>
        ///        <description><paramref name="x"/>大于<paramref name="y"/></description>
        ///    </item>
        ///</list>
        /// </returns>
        /// <exception cref="InvalidOperationException">comparer 为 null，且默认比较器 <see cref="Comparer&lt;T&gt;.Default"/> 找不到<typeparamref name="TSource"/> 类型的 <see cref="IComparable&lt;T&gt;"/> 泛型接口或 <see cref="IComparable"/> 接口的实现。</exception>
        /// <exception cref="ArgumentNullException"><paramref name="x"/>或<paramref name="y"/>等于null。</exception>
        /// <exception cref="ArgumentException"><paramref name="comparer"/> 的实现导致比较时出现错误。 例如，将某个项与其自身进行比较时，<paramref name="comparer"/> 可能不返回 0。</exception>
        public static int CompareTo<TSource>(IEnumerable<TSource> x, IEnumerable<TSource> y, IComparer<TSource> comparer)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }
            
            if (comparer == null)
            {
                comparer = Comparer<TSource>.Default;
            }
            var enumeratorX = x.GetEnumerator();
            var enumeratorY = y.GetEnumerator();
            while (true)
            {
                if (!enumeratorX.MoveNext())
                {
                    if (!enumeratorY.MoveNext())
                    {
                        return 0;
                    }
                    return -1;
                }
                if (!enumeratorY.MoveNext())
                {
                    return 1;
                }
                var currentCompareResult = comparer.Compare(enumeratorX.Current, enumeratorY.Current);
                if (currentCompareResult != 0)
                {
                    return currentCompareResult;
                }
            }
        }

        /// <summary>
        /// 对两个枚举数进行比较。
        /// </summary>
        /// <remarks>
        /// 将对两个枚举数的元素挨个比较。如果任意一次比较不相等，就以该次比较的结果作为最终比较结果。
        /// 如果如此比较到其中一个枚举数的元素已经枚举完毕，那么：
        /// 1.如果另一个枚举数也已枚举完毕，则两者相等；
        /// 2.否则，则以仍有剩余元素者为大。
        /// </remarks>
        /// <typeparam name="TSource">枚举的对象的类型，该类型必须实现<see cref="IComparable&lt;T&gt;"/>接口</typeparam>
        /// <param name="x">要比较的第一个枚举数</param>
        /// <param name="y">要比较的第二个枚举数</param>
        /// <returns>一个有符号整数，指示 <paramref name="x"/> 与 <paramref name="y"/> 的相对值，如下表所示。
        /// <list type="table">
        ///    <listheader>
        ///        <term>值</term>
        ///        <description>含义</description>
        ///    </listheader>
        ///    <item>
        ///        <term>小于0</term>
        ///        <description><paramref name="x"/>小于<paramref name="y"/></description>
        ///    </item>
        ///    <item>
        ///        <term>0</term>
        ///        <description><paramref name="x"/>等于<paramref name="y"/></description>
        ///    </item>
        ///    <item>
        ///        <term>大于0</term>
        ///        <description><paramref name="x"/>大于<paramref name="y"/></description>
        ///    </item>
        ///</list>
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="x"/>或<paramref name="y"/>等于null。</exception>
        public static int CompareTo<TSource>(IEnumerable<TSource> x, IEnumerable<TSource> y) where TSource : IComparable<TSource>
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            var enumeratorX = x.GetEnumerator();
            var enumeratorY = y.GetEnumerator();
            while (true)
            {
                if (!enumeratorX.MoveNext())
                {
                    if (!enumeratorY.MoveNext())
                    {
                        return 0;
                    }
                    return -1;
                }
                if (!enumeratorY.MoveNext())
                {
                    return 1;
                }
                var currentCompareResult = ReferenceEquals(enumeratorX.Current, null) ? (ReferenceEquals(enumeratorY.Current, null) ? 0 : -1) : enumeratorX.Current.CompareTo(enumeratorY.Current);
                if (currentCompareResult != 0)
                {
                    return currentCompareResult;
                }
            }
        }

        /// <summary>
        /// 判断第一个枚举数是否以第二个枚举数作为开始部分
        /// </summary>
        /// <typeparam name="TSource">枚举的对象的类型，该类型必须实现<see cref="IEquatable&lt;T&gt;"/>接口</typeparam>
        /// <param name="x">第一个枚举数</param>
        /// <param name="y">第二个枚举数</param>
        /// <returns>判断结果</returns>
        /// <exception cref="ArgumentNullException"><paramref name="x"/>或<paramref name="y"/>为null。</exception>
        public static bool StartsWith<TSource>(IEnumerable<TSource> x, IEnumerable<TSource> y) where TSource : IEquatable<TSource>
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            var enumeratorX = x.GetEnumerator();
            var enumeratorY = y.GetEnumerator();
            while (true)
            {
                if (!enumeratorX.MoveNext())
                {
                    if (!enumeratorY.MoveNext())
                    {
                        return true;
                    }
                    return false;
                }
                if (!enumeratorY.MoveNext())
                {
                    return true;
                }
                var ifCurrentEqual = ReferenceEquals(enumeratorX.Current, null) ? ReferenceEquals(enumeratorY.Current, null) : enumeratorX.Current.Equals(enumeratorY.Current);
                if (!ifCurrentEqual)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 判断第一个枚举数是否以第二个枚举数作为开始部分
        /// </summary>
        /// <typeparam name="TSource">枚举的对象的类型，该类型必须实现<see cref="IEquatable&lt;T&gt;"/>接口</typeparam>
        /// <param name="x">第一个枚举数</param>
        /// <param name="y">第二个枚举数</param>
        /// <param name="equalityComparer">用于执行相等比较的接口对象</param>
        /// <returns>判断结果</returns>
        /// <exception cref="ArgumentNullException"><paramref name="x"/>,<paramref name="y"/>, 或<paramref name="equalityComparer"/>为null。</exception>
        public static bool StartsWith<TSource>(IEnumerable<TSource> x, IEnumerable<TSource> y, IEqualityComparer<TSource> equalityComparer)
        {
            if (x == null)
            {
                throw new ArgumentNullException("x");
            }
            if (y == null)
            {
                throw new ArgumentNullException("y");
            }

            if (equalityComparer == null)
            {
                throw new ArgumentNullException("equalityComparer");
            }

            var enumeratorX = x.GetEnumerator();
            var enumeratorY = y.GetEnumerator();
            while (true)
            {
                if (!enumeratorX.MoveNext())
                {
                    if (!enumeratorY.MoveNext())
                    {
                        return true;
                    }
                    return false;
                }
                if (!enumeratorY.MoveNext())
                {
                    return true;
                }
                var ifCurrentEqual = equalityComparer.Equals(enumeratorX.Current, enumeratorY.Current);
                if (!ifCurrentEqual)
                {
                    return false;
                }
            }
        }
        #endregion
    }

    internal class EmptyEnumerable<TElement>
    {
        static volatile TElement[] instance;

        public static IEnumerable<TElement> Instance {
            get {
                if (instance == null) instance = new TElement[0];
                return instance;
            }
        }
    }

    internal class IdentityFunction<TElement>
    {
        public static FuncV20<TElement, TElement> Instance {
            get { return x => x; }
        }
    }

    public interface IOrderedEnumerable<TElement> : IEnumerable<TElement>
    {
        IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(FuncV20<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
    }

#if SILVERLIGHT && !FEATURE_NETCORE
    public interface IGrouping<TKey, TElement> : IEnumerable<TElement>
#else
    public interface IGrouping<TKey, TElement> : IEnumerable<TElement>
#endif
    {
        TKey Key { get; }
    }

    public interface ILookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>{
        int Count { get; }
        IEnumerable<TElement> this[TKey key] { get; }
        bool Contains(TKey key);
    }

    public class Lookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>, ILookup<TKey, TElement>{
        IEqualityComparer<TKey> comparer;
        Grouping[] groupings;
        Grouping lastGrouping;
        int count;

        internal static Lookup<TKey, TElement> Create<TSource>(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            if (source == null) throw Error.ArgumentNull("source");
            if (keySelector == null) throw Error.ArgumentNull("keySelector");
            if (elementSelector == null) throw Error.ArgumentNull("elementSelector");
            Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
            foreach (TSource item in source) {
                lookup.GetGrouping(keySelector(item), true).Add(elementSelector(item));
            }
            return lookup;
        }

        internal static Lookup<TKey, TElement> CreateForJoin(IEnumerable<TElement> source, FuncV20<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer) {
            Lookup<TKey, TElement> lookup = new Lookup<TKey, TElement>(comparer);
            foreach (TElement item in source) {
                TKey key = keySelector(item);
                if (key != null) lookup.GetGrouping(key, true).Add(item);
            }
            return lookup;
        }

        Lookup(IEqualityComparer<TKey> comparer) {
            if (comparer == null) comparer = EqualityComparer<TKey>.Default;
            this.comparer = comparer;
            groupings = new Grouping[7];
        }

        public int Count {
            get { return count; }
        }

        public IEnumerable<TElement> this[TKey key] {
            get {
                Grouping grouping = GetGrouping(key, false);
                if (grouping != null) return grouping;
                return EmptyEnumerable<TElement>.Instance;
            }
        }

        public bool Contains(TKey key) {
            return GetGrouping(key, false) != null;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() {
            Grouping g = lastGrouping;
            if (g != null) {
                do {
                    g = g.next;
                    yield return g;
                } while (g != lastGrouping);
            }
        }

        public IEnumerable<TResult> ApplyResultSelector<TResult>(FuncV20<TKey, IEnumerable<TElement>, TResult> resultSelector){
            Grouping g = lastGrouping;
            if (g != null) {
                do {
                    g = g.next;
                    if (g.count != g.elements.Length) { Array.Resize<TElement>(ref g.elements, g.count); }
                    yield return resultSelector(g.key, g.elements);
                }while (g != lastGrouping);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        internal int InternalGetHashCode(TKey key)
        {
            //[....] DevDivBugs 171937. work around comparer implementations that throw when passed null
            return (key == null) ? 0 : comparer.GetHashCode(key) & 0x7FFFFFFF;
        }

        internal Grouping GetGrouping(TKey key, bool create) {
            int hashCode = InternalGetHashCode(key);
            for (Grouping g = groupings[hashCode % groupings.Length]; g != null; g = g.hashNext)
                if (g.hashCode == hashCode && comparer.Equals(g.key, key)) return g;
            if (create) {
                if (count == groupings.Length) Resize();
                int index = hashCode % groupings.Length;
                Grouping g = new Grouping();
                g.key = key;
                g.hashCode = hashCode;
                g.elements = new TElement[1];
                g.hashNext = groupings[index];
                groupings[index] = g;
                if (lastGrouping == null) {
                    g.next = g;
                }
                else {
                    g.next = lastGrouping.next;
                    lastGrouping.next = g;
                }
                lastGrouping = g;
                count++;
                return g;
            }
            return null;
        }

        void Resize() {
            int newSize = checked(count * 2 + 1);
            Grouping[] newGroupings = new Grouping[newSize];
            Grouping g = lastGrouping;
            do {
                g = g.next;
                int index = g.hashCode % newSize;
                g.hashNext = newGroupings[index];
                newGroupings[index] = g;
            } while (g != lastGrouping);
            groupings = newGroupings;
        }

        internal class Grouping : IGrouping<TKey, TElement>, IList<TElement>
        {
            internal TKey key;
            internal int hashCode;
            internal TElement[] elements;
            internal int count;
            internal Grouping hashNext;
            internal Grouping next;

            internal void Add(TElement element) {
                if (elements.Length == count) Array.Resize(ref elements, checked(count * 2));
                elements[count] = element;
                count++;
            }

            public IEnumerator<TElement> GetEnumerator() {
                for (int i = 0; i < count; i++) yield return elements[i];
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            // DDB195907: implement IGrouping<>.Key implicitly
            // so that WPF binding works on this property.
            public TKey Key {
                get { return key; }
            }

            int ICollection<TElement>.Count {
                get { return count; }
            }

            bool ICollection<TElement>.IsReadOnly {
                get { return true; }
            }

            void ICollection<TElement>.Add(TElement item) {
                throw Error.NotSupported();
            }

            void ICollection<TElement>.Clear() {
                throw Error.NotSupported();
            }

            bool ICollection<TElement>.Contains(TElement item) {
                return Array.IndexOf(elements, item, 0, count) >= 0;
            }

            void ICollection<TElement>.CopyTo(TElement[] array, int arrayIndex) {
                Array.Copy(elements, 0, array, arrayIndex, count);
            }

            bool ICollection<TElement>.Remove(TElement item) {
                throw Error.NotSupported();
            }

            int IList<TElement>.IndexOf(TElement item) {
                return Array.IndexOf(elements, item, 0, count);
            }

            void IList<TElement>.Insert(int index, TElement item) {
                throw Error.NotSupported();
            }

            void IList<TElement>.RemoveAt(int index) {
                throw Error.NotSupported();
            }

            TElement IList<TElement>.this[int index] {
                get {
                    if (index < 0 || index >= count) throw Error.ArgumentOutOfRange("index");
                    return elements[index];
                }
                set {
                    throw Error.NotSupported();
                }
            }
        }
    }

    // @
    internal class Set<TElement>
    {
        int[] buckets;
        Slot[] slots;
        int count;
        int freeList;
        IEqualityComparer<TElement> comparer;

        public Set() : this(null) { }

        public Set(IEqualityComparer<TElement> comparer) {
            if (comparer == null) comparer = EqualityComparer<TElement>.Default;
            this.comparer = comparer;
            buckets = new int[7];
            slots = new Slot[7];
            freeList = -1;
        }

        // If value is not in set, add it and return true; otherwise return false
        public bool Add(TElement value) {
            return !Find(value, true);
        }

        // Check whether value is in set
        public bool Contains(TElement value) {
            return Find(value, false);
        }

        // If value is in set, remove it and return true; otherwise return false
        public bool Remove(TElement value) {
            int hashCode = InternalGetHashCode(value);
            int bucket = hashCode % buckets.Length;
            int last = -1;
            for (int i = buckets[bucket] - 1; i >= 0; last = i, i = slots[i].next) {
                if (slots[i].hashCode == hashCode && comparer.Equals(slots[i].value, value)) {
                    if (last < 0) {
                        buckets[bucket] = slots[i].next + 1;
                    }
                    else {
                        slots[last].next = slots[i].next;
                    }
                    slots[i].hashCode = -1;
                    slots[i].value = default(TElement);
                    slots[i].next = freeList;
                    freeList = i;
                    return true;
                }
            }
            return false;
        }

        bool Find(TElement value, bool add) {
            int hashCode = InternalGetHashCode(value);
            for (int i = buckets[hashCode % buckets.Length] - 1; i >= 0; i = slots[i].next) {
                if (slots[i].hashCode == hashCode && comparer.Equals(slots[i].value, value)) return true;
            }
            if (add) {
                int index;
                if (freeList >= 0) {
                    index = freeList;
                    freeList = slots[index].next;
                }
                else {
                    if (count == slots.Length) Resize();
                    index = count;
                    count++;
                }
                int bucket = hashCode % buckets.Length;
                slots[index].hashCode = hashCode;
                slots[index].value = value;
                slots[index].next = buckets[bucket] - 1;
                buckets[bucket] = index + 1;
            }
            return false;
        }

        void Resize() {
            int newSize = checked(count * 2 + 1);
            int[] newBuckets = new int[newSize];
            Slot[] newSlots = new Slot[newSize];
            Array.Copy(slots, 0, newSlots, 0, count);
            for (int i = 0; i < count; i++) {
                int bucket = newSlots[i].hashCode % newSize;
                newSlots[i].next = newBuckets[bucket] - 1;
                newBuckets[bucket] = i + 1;
            }
            buckets = newBuckets;
            slots = newSlots;
        }

        internal int InternalGetHashCode(TElement value)
        {
            //[....] DevDivBugs 171937. work around comparer implementations that throw when passed null
            return (value == null) ? 0 : comparer.GetHashCode(value) & 0x7FFFFFFF;
        }

        internal struct Slot
        {
            internal int hashCode;
            internal TElement value;
            internal int next;
        }
    }

    internal class GroupedEnumerable<TSource, TKey, TElement, TResult> : IEnumerable<TResult>{
        IEnumerable<TSource> source;
        FuncV20<TSource, TKey> keySelector;
        FuncV20<TSource, TElement> elementSelector;
        IEqualityComparer<TKey> comparer;
        FuncV20<TKey, IEnumerable<TElement>, TResult> resultSelector;

        public GroupedEnumerable(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TElement> elementSelector, FuncV20<TKey, IEnumerable<TElement>, TResult> resultSelector, IEqualityComparer<TKey> comparer){
            if (source == null) throw Error.ArgumentNull("source");
            if (keySelector == null) throw Error.ArgumentNull("keySelector");
            if (elementSelector == null) throw Error.ArgumentNull("elementSelector");
            if (resultSelector == null) throw Error.ArgumentNull("resultSelector");
            this.source = source;
            this.keySelector = keySelector;
            this.elementSelector = elementSelector;
            this.comparer = comparer;
            this.resultSelector = resultSelector;
        }

        public IEnumerator<TResult> GetEnumerator(){
            Lookup<TKey, TElement> lookup = Lookup<TKey, TElement>.Create<TSource>(source, keySelector, elementSelector, comparer);
            return lookup.ApplyResultSelector(resultSelector).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator(){
            return GetEnumerator();
        }
    }

    internal class GroupedEnumerable<TSource, TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>
    {
        IEnumerable<TSource> source;
        FuncV20<TSource, TKey> keySelector;
        FuncV20<TSource, TElement> elementSelector;
        IEqualityComparer<TKey> comparer;

        public GroupedEnumerable(IEnumerable<TSource> source, FuncV20<TSource, TKey> keySelector, FuncV20<TSource, TElement> elementSelector, IEqualityComparer<TKey> comparer) {
            if (source == null) throw Error.ArgumentNull("source");
            if (keySelector == null) throw Error.ArgumentNull("keySelector");
            if (elementSelector == null) throw Error.ArgumentNull("elementSelector");
            this.source = source;
            this.keySelector = keySelector;
            this.elementSelector = elementSelector;
            this.comparer = comparer;
        }

        public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator() {
            return Lookup<TKey, TElement>.Create<TSource>(source, keySelector, elementSelector, comparer).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

    internal abstract class OrderedEnumerable<TElement> : IOrderedEnumerable<TElement>
    {
        internal IEnumerable<TElement> source;

        public IEnumerator<TElement> GetEnumerator() {
            Buffer<TElement> buffer = new Buffer<TElement>(source);
            if (buffer.count > 0) {
                EnumerableSorter<TElement> sorter = GetEnumerableSorter(null);
                int[] map = sorter.Sort(buffer.items, buffer.count);
                sorter = null;
                for (int i = 0; i < buffer.count; i++) yield return buffer.items[map[i]];
            }
        }

        internal abstract EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement> next);

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        IOrderedEnumerable<TElement> IOrderedEnumerable<TElement>.CreateOrderedEnumerable<TKey>(FuncV20<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending) {
            OrderedEnumerable<TElement, TKey> result = new OrderedEnumerable<TElement, TKey>(source, keySelector, comparer, descending);
            result.parent = this;
            return result;
        }
    }

    internal class OrderedEnumerable<TElement, TKey> : OrderedEnumerable<TElement>
    {
        internal OrderedEnumerable<TElement> parent;
        internal FuncV20<TElement, TKey> keySelector;
        internal IComparer<TKey> comparer;
        internal bool descending;

        internal OrderedEnumerable(IEnumerable<TElement> source, FuncV20<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending) {
            if (source == null) throw Error.ArgumentNull("source");
            if (keySelector == null) throw Error.ArgumentNull("keySelector");
            this.source = source;
            this.parent = null;
            this.keySelector = keySelector;
            this.comparer = comparer != null ? comparer : Comparer<TKey>.Default;
            this.descending = descending;
        }

        internal override EnumerableSorter<TElement> GetEnumerableSorter(EnumerableSorter<TElement> next) {
            EnumerableSorter<TElement> sorter = new EnumerableSorter<TElement, TKey>(keySelector, comparer, descending, next);
            if (parent != null) sorter = parent.GetEnumerableSorter(sorter);
            return sorter;
        }
    }

    internal abstract class EnumerableSorter<TElement>
    {
        internal abstract void ComputeKeys(TElement[] elements, int count);

        internal abstract int CompareKeys(int index1, int index2);

        internal int[] Sort(TElement[] elements, int count) {
            ComputeKeys(elements, count);
            int[] map = new int[count];
            for (int i = 0; i < count; i++) map[i] = i;
            QuickSort(map, 0, count - 1);
            return map;
        }

        void QuickSort(int[] map, int left, int right) {
            do {
                int i = left;
                int j = right;
                int x = map[i + ((j - i) >> 1)];
                do {
                    while (i < map.Length && CompareKeys(x, map[i]) > 0) i++;
                    while (j >= 0 && CompareKeys(x, map[j]) < 0) j--;
                    if (i > j) break;
                    if (i < j) {
                        int temp = map[i];
                        map[i] = map[j];
                        map[j] = temp;
                    }
                    i++;
                    j--;
                } while (i <= j);
                if (j - left <= right - i) {
                    if (left < j) QuickSort(map, left, j);
                    left = i;
                }
                else {
                    if (i < right) QuickSort(map, i, right);
                    right = j;
                }
            } while (left < right);
        }
    }

    internal class EnumerableSorter<TElement, TKey> : EnumerableSorter<TElement>
    {
        internal FuncV20<TElement, TKey> keySelector;
        internal IComparer<TKey> comparer;
        internal bool descending;
        internal EnumerableSorter<TElement> next;
        internal TKey[] keys;

        internal EnumerableSorter(FuncV20<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending, EnumerableSorter<TElement> next) {
            this.keySelector = keySelector;
            this.comparer = comparer;
            this.descending = descending;
            this.next = next;
        }

        internal override void ComputeKeys(TElement[] elements, int count) {
            keys = new TKey[count];
            for (int i = 0; i < count; i++) keys[i] = keySelector(elements[i]);
            if (next != null) next.ComputeKeys(elements, count);
        }

        internal override int CompareKeys(int index1, int index2) {
            int c = comparer.Compare(keys[index1], keys[index2]);
            if (c == 0) {
                if (next == null) return index1 - index2;
                return next.CompareKeys(index1, index2);
            }
            return descending ? -c : c;
        }
    }

    struct Buffer<TElement>
    {
        internal TElement[] items;
        internal int count;

        internal Buffer(IEnumerable<TElement> source) {
            TElement[] items = null;
            int count = 0;
            ICollection<TElement> collection = source as ICollection<TElement>;
            if (collection != null) {
                count = collection.Count;
                if (count > 0) {
                    items = new TElement[count];
                    collection.CopyTo(items, 0);
                }
            }
            else {
                foreach (TElement item in source) {
                    if (items == null) {
                        items = new TElement[4];
                    }
                    else if (items.Length == count) {
                        TElement[] newItems = new TElement[checked(count * 2)];
                        Array.Copy(items, 0, newItems, 0, count);
                        items = newItems;
                    }
                    items[count] = item;
                    count++;
                }
            }
            this.items = items;
            this.count = count;
        }

        internal TElement[] ToArray() {
            if (count == 0) return new TElement[0];
            if (items.Length == count) return items;
            TElement[] result = new TElement[count];
            Array.Copy(items, 0, result, 0, count);
            return result;
        }
    }

    /// <summary>
    /// This class provides the items view for the Enumerable
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class SystemCore_EnumerableDebugView<T>
    {
        public SystemCore_EnumerableDebugView(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            this.enumerable = enumerable;
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                List<T> tempList = new List<T>();
                IEnumerator<T> currentEnumerator = this.enumerable.GetEnumerator();

                if (currentEnumerator != null)
                {
                    for(count = 0; currentEnumerator.MoveNext(); count++)
                    {
                        tempList.Add(currentEnumerator.Current);
                    }
                }
                if (count == 0)
                {
                    throw new SystemCore_EnumerableDebugViewEmptyException();
                }
                cachedCollection = new T[this.count];
                tempList.CopyTo(cachedCollection, 0);
                return cachedCollection;
            }
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private IEnumerable<T> enumerable;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private T[] cachedCollection;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int count;
    }

    internal sealed class SystemCore_EnumerableDebugViewEmptyException : Exception
    {
        public string Empty
        {
            get
            {
                return Strings.EmptyEnumerable;
            }
        }
    }

    internal class Strings
    {
        public const string EmptyEnumerable = "";
    }

    internal sealed class SystemCore_EnumerableDebugView
    {
        public SystemCore_EnumerableDebugView(IEnumerable enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            this.enumerable = enumerable;
            count = 0;
            cachedCollection = null;
        }

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.RootHidden)]
        public object[] Items
        {
            get
            {
                List<object> tempList = new List<object>();
                IEnumerator currentEnumerator = this.enumerable.GetEnumerator();

                if (currentEnumerator != null)
                {
                    for (count = 0; currentEnumerator.MoveNext(); count++)
                    {
                        tempList.Add(currentEnumerator.Current);
                    }
                }
                if (count == 0)
                {
                    throw new SystemCore_EnumerableDebugViewEmptyException();
                }
                cachedCollection = new object[this.count];
                tempList.CopyTo(cachedCollection, 0);
                return cachedCollection;
            }
        }
        
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private IEnumerable enumerable;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private object[] cachedCollection;

        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private int count;
    }
}
