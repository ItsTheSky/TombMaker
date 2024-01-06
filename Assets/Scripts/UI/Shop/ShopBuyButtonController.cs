using TMPro;
using UnityEngine;

public class ShopBuyButtonController : MonoBehaviour
{
    
    public TMP_Text btnText;

    private Constants.SkinMeta _skinMeta;
    private Constants.Trail _trailMeta;
    private bool _canBuy;
    private bool _isSkin;
    private int _price => _isSkin ? _skinMeta.price : _trailMeta.price;
    
    private ShopController _shopController;
    
    public void EnableTrail(Constants.Trail trailMeta, ShopController shopController)
    {
        GlobalEnable(trailMeta.price, shopController);
        _trailMeta = trailMeta;
        _isSkin = false;
    }
    
    public void EnableSkin(Constants.SkinMeta skinMeta, ShopController shopController)
    {
        GlobalEnable(skinMeta.price, shopController);
        _skinMeta = skinMeta;
        _isSkin = true;
    }

    public void GlobalEnable(int price, ShopController shopController)
    {
        _shopController = shopController;
        
        var currentCoins = PlayerStatsManager.GetCoins();
        _canBuy = price <= currentCoins;
        
        btnText.text = !_canBuy ? "No Coins" : "Buy";
        
        gameObject.SetActive(true);
    }
    
    public void Disable()
    {
        gameObject.SetActive(false);
        
        _canBuy = false;
        _skinMeta = null;
        _trailMeta = null;
    }

    public void OnClick()
    {
        if (!_canBuy)
            return;
        
        PlayerStatsManager.RemoveCoins(_price);

        if (_isSkin)
        {
            PlayerStatsManager.UnlockSkin(_skinMeta.id);
            PlayerStatsManager.SetSelectedSkin(_skinMeta.id);
            _shopController.LoadSkins();
        }
        else
        {
            PlayerStatsManager.UnlockTrail(_trailMeta.id);
            PlayerStatsManager.SetSelectedTrail(_trailMeta.id);
            _shopController.LoadTrails();
        }
        
    }
}