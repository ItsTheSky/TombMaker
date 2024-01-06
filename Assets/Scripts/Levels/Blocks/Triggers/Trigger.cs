using System.Collections.Generic;
using UnityEngine;

public abstract class Trigger : Block
{
    
    public abstract void TriggerAction();
    
    public abstract void ResetChanges();
    
    public bool activateOnce = true;
    
    private bool _activated;

    public override void Init()
    {
        base.Init();
        
        options.Add(new BlockSettingController.ToggleOption("activate_once", "Activate Once",
            "Either the trigger should activate one time only.",
            activateOnce, b => { activateOnce = b; }));
    }

    public override void Enable()
    {
        base.Enable();
        
        _renderer.enabled = true;
        _activated = false;
        
        ResetChanges();
    }
    
    public override void Disable()
    {
        base.Disable();
        
        _renderer.enabled = false;
        _activated = false;
        
        ResetChanges();
    }
    
    public void OnTriggerEnter2D(Collider2D col)
    {
        CheckInit();
        
        if (col.gameObject.CompareTag("player"))
        {
            if (activateOnce && _activated) 
                return;
            TriggerAction();
            _activated = true;
        }
    }
    
    public override Dictionary<string, object> SaveSettings()
    {
        var dict = new Dictionary<string, object>();
        
        dict.Add("activate_once", activateOnce);
        
        return dict;
    }

    public override void LoadSettings(Dictionary<string, object> settings)
    {
        activateOnce = (settings.ContainsKey("activate_once") ? (bool) settings["activate_once"] : true);
    }

    public override void OnPlayerDeath()
    {
        ResetChanges();
        _activated = false;
    }

    public override bool IsTrigger()
    {
        return true;
    }
}