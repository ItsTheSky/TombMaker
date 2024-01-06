using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextBlock : Block
{

    public TMP_Text text;

    public override void Init()
    {
        base.Init();
        
        options.Add(new BlockSettingController.InputOption("text",
            "Text", "The text to display.", "Hello World!", TMP_InputField.ContentType.Standard, value =>
            {
                text.text = value;
            }));
    }
    
    public override void Enable()
    {
        CheckInit();
        
        text.gameObject.SetActive(false);
        _renderer.enabled = true;
    }
    
    public override void Disable()
    {
        CheckInit();
        
        text.gameObject.SetActive(true);
        _renderer.enabled = false;
    }

    public override Dictionary<string, object> SaveSettings()
    {
        return new Dictionary<string, object>()
        {
            {"text", text.text}
        };
    }

    public override void LoadSettings(Dictionary<string, object> settings)
    {
        text.text = (settings.TryGetValue("text", out var setting) ? (string)setting : "Hello World!");
    }
}