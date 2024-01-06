using System.Linq;
using TMPro;
using UnityEngine;

public class DropdownOptionController : OptionController
{
        
    public TMP_Text optionNameText;
    
    public TMP_Dropdown dropdown;
    
    public void Init(string optionName, int value, string[] options, System.Action<int> onValueChanged)
    {
        optionNameText.text = optionName;

        dropdown.ClearOptions();
        dropdown.AddOptions(options.ToList().Select(o => new TMP_Dropdown.OptionData(o)).ToList());
        dropdown.value = value;
        dropdown.onValueChanged.AddListener((v) => onValueChanged(v));
    }
    
    public override void SetValue(object value)
    {
        dropdown.value = (int) value;
    }
}