using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class WheelPart : MonoBehaviour
{

    public int type;
    
    public Image image;
    public WheelController controller;
    public Image icon;

    public void OnPointerEnter(PointerEventData eventData)
    {
        controller.OnWheelPartEnter(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        controller.OnWheelPartExit(this);
    }

    public void ChangeSprite(Sprite sprite)
    {
        icon.sprite = sprite;
    }
}