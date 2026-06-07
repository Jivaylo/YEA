using UnityEngine;

public static class GameSessionState
{
    // One-shot flag: set true when Play is pressed in the main menu, consumed by the
    // game-entry popup so it only appears on that entry (not on minigame returns).
    public static bool justEnteredFromMainMenu = false;

    public static bool skeletonDone = false;
    public static bool memoryDone = false;
    public static bool rhythmDone = false;
    public static bool motionDone = false;

    public static int CompletedCount()
    {
        int count = 0;

        if (skeletonDone) count++;
        if (memoryDone) count++;
        if (rhythmDone) count++;
        if (motionDone) count++;

        return count;
    }

    public static bool FullBrainUnlocked()
    {
        return CompletedCount() >= 4;
    }
}
