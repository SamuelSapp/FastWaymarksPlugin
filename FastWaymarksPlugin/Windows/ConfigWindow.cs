using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace FastWaymarksPlugin.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration Configuration;

    // We give this window a constant ID using ###
    // This allows for labels being dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Fast Waymarks Configuration###FastWaymarksConfig")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 90);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
    }

    public override void Draw()
    {
        var tempAutoCenterOnLoad = Configuration.autoCenterOnLoad;
        if (ImGui.Checkbox("Auto-center when changing zone", ref tempAutoCenterOnLoad))
        {
            Configuration.autoCenterOnLoad = tempAutoCenterOnLoad;
            Configuration.Save();
        }

        /*
        var tempDisplayWaymarkY = Configuration.displayWaymarkY;
        if (ImGui.Checkbox("Edit Waymark Y Placement", ref tempDisplayWaymarkY))
        {
            Configuration.displayWaymarkY = tempDisplayWaymarkY;
            Configuration.Save();
        }
        */
    }
}
