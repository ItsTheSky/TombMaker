using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LavaTrigger : Trigger
{
    public static LavaTrigger OnGoingLavaTrigger;
    
    public GameObject lavaPrefab;

    [HideInInspector] public bool disableMode;
    
    [HideInInspector] public float lavaX = 0;
    [HideInInspector] public float lavaY = 0;
    [HideInInspector] public int side = 0; // 0 = bottom, 1 = left, 2
    [HideInInspector] public int lavaId = 0;
    public float bps = 1; // how many blocks moved per second

    public override void Init()
    {
        base.Init();

        options.Add(new BlockSettingController.ToggleOption("disableMode",
            "Disable Mode", "Should the trigger cancel a created lava instead of creating one.",
            disableMode, value =>
            {
                disableMode = value;
            }));
        
        options.Add(new BlockSettingController.InputOption("x",
            "Lava X", "The starting X position of the lava.",
            lavaX.ToString(), TMPro.TMP_InputField.ContentType.DecimalNumber, value =>
            {
                try
                {
                    lavaX = float.Parse(value);
                }
                catch (Exception e)
                {
                    // ignored
                }
            }));
        options.Add(new BlockSettingController.InputOption("y",
            "Lava Y", "The starting Y position of the lava.",
            lavaY.ToString(), TMPro.TMP_InputField.ContentType.DecimalNumber, value =>
            {
                try {
                    lavaY = float.Parse(value);
                } catch (Exception e) { }
            }));
        options.Add(new BlockSettingController.DropdownOption("side",
            "Lava Side", "The side the lava will come from.",
            new []{"Bottom", "Left", "Top", "Right"}, (int) side, value =>
            {
                side = value;
            }));
        options.Add(new BlockSettingController.SliderOption("bps", "Block per second", "How many blocks will move per second.",
            1, 10, true, bps, value =>
            {
                bps = value;
            }));
    }
    
    public override bool ShouldShowOption(BlockSettingController.BlockOption option)
    {
        if (option.id == "x" || option.id == "y" || option.id == "side" || option.id == "bps")
        {
            if (disableMode)
                return false;
        }
        
        return base.ShouldShowOption(option);
    }

    private GameObject _lava;
    
    public override void TriggerAction()
    {
        if (disableMode)
        {
            if (OnGoingLavaTrigger != null && !OnGoingLavaTrigger.IsDestroyed())
                OnGoingLavaTrigger.ResetChanges();
            return;
        }
        
        if (OnGoingLavaTrigger != null && !OnGoingLavaTrigger.IsDestroyed())
            OnGoingLavaTrigger.ResetChanges();
        OnGoingLavaTrigger = this;
        if (_lava != null && !_lava.IsDestroyed())
            Destroy(_lava);
        
        // we are moving the lava to its middle top position instead of its center. (but need to make it for all rotation)
        float x = lavaX, y = lavaY;
        if (side == 0)
            y -= lavaPrefab.transform.localScale.y / 2;
        else if (side == 1)
            x -= lavaPrefab.transform.localScale.y / 2;
        else if (side == 2)
            y += lavaPrefab.transform.localScale.y / 2;
        else if (side == 3)
            x += lavaPrefab.transform.localScale.y / 2;
        
        var degrees= side * 90;
        var vec3 = new Vector3(x, y, 5);
        _lava = Instantiate(lavaPrefab, vec3, Quaternion.identity);
        _lava.transform.RotateAround(vec3, Vector3.forward, degrees);
        _lava.transform.position = vec3;
            
        var controller = _lava.GetComponent<LavaController>();
        controller.themeColor = EditorValues.LogicScript.GetColor();
        controller.bps = bps;
        controller.side = side;
    }

    public override void ResetChanges()
    {
        if (_lava != null && !_lava.IsDestroyed())
            Destroy(_lava);
    }

    public override Dictionary<string, object> SaveSettings()
    {
        var dict = base.SaveSettings();
        
        dict.Add("x", lavaX);
        dict.Add("y", lavaY);
        dict.Add("side", side);
        dict.Add("bps", bps);
        
        return dict;
    }

    public override void LoadSettings(Dictionary<string, object> settings)
    {
        base.LoadSettings(settings);
        
        lavaX = settings.TryGetValue("x", out var setting2) ? float.Parse(setting2.ToString()) : 0;
        lavaY = settings.TryGetValue("y", out var setting3) ? float.Parse(setting3.ToString()) : 0;
        side = settings.TryGetValue("side", out var setting1) ? int.Parse(setting1.ToString()) : 0;
        bps = settings.TryGetValue("bps", out var setting) ? float.Parse(setting.ToString()) : 1;
    }
}