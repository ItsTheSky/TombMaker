using System.Globalization;
using TMPro;
using UnityEngine.UI;

public class SliderSettingController : SettingController
{
    private SliderSetting _setting;
    public TMP_Text optionName;
    
    public Slider slider;
    public TMP_Text valueText;
    
    public void Init(SliderSetting setting)
    {
        _setting = setting;
        optionName.text = setting.name;
        
        slider.minValue = setting.min;
        slider.maxValue = setting.max;
        slider.wholeNumbers = setting.wholeNumbers;
        slider.value = setting.value;
        slider.onValueChanged.AddListener((v) => OnValueChanged(v));
        
        valueText.text = setting.value.ToString(CultureInfo.InvariantCulture);
    }
    
    public void OnValueChanged(float value)
    {
        valueText.text = value.ToString();
        SetValue(value);
        _setting.onValueChanged(value);
        ClientSettingsManager.SetSetting(_setting.id, value.ToString());
    }
    
    public override void SetValue(object value)
    {
        slider.value = (float) value;
    }
}