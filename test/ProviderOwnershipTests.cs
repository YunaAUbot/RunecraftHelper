namespace RunecraftHelper.Tests;

using Xunit;

public sealed class ProviderOwnershipTests
{
    [Fact]
    public void PriceSourceSettingsBelongOnlyToSharedProvider()
    {
        var names = typeof(RunecraftHelperSettings).GetFields().Select(field => field.Name).ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("League", names);
        Assert.DoesNotContain("LeaguePinned", names);
        Assert.DoesNotContain("UseCustomLeague", names);
        Assert.DoesNotContain("CacheTtlMinutes", names);
        Assert.DoesNotContain("LastSyncUtc", names);
    }
}
