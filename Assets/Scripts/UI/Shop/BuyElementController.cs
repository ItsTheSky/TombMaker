using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyElementController : MonoBehaviour
{
    public TMP_Text price;
    public Image skinImage;
    public GameObject lockIcon;
    public GameObject selectedIcon;
    public Image bgImage;
    
    public Constants.SkinMeta skinMeta;
    public Constants.Trail trailMeta;

    private ShopController _shopController;

    private bool _isSkin;

    public async void InitSkin(Constants.SkinMeta skinMeta, ShopController shopController, int eventSkinId)
    {
        _isSkin = true;
        this.skinMeta = skinMeta;
        _shopController = shopController;
        selectedIcon.SetActive(false);
        
        var selectedSkin = PlayerStatsManager.GetSelectedSkin();
        var previewSkin = _shopController._currentPreviewSkinId;
        if (eventSkinId == -1)
            eventSkinId = 0;
        
        if (!skinMeta.canBuy 
            || PlayerStatsManager.HasSkinUnlocked(skinMeta.id)
            || selectedSkin == skinMeta.id
            || eventSkinId == skinMeta.id)
        {
            skinImage.gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            price.gameObject.SetActive(false);
            lockIcon.SetActive(false);

            if (previewSkin == skinMeta.id)
                bgImage.color /* set it to 0, 255, 255 */ = new Color(0f, 1f, 1f);
            if (selectedSkin == skinMeta.id)
                selectedIcon.SetActive(true);
        }
        else
        {
            price.text = skinMeta.price.ToString();
            lockIcon.SetActive(true);
            skinImage.color /* set it to 210, 210, 0 */ = new Color(0.82f, 0.82f, 0f);
        }
        
        var sprites = Resources.LoadAll<Sprite>("Player/" + skinMeta.name + "/idle");
        skinImage.sprite = sprites[5];
    }
    
    public async void InitTrail(Constants.Trail trail, ShopController shopController)
    {
        _isSkin = false;
        trailMeta = trail;
        _shopController = shopController;
        selectedIcon.SetActive(false);
        
        var selectedTrail = PlayerStatsManager.GetSelectedTrail();
        var previewTrail = _shopController._currentPreviewTrailId;
        bgImage.color = new Color(0f, 0f, 0f);
        
        if (PlayerStatsManager.HasTrailUnlocked(trail.id)
            || selectedTrail == trail.id)
        {
            skinImage.gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            price.gameObject.SetActive(false);
            lockIcon.SetActive(false);

            if (previewTrail == trail.id)
                bgImage.color /* set it to 0, 255, 255 */ = new Color(0f, 1f, 1f);
            if (selectedTrail == trail.id)
                selectedIcon.SetActive(true);
        }
        else
        {
            price.text = trail.price.ToString();
            price.color = new Color(1f, 1f, 0f);
            lockIcon.SetActive(true);
            var trailColor = trailMeta.color;
            skinImage.color /* set it to 210, 210, 0 */ = new Color(trailColor.r * 0.8f, trailColor.g * 0.8f, trailColor.b * 0.8f);
        }
        
        skinImage.sprite = trailMeta.GetIcon();
    }

    public void OnClicked()
    {
        if (_isSkin)
        {
            if (PlayerStatsManager.HasSkinUnlocked(skinMeta.id))
                PlayerStatsManager.SetSelectedSkin(skinMeta.id);
        
            _shopController.SelectSkin(this);
        }
        else
        {
            if (PlayerStatsManager.HasTrailUnlocked(trailMeta.id))
                PlayerStatsManager.SetSelectedTrail(trailMeta.id);
            
            _shopController.SelectTrail(this);
        }
    }
    
    public void Select()
    {
        bgImage.color /* set it to 0, 255, 255 */ = _isSkin ? new Color(0f, 1f, 1f) : new Color(1f, 0f, 1f);
    }
    
    public void Deselect()
    {
        bgImage.color /* set it to 255, 255, 0 */ = _isSkin ? new Color(1f, 1f, 0f) : new Color(0f, 0f, 0f);
    }
}
