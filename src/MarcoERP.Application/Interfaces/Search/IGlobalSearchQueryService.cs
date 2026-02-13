using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MarcoERP.Application.DTOs.Search;

namespace MarcoERP.Application.Interfaces.Search
{
    public interface IGlobalSearchQueryService
    {
        Task<IReadOnlyList<GlobalSearchHitDto>> SearchAsync(string query, CancellationToken cancellationToken = default);
    }
}
