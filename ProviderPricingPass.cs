namespace RunecraftHelper;

using System;
using GameHelper.Plugin.Price;

internal sealed class ProviderPricingPass
{
    private readonly IPriceProvider? provider;

    private ProviderPricingPass(IPriceProvider? provider) => this.provider = provider;

    public static ProviderPricingPass Capture(Func<IPriceProvider?> getProvider)
    {
        ArgumentNullException.ThrowIfNull(getProvider);
        try
        {
            return new ProviderPricingPass(getProvider());
        }
        catch
        {
            return new ProviderPricingPass(null);
        }
    }

    public bool TryGetExaltedPrice(string itemName, out double exaltedPrice) =>
        this.TryGetExaltedPrice(itemName, string.Empty, string.Empty, out exaltedPrice);

    public bool TryGetPriceByArtId(string internalPathBasename, out double exaltedPrice) =>
        this.TryGetExaltedPrice(string.Empty, internalPathBasename, string.Empty, out exaltedPrice);

    public bool TryGetNameByArtId(string internalPathBasename, out string displayName) =>
        this.TryResolveDisplayName(internalPathBasename, out displayName);

    public bool TryGetExaltedPrice(
        string itemName,
        string internalPathBasename,
        string fullItemPath,
        out double exaltedPrice)
    {
        exaltedPrice = 0;
        if (this.provider is null ||
            (string.IsNullOrWhiteSpace(itemName) && string.IsNullOrWhiteSpace(internalPathBasename)))
        {
            return false;
        }

        try
        {
            var query = new PriceQuery(
                itemName ?? string.Empty,
                Array.Empty<string>(),
                internalPathBasename ?? string.Empty,
                fullItemPath ?? string.Empty,
                itemName ?? string.Empty);
            if (!this.provider.TryGetPrice(query, out var quote) ||
                quote is null || quote.ExaltedValue <= 0m || string.IsNullOrWhiteSpace(quote.SourceName))
            {
                return false;
            }

            exaltedPrice = (double)quote.ExaltedValue;
            return double.IsFinite(exaltedPrice) && exaltedPrice > 0;
        }
        catch
        {
            exaltedPrice = 0;
            return false;
        }
    }

    public bool TryResolveDisplayName(string internalPathBasename, out string displayName)
    {
        displayName = string.Empty;
        if (this.provider is null || string.IsNullOrWhiteSpace(internalPathBasename)) return false;
        try
        {
            var query = new PriceQuery(
                string.Empty,
                Array.Empty<string>(),
                internalPathBasename,
                string.Empty,
                string.Empty);
            return this.provider.TryResolveDisplayName(query, out displayName) &&
                   !string.IsNullOrWhiteSpace(displayName);
        }
        catch
        {
            displayName = string.Empty;
            return false;
        }
    }

    public bool TryGetStatus(out PriceProviderStatus status)
    {
        status = null!;
        if (this.provider is null) return false;
        try
        {
            status = this.provider.Status;
            return status is not null;
        }
        catch
        {
            status = null!;
            return false;
        }
    }

    public void RequestRefresh()
    {
        try
        {
            this.provider?.RequestRefresh();
        }
        catch
        {
        }
    }
}
