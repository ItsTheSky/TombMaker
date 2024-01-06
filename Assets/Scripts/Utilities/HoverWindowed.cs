using System;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.EventSystems;

public class HoverWindowed : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    private static GameObject Window;
    
    public string Text;
    private bool _isHovering;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Window == null)
            Window = Resources.Load<GameObject>("HoverWindow");
        _isHovering = true;
        //EditorValues.UpdateScale(Window.transform, false);
        // TODO: make hoverwindow size dynamic

        var window = Instantiate(Window, FindObjectOfType<Canvas>().transform);
        window.name = "--- HoverWindow ---";
        var text = window.GetComponentInChildren<TMPro.TMP_Text>();
        text.text = Text;
        
        var maxCharsPerLine = 20;
        var currentChars = Text.Length;
        
        // Adapt the size of the window to the text
        var rectTransform = window.GetComponent<RectTransform>();
        double width = Math.Ceiling((double)currentChars / maxCharsPerLine);
        rectTransform.sizeDelta = new Vector2(maxCharsPerLine * 33, (float) width * 45f); 

        MoveWindow(window);
    }

    private void Update()
    {
        if (_isHovering)
        {
            var window = GameObject.Find("--- HoverWindow ---");
            if (window != null) MoveWindow(window);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovering = false;
        Destroy(GameObject.Find("--- HoverWindow ---"));
    }
    
    // method to move the window to the mouse position
    public void MoveWindow(GameObject window)
    {
        var width = window.transform.GetComponent<RectTransform>().sizeDelta.x;
        var mouse = Input.mousePosition;
        
        if (mouse.x + 10 + width > Screen.width)
        {
            window.transform.position = new Vector3(mouse.x - 5, mouse.y + 5, 0);
        }
        else
        {
            window.transform.position = new Vector3(mouse.x + 5, mouse.y + 5, 0);
        }
    }
}