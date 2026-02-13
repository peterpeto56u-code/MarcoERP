using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.Reporting.Interfaces;
using MarcoERP.Application.Reporting.Models;

namespace MarcoERP.WpfUI.Reporting
{
    /// <summary>
    /// A virtualizing collection that loads report pages on demand.
    /// Implements <see cref="IList"/> for WPF DataGrid binding, fetching pages lazily.
    /// Uses a page cache to avoid redundant DB queries.
    /// </summary>
    public sealed class VirtualizingReportCollection<TRow> : IList, INotifyCollectionChanged, INotifyPropertyChanged
        where TRow : ReportRowBase
    {
        private readonly IPagedReportQuery<TRow> _query;
        private readonly Dictionary<int, TRow[]> _pageCache = new();
        private readonly SemaphoreSlim _loadLock = new(1, 1);

        private IReadOnlyList<ActiveFilter> _filters = Array.Empty<ActiveFilter>();
        private SortDefinition _sort;
        private int _pageSize = 50;
        private int _totalCount;

        /// <summary>Raised when any page is loaded.</summary>
        public event Action<int> PageLoaded;

        public VirtualizingReportCollection(IPagedReportQuery<TRow> query)
        {
            _query = query ?? throw new ArgumentNullException(nameof(query));
        }

        // ── Configuration ──

        /// <summary>Updates query parameters and invalidates the cache.</summary>
        public void Configure(IReadOnlyList<ActiveFilter> filters, SortDefinition sort, int pageSize)
        {
            _filters = filters ?? Array.Empty<ActiveFilter>();
            _sort = sort;
            _pageSize = Math.Max(1, pageSize);
            Invalidate();
        }

        /// <summary>Sets the known total count (from initial query).</summary>
        public void SetTotalCount(int totalCount)
        {
            _totalCount = totalCount;
            OnPropertyChanged(nameof(Count));
            OnCollectionReset();
        }

        /// <summary>Clears the page cache, forcing reload on next access.</summary>
        public void Invalidate()
        {
            _pageCache.Clear();
            OnCollectionReset();
        }

        // ── IList Implementation ──

        public int Count => _totalCount;
        public bool IsReadOnly => true;
        public bool IsFixedSize => false;
        public bool IsSynchronized => false;
        public object SyncRoot { get; } = new();

        public object this[int index]
        {
            get
            {
                if (index < 0 || index >= _totalCount)
                    return null;

                var pageIndex = index / _pageSize;
                var offset = index % _pageSize;

                if (_pageCache.TryGetValue(pageIndex, out var page) && offset < page.Length)
                    return page[offset];

                // Trigger async load; return placeholder
                _ = LoadPageAsync(pageIndex);
                return null;
            }
            set => throw new NotSupportedException("Read-only collection.");
        }

        /// <summary>
        /// Synchronously loads a specific page and returns the items.
        /// Prefer <see cref="LoadPageAsync"/> for non-blocking access.
        /// </summary>
        public async Task<TRow[]> LoadPageAsync(int pageIndex, CancellationToken ct = default)
        {
            if (_pageCache.TryGetValue(pageIndex, out var cached))
                return cached;

            await _loadLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                // Double-check after acquiring lock
                if (_pageCache.TryGetValue(pageIndex, out cached))
                    return cached;

                var result = await _query.GetPageAsync(_filters, _sort, pageIndex, _pageSize, ct)
                    .ConfigureAwait(false);

                var items = new TRow[result.Items.Count];
                for (var i = 0; i < result.Items.Count; i++)
                    items[i] = result.Items[i];

                _pageCache[pageIndex] = items;
                _totalCount = result.TotalCount;

                PageLoaded?.Invoke(pageIndex);
                OnPropertyChanged(nameof(Count));
                OnCollectionReset();

                return items;
            }
            finally
            {
                _loadLock.Release();
            }
        }

        // ── IList methods (subset for WPF binding) ──

        public int Add(object value) => throw new NotSupportedException();
        public void Clear() { _pageCache.Clear(); _totalCount = 0; OnCollectionReset(); }
        public bool Contains(object value) => false;
        public int IndexOf(object value) => -1;
        public void Insert(int index, object value) => throw new NotSupportedException();
        public void Remove(object value) => throw new NotSupportedException();
        public void RemoveAt(int index) => throw new NotSupportedException();
        public void CopyTo(Array array, int index) { }
        public IEnumerator GetEnumerator() => new VirtualEnumerator(this);

        // ── INotifyCollectionChanged ──

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionReset()
            => CollectionChanged?.Invoke(this,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

        // ── INotifyPropertyChanged ──

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        // ── Enumerator ──

        private sealed class VirtualEnumerator : IEnumerator
        {
            private readonly VirtualizingReportCollection<TRow> _collection;
            private int _index = -1;

            public VirtualEnumerator(VirtualizingReportCollection<TRow> collection)
                => _collection = collection;

            public object Current => _index >= 0 && _index < _collection.Count
                ? _collection[_index] : null;

            public bool MoveNext() => ++_index < _collection.Count;
            public void Reset() => _index = -1;
        }
    }
}
