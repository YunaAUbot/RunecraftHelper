namespace GameHelper.Plugin.Price;

public sealed record PriceQuote(decimal ChaosValue, decimal DivineValue, decimal ExaltedValue, string SourceName);
public sealed record PriceProviderStatus(string ProviderName, string Source, string League, bool IsFetching, int ItemCount, DateTimeOffset LastFetchUtc);

public sealed class PriceQuery
{
    public PriceQuery(string itemName, IEnumerable<string> explicitMods, string internalPathBasename, string fullItemPath, string scoutText)
    {
        ItemName = itemName;
        ExplicitMods = explicitMods.ToArray();
        InternalPathBasename = internalPathBasename;
        FullItemPath = fullItemPath;
        ScoutText = scoutText;
    }

    public string ItemName { get; }
    public IReadOnlyList<string> ExplicitMods { get; }
    public string InternalPathBasename { get; }
    public string FullItemPath { get; }
    public string ScoutText { get; }
}

public interface IPriceProvider
{
    PriceProviderStatus Status { get; }
    bool TryGetPrice(PriceQuery query, out PriceQuote quote);
    bool TryResolveDisplayName(PriceQuery query, out string displayName);
    bool IsGenericLookupName(string itemName);
    bool HasPriceDataForName(string itemName);
    void RequestRefresh();
}
