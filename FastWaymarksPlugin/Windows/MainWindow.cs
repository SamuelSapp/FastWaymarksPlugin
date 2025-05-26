using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using Dalamud.Interface.Components;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;

namespace FastWaymarksPlugin.Windows;

public class MainWindow : Window, IDisposable
{
    private readonly Plugin Plugin;

    internal ScratchPreset ScratchEditingPreset { get; private set; }
    public WaymarkPreset testWaymarkPreset;
    internal bool zoneChanged = false;

    public MainWindow(Plugin plugin)
        : base("Fast Waymarks##FastWaymarksMain", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 247),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Plugin = plugin;
    }

    public void Dispose() { }

    public override void OnOpen()
    {

        FieldMarkerPreset currentWaymarks = new();
        if (MemoryHandler.GetCurrentWaymarksAsPresetData(ref currentWaymarks))
        {
            ScratchEditingPreset = new ScratchPreset(WaymarkPreset.Parse(currentWaymarks));
        }
        preparePreset(false);
    }

    public override void Update()
    {
        if (Plugin.Configuration.autoCenterOnLoad)
        {
            if (zoneChanged)
            {
                var actor = Plugin.ClientState.LocalPlayer;
                if (actor != null)
                {
                    if (actor.Position.X < 50.0f && actor.Position.Z < 50.0f)
                    {
                        Plugin.Configuration.WaymarksCenterX = 0.0f;
                        Plugin.Configuration.WaymarksCenterZ = 0.0f;
                    }
                    else
                    {
                        Plugin.Configuration.WaymarksCenterX = 100.0f;
                        Plugin.Configuration.WaymarksCenterZ = 100.0f;
                    }
                    Plugin.Configuration.WaymarksCenterY = actor.Position.Y;
                    changeSetting();
                    Plugin.Configuration.Save();
                    zoneChanged = false;

                    if (Plugin.MapWindow.IsOpen)
                    {
                        Plugin.ToggleMapUI();
                    }
                }
            }
        }
        else
        {
            if (zoneChanged)
            {
                zoneChanged = false;
            }
        }
    }

    public override void Draw()
    {
        if (ScratchEditingPreset != null)
        {
            if (Plugin.Configuration.hasSettingChanged)
            {
                preparePreset(false);

                Plugin.Configuration.hasSettingChanged = false;
                Plugin.Configuration.Save();
            }

            var orderNames = Enum.GetNames<WaymarkOrder>();
            var tempOrder = (int)Plugin.Configuration.Order;
            if (ImGui.SliderInt("Order", ref tempOrder, 0, orderNames.Length - 1, $"{orderNames[tempOrder]}"))
            {
                Plugin.Configuration.Order = (WaymarkOrder)tempOrder;
                changeSetting();
                Plugin.Configuration.Save();
            }

            var shapeNames = Enum.GetNames<WaymarkShape>();
            var tempShape = (int)Plugin.Configuration.Shape;
            if (ImGui.SliderInt("Shape", ref tempShape, 0, shapeNames.Length - 1, $"{shapeNames[tempShape]}"))
            {
                Plugin.Configuration.Shape = (WaymarkShape)tempShape;
                changeSetting();
                Plugin.Configuration.Save();
            }

            var tempDisplayWaymarkY = Plugin.Configuration.displayWaymarkY;
            if (tempDisplayWaymarkY)
            {
                var tempWaymarksCenter = new Vector3(Plugin.Configuration.WaymarksCenterX, Plugin.Configuration.WaymarksCenterZ, Plugin.Configuration.WaymarksCenterY);
                if (ImGui.DragFloat3("Center", ref tempWaymarksCenter, 0.01f, -1000, 1000))
                {
                    Plugin.Configuration.WaymarksCenterX = tempWaymarksCenter.X;
                    Plugin.Configuration.WaymarksCenterZ = tempWaymarksCenter.Y;
                    Plugin.Configuration.WaymarksCenterY = tempWaymarksCenter.Z;
                    changeSetting();
                    Plugin.Configuration.Save();
                }
            }
            else
            {
                var tempWaymarksCenter = new Vector2(Plugin.Configuration.WaymarksCenterX, Plugin.Configuration.WaymarksCenterZ);
                if (ImGui.DragFloat2("Center", ref tempWaymarksCenter, 0.01f, -1000, 1000))
                {
                    Plugin.Configuration.WaymarksCenterX = tempWaymarksCenter.X;
                    Plugin.Configuration.WaymarksCenterZ = tempWaymarksCenter.Y;
                    changeSetting();
                    Plugin.Configuration.Save();
                }
            }

            if (ImGui.Button("Center on Player"))
            {
                var actor = Plugin.ClientState.LocalPlayer;
                if (actor != null)
                {
                    Plugin.Configuration.WaymarksCenterX = actor.Position.X;
                    Plugin.Configuration.WaymarksCenterZ = actor.Position.Z;
                    Plugin.Configuration.WaymarksCenterY = actor.Position.Y;
                    changeSetting();
                    Plugin.Configuration.Save();
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("Center to Arena"))
            {
                var actor = Plugin.ClientState.LocalPlayer;
                if (actor != null)
                {
                    if (actor.Position.X < 50.0f && actor.Position.Z < 50.0f)
                    {
                        Plugin.Configuration.WaymarksCenterX = 0.0f;
                        Plugin.Configuration.WaymarksCenterZ = 0.0f;
                    }
                    else
                    {
                        Plugin.Configuration.WaymarksCenterX = 100.0f;
                        Plugin.Configuration.WaymarksCenterZ = 100.0f;
                    }
                    Plugin.Configuration.WaymarksCenterY = actor.Position.Y;
                    changeSetting();
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
                    changeSetting();
                    Plugin.Configuration.Save();
                }
            }
            else
            {
                var tempWaymarksRadius = Plugin.Configuration.WaymarksRadius;
                if (ImGui.DragFloat("Radius", ref tempWaymarksRadius, 0.01f, 0, 50))
                {
                    Plugin.Configuration.WaymarksRadius = tempWaymarksRadius;
                    changeSetting();
                    Plugin.Configuration.Save();
                }
            }

            var tempWaymarksRotationOffset = Plugin.Configuration.WaymarksRotationOffset;
            if (ImGui.SliderFloat("Rotation", ref tempWaymarksRotationOffset, 0f, 360f, $"%.1f"))
            {
                Plugin.Configuration.WaymarksRotationOffset = tempWaymarksRotationOffset;
                changeSetting();
                Plugin.Configuration.Save();
            }

            /*
            if (ImGui.Button("Show Settings"))
            {
                Plugin.ToggleConfigUI();
            }
            */
            var isSafeToDirectPlacePreset = MemoryHandler.IsSafeToDirectPlacePreset();
            if (!isSafeToDirectPlacePreset) ImGui.BeginDisabled();

            if (ImGui.Button("Map Preview"))
            {
                preparePreset(false);
                Plugin.ToggleMapUI();
            }

            ImGui.SameLine();

            if (ImGui.Button("Place Waymarks"))
            {
                preparePreset(true);
                testWaymarkPreset = ScratchEditingPreset.GetPreset();
                MemoryHandler.PlacePreset(testWaymarkPreset.GetAsGamePreset());
                preparePreset(false);
            }
            if (!isSafeToDirectPlacePreset) ImGui.EndDisabled();

            ImGui.SameLine();

            ImGuiComponents.HelpMarker("Fast Waymarks are only placeable within instanced zones", Dalamud.Interface.FontAwesomeIcon.QuestionCircle);

            ImGui.SameLine();

            if (ImGuiComponents.IconButton(Dalamud.Interface.FontAwesomeIcon.Wrench))
            {
                Plugin.ToggleConfigUI();
            }

            ImGui.Spacing();
        }
        else
        {
            ImGui.TextUnformatted($"The waymarks failed to compile");
            if (ImGui.Button("Reload Waymarks"))
            {
                FieldMarkerPreset currentWaymarks = new();
                if (MemoryHandler.GetCurrentWaymarksAsPresetData(ref currentWaymarks))
                {
                    ScratchEditingPreset = new ScratchPreset(WaymarkPreset.Parse(currentWaymarks));
                }
                preparePreset(false);
            }
        }
    }

    public void UpdateMapID()
    {
        if (ScratchEditingPreset != null)
        {
            ScratchEditingPreset.MapID = ZoneInfoHandler.GetContentFinderIDFromTerritoryTypeID(Plugin.ClientState.TerritoryType);
        }
        zoneChanged = true;
    }

    private void changeSetting()
    {
        Plugin.Configuration.hasSettingChanged = true;
    }

    private void preparePreset(bool toPlace)
    {
        int[] PO = getWaymarkOrder(Plugin.Configuration.Order);

        for (int i = 0; i < PO.Length; i++)
        {
            var tempX = calculateX(i);
            var tempZ = calculateZ(i);
            float tempY;
            bool isActive;
            if (calculateY(i, toPlace, tempX, tempZ, out tempY))
            {
                isActive = true;
            }
            else
            {
                isActive = false;
            }
            var tempCoord = new Vector3(tempX, tempY, tempZ);
            ScratchEditingPreset.SetWaymark(PO[i], isActive, tempCoord);
        }
    }

    private float calculateX(int idx)
    {
        var tempX = 0f;
        var startX = Plugin.Configuration.WaymarksCenterX;

        var rotationOperands = Utils.PIOverFour * (idx + (Plugin.Configuration.WaymarksRotationOffset / 45f) - 2);
        var rotation = (float)Math.Cos(rotationOperands);

        var radius = Plugin.Configuration.WaymarksRadius;
        var radiusB = Plugin.Configuration.WaymarksRadiusB;
        var squareCornerRadius = radius * Utils.SquareCornerFactor;

        switch (Plugin.Configuration.Shape)
        {
            case WaymarkShape.Circle:
                //Circle
                tempX = startX + (rotation * radius);
                break;
            case WaymarkShape.Square:
                //Square
                if (idx % 2 == 0)
                {
                    tempX = startX + (rotation * radius);
                }
                else
                {
                    tempX = startX + (rotation * squareCornerRadius);
                }
                break;
            case WaymarkShape.Diamond:
                //Diamond
                if (idx % 2 == 0)
                {
                    tempX = startX + (rotation * squareCornerRadius);

                }
                else
                {
                    tempX = startX + (rotation * radius);
                }
                break;
            case WaymarkShape.Star:
                //Star
                if (idx % 2 == 0)
                {
                    tempX = startX + (rotation * radius);
                }
                else
                {
                    tempX = startX + (rotation * radiusB);
                }
                break;
        }
        /*
        Plugin.Log.Debug($"Data of Waymark {idx}:\r\n" +
                                $"startX: {startX}\r\n" +
                                $"rotation: {rotation}\r\n" +
                                $"rotationOffset: {rotationOffset}\r\n" +
                                $"radius: {radius}\r\n" +
                                $"combined: {tempX}");
        */

        return tempX;
    }

    private bool calculateY(int idx, bool toPlace, float tempX, float tempZ, out float tempY)
    {
        tempY = 0f;
        if (toPlace)
        {
            if (!Plugin.Configuration.displayWaymarkY)
            {
                var actor = Plugin.ClientState.LocalPlayer;
                if (actor != null)
                {
                    tempY = actor.Position.Y;
                    var tempOrigin = new Vector3(tempX, tempY + 20f, tempZ);
                    RaycastHit hitInfo;
                    if (Raycast(tempOrigin, -Vector3.UnitY, out hitInfo))
                    {
                        Plugin.Log.Debug($"Waymark {idx} hitInfo: [{hitInfo.Point.X}, {hitInfo.Point.Y}, {hitInfo.Point.Z}] [{hitInfo.Distance}]");
                        tempY = hitInfo.Point.Y;
                        if (hitInfo.Distance <= 100f)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                tempY = Plugin.Configuration.WaymarksCenterY;
                return true;
            }
        }
        return true;
    }

    private float calculateZ(int idx)
    {
        var tempZ = 0f;
        var startZ = Plugin.Configuration.WaymarksCenterZ;

        var rotationOperands = Utils.PIOverFour * (idx + (Plugin.Configuration.WaymarksRotationOffset / 45f) - 2);
        var rotation = (float)Math.Sin(rotationOperands);

        var radius = Plugin.Configuration.WaymarksRadius;
        var radiusB = Plugin.Configuration.WaymarksRadiusB;
        var squareCornerRadius = radius * Utils.SquareCornerFactor;
        switch (Plugin.Configuration.Shape)
        {
            case WaymarkShape.Circle:
                //Circle
                tempZ = startZ + (rotation * radius);
                break;
            case WaymarkShape.Square:
                //Square
                if (idx % 2 == 0)
                {
                    tempZ = startZ + (rotation * radius);
                }
                else
                {
                    tempZ = startZ + (rotation * squareCornerRadius);
                }
                break;
            case WaymarkShape.Diamond:
                //Diamond
                if (idx % 2 == 0)
                {
                    tempZ = startZ + (rotation * squareCornerRadius);
                }
                else
                {
                    tempZ = startZ + (rotation * radius);
                }
                break;
            case WaymarkShape.Star:
                //Star
                if (idx % 2 == 0)
                {
                    tempZ = startZ + (rotation * radius);
                }
                else
                {
                    tempZ = startZ + (rotation * radiusB);
                }
                break;
        }
        /*
        Plugin.Log.Debug($"Data of Waymark {idx}:\r\n" +
                                $"startZ: {startZ}\r\n" +
                                $"rotation: {rotation}\r\n" +
                                $"rotationOffset: {rotationOffset}\r\n" +
                                $"radius: {radius}\r\n" +
                                $"squareCornerRadius: {squareCornerRadius}\r\n" +
                                $"combined: {tempZ}");
        */
        return tempZ;
    }

    private int[] getWaymarkOrder(WaymarkOrder order)
    {
        int[] WO = { 0, 1, 2, 3, 4, 5, 6, 7 };

        switch (order)
        {
            case WaymarkOrder.Proper:
                //Proper Order 
                //(A 1 B 2 C 3 D 4)
                WO = [0, 4, 1, 5, 2, 6, 3, 7];
                break;
            case WaymarkOrder.Partyfinder:
                //Party Finder Order
                //(A 2 B 3 C 4 D 1)
                WO = [0, 5, 1, 6, 2, 7, 3, 4];
                break;
            case WaymarkOrder.LetterNumber:
                //Letter-Number Order
                //(A B C D 1 2 3 4)
                WO = [0, 1, 2, 3, 4, 5, 6, 7];
                break;
        }

        return WO;
    }

    public bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance = 1000000f)
    {
        var result = BGCollisionModule.RaycastMaterialFilter(origin, direction, out hitInfo, maxDistance);
        return result;
    }
    
     
}
