namespace RunecraftHelper;

internal static class RunecraftFrameWorkPolicy
{
    // Building the localized BaseItemTypes map is an expensive remote-memory scan.
    // It is only needed by windows that actually render localized catalog rewards;
    // the panel overlay and locked-row highlight read their identity from live rows.
    public static bool NeedsLocalizedRewardMap(bool showMonolithRewards, bool showDebugWindow) =>
        showMonolithRewards || showDebugWindow;
}
