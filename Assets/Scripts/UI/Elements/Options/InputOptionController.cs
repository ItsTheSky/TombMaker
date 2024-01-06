using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InputOptionController : OptionController
{
    
    public TMP_Text optionNameText;
    
    public TMP_InputField inputField;

    public void Init(string optionName, string value, TMP_InputField.ContentType contentType,
        System.Action<string> onValueChanged)
    {
        optionNameText.text = optionName;

        inputField.text = value;
        inputField.contentType = contentType;
        inputField.onValueChanged.AddListener((v) => onValueChanged(v));
    }

    public override void SetValue(object value)
    {
        inputField.text = value.ToString();
    }    
}