using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class DropdownSetting : GlobalSetting
{
        
    public readonly string[] options;
    public readonly System.Action<int> onValueChanged;
    public readonly Func<bool> shouldShow;
    public int value;
    
    public DropdownSetting(string id,
        string name,
        string helpText,
        string[] options,
        int value,
        Action<int> onValueChanged,
        Func<bool> shouldShow = null)
    {
        this.id = id;
        this.name = name;
        this.description = helpText;
        this.options = options;
        this.value = value;
        this.onValueChanged = onValueChanged;
        this.shouldShow = shouldShow;
        type = new DropdownSettingType();
    }
    
    public override void SetValue(object value)
    {
        base.SetValue(value);
        this.value = (int) value;
    }
    
    public override string Serialize()
    {
        return value.ToString();
    }
    
    public override void Deserialize(string serialized)
    {
        value = int.Parse(serialized);
        
    }
    
    public override void ForceUpdate()
    {
        onValueChanged(value);
    }

    public override bool ShouldShow()
    {
        return shouldShow?.Invoke() ?? true;
    }
}

public class DropdownSettingType : SettingType
{
    public override GameObject AddSetting(GameObject container, GlobalSetting setting)
    {
        var dropdownSetting = (DropdownSetting) setting;
        var dropdown = Object.Instantiate(Resources.Load<GameObject>("Settings/DropdownSetting"), container.transform);
        dropdown.GetComponent<DropdownSettingController>().Init(dropdownSetting);
        
        return dropdown;
    }
}