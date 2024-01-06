using System.Linq;
using TMPro;

public class DropdownSettingController : SettingController
{
    private DropdownSetting _setting;
    public TMP_Text optionName;
    
    public TMP_Dropdown dropdown;

    public void Init(DropdownSetting setting)
    {
        _setting = setting;
        optionName.text = setting.name;
        
        dropdown.ClearOptions();
        dropdown.AddOptions(setting.options.ToList());
        dropdown.value = setting.value;
        dropdown.onValueChanged.AddListener((v) => OnValueChanged(v));
    }
    
    public void OnValueChanged(int value)
    {
        SetValue(value);
        _setting.onValueChanged(value);
        ClientSettingsManager.SetSetting(_setting.id, value.ToString());
    }
    
    public override void SetValue(object value)
    {
        dropdown.value = (int) value;
    }
}