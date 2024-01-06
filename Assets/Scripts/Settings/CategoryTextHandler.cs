using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CategoryTextHandler : MonoBehaviour, IPointerDownHandler
{
    private SettingCategory _category;
    private MainSettingsController _mainSettingsController;
    
    public void Init(SettingCategory category, MainSettingsController mainSettingsController)
    {
        _category = category;
        _mainSettingsController = mainSettingsController;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_mainSettingsController.previousCategoryText != null)
            _mainSettingsController.previousCategoryText.color = Color.black;
        
        _mainSettingsController.previousCategoryText = GetComponent<TMP_Text>();
        if (_mainSettingsController.previousCategoryText != null)
            _mainSettingsController.previousCategoryText.color = Color.magenta;
        
        _mainSettingsController.ShowCategory(_category);
    }
}