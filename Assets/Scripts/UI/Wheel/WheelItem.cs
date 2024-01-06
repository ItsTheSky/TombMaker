using UnityEngine;
using UnityEngine.EventSystems;

public class WheelItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public WheelPart parent;
    
    public void OnPointerEnter(PointerEventData eventData)
    {   
        parent.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        parent.OnPointerExit(eventData);
    }
    
}