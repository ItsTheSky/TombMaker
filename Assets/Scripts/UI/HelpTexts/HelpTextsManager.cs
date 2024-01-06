using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpTextsManager : MonoBehaviour
{
    public static HelpTextsManager Instance { get; private set; }

    public void Init()
    {
        Instance = this;
        gameObject.transform.parent.gameObject.SetActive(false);

        foreach (Transform child in transform)
        {
            _helpTexts.Add(child.name, child.gameObject.GetComponent<Animator>());
        }
        
        _closeButton = gameObject.transform.parent.gameObject.GetComponentInChildren<Button>();
    }
    // #########################################################
    
    private readonly Dictionary<string, Animator> _helpTexts = new (); 
    private string _currentHelpTextName;
    private Button _closeButton;
    
    public void ShowHelpTextInternal(string name)
    {
        if (_helpTexts.ContainsKey(name))
        {
            var animator = _helpTexts[name];
            gameObject.transform.parent.gameObject.SetActive(true);
            
            var categorizedHelpBox = animator.gameObject.GetComponent<CategorizedHelpBox>();
            if (categorizedHelpBox != null && !categorizedHelpBox.initialized)
                categorizedHelpBox.Init();
            
            animator.gameObject.SetActive(true);
            animator.transform.SetAsLastSibling();
            animator.Play("OpenBox");
            
            _currentHelpTextName = name;
            _closeButton.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError($"Help text with name {name} not found!");
        }
    }
    
    public void CloseHelpText()
    {
        if (_currentHelpTextName == null)
        {
            Debug.LogError("No help text is currently active!");
            return;
        }
        
        if (_helpTexts.TryGetValue(_currentHelpTextName, out var text))
        {
            text.Play("CloseBox");
            _closeButton.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError($"Help text with name {name} not found!");
        }
    }

    public void FinishCloseHelpText()
    {
        gameObject.transform.parent.gameObject.SetActive(false);
        _currentHelpTextName = null;
    }
    
    public static void ShowHelpText(string name) => Instance.ShowHelpTextInternal(name);
}