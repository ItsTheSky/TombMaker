using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FrameBlockUnit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    
    [Header("References")]
    public GameObject selectedOverlay;
    public Image tintedSprite;
    public Image iconSprite;
    
    [Header("Values")]
    public int frameBlock;
    public FrameBlockSelectorController frameBlockSelectorController;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        var color = tintedSprite.color;
        tintedSprite.color = new Color(color.r, color.g, color.b, 0.8f);
        
        frameBlockSelectorController.FrameBlockEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var color = tintedSprite.color;
        tintedSprite.color = new Color(color.r, color.g, color.b, 1f);
    }
    
    public void Select()
    {
        frameBlockSelectorController.selectedFrameBlockUnit = this;
        selectedOverlay.SetActive(true);
    }
    
    public void Deselect()
    {
        frameBlockSelectorController.selectedFrameBlockUnit = null;
        selectedOverlay.SetActive(false);
    }

    public void RefreshSprite(Sprite[] sprites)
    {
        this.iconSprite.sprite = sprites[frameBlock];
    }
}