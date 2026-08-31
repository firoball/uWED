/// <summary>
/// Converts between the lock-angle popup's index values and the actual
/// angle in degrees. Keeps both directions of the mapping in one place
/// so the popup's option list and the degree values stay in sync.
/// </summary>
public static class LockAngleUtility
{
    /// <summary>Converts a popup index to its angle in degrees.</summary>
    public static float IndexToDegrees(int index)
    {
        if (index == 1 || index == 2) //1, 2
            return index;
        if (index == 3 || index == 4) //5, 10
            return (index - 2) * 5;
        if (index == 11) //120
            return 120f;
        return (index - 4) * 15; //15, 30, 45, 60, 75, 90
    }

    /// <summary>Converts an angle in degrees back to its popup index.</summary>
    public static int DegreesToIndex(float degrees)
    {
        if (degrees >= 120f) //120
            return 11;
        if (degrees >= 15f) //15, 30, 45, 60, 75, 90
            return (int)(degrees / 15) + 4;
        if (degrees >= 5f) //5, 10
            return (int)(degrees / 5) + 2;
        return (int)degrees; //1, 2
    }
}
