using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Beside the sprite swap, when we select the button we have to lower the text a bit (child of the button)
public class DropdownController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private TMP_Dropdown _dropdown;
    public int offset = 10;
    
    void Start()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        foreach (Transform child in _dropdown.transform)
        {
            child.transform.localPosition += new Vector3(0, -offset, 0);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        foreach (Transform child in _dropdown.transform)
        {
            child.transform.localPosition -= new Vector3(0, -offset, 0);
        }
    }
}