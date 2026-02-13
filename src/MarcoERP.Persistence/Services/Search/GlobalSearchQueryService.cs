using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.DTOs.Search;
using MarcoERP.Application.Interfaces.Search;

namespace MarcoERP.Persistence.Services.Search
{
    public sealed class GlobalSearchQueryService : IGlobalSearchQueryService
    {
        private const int DefaultTake = 6;

        private readonly MarcoDbContext _db;

        public GlobalSearchQueryService(MarcoDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IReadOnlyList<GlobalSearchHitDto>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            query = query?.Trim();
            if (string.IsNullOrWhiteSpace(query))
                return Array.Empty<GlobalSearchHitDto>();

            var like = $"%{query}%";

            var results = new List<GlobalSearchHitDto>(DefaultTake * 7);

            await AddRangeAsync(results, CompiledQueries.SearchCustomers(_db, like, DefaultTake), cancellationToken);
            await AddRangeAsync(results, CompiledQueries.SearchProducts(_db, like, DefaultTake), cancellationToken);
            await AddRangeAsync(results, CompiledQueries.SearchSalesInvoices(_db, like, DefaultTake), cancellationToken);
            await AddRangeAsync(results, CompiledQueries.SearchPurchaseInvoices(_db, like, DefaultTake), cancellationToken);
            await AddRangeAsync(results, CompiledQueries.SearchJournalEntries(_db, like, DefaultTake), cancellationToken);
            await AddRangeAsync(results, CompiledQueries.SearchCashReceipts(_db, like, DefaultTake), cancellationToken);
            await AddRangeAsync(results, CompiledQueries.SearchCashPayments(_db, like, DefaultTake), cancellationToken);
            await AddRangeAsync(results, CompiledQueries.SearchSuppliers(_db, like, DefaultTake), cancellationToken);

            return results;
        }

        private static async Task AddRangeAsync(List<GlobalSearchHitDto> target, IAsyncEnumerable<GlobalSearchHitDto> source, CancellationToken ct)
        {
            if (source == null)
                return;

            await using var enumerator = source.GetAsyncEnumerator(ct);
            while (await enumerator.MoveNextAsync())
            {
                if (enumerator.Current != null)
                    target.Add(enumerator.Current);
            }
        }
    }
}
