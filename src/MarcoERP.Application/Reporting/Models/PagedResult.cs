using System;
using System.Collections.Generic;

namespace MarcoERP.Application.Reporting.Models
{
    /// <summary>
    /// Server-side paged query result with metadata for virtualization.
    /// </summary>
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int TotalCount { get; }
        public int PageIndex { get; }
        public int PageSize { get; }

        public int TotalPages => PageSize > 0
            ? (int)Math.Ceiling((double)TotalCount / PageSize)
            : 0;

        public bool HasPreviousPage => PageIndex > 0;
        public bool HasNextPage => PageIndex < TotalPages - 1;

        public PagedResult(IReadOnlyList<T> items, int totalCount, int pageIndex, int pageSize)
        {
            Items = items ?? Array.Empty<T>() as IReadOnlyList<T>;
            TotalCount = totalCount;
            PageIndex = pageIndex;
            PageSize = pageSize;
        }

        /// <summary>Creates an empty result.</summary>
        public static PagedResult<T> Empty(int pageSize = 50)
            => new(Array.Empty<T>(), 0, 0, pageSize);
    }
}
