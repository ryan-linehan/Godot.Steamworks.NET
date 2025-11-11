using Godot;
using Godot.Steamworks.Net;
using System;
using System.Collections.Generic;
[Tool]
public partial class SteamPanel : TabContainer
{
    public AchievementsTab AchievementsTab = null!;
    public SettingsTab SettingsTab = null!;
    Dictionary<long, ISteamPanelTab> tabs = new Dictionary<long, ISteamPanelTab>();
    public override void _Ready()
    {
        base._Ready();
        TabChanged += OnTabChanged;
        for (int i = 0; i < GetTabCount(); i++)
        {
            var child = GetChild(i);
            if (child is SettingsTab settingsTab)
            {
                tabs[i] = settingsTab;
                SettingsTab = settingsTab;
            }
            else if (child is AchievementsTab achievementsTab)
            {
                tabs[i] = achievementsTab;
                AchievementsTab = achievementsTab;
            }
        }
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        TabChanged -= OnTabChanged;
    }


    private void OnTabChanged(long tab)
    {
        if (tabs.TryGetValue(tab, out var steamTab))
        {
            steamTab.Init();
        }
    }

}
