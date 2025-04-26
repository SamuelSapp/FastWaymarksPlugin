using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using Dalamud.Game.Command;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using ImGuiNET;
using Newtonsoft.Json;
using FastWaymarksPlugin.Windows;

namespace FastWaymarksPlugin;

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
    private MainWindow MainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);
        MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);

        ZoneInfoHandler.Init();

        foreach (var command in CommandList)
        {
            Commands.AddHandler(command, new CommandInfo(OnCommand)
            {
                HelpMessage = "Toggles main plugin window."
            });
        }
        

        PluginInterface.UiBuilder.Draw += DrawUI;

        // This adds a button to the plugin installer entry of this plugin which allows
        // to toggle the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        // Adds another button that is doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [FastWaymarksPlugin] ===A cool log message from Sample Plugin===
        Log.Information($"===A cool log message from {PluginInterface.Manifest.Name}===");
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        foreach (var command in CommandList)
        {
            Commands.RemoveHandler(command);
        }
        
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    
}
