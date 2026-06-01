namespace TuneVault.Application.Features.Search.Queries.SearchMedia;

public sealed record SearchMediaQuery(string Keyword, int Page = 1, int PageSize = 20);
