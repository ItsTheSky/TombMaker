using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderOptionController : OptionController
{
    public TMP_Text optionName;
        
    public Slider slider;
    public TMP_Text valueText;
    
    public void Init(string optionName, float min, float max, bool wholeNumbers, float value, System.Action<float> onValueChanged)
    {
        this.optionName.text = optionName;
        
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = wholeNumbers;
        slider.value = value;
        slider.onValueChanged.AddListener((v) => onValueChanged(v));
        
        valueText.text = value.ToString();
    }
    
    public void OnValueChanged(float value)
    {
        valueText.text = value.ToString();
        SetValue(value);
    }
    
    public override void SetValue(object value)
    {
        slider.value = (float) value;
    }
}