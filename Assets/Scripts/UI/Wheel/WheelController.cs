using System;
using UnityEngine;

public class WheelController : MonoBehaviour
{

    public WheelPart[] wheelParts = new WheelPart[12];
    public WheelPart lastPart;

    private void Start()
    {
        var sprites = Resources.LoadAll<Sprite>("blocks/full");
        
        foreach (WheelPart wheelPart in wheelParts)
        {
            if (wheelPart != null)
            {
                wheelPart.controller = this;
                wheelPart.ChangeSprite(sprites[wheelPart.type]);
            }
        }
    }
    
    public void OnWheelPartEnter(WheelPart wheelPart)
    {
        wheelPart.image.color = new Color(0.0f, 1.0f, 1.0f, 1.0f);
        lastPart = wheelPart;
    }
    
    public void OnWheelPartExit(WheelPart wheelPart)
    {
        wheelPart.image.color = new Color(1.0f, 1.0f, 0.0f, 1.0f);
        lastPart = null;
    }

    public int GetLastType()
    {
        return lastPart == null ? 0 : lastPart.type;
    }
}