namespace RunecraftHelper.Tests;

using GameHelper.Plugin.Price;
using Xunit;

public sealed class ProviderPricingPassTests
{
    [Fact]
    public void CapturesProviderOnceAndBuildsCompleteQuery()
    {
        var provider = new StubProvider { Quote = new PriceQuote(4m, 0.2m, 7.5m, "test") };
        var captures = 0;
        var pricing = ProviderPricingPass.Capture(() => { captures++; return provider; });

        var found = pricing.TryGetExaltedPrice("Perfect Regal Orb", "CurrencyUpgradeMagicToRare3", "Metadata/Items/Currency/CurrencyUpgradeMagicToRare3", out var value);

        Assert.True(found);
        Assert.Equal(7.5, value);
        Assert.Equal(1, captures);
        Assert.NotNull(provider.Query);
        Assert.Equal("Perfect Regal Orb", provider.Query!.ItemName);
        Assert.Equal("CurrencyUpgradeMagicToRare3", provider.Query.InternalPathBasename);
        Assert.Equal("Metadata/Items/Currency/CurrencyUpgradeMagicToRare3", provider.Query.FullItemPath);
        Assert.Equal("Perfect Regal Orb", provider.Query.ScoutText);
    }

    [Fact]
    public void MissingProviderAndInvalidQuoteFailClosed()
    {
        Assert.False(ProviderPricingPass.Capture(() => null).TryGetExaltedPrice("Orb", "", "", out _));
        var provider = new StubProvider { Quote = new PriceQuote(1m, 1m, 0m, "test") };
        Assert.False(ProviderPricingPass.Capture(() => provider).TryGetExaltedPrice("Orb", "", "", out _));
    }

    [Fact]
    public void StatusNameResolutionAndRefreshUseCapturedProvider()
    {
        var provider = new StubProvider
        {
            ResolvedName = "Perfect Regal Orb",
            Status = new PriceProviderStatus("NinjaPricer", "poe.ninja", "Test League", false, 42, DateTimeOffset.UtcNow),
        };
        var pricing = ProviderPricingPass.Capture(() => provider);

        Assert.True(pricing.TryResolveDisplayName("CurrencyUpgradeMagicToRare3", out var name));
        Assert.Equal("Perfect Regal Orb", name);
        Assert.True(pricing.TryGetStatus(out var status));
        Assert.Equal("NinjaPricer", status.ProviderName);
        pricing.RequestRefresh();
        Assert.True(provider.RefreshRequested);
    }

    [Fact]
    public void CompatibilityLookupsUseNameAndInternalId()
    {
        var provider = new StubProvider
        {
            Quote = new PriceQuote(4m, .2m, 3.25m, "test"),
            ResolvedName = "Greater Regal Orb",
        };
        var pricing = ProviderPricingPass.Capture(() => provider);

        Assert.True(pricing.TryGetExaltedPrice("Greater Regal Orb", out var byName));
        Assert.Equal(3.25, byName);
        Assert.Equal("Greater Regal Orb", provider.Query!.ItemName);
        Assert.True(pricing.TryGetPriceByArtId("CurrencyUpgradeMagicToRare2", out var byId));
        Assert.Equal(3.25, byId);
        Assert.Equal("CurrencyUpgradeMagicToRare2", provider.Query!.InternalPathBasename);
        Assert.True(pricing.TryGetNameByArtId("CurrencyUpgradeMagicToRare2", out var name));
        Assert.Equal("Greater Regal Orb", name);
    }

    [Fact]
    public void ProviderExceptionsFailClosed()
    {
        var pricing = ProviderPricingPass.Capture(() => new ThrowingProvider());
        Assert.False(pricing.TryGetExaltedPrice("Orb", "id", "path", out _));
        Assert.False(pricing.TryResolveDisplayName("id", out _));
        Assert.False(pricing.TryGetStatus(out _));
        pricing.RequestRefresh();
    }

    [Fact]
    public void MemoizesWithinPassButRefreshesNextPass()
    {
        var provider = new StubProvider { Quote = new PriceQuote(1m, 1m, 3m, "test") };
        var pass = ProviderPricingPass.Capture(() => provider);
        for (var i = 0; i < 100; i++) Assert.True(pass.TryGetExaltedPrice("Orb", out _));
        Assert.Equal(1, provider.PriceCalls);
        Assert.True(ProviderPricingPass.Capture(() => provider).TryGetExaltedPrice("Orb", out _));
        Assert.Equal(2, provider.PriceCalls);
    }

    private sealed class StubProvider : IPriceProvider
    {
        public int PriceCalls { get; private set; }
        public PriceQuery? Query { get; private set; }
        public PriceQuote? Quote { get; init; }
        public string ResolvedName { get; init; } = string.Empty;
        public bool RefreshRequested { get; private set; }
        public PriceProviderStatus Status { get; init; } = new("test", "test", "test", false, 1, DateTimeOffset.UtcNow);
        public bool TryGetPrice(PriceQuery query, out PriceQuote quote) { PriceCalls++; Query = query; quote = Quote!; return Quote is not null; }
        public bool TryResolveDisplayName(PriceQuery query, out string displayName) { Query = query; displayName = ResolvedName; return ResolvedName.Length > 0; }
        public bool IsGenericLookupName(string itemName) => false;
        public bool HasPriceDataForName(string itemName) => true;
        public void RequestRefresh() => RefreshRequested = true;
    }

    private sealed class ThrowingProvider : IPriceProvider
    {
        public PriceProviderStatus Status => throw new InvalidOperationException();
        public bool TryGetPrice(PriceQuery query, out PriceQuote quote) => throw new InvalidOperationException();
        public bool TryResolveDisplayName(PriceQuery query, out string displayName) => throw new InvalidOperationException();
        public bool IsGenericLookupName(string itemName) => throw new InvalidOperationException();
        public bool HasPriceDataForName(string itemName) => throw new InvalidOperationException();
        public void RequestRefresh() => throw new InvalidOperationException();
    }
}
