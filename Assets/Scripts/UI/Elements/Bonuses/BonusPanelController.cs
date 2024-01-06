using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BonusPanelController : MonoBehaviour
{

    public TMP_Text name;
    public TMP_Text description;
    public Button upgradeButton;
    public Animator iconAnimator;
    
    private Constants.Bonus bonus;
    private ShopController shopController;
    
    public void SetBonus(Constants.Bonus bonus, ShopController shopController)
    {
        this.bonus = bonus;
        this.shopController = shopController;
        
        description.text = bonus.description;
        RefreshTexts();
        
        upgradeButton.onClick.AddListener(Upgrade);
        iconAnimator.Play(this.bonus.animation);
        print("Set bonus " + bonus.name + ", playing animation " + this.bonus.animation);
    }
    
    private void Upgrade()
    {
        var currentLevel = PlayerStatsManager.GetBonusLevel(bonus.id);
        var price = bonus.GetPrice(currentLevel);
        if (PlayerStatsManager.GetCoins() < price) 
            return;
        
        PlayerStatsManager.SetCoins(PlayerStatsManager.GetCoins() - price);
        PlayerStatsManager.SetBonusLevel(bonus.id, currentLevel + 1);
        
        RefreshTexts();
        
        shopController.coinsText.text = PlayerStatsManager.GetCoins().ToString();
        AudioManager.PlaySound("Upgrade");
    }

    public void RefreshTexts()
    {
        var currentLevel = PlayerStatsManager.GetBonusLevel(bonus.id);
        var maxLevel = bonus.maxLevel;
        name.text = bonus.name + ": " + bonus.GetLevelText(currentLevel);

        if (currentLevel < maxLevel)
        {
            upgradeButton.GetComponentInChildren<TMP_Text>().text = "Buy (" + bonus.GetPrice(currentLevel) + ")";
        }
        else
        {
            upgradeButton.GetComponentInChildren<TMP_Text>().text = "Maxed out";
            upgradeButton.interactable = false;
        }
    }
    
}