using System;
using System.Collections.Generic;
using UnityEngine;

public class CategoryManager : MonoBehaviour
{
    
    public List<GameObject> categories;
    public List<GameObject> categoryButtons;
    
    public CategoryButton currentCategoryButton;
    public ItemButton currentItemButton;

    // style
    public Color selectedColor;
    public Color unselectedColor;
    
    public Color selectedPressedColor;
    public Color unselectedPressedColor;

    private void Awake()
    {
        EditorValues.CategoryManager = this;
    }

    private void Start()
    {
        foreach (var category in categories)
        {
            category.SetActive(false);
            foreach (var btn in category.GetComponentsInChildren<ItemButton>())
            {
                btn.categoryManager = this;
                btn.logic = EditorValues.LogicScript;
            }
        }

        for (int i = 0; i < categories.Count; i++)
        {
            var btn = categoryButtons[i].GetComponent<CategoryButton>();
            btn.category = categories[i];
            btn.categoryManager = this;

            if (i == 0)
            {
                btn.Select();
                categories[i].SetActive(true);
                
                var itemBtn = categories[i].GetComponentInChildren<ItemButton>();
                itemBtn.Select();
            }
        }
    }

}