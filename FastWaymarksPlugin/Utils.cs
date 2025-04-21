using Dalamud.Utility;
using Lumina.Text.ReadOnly;

namespace FastWaymarksPlugin;

public static class Utils
{
    public static string ToStr(ReadOnlySeString content) => content.ToDalamudString().ToString();
}
