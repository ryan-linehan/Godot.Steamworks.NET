using Godot;
using Godot.Steamworks.Net;
using Godot.Steamworks.Net.Utils;
using System.Collections.Generic;
using System.Linq;
[Tool]
public partial class SteamPanel : TabContainer
{
    [Export]
    public Godot.Collections.Dictionary<GodotSteamworksModules, PackedScene> ModuleTabs = new Godot.Collections.Dictionary<GodotSteamworksModules, PackedScene>();
    Dictionary<long, ISteamPanelTab> tabs = new Dictionary<long, ISteamPanelTab>();
    public override void _EnterTree()
    {
        TabChanged += OnTabChanged;
        base._EnterTree();
    }

    public override void _Ready()
    {
        base._Ready();
        // TODO: Save enabled modules and restore them here
        foreach (var item in ModuleTabs.OrderBy(x => x.Key != GodotSteamworksModules.Settings)
                                       .ThenByDescending(x => x.Key.GetDescription()))
        {
            var tabInstance = item.Value.Instantiate<Control>();
            if (tabInstance is ISteamPanelTab steamTab)
            {
                tabs[GetTabCount()] = steamTab;
            }
            AddChild(tabInstance);
        }
    }

    private void InitializeTabs()
    {
        foreach (var tab in tabs.Values)
        {
            tab.Init();
        }
    }

    public void AddPanel(GodotSteamworksModules module)
    {
        if (tabs.TryGetValue((long)module, out var panel))
            AddChild(panel as Control);
    }

    public void RemovePanel(GodotSteamworksModules module)
    {
        GD.Print("Removing panel for module: " + module);
        if (tabs.TryGetValue((long)module, out var panel))
        {
            GD.Print("???");
            RemoveChild(panel as Control);
        }
            
    }

    public override void _ExitTree()
    {
        GD.Print("Exiting SteamPanel");
        TabChanged -= OnTabChanged;
        base._ExitTree();        
    }


    private void OnTabChanged(long tab)
    {
        if (tabs.TryGetValue(tab, out var steamTab))
        {
            steamTab.Init();
        }
    }

}
