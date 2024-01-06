using System;
using UnityEngine;
using UnityEngine.UI;

public class HelpButtonController : MonoBehaviour
{

    public string name;

    private void Start()
    {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }
    
    private void OnClick()
    {
        HelpTextsManager.ShowHelpText(name);
    }
}