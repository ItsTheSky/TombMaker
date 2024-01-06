using System;
using UnityEngine;
using UnityEngine.UI;

public class CategoryButton : MonoBehaviour
{

    public SpriteState spriteState;
    public CategoryManager categoryManager;
    public GameObject category;
    
    public Image image;
    private bool _started;
    private bool _selected;

    private void Start()
    {
        if (_started) 
            return;
        
        _started = true;
        categoryManager = GameObject.Find("Categories").GetComponent<CategoryManager>();
        image = GetComponent<Image>();
        spriteState = GetComponent<Button>().spriteState;
    }

    public void Deselect()
    {
        _selected = false;
        category.SetActive(false);
        
        image.color = categoryManager.unselectedColor;
    }

    public void Select()
    {
        if (!_started) 
            Start();
        if (categoryManager.currentCategoryButton == this)
            return;
        
        _selected = true;
        if (categoryManager.currentCategoryButton != null)
            categoryManager.currentCategoryButton.Deselect();

        categoryManager.currentCategoryButton = this;
        category.SetActive(true);
        image.color = categoryManager.selectedColor;
    }
    
}