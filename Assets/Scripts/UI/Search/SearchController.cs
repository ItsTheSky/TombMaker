using System;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchController : MonoBehaviour
{

    public TMP_InputField inputName;
    public TMP_InputField inputAuthor;
    public SearchStarsController starsController;
    public Toggle verifiedToggle;
    public TMP_Dropdown modeDropdown;
    
    public BrowseLevelsControlle BrowseLevelsControlle;
    
    public void Search()
    { 
        SearchData data = new SearchData
        {
            name = inputName.text,
            author = inputAuthor.text,
            stars = starsController.starsCount == 0 ? -1 : starsController.starsCount,
            verified = verifiedToggle.isOn,
            mode = modeDropdown.value == 0 ? -1 : modeDropdown.value - 1
        };

        BrowseLevelsControlle.gameObject.SetActive(true);
        gameObject.SetActive(false);
        
        BrowseLevelsControlle.SearchLevels(data);
    }
    
    [Serializable]
    public class SearchData
    {
        
        public string name;
        public string author;
        public int stars;
        public bool verified;
        public int mode;

    }
}