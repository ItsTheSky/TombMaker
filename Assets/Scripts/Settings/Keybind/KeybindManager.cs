using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public static class KeybindManager
{
    #region Keybinds

    // Player
    public static readonly Keybind MovePlayerUp = new() { id = 0, name = "Move Player Up", description = "The keybind used to move the player up.", value = KeyCode.UpArrow };
    public static readonly Keybind MovePlayerDown = new() { id = 1, name = "Move Player Down", description = "The keybind used to move the player down.", value = KeyCode.DownArrow };
    public static readonly Keybind MovePlayerLeft = new() { id = 2, name = "Move Player Left", description = "The keybind used to move the player left.", value = KeyCode.LeftArrow };
    public static readonly Keybind MovePlayerRight = new() { id = 3, name = "Move Player Right", description = "The keybind used to move the player right.", value = KeyCode.RightArrow };
    public static readonly Keybind ActivateShield = new() { id = 4, name = "Activate Shield", description = "The keybind used to activate the player's shield.", value = KeyCode.B };
    
    // Editor
    public static readonly Keybind ToggleEditor = new() { id = 5, name = "Toggle Editor", description = "The keybind used to toggle the editor.", value = KeyCode.Space };
    public static readonly Keybind ShowFrameWheel = new() { id = 6, name = "Show Frame Wheel", description = "The keybind used to show the frame wheel.", value = KeyCode.E };
    public static readonly Keybind RotateBlock = new() { id = 7, name = "Rotate Block", description = "The keybind used to rotate the block.", value = KeyCode.R };
    public static readonly Keybind MoveCameraUp = new() { id = 8, name = "Move Camera Up", description = "The keybind used to move the camera up.", value = KeyCode.W };
    public static readonly Keybind MoveCameraDown = new() { id = 9, name = "Move Camera Down", description = "The keybind used to move the camera down.", value = KeyCode.S };
    public static readonly Keybind MoveCameraLeft = new() { id = 10, name = "Move Camera Left", description = "The keybind used to move the camera left.", value = KeyCode.A };
    public static readonly Keybind MoveCameraRight = new() { id = 11, name = "Move Camera Right", description = "The keybind used to move the camera right.", value = KeyCode.D };

    public static readonly List<Keybind> Keybinds = new()
    {
        MovePlayerUp, MovePlayerDown, MovePlayerLeft, MovePlayerRight, ActivateShield,
        ToggleEditor, ShowFrameWheel, RotateBlock, MoveCameraUp, MoveCameraDown, MoveCameraLeft, MoveCameraRight
    };

    #endregion

    #region Meta
    private static bool _loaded = false;
    
    public static SettingCategory CreateCategory()
    {
        var categories = new List<GlobalSetting>();
        foreach (Keybind keybind in Keybinds)
            categories.Add(new KeybindSetting("keybind."+keybind.id.ToString(), keybind.name, keybind.description, keybind.value, value => ChangeKeybind(keybind.id, value)));
        
        return new SettingCategory("Keybinds", categories.ToArray());
    }

    public class Keybind
    {

        public int id;
        public string name;
        public string description;
        public KeyCode value;

    }
    
    public static void ChangeKeybind(int id, KeyCode value)
    {
        CheckIfLoaded();
        
        var keybind = Keybinds.Find(keybind => keybind.id == id);
        keybind.value = value;
        ClientSettingsManager.SetSetting("keybind." + keybind.id.ToString(), value.ToString());
    }
    
    public static void LoadKeybinds()
    {
        foreach (Keybind keybind in Keybinds)
        {
            var setting = ClientSettingsManager.GetSetting("keybind." + keybind.id.ToString());
            if (setting == null) continue;
            keybind.value = (KeyCode) System.Enum.Parse(typeof(KeyCode), setting);
        }
        
        _loaded = true;
    }
    
    public static void CheckIfLoaded()
    {
        if (!_loaded)
            LoadKeybinds();
    }

    #endregion
    
}