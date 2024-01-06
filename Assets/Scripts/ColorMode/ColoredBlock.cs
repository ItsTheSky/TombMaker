using System;
using UnityEngine;

public class ColoredBlock : MonoBehaviour
{ 
    [Header("Borders")]
    public GameObject topBorderSprite;
    public GameObject bottomBorderSprite;
    public GameObject leftBorderSprite;
    public GameObject rightBorderSprite;
    
    [Header("Values")]
    public SpriteRenderer spriteRenderer;
    public ColorLogic colorLogic;
    public Vector2Int pos;
    
    public void Init(Vector2Int pos, Color color, ColorLogic colorLogic)
    {
        this.colorLogic = colorLogic;
        this.pos = pos;
        
        transform.position = new Vector3(pos.x, pos.y, 5); // behind everything
        spriteRenderer.color = color;
        
        CheckBorders();
    }

    private void CheckBorders()
    {
        //var top = colorLogic.logic.GetBlock(new Vector2(pos.x, pos.y + 1))?.GetBlock().isSolid;
        var top = colorLogic.colorBlocks.ContainsKey(new Vector2Int(pos.x, pos.y + 1));
        var bottom = colorLogic.colorBlocks.ContainsKey(new Vector2Int(pos.x, pos.y - 1));
        var left = colorLogic.colorBlocks.ContainsKey(new Vector2Int(pos.x - 1, pos.y));
        var right = colorLogic.colorBlocks.ContainsKey(new Vector2Int(pos.x + 1, pos.y));

        topBorderSprite.SetActive(!top);
        bottomBorderSprite.SetActive(!bottom);
        leftBorderSprite.SetActive(!left);
        rightBorderSprite.SetActive(!right);
    }

    private void Update()
    {
        CheckBorders();
    }
}