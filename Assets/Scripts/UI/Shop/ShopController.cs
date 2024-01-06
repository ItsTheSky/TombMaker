using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    public TMP_Text coinsText;
    public ShopBuyButtonController buyButtonController;
    public GameObject comingSoonObject;
    public List<ShopCategoryButtonController> categories = new ();
    [HideInInspector] public ShopCategoryButtonController selectedCategory;
    
    public GameObject eventSkinButton;
    
    // ======== Skins
    public GameObject skinElementObject;
    public GameObject skinElementsObject;
    public GameObject skinElementPrefab;
    public ShopSkinDemoController skinDemoController;
    [FormerlySerializedAs("selectedSkinElementController")] [HideInInspector] 
    public BuyElementController selectedBuyElementController;
    
    [HideInInspector] public long _currentPreviewSkinId;
    
    private BuyElementController _defaultSkinController;
    
    // ======== Trails
    public GameObject trailElementObject;
    public GameObject trailElementsObject;
    public GameObject trailElementPrefab;
    public Image trailDemoImage;
    [HideInInspector] public BuyElementController selectedTrailElementController;
    
    [HideInInspector] public long _currentPreviewTrailId;
    
    private BuyElementController _defaultTrailController;
    
    // ======== Bonuses
    public GameObject bonusPanelPrefab;
    public GameObject bonusContainer;
    public GameObject bonusObject;

    private bool _isAdmin;
    private string _currentCategory;
    private MainController _mainController;
    
    public async void Init(MainController mainController)
    {
        _mainController = mainController;
        foreach (var category in categories)
            category.Init(this);

        foreach (var children in bonusContainer.transform)
            Destroy((children as Transform)?.gameObject);
        
        foreach (var bonus in Constants.BONUSES) 
            Instantiate(bonusPanelPrefab, bonusContainer.transform)
                .GetComponent<BonusPanelController>()
                .SetBonus(bonus, this);
        
        comingSoonObject.SetActive(false);
        bonusObject.SetActive(false);
        eventSkinButton.SetActive(false);
        
        selectedBuyElementController = null;
        skinDemoController.SwitchSkin(Constants.GetSkinMeta(PlayerStatsManager.GetSelectedSkin()));
        _currentPreviewSkinId = PlayerStatsManager.GetSelectedSkin();
        
        selectedTrailElementController = null;
        trailDemoImage.sprite = Constants.Trails[PlayerStatsManager.GetSelectedTrail()].GetIcon();
        _currentPreviewTrailId = PlayerStatsManager.GetSelectedTrail();
        
        coinsText.text = PlayerStatsManager.GetCoins().ToString();
        LoadSkins();
        LoadTrails();
        _wasBuyButtonEnabled = buyButtonController.gameObject.activeSelf;
        
        SwitchCategory("skins");
        
        _isAdmin = await NewDB.IsAdmin();
        eventSkinButton.SetActive(_currentCategory == "skins" && _isAdmin);
        
        _mainController.RefreshLevelInfos();
    }
    
    public void LoadSkins()
    {
        var eventSkinId = -1;
        coinsText.text = PlayerStatsManager.GetCoins().ToString();
        _mainController.RefreshLevelInfos();

        foreach (Transform child in skinElementsObject.transform)
            Destroy(child.gameObject);
        
        foreach (var skin in Constants.SKINS)
        {
            var skinElement = Instantiate(skinElementPrefab, skinElementsObject.transform);
            skinElement.GetComponent<BuyElementController>().InitSkin(skin, this, eventSkinId);

            if (skin.id == _currentPreviewSkinId)
            {
                selectedBuyElementController = skinElement.GetComponent<BuyElementController>();
                selectedBuyElementController.Select();
            }
            
            if (skin.id == PlayerStatsManager.GetSelectedSkin())
                _defaultSkinController = skinElement.GetComponent<BuyElementController>();
        }
        
        if (selectedBuyElementController == null)
            selectedBuyElementController = skinElementsObject.transform.GetChild(0).GetComponent<BuyElementController>();
        skinDemoController.SwitchSkin(selectedBuyElementController.skinMeta);
        
        // ======== Buy button
        if (selectedBuyElementController.skinMeta.canBuy && !PlayerStatsManager.HasSkinUnlocked(selectedBuyElementController.skinMeta.id))
        {
            buyButtonController.EnableSkin(selectedBuyElementController.skinMeta, this);
        }
        else
        {
            buyButtonController.Disable();
        }
    }
    
    public void LoadTrails()
    {
        coinsText.text = PlayerStatsManager.GetCoins().ToString();
        _mainController.RefreshLevelInfos();

        foreach (Transform child in trailElementsObject.transform)
            Destroy(child.gameObject);
        
        foreach (var trail in Constants.Trails) 
        {
            var trailElement = Instantiate(trailElementPrefab, trailElementsObject.transform);
            trailElement.GetComponent<BuyElementController>().InitTrail(trail.Value, this);

            if (trail.Key == _currentPreviewTrailId)
            {
                selectedTrailElementController = trailElement.GetComponent<BuyElementController>();
                selectedTrailElementController.Select();
            }
            
            if (trail.Key == PlayerStatsManager.GetSelectedTrail())
                _defaultTrailController = trailElement.GetComponent<BuyElementController>();
        } 
        
        if (selectedTrailElementController == null)
            selectedTrailElementController = trailElementsObject.transform.GetChild(0).GetComponent<BuyElementController>();
        
        trailDemoImage.sprite = Constants.Trails[selectedTrailElementController.trailMeta.id].GetIcon();
        trailDemoImage.color = Constants.Trails[selectedTrailElementController.trailMeta.id].color;
        
        // ======== Buy button
        if (selectedTrailElementController.trailMeta.canBuy && !PlayerStatsManager.HasTrailUnlocked(selectedTrailElementController.trailMeta.id))
        {
            buyButtonController.EnableTrail(selectedTrailElementController.trailMeta, this);
        }
        else
        {
            buyButtonController.Disable();
        }
    }
    
    private bool _wasBuyButtonEnabled;
    
    public void SelectSkin(BuyElementController buyElementController)
    {
        if (buyElementController == selectedBuyElementController)
            return;
        _currentPreviewSkinId = buyElementController.skinMeta.id;
        LoadSkins();

        skinDemoController.SwitchSkin(selectedBuyElementController.skinMeta);
    }
    
    public void SelectTrail(BuyElementController buyElementController)
    {
        if (buyElementController == selectedBuyElementController)
            return;
        _currentPreviewTrailId = buyElementController.trailMeta.id;
        LoadTrails();

        trailDemoImage.sprite = Constants.Trails[selectedTrailElementController.trailMeta.id].GetIcon();
        trailDemoImage.color = Constants.Trails[selectedTrailElementController.trailMeta.id].color;
    }
    
    public void SwitchCategory(string category)
    {
        if (selectedCategory != null && selectedCategory.category == category)
            return;
        
        if (selectedCategory != null)
            selectedCategory.Deselect();
        
        _currentCategory = category;
        selectedCategory = categories.Find(x => x.category == category);
        selectedCategory.Select();
        
        if (category == "skins")
        {
            skinElementObject.gameObject.SetActive(true);
            skinDemoController.SwitchSkin(selectedBuyElementController.skinMeta);
            
            SelectSkin(_defaultSkinController);
            comingSoonObject.SetActive(false);
            bonusObject.SetActive(false);
            trailElementObject.gameObject.SetActive(false);
        }
        else if (category == "bonus")
        {
            _wasBuyButtonEnabled = buyButtonController.gameObject.activeSelf;
            skinElementObject.gameObject.SetActive(false);
            trailElementObject.gameObject.SetActive(false);
            buyButtonController.gameObject.SetActive(false);
            
            bonusObject.SetActive(true);
            comingSoonObject.SetActive(false);
        }
        else if (category == "trails")
        {
            trailElementObject.gameObject.SetActive(true);
            trailDemoImage.sprite = Constants.Trails[PlayerStatsManager.GetSelectedTrail()].GetIcon();
            
            SelectSkin(_defaultSkinController);
            comingSoonObject.SetActive(false);
            bonusObject.SetActive(false);
            skinElementObject.gameObject.SetActive(false);
        }
        else
        {
            _wasBuyButtonEnabled = buyButtonController.gameObject.activeSelf;
            skinElementObject.gameObject.SetActive(false);
            trailElementObject.gameObject.SetActive(false);
            buyButtonController.gameObject.SetActive(false);
            
            bonusObject.SetActive(false);
            comingSoonObject.SetActive(true);
        }
    }

    public void ChangeEventSkin()
    {
        //DatabaseManager.SetEventSkin((int) _currentPreviewSkinId);
    }

    public void BuyShield()
    {
        var shields = PlayerStatsManager.GetShields();
        if (shields >= 100)
            return;
        var price = 300;
        if (PlayerStatsManager.GetCoins() < price)
            return;
        PlayerStatsManager.SetCoins(PlayerStatsManager.GetCoins() - price);
        PlayerStatsManager.SetShields(shields + 1);
        coinsText.text = PlayerStatsManager.GetCoins().ToString();
        _mainController.RefreshLevelInfos();
        
        AudioManager.PlaySound("Buy");
        
        // also refresh current shop category
        SwitchCategory(_currentCategory);
        if (_currentCategory == "skins")
        {
            LoadSkins();
            SelectSkin(selectedBuyElementController);
        }
        else if (_currentCategory == "trails")
        {
            LoadTrails();
            SelectTrail(selectedTrailElementController);
        }
    }
}
