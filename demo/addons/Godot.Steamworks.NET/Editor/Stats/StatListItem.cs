using Godot;
using Godot.Steamworks.Net;
using Godot.Steamworks.Net.Models;
using System;

[Tool]
public partial class StatListItem : Control
{
    [Export]
    Label NameLabel = null!;
    [Export]
    Label ValueLabel = null!;
    [Export]
    SpinBox ValueSpinBox = null!;
    
    private string _statKey = string.Empty;
    private StatType _statType = StatType.Int;
    private Button SetButton = null!;

    public override void _Ready()
    {
        SetButton = GetNode<Button>("%SetButton");
        SetButton.Pressed += OnSetButtonPressed;
        base._Ready();
    }

    public override void _ExitTree()
    {
        SetButton.Pressed -= OnSetButtonPressed;
        base._ExitTree();
    }

    private void OnSetButtonPressed()
    {
        if (_statType == StatType.Int)
        {
            GodotSteamworks.Stats.SetStatInt(_statKey, (int)ValueSpinBox.Value);
            int newValue = GodotSteamworks.Stats.GetStatInt(_statKey);
            ValueLabel.Text = newValue.ToString();
            ValueSpinBox.Value = newValue;
        }
        else
        {
            GodotSteamworks.Stats.SetStatFloat(_statKey, (float)ValueSpinBox.Value);
            float newValue = GodotSteamworks.Stats.GetStatFloat(_statKey);
            ValueLabel.Text = newValue.ToString("F2");
            ValueSpinBox.Value = newValue;
        }
    }

    public void SetStat(Stat stat)
    {
        _statKey = stat.Key;
        _statType = stat.Type;
        NameLabel.Text = stat.Name;
        
        if (_statType == StatType.Int)
        {
            ValueLabel.Text = ((int)stat.Value).ToString();
            ValueSpinBox.Value = (int)stat.Value;
            ValueSpinBox.Step = 1;
        }
        else
        {
            ValueLabel.Text = stat.Value.ToString("F2");
            ValueSpinBox.Value = stat.Value;
            ValueSpinBox.Step = 0.01;
        }
    }
}
