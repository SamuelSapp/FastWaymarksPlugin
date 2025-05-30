﻿using Dalamud.Game.Command;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FastWaymarksPlugin.Windows;

namespace FastWaymarksPlugin;

/*
Portions of code within this plugin were taken from WaymarkPresetPlugin by Infiziert90.
https://github.com/Infiziert90/WaymarkPresetPlugin
*/

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IDataManager Data { get; private set; } = null!;
    [PluginService] public static ITextureProvider Texture { get; private set; } = null!;
    [PluginService] public static ICommandManager Commands { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] public static IGameGui GameGui { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static ICondition Condition { get; private set; } = null!;

    private static readonly string[] CommandList = new[] { "/fw", "/fastwaymarks" };

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("FastWaymarksPlugin");
    private ConfigWindow ConfigWindow { get; init; }
    public MainWindow MainWindow { get; init; }
    public MapWindow MapWindow { get; init; }

    internal readonly IDalamudTextureWrap[] WaymarkIconTextures = new IDalamudTextureWrap[8];

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);
        MapWindow = new MapWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(MapWindow);

        ZoneInfoHandler.Init();

        foreach (var command in CommandList)
        {
            Commands.AddHandler(command, new CommandInfo(OnCommand)
            {
                HelpMessage = "Toggles main plugin window."
            });
        }

        ClientState.TerritoryChanged += TerritoryChanged;

        PluginInterface.UiBuilder.Draw += DrawUI;

        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        //Load waymark icons.
        WaymarkIconTextures[0] ??= Texture.GetFromGameIcon(61241).RentAsync().Result; //A
        WaymarkIconTextures[1] ??= Texture.GetFromGameIcon(61242).RentAsync().Result; //B
        WaymarkIconTextures[2] ??= Texture.GetFromGameIcon(61243).RentAsync().Result; //C
        WaymarkIconTextures[3] ??= Texture.GetFromGameIcon(61247).RentAsync().Result; //D
        WaymarkIconTextures[4] ??= Texture.GetFromGameIcon(61244).RentAsync().Result; //1
        WaymarkIconTextures[5] ??= Texture.GetFromGameIcon(61245).RentAsync().Result; //2
        WaymarkIconTextures[6] ??= Texture.GetFromGameIcon(61246).RentAsync().Result; //3
        WaymarkIconTextures[7] ??= Texture.GetFromGameIcon(61248).RentAsync().Result; //4
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();
        MapWindow.Dispose();

        ClientState.TerritoryChanged -= TerritoryChanged;

        PluginInterface.UiBuilder.Draw -= DrawUI;

        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUI;
        
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUI;

        foreach (var command in CommandList)
        {
            Commands.RemoveHandler(command);
        }

        foreach (var t in WaymarkIconTextures)
        {
            t?.Dispose();
        }
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void TerritoryChanged(ushort territoryType)
    {
        Plugin.Log.Debug($"Territory Changed to: {territoryType}");
        MainWindow.UpdateMapID();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();
    public void ToggleMapUI() => MapWindow.Toggle();

}
