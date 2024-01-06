using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Beside the sprite swap, when we select the button we have to lower the text a bit (child of the button)
// NOTE: only do that if it's left click
public class ButtonController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Transform _transform;
    private Button _button;
    public int offset = 5;
    
    void Start()
    {
        _transform = GetComponent<Transform>();
        _button = GetComponent<Button>();

        if (_button != null)
        {
            _wasInteractable = _button.interactable;
            FormatObjects(true);   
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (_button != null && !_button.interactable) return;
        
        foreach (Transform child in _transform)
        {
            child.transform.localPosition += new Vector3(0, -offset, 0);
            AudioManager.instance?.Play("ButtonPressed");
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (_button != null && !_button.interactable) return;
        
        foreach (Transform child in _transform)
        {
            child.transform.localPosition -= new Vector3(0, -offset, 0);
        }
    }
    
    // We also need to check if the state of interaction of the button is enabled or not,
    // if it is not enabled, we also need to lower the text a bit
    private bool _wasInteractable;

    private void Update()
    {
        if (_button == null)
            return;
        
        if (_button.interactable == _wasInteractable)
            return;
        
        _wasInteractable = _button.interactable;

        FormatObjects();
    }

    public void FormatObjects(bool firstCheck = false)
    {
        foreach (Transform child in _transform)
        {
            if (_button.interactable)
            {
                if (!firstCheck)
                {
                    child.transform.localPosition -= new Vector3(0, -offset, 0);
                } 
            }
            else
            {
                child.transform.localPosition += new Vector3(0, -offset, 0);
            }
        }
    }
}