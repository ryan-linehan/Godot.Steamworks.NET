using System;
using System.Collections.Generic;
using SteamWebAPI2.Interfaces;
using Godot.Steamworks.Net.Utils;
using Steamworks;

namespace Godot.Steamworks.Net;


[Tool]
public partial class SettingsTab : MarginContainer, ISteamPanelTab
{
    [Export]
    private LineEdit _steamApiKeyLineEdit = null!;
    [Export]
    private Label _steamAppIdLabel = null!;
    [Export]
    private Button _setupWebApi = null!;
    [Export]
    private Control _moduleCheckboxContainer = null!;
    [Export]
    private Button _initializeEditorButton = null!;

    private List<ModuleCheckBox> _moduleCheckBoxes = new();
    private bool _isValidated = false;
    private bool _buttonPressedOnce = false;

    public override void _Ready()
    {
        base._Ready();
        _setupWebApi.Pressed += OnSetupWebApiPressed;
        _initializeEditorButton.Pressed += InitializeEditorIntegration;
        // Find all module checkboxes in the parent panel
        FindModuleCheckBoxes();
        DisableAllModules();
    }

    private async void InitializeEditorIntegration()
    {
        // GodotSteamworks.Instance.InitGodotSteamworks();
        SteamAPI.Init();
        if (!string.IsNullOrEmpty(_steamApiKeyLineEdit.Text))
            OnSetupWebApiPressed();
        // Init is called when the tab is selected
        // Check if we have saved config and load it
        if (SteamConfigManager.HasValidConfig() && !_isValidated)
        {
            var appId = SteamUtils.GetAppID().ToString();
            var apiKey = SteamConfigManager.LoadApiKey();
            _steamAppIdLabel.Text = appId;
            _steamApiKeyLineEdit.Text = apiKey;

            // Auto-validate the loaded config
            await ValidateAndSaveAsync(appId, apiKey);
        }
        EnableAllModules();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        _setupWebApi.Pressed -= OnSetupWebApiPressed;
        _initializeEditorButton.Pressed -= InitializeEditorIntegration;
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
        var apiKey = _steamApiKeyLineEdit.Text.Trim();

        if (!GodotSteamworks.Instance.IsInitialized)
        {
            GD.PrintErr("App ID not found in steam_appid.txt");
            DisableAllModules();
            return;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            GD.PrintErr("API Key is required");
            DisableAllModules();
            return;
        }

        await ValidateAndSaveAsync(SteamUtils.GetAppID().ToString(), apiKey);
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
            var steamUserInterface = webInterfaceFactory.CreateSteamWebInterface<SteamWebAPI2.Interfaces.SteamUserStats>();
            var gameStats = await steamUserInterface.GetSchemaForGameAsync(SteamUtils.GetAppID().m_AppId);

            if (gameStats?.Data != null)
            {
                GD.Print("Valid Steam API credentials. Game: " + gameStats.Data.GameName);

                // Save to config
                var saveError = SteamConfigManager.SaveApiKey(apiKey);
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
    }
}
