using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ToggleOptionController : OptionController
{
    public TMP_Text optionName;

    public Sprite onIcon;
    public Sprite offIcon;

    public Image iconImage;

    private Action<bool> _onToggle;
    private bool _on;

    public void Init(string name, bool on, Action<bool> onToggle)
    {
        optionName.text = name;
        _on = on;
        this._onToggle = onToggle;
        
        RefreshIcon();
    }
    
    public void Toggle()
    {
        _on = !_on;
        _onToggle?.Invoke(_on);
        
        RefreshIcon();
    }
    
    public void RefreshIcon()
    {
        iconImage.sprite = _on ? onIcon : offIcon;
    }
    
    public bool IsOn()
    {
        return _on;
    }
    
    public override void SetValue(object value)
    {
        _on = (bool) value;
        RefreshIcon();
    }
}