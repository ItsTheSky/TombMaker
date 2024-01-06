using System;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBlock : Block
{
    
    public long spawnRotation = 1; // 1: down, 2: right, 3: up, 4: left
    public bool stickyCamera = true;
    public bool showDoor;
    public long cameraZoom = 5;
    public double cameraX = 0.5f;
    public double cameraY = 0.5f;
    public bool unlockedX;
    public bool unlockedY;
    public long style;
    public long color;
    
    public override void Init()
    {
        base.Init();
        uniqueLayer = true;
        canEdit = false;

        /*
        options.Add(new BlockSettingController.DropdownOption("rotation",
            "Starting Rotation", "Change the default rotation of the player",
            new []{"Down", "Right", "Up", "Left"}, (int) spawnRotation, value =>
            {
                spawnRotation = value + 1;
            }));

        options.Add(new BlockSettingController.SliderOption("zoom",
            "Camera Distance", "Change how far the camera is from the player.",
            3, 15, true, cameraZoom, value =>
            {
                cameraZoom = (int)value;
            }));
        options.Add(new BlockSettingController.ToggleOption("sticky",
            "Sticky Camera", "Either the camera will follow the player.",
            stickyCamera, value =>
            {
                stickyCamera = value;
            }));
        options.Add(new BlockSettingController.InputOption("x",
            "Camera X", "The X position of the camera.",
            cameraX.ToString(), TMPro.TMP_InputField.ContentType.DecimalNumber, value =>
            {
                cameraX = int.Parse(value);
            }));
        options.Add(new BlockSettingController.InputOption("y",
            "Camera Y", "The Y position of the camera.",
            cameraY.ToString(), TMPro.TMP_InputField.ContentType.DecimalNumber, value =>
            {
                cameraY = int.Parse(value);
            }));
        options.Add(new BlockSettingController.ToggleOption("door",
            "Show Entrance", "Either a decorative door will be shown when the level starts.",
            showDoor, value =>
            {
                showDoor = value;
            }));
        options.Add(new BlockSettingController.DropdownOption("style",
            "Blocks Style", "Change the style of the blocks.",
            new []{"Default", "Bricks", "Triangles", "Lines", "Blocks"}, (int) style, value =>
            {
                // If we have a value of 4, we need to add 2 to the value to skip the spikes
                style = value + (value >= 4 ? 2 : 0);
            }));
        options.Add(new BlockSettingController.DropdownOption("color",
            "Blocks Color", "Change the color of the blocks.",
            new []{"Default", "Blue", "Indigo", "Red", "Green"}, (int) color, value =>
            {
                color = value;
                _playerScript.logic.UpdateBlocksColor();
            }));

        options.Add(new BlockSettingController.ToggleOption("unlocked_x",
            "Unlocked X", "The camera will follow the player horizontally.",
            unlockedX, value =>
            {
                unlockedX = value;
            }));
        options.Add(new BlockSettingController.ToggleOption("unlocked_y",
            "Unlocked Y", "The camera will follow the player vertically.",
            unlockedY, value =>
            {
                unlockedY = value;
            })); */
    }

    public override bool ShouldShowOption(BlockSettingController.BlockOption option)
    {
        return !stickyCamera || option.id != "y" && option.id != "x" && option.id != "unlocked_x" && option.id != "unlocked_y";
    }
    
    public override void LoadSettings(Dictionary<string, object> settings)
    {
        showDoor = (!settings.TryGetValue("showDoor", out var setting) || (bool) setting);
        spawnRotation = (settings.TryGetValue("spawnRotation", out var setting1) ? (long) setting1 : 1);
        stickyCamera = (!settings.TryGetValue("stickyCamera", out var setting2) || (bool) setting2);
        cameraZoom = (settings.TryGetValue("cameraZoom", out var setting3) ? (long) setting3 : 5);
        style = (settings.TryGetValue("style", out var setting4) ? (long) setting4 : 0);
        color = (settings.TryGetValue("color", out var setting5) ? (long) setting5 : 0);

        try
        {
            cameraX = (settings.TryGetValue("cameraX", out var setting6) ? (double) setting6 : 0.5f);
            cameraY = (settings.TryGetValue("cameraY", out var setting7) ? (double) setting7 : 0.5f);
        }
        catch (Exception e)
        {
            cameraX = (settings.TryGetValue("cameraX", out var setting6) ? (long) setting6 : 0.5f);
            cameraY = (settings.TryGetValue("cameraY", out var setting7) ? (long) setting7 : 0.5f);
        }
        
        unlockedX = (settings.TryGetValue("unlockedX", out var setting8) ? (bool) setting8 : false);
        unlockedY = (settings.TryGetValue("unlockedY", out var setting9) ? (bool) setting9 : false);
    }
    
    public override bool CanBeMoveReplaced()
    {
        return false;
    }

    public override bool CanBeDeletedManually()
    {
        return false;
    }
}