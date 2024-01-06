using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    
    public LogicScript logic;
    public int blockType;
    public CategoryManager categoryManager;
    public Image image;
    
    private bool _started;
    private bool _selected;

    private void Start()
    {
        if (_started) 
            return;
        
        image = GetComponent<Image>();
    }

    public void Select()
    {
        if (!_started)
            Start();
        if (categoryManager.currentItemButton == this)
            return;
        
        _selected = true;
        
        logic.ChangeBlockType(blockType);
        if (categoryManager.currentItemButton != null) 
            categoryManager.currentItemButton.Deselect();
        
        categoryManager.currentItemButton = this; 
        image.color = categoryManager.selectedColor;
    }
    
    public void Deselect()
    {
        if (!_started)
            Start();
        _selected = false;
        
        image.color = categoryManager.unselectedColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        image.color = _selected ? categoryManager.selectedPressedColor : categoryManager.unselectedPressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        image.color = _selected ? categoryManager.selectedColor : categoryManager.unselectedColor;
    }
}