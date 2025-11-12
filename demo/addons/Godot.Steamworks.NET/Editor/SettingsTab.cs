using System;
using System.Collections.Generic;
using SteamWebAPI2.Interfaces;
using Godot.Steamworks.Net.Utils;

namespace Godot.Steamworks.Net;


[Tool]
public partial class SettingsTab : MarginContainer, ISteamPanelTab
{
    [Export]
    private LineEdit _steamApiKeyLineEdit = null!;
    [Export]
    private LineEdit _steamAppIdLineEdit = null!;
    [Export]
    private Button _setupWebApi = null!;
    [Export]
    private Control _moduleCheckboxContainer = null!;

    private List<ModuleCheckBox> _moduleCheckBoxes = new();
    private bool _isValidated = false;
    private bool _buttonPressedOnce = false;
    
    public override void _Ready()
    {
        base._Ready();        
        _setupWebApi.Pressed += OnSetupWebApiPressed;


        // Find all module checkboxes in the parent panel
        FindModuleCheckBoxes();

        // Start with modules disabled until validation
        DisableAllModules();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        _setupWebApi.Pressed -= OnSetupWebApiPressed;
    }

    private void FindModuleCheckBoxes()
    {
        _moduleCheckBoxes.Clear();
        // Get all ModuleCheckBox children from the container
        foreach (var child in _moduleCheckboxContainer.GetChildren())
        {
            if (child is ModuleCheckBox moduleCheckBox)
            {
                _moduleCheckBoxes.Add(moduleCheckBox);
            }
        }
    }

    private async void OnSetupWebApiPressed()
    {
        var appId = _steamAppIdLineEdit.Text.Trim();
        var apiKey = _steamApiKeyLineEdit.Text.Trim();

        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(apiKey))
        {
            GD.PrintErr("App ID and API Key are required");
            DisableAllModules();
            return;
        }

        await ValidateAndSaveAsync(appId, apiKey);
    }

    private async System.Threading.Tasks.Task ValidateAndSaveAsync(string appId, string apiKey)
    {
        try
        {
            if (!uint.TryParse(appId, out uint appIdUint))
            {
                GD.PrintErr("Invalid App ID format");
                DisableAllModules();
                _isValidated = false;
                return;
            }

            // Get the static factory with the API key
            var webInterfaceFactory = GodotSteamworksEditorPlugin.GetSteamWebInterfaceFactory(apiKey);
            var steamUserInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUserStats>();
            var gameStats = await steamUserInterface.GetSchemaForGameAsync(appIdUint);

            if (gameStats?.Data != null)
            {
                GD.Print("Valid Steam API credentials. Game: " + gameStats.Data.GameName);

                // Save to config
                var saveError = SteamConfigManager.SaveConfig(appId, apiKey);
                if (saveError == Error.Ok)
                {
                    _isValidated = true;
                    _buttonPressedOnce = true;
                    EnableAllModules();
                }
                else
                {
                    GD.PrintErr("Failed to save configuration");
                    DisableAllModules();
                    _isValidated = false;
                }
            }
            else
            {
                GD.PrintErr("Invalid Steam API credentials or App ID");
                DisableAllModules();
                _isValidated = false;
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error validating Steam API credentials: {ex.Message}");
            DisableAllModules();
            _isValidated = false;
        }
    }

    private void DisableAllModules()
    {
        FindModuleCheckBoxes();
        foreach (var moduleCheckBox in _moduleCheckBoxes)
        {
            moduleCheckBox.DisableModule();
        }
    }

    private void EnableAllModules()
    {
        FindModuleCheckBoxes();
        foreach (var moduleCheckBox in _moduleCheckBoxes)
        {
            moduleCheckBox.EnableModule();
        }
    }

    public async void Init()
    {
        // Init is called when the tab is selected
        // Check if we have saved config and load it
        if (SteamConfigManager.HasValidConfig() && !_isValidated)
        {
            var (appId, apiKey) = SteamConfigManager.LoadConfig();
            _steamAppIdLineEdit.Text = appId;
            _steamApiKeyLineEdit.Text = apiKey;

            // Auto-validate the loaded config
            await ValidateAndSaveAsync(appId, apiKey);
        }
    }
}
