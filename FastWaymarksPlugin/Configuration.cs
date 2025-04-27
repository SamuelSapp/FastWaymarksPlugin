using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Numerics;

namespace FastWaymarksPlugin;

public enum WaymarkOrder
{
    Proper,
    Partyfinder,
    LetterNumber
}

public enum WaymarkShape
{
    Circle,
    Square,
    Diamond,
    Star
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool CenterOnPlayer {get; set;} = false;
    public bool hasSettingChanged {get; set;} = false;
    public WaymarkOrder Order {get; set;} = WaymarkOrder.Proper;
    public WaymarkShape Shape {get; set;} = WaymarkShape.Circle;
    public float WaymarksCenterX {get; set;} = 0f;
    public float WaymarksCenterZ {get; set;} = 0f;
    public float WaymarksRadius {get; set;} = 10f;
    public float WaymarksRadiusB {get; set;} = 10f;
    public float WaymarksRotationOffset {get; set;} = 0f;


    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
