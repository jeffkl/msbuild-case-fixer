using System;
using System.Collections;
using System.Collections.Generic;

namespace MSBuildCaseFixer
{
    internal class CollectionWrapper<T, TWrapped> : IReadOnlyCollection<T>, IEnumerator<T>
    {
        private readonly Func<TWrapped, T> _createWrapperFunc;
        private readonly IEnumerator<TWrapped> _enumerator;

        public CollectionWrapper(ICollection<TWrapped> collection, Func<TWrapped, T> createWrapperFunc)
            : this(collection?.GetEnumerator(), collection?.Count, createWrapperFunc)
        {
        }

        public CollectionWrapper(IReadOnlyCollection<TWrapped> collection, Func<TWrapped, T> createWrapperFunc)
            : this(collection?.GetEnumerator(), collection?.Count, createWrapperFunc)
        {
        }

        public CollectionWrapper(IEnumerable<TWrapped> collection, int count, Func<TWrapped, T> createWrapperFunc)
            : this(collection?.GetEnumerator(), count, createWrapperFunc)
        {
        }

        private CollectionWrapper(IEnumerator<TWrapped>? enumerator, int? count, Func<TWrapped, T> createWrapperFunc)
        {
            _enumerator = enumerator ?? throw new ArgumentNullException(nameof(enumerator));
            Count = count.GetValueOrDefault();
            _createWrapperFunc = createWrapperFunc ?? throw new ArgumentNullException(nameof(createWrapperFunc));
        }

        public int Count { get; }

        public T Current => _createWrapperFunc(_enumerator.Current);

        object IEnumerator.Current => _createWrapperFunc(_enumerator.Current)!;

        public void Dispose()
        {
            _enumerator.Dispose();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        public void Reset()
        {
            _enumerator.Reset();
        }
    }
}