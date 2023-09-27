using System;
using System.Collections;
using System.Collections.Generic;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents a wrapper of a collection of one type of object with another.
    /// </summary>
    /// <typeparam name="T">The type of the collection being wrapped.</typeparam>
    /// <typeparam name="TWrapped">The type to wrap the items with.</typeparam>
    internal record struct CollectionWrapper<T, TWrapped> : IReadOnlyCollection<T>, IEnumerator<T>
    {
        /// <summary>
        /// A <see cref="Func{T, TResult}" /> which wraps a single item in the collection.
        /// </summary>
        private readonly Func<TWrapped, T> _createWrapperFunc;

        /// <summary>
        /// A <see cref="IEnumerable{T}" /> used to enumerate the wrapped colleciton.
        /// </summary>
        private readonly IEnumerator<TWrapped> _enumerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionWrapper{T, TWrapped}" /> class.
        /// </summary>
        /// <param name="collection">An <see cref="ICollection{T}" /> containing objects to wrap.</param>
        /// <param name="createWrapperFunc">A <see cref="Func{T, TResult}" /> which can wrap a single item in the collection.</param>
        public CollectionWrapper(ICollection<TWrapped> collection, Func<TWrapped, T> createWrapperFunc)
            : this(collection?.GetEnumerator(), collection?.Count, createWrapperFunc)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionWrapper{T, TWrapped}" /> class.
        /// </summary>
        /// <param name="collection">An <see cref="IReadOnlyCollection{T}" /> containing objects to wrap.</param>
        /// <param name="createWrapperFunc">A <see cref="Func{T, TResult}" /> which can wrap a single item in the collection.</param>
        public CollectionWrapper(IReadOnlyCollection<TWrapped> collection, Func<TWrapped, T> createWrapperFunc)
            : this(collection?.GetEnumerator(), collection?.Count, createWrapperFunc)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionWrapper{T, TWrapped}" /> class.
        /// </summary>
        /// <param name="collection">An <see cref="IEnumerable{T}" /> containing objects to wrap.</param>
        /// <param name="count">The number of items in the collection being wrapped.</param>
        /// <param name="createWrapperFunc">A <see cref="Func{T, TResult}" /> which can wrap a single item in the collection.</param>
        public CollectionWrapper(IEnumerable<TWrapped> collection, int count, Func<TWrapped, T> createWrapperFunc)
            : this(collection?.GetEnumerator(), count, createWrapperFunc)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionWrapper{T, TWrapped}" /> class.
        /// </summary>
        /// <param name="enumerator">An <see cref="IEnumerator{T}" /> containing objects to wrap.</param>
        /// <param name="count">The number of items in the collection being wrapped.</param>
        /// <param name="createWrapperFunc">A <see cref="Func{T, TResult}" /> which can wrap a single item in the collection.</param>
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