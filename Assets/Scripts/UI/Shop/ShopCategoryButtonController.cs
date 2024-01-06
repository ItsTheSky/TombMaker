using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCategoryButtonController : MonoBehaviour
{
    public string category;
    
    public Button button;
    [HideInInspector] public ShopController shopController;
    public TMP_Text btnText;

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }
    
    public void Init(ShopController shopController)
    {
        this.shopController = shopController;
        this.btnText.text = category;
    }
    
    public void OnClick()
    {
        shopController.SwitchCategory(category);
    }

    public void Select()
    {
        btnText.color = new Color(1, 0, 1, 1);
    }
    
    public void Deselect()
    {
        btnText.color = new Color(1, 1, 0, 1);
    }
}