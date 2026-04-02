using MediatR;
using QIM.Application.Interfaces;
using QIM.Shared.Models;

namespace QIM.Application.Features.Keywords;

// ── Search Keywords (autocomplete for keyword multi-select) ──
public record SearchKeywordsQuery(string Query, int Limit = 20) : IRequest<Result<List<string>>>;

public class SearchKeywordsHandler : IRequestHandler<SearchKeywordsQuery, Result<List<string>>>
{
    private readonly IUnitOfWork _uow;

    public SearchKeywordsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<List<string>>> Handle(SearchKeywordsQuery request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Length < 1)
            return Result<List<string>>.Success(new List<string>());

        var allMatching = await _uow.BusinessKeywords.GetAllAsync(
            k => k.Keyword.Contains(request.Query));

        var keywords = allMatching
            .Select(k => k.Keyword)
            .Distinct()
            .OrderBy(k => k)
            .Take(request.Limit)
            .ToList();

        return Result<List<string>>.Success(keywords);
    }
}
