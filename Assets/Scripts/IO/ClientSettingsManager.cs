using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ClientSettingsManager
{
    public static readonly SettingCategory[] SETTINGS = new SettingCategory[]
    {
        new ("Display", new GlobalSetting[]
        {
            /* new DropdownSetting("ui_size", "UI Size", "Change the size of all the game's UI.", new []{"small", "medium", "big"}, 0, 
                value =>
                {
                    var lastValue = GetSetting("ui_size");
                    var formattedSize = value switch { 0 => 1f, 1 => 1.5f, 2 => 2f, _ => 1.5f };
                    var screenRes = Utilities.GetResolutionByIndex(int.Parse(GetSetting("screen_res")));
                    
                    Action action = () =>
                    {
                        EditorValues.UIScaleMultiplier = formattedSize;
                        bool isEditor = SceneManager.GetActiveScene().name == "LevelScene";
                        if (isEditor)
                        {
                            GameObject.Find("LogicManager").GetComponent<LogicScript>().UpdateScales();
                        }
                        else
                        {
                            GameObject.Find("Canvas").GetComponent<MainController>().UpdateLocalScale();
                        }
                    };
                    
                    Action noAction = () =>
                    {
                        SetSetting("ui_size", lastValue);
                    };
                    
                    // display a warning if the screen res is too small, basically:
                    // - if 'value' is 1, display warning under 2k
                    // - if 'value' is 2, display warning under 4k
                    if (value == 1 && screenRes.width < 2560)
                    {
                        PopupController.Show("Warning", "Your screen resolution is too small for this UI size. " +
                                                        "You may experience some issues with the UI.", action, null);
                    }
                    else if (value == 2 && screenRes.width < 3840)
                    {
                        PopupController.Show("Warning", "Your screen resolution is too small for this UI size. " +
                                                        "You may experience some issues with the UI.", action, null);
                    } else
                    {
                        action();
                    }
                    
                }), */
            new DropdownSetting("screen_type", "Screen Type", "Change the desired screen type.", new []{"Windowed", "Exclusive", "Borderless"}, 0, 
                value =>
                {
                    var formattedType = value switch { 0 => FullScreenMode.Windowed, 1 => FullScreenMode.ExclusiveFullScreen, 2 => FullScreenMode.FullScreenWindow, _ => FullScreenMode.Windowed };
                    
                    Screen.fullScreenMode = formattedType;
                }, Utilities.IsDesktopPlatform),
            new DropdownSetting("screen_res", "Screen Resolution", "Change the desired screen resolution.", Utilities.GetAvailableResolutionsAsString(), Utilities.GetAvailableResolutionsAsString().Length - 1, 
                value =>
                {
                    var formattedRes = Utilities.GetResolutionByIndex(value);
                    Screen.SetResolution(formattedRes.width, formattedRes.height, Screen.fullScreenMode);
                    
                    // now the ui scale is calculated based on the screen res, so we need to update it too.
                    // For 1080p the scale is 1, for 2k it's 1.5 and for 4k it's 2. thus the function:
                    // f(x) = 0.5x + 1
                    var uiScale = formattedRes.width > 1920 
                        ? (formattedRes.width / 1920f) / 2f + 1f 
                        : formattedRes.width / 1920f;

                    EditorValues.UIScaleMultiplier = uiScale;
                    bool isEditor = SceneManager.GetActiveScene().name == "LevelScene";
                    if (isEditor)
                    {
                        GameObject.Find("LogicManager").GetComponent<LogicScript>().UpdateScales();
                    }
                    else
                    {
                        GameObject.Find("Canvas").GetComponent<MainController>().UpdateLocalScale();
                    }
                }, Utilities.IsDesktopPlatform),
            new DropdownSetting("max_fps", "Max FPS", "Change the max FPS.", new []{"30", "60", "120", "150", "300"}, 1, 
                value =>
                {
                    var formattedFPS = value switch { 0 => 30, 1 => 60, 2 => 120, 3 => 150, 4 => 300, _ => 60 };
                    Application.targetFrameRate = formattedFPS;
                }),
        }),
        
        new ("Audio", new GlobalSetting[]
        {
            new SliderSetting("master_volume", "Master Volume", "Change the master volume.", 0, 100, true, 80, value => AudioListener.volume = value / 100f),
            new SliderSetting("music_volume", "Music Volume", "Change the music volume.", 0, 100, true, 80, value => GameObject.Find("AudioManager").GetComponent<AudioManager>().audioMixer.SetFloat("Music", 
                value == 0 ? -80 : Mathf.Log10(value / 100f) * 20)),
            new SliderSetting("sfx_volume", "SFX Volume", "Change the SFX volume.", 0, 100, true, 80, value => GameObject.Find("AudioManager").GetComponent<AudioManager>().audioMixer.SetFloat("Effects", 
                value == 0 ? -80 : Mathf.Log10(value / 100f) * 20)),
        }),
        
        KeybindManager.CreateCategory(),
        
        new ("Editor", new GlobalSetting[]
        {
            new DropdownSetting("editor_arrows_function", "Arrow Functions", "How should works the up/down arrow in the editor", new []{"Switch Layers", "Move Selected Blocks"}, 1,
                value =>
                {
                    LogicScript.UseArrowsForLayers = value == 0;
                }),
            new DropdownSetting("editor_edit_block", "Edit Block", "Change the mouse button used to quick edit", new []{"Middle Click", "Right Click"}, 0,
                value =>
                {
                    var code = value switch {
                        0 => KeyCode.Mouse2,
                        1 => KeyCode.Mouse1,
                        _ => KeyCode.A
                    };

                    LogicScript.KeyCodeForEditing = code;
                }),
            new DropdownSetting("editor_delete_block", "Delete Block", "Change the mouse button used to quick delete", new []{"Middle Click", "Right Click"}, 1,
                value =>
                {
                    var code = value switch {
                        0 => KeyCode.Mouse2,
                        1 => KeyCode.Mouse1,
                        _ => KeyCode.A
                    };

                    LogicScript.KeyCodeForDeleting = code;
                }),
            new DropdownSetting("editor_moving", "Moving", "Change the mouse button used to move the camera", new []{"Middle Click", "Right Click"}, 0,
                value =>
                {
                    var code = value switch {
                        0 => KeyCode.Mouse2,
                        1 => KeyCode.Mouse1,
                        _ => KeyCode.A
                    };

                    LogicScript.KeyCodeForMoving = code;
                }),
            new DropdownSetting("editor_guide", "Show Editor Guide", "Open the editor guide when opening the editor", new []{"Yes", "No"}, 0,
                value =>
                {
                    LogicScript.ShowEditorGuide = value == 0;
                }),
            new DropdownSetting("editor_grid", "Show Editor Grid", "Show a grid inside the editor.", new []{"Yes", "No"}, 0,
                value =>
                {
                    LogicScript.ShowEditorGrid = value == 0;
                    if (EditorValues.LogicScript != null && !EditorValues.LogicScript.IsDestroyed())
                        EditorValues.LogicScript.UpdateGrid();
                }),
        }),
        
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        new ("Debug", new GlobalSetting[]
        {
            new DropdownSetting("server", "Server", "Change the server used by the game to use online features", new []{"global", "local"}, 0, 
                value => NewDB._databaseURL =
                    value == 0 ? NewDB.RemoteHost : NewDB.LocalHost),
            //new SliderSetting("dummy_slider", "Dummy Slider", "This is a dummy slider", 0, 100, true, 50, value => Debug.Log("Dummy slider value changed to " + value))
        }),
#endif
    };
    
    // ##############################
    
    public static void SetSetting(string id, string value)
    {
        GlobalIOManager.Settings[id] = value;
        GlobalIOManager.Save();
    }
    
    public static string GetSetting(string id)
    {
        return GlobalIOManager.Settings[id];
    }
    
    // ##############################

    public static void LoadSetting(GlobalSetting setting)
    {
        if (GlobalIOManager.Settings.ContainsKey(setting.id))
            setting.Deserialize(GlobalIOManager.Settings[setting.id]);
        else
            GlobalIOManager.Settings[setting.id] = setting.Serialize();
    }
    
    public static void LoadAndInitSettings()
    {
        foreach (var category in SETTINGS)
        {
            foreach (var setting in category.settings)
            {
                if (!setting.ShouldShow())
                    return;
                LoadSetting(setting);
                setting.ForceUpdate();
            }
        }
    }
}