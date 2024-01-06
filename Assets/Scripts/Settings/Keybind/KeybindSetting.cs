using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class KeybindSetting : GlobalSetting
{
    
    public readonly System.Action<KeyCode> onValueChanged;
    public readonly Func<bool> shouldShow;
    public KeyCode value;
    
    public KeybindSetting(string id,
        string name,
        string helpText,
        KeyCode value,
        Action<KeyCode> onValueChanged,
        Func<bool> shouldShow = null)
    {
        this.id = id;
        this.name = name;
        this.description = helpText;
        this.value = value;
        this.onValueChanged = onValueChanged;
        this.shouldShow = shouldShow;
        type = new KeybindSettingType();
    }
    
    public override string Serialize()
    {
        return value.ToString();
    }

    public override void Deserialize(string serialized)
    {
        value = (KeyCode) Enum.Parse(typeof(KeyCode), serialized);
    }

    public override bool ShouldShow()
    {
        return shouldShow?.Invoke() ?? true;
    }

    public override void SetValue(object value)
    {
        base.SetValue(value);
        this.value = (KeyCode) value;
    }
}

public class KeybindSettingType : SettingType
{
    public override GameObject AddSetting(GameObject container, GlobalSetting setting)
    {
        var keybindSetting = (KeybindSetting) setting;
        var keybind = Object.Instantiate(Resources.Load<GameObject>("Settings/KeybindSetting"), container.transform);
        keybind.GetComponent<KeybindSettingController>().Init(keybindSetting);
        
        return keybind;
    }
}