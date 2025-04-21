using System;
using System.Numerics;
using System.Linq;
using System.Collections;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using Lumina.Excel.Sheets;

namespace FastWaymarksPlugin.Windows;

public class MainWindow : Window, IDisposable
{
#pragma warning disable IDE1006 // Naming Styles
    private readonly Plugin Plugin;
#pragma warning restore IDE1006 // Naming Styles

    internal ScratchPreset ScratchEditingPreset { get; private set; }

    public WaymarkPreset testWaymarkPreset;

    public MainWindow(Plugin plugin, string goatImagePath)
        : base("My Amazing Window##With a hidden ID", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void OnOpen() 
    {
        FieldMarkerPreset currentWaymarks = new();
        if (MemoryHandler.GetCurrentWaymarksAsPresetData(ref currentWaymarks))
            ScratchEditingPreset = new ScratchPreset(WaymarkPreset.Parse(currentWaymarks));
    }

    public override void Draw()
    {
        if (ScratchEditingPreset != null)
        {
            ImGui.TextUnformatted($"The waymark config bool is {Plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");

            if (ImGui.Button("Show Settings"))
            {
                Plugin.ToggleConfigUI();
            }

            if (ImGui.Button("Place Waymarks"))
            {
                //makePreset();
                testWaymarkPreset = ScratchEditingPreset.GetPreset();
                MemoryHandler.PlacePreset(testWaymarkPreset.GetAsGamePreset());
            }

            if (ImGui.Button("Save Waymarks"))
            {
                FieldMarkerPreset currentWaymarks = new();
                if (MemoryHandler.GetCurrentWaymarksAsPresetData(ref currentWaymarks))
                    ScratchEditingPreset = new ScratchPreset(WaymarkPreset.Parse(currentWaymarks));
            }

            ImGui.Spacing();
        } else {
            ImGui.TextUnformatted($"The waymark config bool is not working");
            if (ImGui.Button("Save Waymarks"))
            {
                FieldMarkerPreset currentWaymarks = new();
                if (MemoryHandler.GetCurrentWaymarksAsPresetData(ref currentWaymarks))
                    ScratchEditingPreset = new ScratchPreset(WaymarkPreset.Parse(currentWaymarks));
            }
        }
    }

    private void makePreset() 
    {
        foreach (var waymark in ScratchEditingPreset.Waymarks)
        {
            waymark.Active = true;

            waymark.X = 0.000f;
            waymark.Y = 0.000f;
            waymark.Z = 0.000f;
        }
    }
}
