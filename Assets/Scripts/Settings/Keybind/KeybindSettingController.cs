using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class KeybindSettingController : SettingController
{
    public static GameObject waitingForInputOverlay;
    
    public TMP_Text optionName;
    
    public TMP_Text keybindText;
    
    private KeybindSetting _setting;
    private bool _waitingForInput = false;

    public void Init(KeybindSetting setting)
    {
        _setting = setting;
        optionName.text = setting.name;
        
        keybindText.text = setting.value.ToString();
    }
    
    public override void SetValue(object value)
    {
        keybindText.text = ((KeyCode) value).ToString();
    }
    
    public void OnClick()
    {
        GetWaitingForInputOverlay().SetActive(true);
        _waitingForInput = true;
        keybindText.text = "...";
    }

    private void Update()
    {
        if (!_waitingForInput) return;
        
        foreach(KeyCode key in Enum.GetValues(typeof(KeyCode)))
        {
            if(Input.GetKey(key))
            {
                if (key == KeyCode.Escape)
                {
                    _waitingForInput = false;
                    GetWaitingForInputOverlay().SetActive(false);
                    SetValue(_setting.value);
                    return;
                }
                
                _waitingForInput = false;
                GetWaitingForInputOverlay().SetActive(false);
                SetValue(key);
                _setting.onValueChanged(key);
                _setting.value = key;
                ClientSettingsManager.SetSetting(_setting.id, key.ToString());
            }
        }
    }

    public GameObject GetWaitingForInputOverlay()
    {
        if (waitingForInputOverlay == null)
            waitingForInputOverlay = Instantiate(Resources.Load<GameObject>("Settings/WaitingForInputOverlay"),
                GameObject.Find("Canvas").transform);
        
        return waitingForInputOverlay;
    }
}