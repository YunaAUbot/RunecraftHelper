namespace RunecraftHelper.Tests;

using Xunit;

public sealed class RunecraftFrameWorkPolicyTests
{
    [Fact]
    public void LockedRecipeHighlightDoesNotRequireLocalizedRewardTableScan()
    {
        Assert.False(RunecraftFrameWorkPolicy.NeedsLocalizedRewardMap(
            showMonolithRewards: false,
            showDebugWindow: false));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void RewardAndDebugWindowsRequireLocalizedRewardNames(bool showRewards, bool showDebug)
    {
        Assert.True(RunecraftFrameWorkPolicy.NeedsLocalizedRewardMap(showRewards, showDebug));
    }
}
