using System;
using Dalamud.Utility;
using Lumina.Text.ReadOnly;

namespace FastWaymarksPlugin;

public static class Utils
{
    public static string ToStr(ReadOnlySeString content) => content.ToDalamudString().ToString();
    public static float PIOverFour = (float)(Math.PI/4d);
    public static float SquareCornerFactor = (float)(1d/Math.Cos(PIOverFour));
}
