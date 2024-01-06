using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CategorizedHelpBox : MonoBehaviour
{
        
    [Header("References")]
    public GameObject content;
    public GameObject categoryButtonPrefab;
    public GameObject categoryButtonContainer;
    public string[] hiddenCategories = Array.Empty<string>();
    
    [HideInInspector] public Dictionary<string, GameObject> categories = new ();
    [HideInInspector] public Dictionary<string, Button> categoryButtons = new ();
    [HideInInspector] public GameObject currentCategory;
    [HideInInspector] public TMP_Text title;
    [HideInInspector] public bool initialized;

    public void Init()
    {
        initialized = true;
        // title object is always called "Title"
        title = transform.Find("Title").GetComponent<TMP_Text>();
        
        foreach (Transform child in content.transform)
        {
            categories.Add(child.name, child.gameObject);
        }
        
        // First register all categoreis with their game objects
        foreach (var category in categories)
        {
            if (hiddenCategories.Contains(category.Key))
                continue;
            
            var button = Instantiate(categoryButtonPrefab, categoryButtonContainer.transform).GetComponent<Button>();
            button.GetComponentInChildren<TMP_Text>().text = category.Key;
            button.onClick.AddListener(() => ShowCategory(category.Key));
            button.interactable = true;
            
            categoryButtons.Add(category.Key, button.GetComponent<Button>());
        }
        
        // Then hide all categories
        foreach (var category in categories)
            category.Value.SetActive(false);
        
        ShowCategory(categories.First().Key);
    }
    
    public void ShowCategory(string name)
    {
        if (categories.TryGetValue(name, out var category))
        {
            if (currentCategory != null) {
                currentCategory.SetActive(false);
                categoryButtons[currentCategory.name].interactable = true;
            }
            
            categoryButtons[name].interactable = false;
            
            category.SetActive(true);
            currentCategory = category;
            title.text = name;
        }
        else
        {
            Debug.LogError($"Category with name {name} not found!");
        }
    }

}