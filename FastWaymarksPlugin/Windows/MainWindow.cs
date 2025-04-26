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
using Dalamud.Interface.Components;

namespace FastWaymarksPlugin.Windows;

public class MainWindow : Window, IDisposable
{
#pragma warning disable IDE1006 // Naming Styles
    private readonly Plugin Plugin;
#pragma warning restore IDE1006 // Naming Styles

    internal ScratchPreset ScratchEditingPreset { get; private set; }

    public WaymarkPreset testWaymarkPreset;

    public MainWindow(Plugin plugin)
        : base("Fast Waymarks##FastWaymarksMain", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(322, 247),
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
            //ImGui.TextUnformatted($"The waymark config bool is {Plugin.Configuration.Order.ToString()}");

            /*
            var orderNames = Enum.GetNames<WaymarkOrder>();
            var tempOrder = (int) Plugin.Configuration.Order;
            if (ImGui.Combo("Waymarks Order", ref tempOrder, orderNames, orderNames.Length)) 
            {
                Plugin.Configuration.Order = (WaymarkOrder) tempOrder;
                Plugin.Configuration.Save();
            }
            */

            var orderNames = Enum.GetNames<WaymarkOrder>();
            var tempOrder = (int) Plugin.Configuration.Order;
            if (ImGui.SliderInt("Order", ref tempOrder, 0, orderNames.Length-1, $"{orderNames[tempOrder]}")) 
            {
                Plugin.Configuration.Order = (WaymarkOrder) tempOrder;
                Plugin.Configuration.Save();
            }

            /*
            var shapeNames = Enum.GetNames<WaymarkShape>();
            var tempShape = (int) Plugin.Configuration.Shape;
            if (ImGui.Combo("Waymarks Shape", ref tempShape, shapeNames, shapeNames.Length)) 
            {
                Plugin.Configuration.Shape = (WaymarkShape) tempShape;
                Plugin.Configuration.Save();
            }
            */

            var shapeNames = Enum.GetNames<WaymarkShape>();
            var tempShape = (int) Plugin.Configuration.Shape;
            if (ImGui.SliderInt("Shape", ref tempShape, 0, shapeNames.Length-1, $"{shapeNames[tempShape]}")) 
            {
                Plugin.Configuration.Shape = (WaymarkShape) tempShape;
                Plugin.Configuration.Save();
            }

            var tempWaymarksCenter = new Vector2(Plugin.Configuration.WaymarksCenterX,Plugin.Configuration.WaymarksCenterZ);
            if (ImGui.DragFloat2("Center", ref tempWaymarksCenter, 0.01f, -1000, 1000))
            {
                Plugin.Configuration.WaymarksCenterX = tempWaymarksCenter.X;
                Plugin.Configuration.WaymarksCenterZ = tempWaymarksCenter.Y;
                Plugin.Configuration.Save();
            }

            if (ImGui.Button("Center on Player"))
            {
                var actor = Plugin.ClientState.LocalPlayer;
                if (actor != null) 
                {
                    Plugin.Configuration.WaymarksCenterX = actor.Position.X;
                    Plugin.Configuration.WaymarksCenterZ = actor.Position.Z;
                    Plugin.Configuration.Save();
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("Center to Arena"))
            {
                var actor = Plugin.ClientState.LocalPlayer;
                if (actor != null) 
                {
                    if (actor.Position.X < 50.0f && actor.Position.Y < 50.0f)
                    {
                        Plugin.Configuration.WaymarksCenterX = 0.0f;
                        Plugin.Configuration.WaymarksCenterZ = 0.0f;
                    } else 
                    {
                        Plugin.Configuration.WaymarksCenterX = 100.0f;
                        Plugin.Configuration.WaymarksCenterZ = 100.0f;
                    }
                    Plugin.Configuration.Save();
                }
            }

            ImGui.SameLine();

            ImGuiComponents.HelpMarker("May not work for irregularly-shaped arenas", Dalamud.Interface.FontAwesomeIcon.QuestionCircle);

            ImGui.NewLine();

            if (Plugin.Configuration.Shape == WaymarkShape.Star)
            {
                var tempWaymarksRadii = new Vector2(Plugin.Configuration.WaymarksRadius, Plugin.Configuration.WaymarksRadiusB);
                if (ImGui.DragFloat2("Radii", ref tempWaymarksRadii, 0.01f, 0, 50))
                {
                    Plugin.Configuration.WaymarksRadius = tempWaymarksRadii.X;
                    Plugin.Configuration.WaymarksRadiusB = tempWaymarksRadii.Y;
                    Plugin.Configuration.Save();
                }
            } else 
            {
                var tempWaymarksRadius = Plugin.Configuration.WaymarksRadius;
                if (ImGui.DragFloat("Radius", ref tempWaymarksRadius, 0.01f, 0, 50))
                {
                    Plugin.Configuration.WaymarksRadius = tempWaymarksRadius;
                    Plugin.Configuration.Save();
                }
            }

            var tempWaymarksRotationOffset = Plugin.Configuration.WaymarksRotationOffset;
            if (ImGui.SliderFloat("Rotation", ref tempWaymarksRotationOffset, 0f, 360f, $"{tempWaymarksRotationOffset,8:##0.0}"))
            {
                Plugin.Configuration.WaymarksRotationOffset = tempWaymarksRotationOffset;
                Plugin.Configuration.Save();
            }

            /*
            if (ImGui.Button("Show Settings"))
            {
                Plugin.ToggleConfigUI();
            }
            */
            if (!MemoryHandler.IsSafeToDirectPlacePreset()) ImGui.BeginDisabled();
            
            if (ImGui.Button("Place Waymarks"))
            {
                preparePreset();
                testWaymarkPreset = ScratchEditingPreset.GetPreset((int)Plugin.Configuration.Order);
                MemoryHandler.PlacePreset(testWaymarkPreset.GetAsGamePreset());
            }
            ImGui.EndDisabled();

            ImGui.SameLine();

            ImGuiComponents.HelpMarker("Fast Waymarks are only placeable within instanced zones", Dalamud.Interface.FontAwesomeIcon.QuestionCircle);

            /*
            if (ImGui.Button("Save Waymarks"))
            {
                FieldMarkerPreset currentWaymarks = new();
                if (MemoryHandler.GetCurrentWaymarksAsPresetData(ref currentWaymarks))
                    ScratchEditingPreset = new ScratchPreset(WaymarkPreset.Parse(currentWaymarks));
            }
            */

            ImGui.Spacing();
        } else {
            ImGui.TextUnformatted($"The waymarks failed to compile");
            if (ImGui.Button("Reload Waymarks"))
            {
                FieldMarkerPreset currentWaymarks = new();
                if (MemoryHandler.GetCurrentWaymarksAsPresetData(ref currentWaymarks))
                    ScratchEditingPreset = new ScratchPreset(WaymarkPreset.Parse(currentWaymarks));
            }
        }
    }

    private void preparePreset() 
    {
        Vector3 playerPos;
        if (Plugin.ClientState.LocalPlayer != null)
        {
            playerPos = Plugin.ClientState.LocalPlayer.Position;
        } else
        {
            playerPos = new Vector3(0f,0f,0f);
        }
        for (int i = 0; i < 8; i++)
        {
            var tempCoord = new Vector3(calculateX(i), playerPos.Y, calculateZ(i));
            ScratchEditingPreset.SetWaymark(i,true,tempCoord);
        }
        /*
        foreach (var waymark in ScratchEditingPreset.Waymarks)
        {
            
            waymark.Active = true;

            waymark.X = calculateX(waymark.ID);
            waymark.Y = playerPos.Y;
            waymark.Z = calculateZ(waymark.ID);
        }
        */
    }

    private float calculateX(int idx)
    {
        var tempX = 0f;
        var startX = Plugin.Configuration.WaymarksCenterX;
        var rotationOffset = (float)((2*Math.PI*Plugin.Configuration.WaymarksRotationOffset/360f)-Math.PI/2);
        var rotation = (float)Math.Cos((2*Math.PI*(idx/8f)) + rotationOffset);
        var radius = Plugin.Configuration.WaymarksRadius;
        var radiusB = Plugin.Configuration.WaymarksRadiusB;
        var squareCornerRadius = (float)(radius/Math.Cos(Math.PI/4));
        switch(Plugin.Configuration.Shape)
        {
            case WaymarkShape.Circle:
                //Circle
                tempX = startX + (rotation * radius);
            break;
            case WaymarkShape.Square:
                //Square
                if (idx%2 == 0)
                {
                    tempX = startX + (rotation * radius);
                } else
                {
                    tempX = startX + (rotation * squareCornerRadius);
                }
            break;
            case WaymarkShape.Diamond:
                //Diamond
                if (idx%2 == 0)
                {
                    tempX = startX + (rotation * squareCornerRadius);
                    
                } else
                {
                    tempX = startX + (rotation * radius);
                }
            break;
            case WaymarkShape.Star:
                //Star
                if (idx%2 == 0)
                {
                    tempX = startX + (rotation * radius);
                } else
                {
                    tempX = startX + (rotation * radiusB);
                }
            break;
        }
        Plugin.Log.Debug($"Data of Waymark {idx}:\r\n" +
                                $"startX: {startX}\r\n" +
                                $"rotation: {rotation}\r\n" +
                                $"rotationOffset: {rotationOffset}\r\n" +
                                $"radius: {radius}\r\n" +
                                $"combined: {tempX}");
        
        return tempX;
    }

    private float calculateZ(int idx)
    {
        var tempZ = 0f;
        var startZ = Plugin.Configuration.WaymarksCenterZ;
        var rotationOffset = (float)((2*Math.PI*Plugin.Configuration.WaymarksRotationOffset/360f)-Math.PI/2);
        var rotation = (float)Math.Sin((2*Math.PI*(idx/8f)) + rotationOffset);
        var radius = Plugin.Configuration.WaymarksRadius;
        var radiusB = Plugin.Configuration.WaymarksRadiusB;
        var squareCornerRadius = (float)(radius/Math.Sin(Math.PI/4));
        switch(Plugin.Configuration.Shape)
        {
            case WaymarkShape.Circle:
                //Circle
                tempZ = startZ + (rotation * radius);
            break;
            case WaymarkShape.Square:
                //Square
                if (idx%2 == 0)
                {
                    tempZ = startZ + (rotation * radius);
                } else
                {
                    tempZ = startZ + (rotation * squareCornerRadius);
                }
            break;
            case WaymarkShape.Diamond:
                //Diamond
                if (idx%2 == 0)
                {
                    tempZ = startZ + (rotation * squareCornerRadius);
                } else
                {
                    tempZ = startZ + (rotation * radius);
                }
            break;
            case WaymarkShape.Star:
                //Star
                if (idx%2 == 0)
                {
                    tempZ = startZ + (rotation * radius);
                } else
                {
                    tempZ = startZ + (rotation * radiusB);
                }
            break;
        }
        Plugin.Log.Debug($"Data of Waymark {idx}:\r\n" +
                                $"startZ: {startZ}\r\n" +
                                $"rotation: {rotation}\r\n" +
                                $"rotationOffset: {rotationOffset}\r\n" +
                                $"radius: {radius}\r\n" +
                                $"squareCornerRadius: {squareCornerRadius}\r\n" +
                                $"combined: {tempZ}");
        
        return tempZ;
    }
}
