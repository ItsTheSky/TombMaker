using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : Trigger
{
    [HideInInspector] public bool reset = false;
    
    [HideInInspector] public bool stickyCamera = true;
    [HideInInspector] public long cameraZoom = 5;
    [HideInInspector] public double cameraX = 0.5f;
    [HideInInspector] public double cameraY = 0.5f;

    [HideInInspector] public bool unlockedX = false;
    [HideInInspector] public bool unlockedY = false;

    public override void Init()
    {
        base.Init();
        
        options.Add(new BlockSettingController.ToggleOption("camera_reset",
            "Reset Camera", "Reset the camera to level's settings",
            reset, value =>
            {
                reset = value;
            }));
        
        options.Add(new BlockSettingController.SliderOption("zoom",
            "Camera Distance", "Change how far the camera is from the player.",
            3, 15, true, cameraZoom, value =>
            {
                cameraZoom = (int) value;
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
                try {
                    cameraX = double.Parse(value);
                } catch (Exception e) { /* ignored */  }
            }));
        options.Add(new BlockSettingController.InputOption("y",
            "Camera Y", "The Y position of the camera.",
            cameraY.ToString(), TMPro.TMP_InputField.ContentType.DecimalNumber, value =>
            {
                try {
                    cameraY = double.Parse(value);
                } catch (Exception e) { /* ignored */  }
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
            }));
    }

    public override bool ShouldShowOption(BlockSettingController.BlockOption option)
    {
        // if reset is checked, hide other camera options.
        // then, if sticky is checked, hide x and y.
        if (option.id == "zoom" || option.id == "sticky" || option.id == "x" || option.id == "y")
        {
            if (reset)
                return false;
            if (option.id == "x" || option.id == "y" || option.id == "unlocked_x" || option.id == "unlocked_y")
            {
                if (stickyCamera)
                    return false;
            }
        }
        
        return base.ShouldShowOption(option);
    }

    public override void TriggerAction()
    {
        EditorValues.LogicScript.defaultCamera = reset;
        
        EditorValues.LogicScript._cameraZoom = cameraZoom;
        EditorValues.LogicScript._stickyCamera = stickyCamera;
        EditorValues.LogicScript._cameraX = cameraX;
        EditorValues.LogicScript._cameraY = cameraY;
        EditorValues.LogicScript._unlockedX = unlockedX;
        EditorValues.LogicScript._unlockedY = unlockedY;

        EditorValues.LogicScript.UpdateCamera();
    }

    public override void ResetChanges()
    {
        EditorValues.LogicScript.defaultCamera = true;
        EditorValues.LogicScript.UpdateCamera();
    }

    public override Dictionary<string, object> SaveSettings()
    {
        var dict = base.SaveSettings();
        
        dict.Add("reset", reset);
        dict.Add("stickyCamera", stickyCamera);
        dict.Add("cameraZoom", cameraZoom);
        dict.Add("cameraX", cameraX);
        dict.Add("cameraY", cameraY);
        dict.Add("unlockedX", unlockedX);
        dict.Add("unlockedY", unlockedY);
        
        return dict;
    }

    public override void LoadSettings(Dictionary<string, object> settings)
    {
        base.LoadSettings(settings);

        reset = (settings.ContainsKey("reset") ? (bool) settings["reset"] : false);
        stickyCamera = (settings.ContainsKey("stickyCamera") ? (bool) settings["stickyCamera"] : true);
        cameraZoom = (settings.ContainsKey("cameraZoom") ? (long) settings["cameraZoom"] : 5);
        
        try
        {
            cameraX = (settings.ContainsKey("cameraX") ? (double)settings["cameraX"] : 0.5f);
            cameraY = (settings.ContainsKey("cameraY") ? (double)settings["cameraY"] : 0.5f);
        }
        catch (Exception e)
        {
            cameraX = (settings.ContainsKey("cameraX") ? (long) settings["cameraX"] : 0.5f);
            cameraY = (settings.ContainsKey("cameraY") ? (long) settings["cameraY"] : 0.5f);
        }
        
        unlockedX = (settings.ContainsKey("unlockedX") ? (bool) settings["unlockedX"] : false);
        unlockedY = (settings.ContainsKey("unlockedY") ? (bool) settings["unlockedY"] : false);
    }
}