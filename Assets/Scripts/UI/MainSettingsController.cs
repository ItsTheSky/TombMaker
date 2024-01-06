using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class MainSettingsController : MonoBehaviour
{
    public GameObject settingsContainer;
    public GameObject categoryText;
    
    [CanBeNull] [HideInInspector] public TMP_Text previousCategoryText;

    public void Start()
    {

        var categories = ClientSettingsManager.SETTINGS;

        object firstCategoryTxt = null;
        
        for (int i = 0; i < categories.Length; i++)
        {
            var categoryTxtObj = i == 0 ? categoryText : Instantiate(categoryText, categoryText.transform.parent.transform);
            
            categoryTxtObj.GetComponent<CategoryTextHandler>().Init(categories[i], this);
            categoryTxtObj.GetComponent<TMP_Text>().text = categories[i].name;
            
            if (i == 0)
                firstCategoryTxt = categoryTxtObj;
        }
        
        ((GameObject) firstCategoryTxt)?.GetComponent<CategoryTextHandler>()?.OnPointerDown(null);
    }

    public void ShowCategory(SettingCategory category)
    {
        foreach (Transform child in settingsContainer.transform)
            Destroy(child.gameObject);
        
        var settings = category.settings;
        foreach (GlobalSetting setting in settings)
        {
            ClientSettingsManager.LoadSetting(setting);
            var obj = setting.AddSelf(settingsContainer);
            
            obj.GetComponent<HoverWindowed>().Text = setting.description;
        }
    }
}