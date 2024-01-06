using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class SliderSetting : GlobalSetting
{
        
    public readonly float min;
    public readonly float max;
    public readonly bool wholeNumbers;
    public readonly System.Action<float> onValueChanged;
    public readonly Func<bool> shouldShow;
    public float value;
    
    public SliderSetting(string id,
        string name,
        string helpText,
        float min,
        float max,
        bool wholeNumbers,
        float value,
        Action<float> onValueChanged,
        Func<bool> shouldShow = null)
    {
        this.id = id;
        this.name = name;
        this.description = helpText;
        this.min = min;
        this.max = max;
        this.wholeNumbers = wholeNumbers;
        this.value = value;
        this.onValueChanged = onValueChanged;
        this.shouldShow = shouldShow;
        type = new SliderSettingType();
    }
    
    public override void SetValue(object value)
    {
        base.SetValue(value);
        this.value = (float) value;
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

public class SliderSettingType : SettingType
{
    public override GameObject AddSetting(GameObject container, GlobalSetting setting)
    {
        var sliderSetting = (SliderSetting) setting;
        var slider = Object.Instantiate(Resources.Load<GameObject>("Settings/SliderSetting"), container.transform);
        slider.GetComponent<SliderSettingController>().Init(sliderSetting);
        
        return slider;
    }
}