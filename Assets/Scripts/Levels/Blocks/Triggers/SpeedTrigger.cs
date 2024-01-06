using System.Collections.Generic;
using UnityEngine;

public class SpeedTrigger : Trigger
{

    [HideInInspector] public long speed = PlayerScript.DefaultPlayerSpeed;

    public override void Init()
    {
        base.Init();
        
        options.Add(new BlockSettingController.SliderOption("speed", "Speed", 
            "Change the speed of the player. Default speed is "+PlayerScript.DefaultPlayerSpeed+".", 10, 50, true, 
            speed, value =>
            {
                speed = (int) value;
            }));
    }

    public override void TriggerAction()
    {
        EditorValues.PlayerScript.speed = speed;
    }

    public override void ResetChanges()
    {
        EditorValues.PlayerScript.speed = PlayerScript.DefaultPlayerSpeed;
    }

    public override void LoadSettings(Dictionary<string, object> settings)
    {
        base.LoadSettings(settings);
        
        speed = (settings.ContainsKey("speed") ? (long) settings["speed"] : PlayerScript.DefaultPlayerSpeed);
    }

    public override Dictionary<string, object> SaveSettings()
    {
        var dict = base.SaveSettings();
        
        dict.Add("speed", speed);
        
        return dict;
    }
}