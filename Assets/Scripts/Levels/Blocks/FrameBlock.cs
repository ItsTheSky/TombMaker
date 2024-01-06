using System.Collections.Generic;
using UnityEngine;

public class FrameBlock : MonoBehaviour
{
    
    public SpriteRenderer spriteRenderer;

    public int frameType; // 0: corner, 1: wall, 2: inner
    public int rotation; // 0: u-l, 1: u-r, 2: d-r, 3: d-l --- 0: u, 1: r, 2: d, 3: l (for wall)

    public AnimationClip shiningClip;

    public static Dictionary<int, Dictionary<int, FrameBlock[]>> cached = new();
    private static Dictionary<string, Sprite[]> cachedSkins = new();

    public void UpdateSprite(string skinName, Color color)
    {
        var variants = Constants.GetVariants(skinName);
        if (!cachedSkins.ContainsKey(skinName))
            cachedSkins.Add(skinName, Resources.LoadAll<Sprite>("blocks/" + skinName));
        var sprites = cachedSkins[skinName];
        var randomVariant = Mathf.RoundToInt(Random.Range(0, variants));
        
        var sprite = sprites[frameType * 4 + rotation + (12 * randomVariant)];
     
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = GetParent()?.type == 1 ? Color.cyan : color;
    }

    public void UpdateSprite(string skinName, Constants.BlockColor color)
    {
        UpdateSprite(skinName, color.color);
    }

    public FullFrameBlock GetParent()
    {
        return transform.parent.GetComponent<FullFrameBlock>();
    }

    /**
     * Will basically create multiple "small" FrameBlock (they're half the size of a normal block)
     * so to fit the size of the FullFrameBlock. For instance, frameType = 0 and rotation = 0 will
     * be top-left corner of the FullFrameBlock, so we'll have to create:
     * - 1 FrameBlock with frameType = 0 (corner) and rotation = 0 (up-left) for the top left corner, with x and y mod of 0
     * - 1 FrameBlock with frameType = 1 (wall) and rotation = 0 (up) for the top wall, with x mod of 1 and y mod of 0
     * - 1 FrameBlock with frameType = 1 (wall) and rotation = 3 (left) for the left wall, with x mod of 0 and y mod of 1
     */
    public static FrameBlock[] GenerateBlocks(GameObject parent, int frameType, int rotation)
    {
        var frameBlocks = new FrameBlock[4]; // 4 is the max number of FrameBlock we can have for a FullFrameBlock

        if (frameType == 0) // corners
        {
            
            if (rotation == 0) { // top-left corner
                frameBlocks[0] = CreateBlock(parent, 0, 0); // top-left corner (offset: 0, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 0, 1); // top wall (offset: 1, 0)
                frameBlocks[2] = CreateBlock(parent, 1, 3, 0, 1); // left wall (offset: 0, 1)
            } else if (rotation == 1) { // top-right corner
                frameBlocks[0] = CreateBlock(parent, 0, 1, 1, 0); // top-left corner (offset: 1, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 0, 0, 0); // top wall (offset: 0, 0)
                frameBlocks[2] = CreateBlock(parent, 1, 1, 1, 1); // right wall (offset: 1, 1)
            } else if (rotation == 2) { // bottom-right corner
                frameBlocks[0] = CreateBlock(parent, 0, 2, 1, 1); // top-left corner (offset: 1, 1)
                frameBlocks[1] = CreateBlock(parent, 1, 2, 0, 1); // left wall (offset: 0, 1)
                frameBlocks[2] = CreateBlock(parent, 1, 1, 1, 0); // right wall (offset: 1, 0)
            } else if (rotation == 3) { // bottom-left corner
                frameBlocks[0] = CreateBlock(parent, 0, 3, 0, 1); // top-left corner (offset: 0, 1)
                frameBlocks[1] = CreateBlock(parent, 1, 2, 1, 1); // left wall (offset: 1, 1)
                frameBlocks[2] = CreateBlock(parent, 1, 3, 0, 0); // bottom wall (offset: 0, 0)
            }
            
        } else if (frameType == 1) // wall
        {

            if (rotation == 0) // up
            {
                frameBlocks[0] = CreateBlock(parent, 1, 0, 0, 0); // up wall (offset: 0, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 0, 1, 0); // up wall (offset: 1, 0)
            } else if (rotation == 1) // right
            {
                frameBlocks[0] = CreateBlock(parent, 1, 1, 1, 0); // right wall (offset: 1, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 1, 1, 1); // right wall (offset: 1, 1)
            } else if (rotation == 2) // down
            {
                frameBlocks[0] = CreateBlock(parent, 1, 2, 0, 1); // down wall (offset: 0, 1)
                frameBlocks[1] = CreateBlock(parent, 1, 2, 1, 1); // down wall (offset: 1, 1)
            } else if (rotation == 3) // left
            {
                frameBlocks[0] = CreateBlock(parent, 1, 3, 0, 0); // left wall (offset: 0, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 3, 0, 1); // left wall (offset: 0, 1)
            }
            
        } else if (frameType == 2) // inner
        {
            
            if (rotation == 0) 
                frameBlocks[0] = CreateBlock(parent, 2, 0, 0, 0); // inner (offset: 0, 0)
            else if (rotation == 1)
                frameBlocks[0] = CreateBlock(parent, 2, 1, 1, 0); // inner (offset: 1, 0)
            else if (rotation == 2)
                frameBlocks[0] = CreateBlock(parent, 2, 2, 1, 1); // inner (offset: 1, 1)
            else if (rotation == 3)
                frameBlocks[0] = CreateBlock(parent, 2, 3, 0, 1); // inner (offset: 0, 1)
            
        } else if (frameType == 3) // 'U' shape
        {
            if (rotation == 0)
            {
                frameBlocks[0] = CreateBlock(parent, 0, 0, 0, 0); // top-left corner (offset: 0, 0)
                frameBlocks[1] = CreateBlock(parent, 0, 1, 1, 0); // top-right corner (offset: 1, 0)
                frameBlocks[2] = CreateBlock(parent, 1, 3, 0, 1); // left wall (offset: 0, 1)
                frameBlocks[3] = CreateBlock(parent, 1, 1, 1, 1); // right wall (offset: 1, 1)
            } else if (rotation == 1)
            {
                frameBlocks[0] = CreateBlock(parent, 0, 1, 1, 0); // top-right corner (offset: 1, 0)
                frameBlocks[1] = CreateBlock(parent, 0, 2, 1, 1); // bottom-right corner (offset: 1, 1)
                frameBlocks[2] = CreateBlock(parent, 1, 0, 0, 0); // top wall (offset: 0, 0)
                frameBlocks[3] = CreateBlock(parent, 1, 2, 0, 1); // down wall (offset: 0, 1)
            } else if (rotation == 2)
            {
                frameBlocks[0] = CreateBlock(parent, 0, 2, 1, 1); // bottom-right corner (offset: 1, 1)
                frameBlocks[1] = CreateBlock(parent, 0, 3, 0, 1); // bottom-left corner (offset: 0, 1)
                frameBlocks[2] = CreateBlock(parent, 1, 1, 1, 0); // right wall (offset: 1, 1)
                frameBlocks[3] = CreateBlock(parent, 1, 3, 0, 0); // left wall (offset: 0, 0)
            } else if (rotation == 3)
            {
                frameBlocks[0] = CreateBlock(parent, 0, 3, 0, 1); // bottom-left corner (offset: 0, 1)
                frameBlocks[1] = CreateBlock(parent, 0, 0, 0, 0); // top-left corner (offset: 0, 0)
                frameBlocks[2] = CreateBlock(parent, 1, 2, 1, 1); // down wall (offset: 1, 1)
                frameBlocks[3] = CreateBlock(parent, 1, 0, 1, 0); // top wall (offset: 0, 0)
            }

        } else if (frameType == 4) // corner but with the inner included
        {

            if (rotation == 0)
            {
                frameBlocks[0] = CreateBlock(parent, 0, 0, 0, 0); // top-left corner (offset: 0, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 0, 1, 0); // top wall (offset: 1, 0)
                frameBlocks[2] = CreateBlock(parent, 1, 3, 0, 1); // left wall (offset: 0, 1)
                frameBlocks[3] = CreateBlock(parent, 2, 2, 1, 1); // inner (offset: 1, 1)
            } else if (rotation == 1)
            {
                frameBlocks[0] = CreateBlock(parent, 0, 1, 1, 0); // top-right corner (offset: 1, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 0, 0, 0); // top wall (offset: 0, 0)
                frameBlocks[2] = CreateBlock(parent, 1, 1, 1, 1); // right wall (offset: 1, 1)
                frameBlocks[3] = CreateBlock(parent, 2, 3, 0, 1); // inner (offset: 0, 1)
            } else if (rotation == 2)
            {
                frameBlocks[0] = CreateBlock(parent, 0, 2, 1, 1); // bottom-right corner (offset: 1, 1)
                frameBlocks[1] = CreateBlock(parent, 1, 2, 0, 1); // left wall (offset: 0, 1)
                frameBlocks[2] = CreateBlock(parent, 1, 1, 1, 0); // right wall (offset: 1, 0)
                frameBlocks[3] = CreateBlock(parent, 2, 0, 0, 0); // inner (offset: 0, 0)
            } else if (rotation == 3)
            {
                frameBlocks[0] = CreateBlock(parent, 0, 3, 0, 1); // bottom-left corner (offset: 0, 1)
                frameBlocks[1] = CreateBlock(parent, 1, 2, 1, 1); // left wall (offset: 1, 1)
                frameBlocks[2] = CreateBlock(parent, 1, 3, 0, 0); // bottom wall (offset: 0, 0)
                frameBlocks[3] = CreateBlock(parent, 2, 1, 1, 0); // inner (offset: 1, 0)
            }

        } else if (frameType == 5) // 'tunnel', two wall
        {

            if (rotation == 0)
            {
                frameBlocks[0] = CreateBlock(parent, 1, 0, 0, 0); // up wall (offset: 0, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 0, 1, 0); // up wall (offset: 1, 0)
                frameBlocks[2] = CreateBlock(parent, 1, 2, 0, 1); // left wall (offset: 0, 1)
                frameBlocks[3] = CreateBlock(parent, 1, 2, 1, 1); // left wall (offset: 1, 1)
            } else if (rotation == 1)
            {
                frameBlocks[0] = CreateBlock(parent, 1, 1, 1, 0); // right wall (offset: 1, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 1, 1, 1); // right wall (offset: 1, 1)
                frameBlocks[2] = CreateBlock(parent, 1, 3, 0, 0); // down wall (offset: 0, 0)
                frameBlocks[3] = CreateBlock(parent, 1, 3, 0, 1); // down wall (offset: 0, 1)
            } else if (rotation == 2) // down
            {
                frameBlocks[0] = CreateBlock(parent, 1, 2, 0, 1); // down wall (offset: 0, 1)
                frameBlocks[1] = CreateBlock(parent, 1, 2, 1, 1); // down wall (offset: 1, 1)
                frameBlocks[2] = CreateBlock(parent, 1, 0, 0, 0); // up wall (offset: 0, 0)
                frameBlocks[3] = CreateBlock(parent, 1, 0, 1, 0); // up wall (offset: 1, 0)
            } else if (rotation == 3) // left
            {
                frameBlocks[0] = CreateBlock(parent, 1, 3, 0, 0); // left wall (offset: 0, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 3, 0, 1); // left wall (offset: 0, 1)
                frameBlocks[2] = CreateBlock(parent, 1, 1, 1, 0); // right wall (offset: 1, 0)
                frameBlocks[3] = CreateBlock(parent, 1, 1, 1, 1); // right wall (offset: 1, 1)
            }
            
        } else if (frameType == 6) // Wall with two corners
        {

            if (rotation == 0)
            {
                frameBlocks[0] = CreateBlock(parent, 1, 0); // up wall (offset: 0, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 0, 1); // up wall (offset: 1, 0)
                frameBlocks[2] = CreateBlock(parent, 2, 3, 0, 1); // bottom-left corner (offset: 0, 1)
                frameBlocks[3] = CreateBlock(parent, 2, 2, 1, 1); // bottom-right corner (offset: 1, 1)
            } else if (rotation == 1)
            {
                frameBlocks[0] = CreateBlock(parent, 1, 1, 1, 0); // right wall (offset: 1, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 1, 1, 1); // right wall (offset: 1, 1)
                frameBlocks[2] = CreateBlock(parent, 2, 0, 0, 0); // top-left corner (offset: 0, 0)
                frameBlocks[3] = CreateBlock(parent, 2, 3, 0, 1); // bottom-left corner (offset: 0, 1)
            } else if (rotation == 2)
            {
                frameBlocks[0] = CreateBlock(parent, 1, 2, 0, 1); // down wall (offset: 0, 1)
                frameBlocks[1] = CreateBlock(parent, 1, 2, 1, 1); // down wall (offset: 1, 1)
                frameBlocks[2] = CreateBlock(parent, 2, 1, 1, 0); // top-right corner (offset: 1, 0)
                frameBlocks[3] = CreateBlock(parent, 2, 0, 0, 0); // top-left corner (offset: 0, 0)
            } else if (rotation == 3)
            {
                frameBlocks[0] = CreateBlock(parent, 1, 3, 0, 0); // left wall (offset: 0, 0)
                frameBlocks[1] = CreateBlock(parent, 1, 3, 0, 1); // left wall (offset: 0, 1)
                frameBlocks[2] = CreateBlock(parent, 2, 2, 1, 1); // bottom-right corner (offset: 1, 1)
                frameBlocks[3] = CreateBlock(parent, 2, 1, 1, 0); // top-right corner (offset: 1, 0)
            }
            
        } else if (frameType == 7) // Two opposite inners
        {

            if (rotation == 0)
            {
                frameBlocks[0] = CreateBlock(parent, 2, 0); // top-left inner (offset: 0, 0)
                frameBlocks[1] = CreateBlock(parent, 2, 2, 1, 1); // bottom-right inner (offset: 1, 1)
            }  else if (rotation == 1)
            {
                frameBlocks[0] = CreateBlock(parent, 2, 1, 1, 0); // top-right inner (offset: 1, 0)
                frameBlocks[1] = CreateBlock(parent, 2, 3, 0, 1); // bottom-left inner (offset: 0, 1)
            } else if (rotation == 2)
            {
                frameBlocks[0] = CreateBlock(parent, 2, 2, 1, 1); // bottom-right inner (offset: 1, 1)
                frameBlocks[1] = CreateBlock(parent, 2, 0, 0, 0); // top-left inner (offset: 0, 0)
            } else if (rotation == 3)
            {
                frameBlocks[0] = CreateBlock(parent, 2, 3, 0, 1); // bottom-left inner (offset: 0, 1)
                frameBlocks[1] = CreateBlock(parent, 2, 1, 1, 0); // top-right inner (offset: 1, 0)
            }
            
        } else if (frameType == 8) // 3 inners
        {

            if (rotation == 0)
            {
                frameBlocks[0] = CreateBlock(parent, 2, 0); // top-left inner (offset: 0, 0)
                frameBlocks[1] = CreateBlock(parent, 2, 1, 1, 0); // top-left inner (offset: 1, 0) 
                frameBlocks[2] = CreateBlock(parent, 2, 2, 1, 1); // bottom-right inner (offset: 1, 1)
            } else if (rotation == 1)
            {
                frameBlocks[0] = CreateBlock(parent, 2, 1, 1, 0); // top-right inner (offset: 1, 0)
                frameBlocks[1] = CreateBlock(parent, 2, 2, 1, 1); // bottom-right inner (offset: 1, 1)
                frameBlocks[2] = CreateBlock(parent, 2, 3, 0, 1); // bottom-left inner (offset: 0, 1)
            } else if (rotation == 2)
            {
                frameBlocks[0] = CreateBlock(parent, 2, 2, 1, 1); // bottom-right inner (offset: 1, 1)
                frameBlocks[1] = CreateBlock(parent, 2, 3, 0, 1); // bottom-left inner (offset: 0, 1)
                frameBlocks[2] = CreateBlock(parent, 2, 0, 0, 0); // top-left inner (offset: 0, 0)
            } else if (rotation == 3)
            {
                frameBlocks[0] = CreateBlock(parent, 2, 3, 0, 1); // bottom-left inner (offset: 0, 1)
                frameBlocks[1] = CreateBlock(parent, 2, 0, 0, 0); // top-left inner (offset: 0, 0)
                frameBlocks[2] = CreateBlock(parent, 2, 1, 1, 0); // top-right inner (offset: 1, 0)
            }

        } else if (frameType == 9) // 4 inners
        {
            
            frameBlocks[0] = CreateBlock(parent, 2, 0); // top-left inner (offset: 0, 0)
            frameBlocks[1] = CreateBlock(parent, 2, 1, 1, 0); // top-right inner (offset: 1, 0)
            frameBlocks[2] = CreateBlock(parent, 2, 2, 1, 1); // bottom-right inner (offset: 1, 1)
            frameBlocks[3] = CreateBlock(parent, 2, 3, 0, 1); // bottom-left inner (offset: 0, 1)
            
        } else if (frameType == 10) // full block
        {
            
            frameBlocks[0] = CreateBlock(parent, 0, 0, 0, 0); // top-left corner (offset: 0, 0)
            frameBlocks[1] = CreateBlock(parent, 0, 1, 1, 0); // top-right corner (offset: 1, 0)
            frameBlocks[2] = CreateBlock(parent, 0, 2, 1, 1); // bottom-right corner (offset: 1, 1)
            frameBlocks[3] = CreateBlock(parent, 0, 3, 0, 1); // bottom-left corner (offset: 0, 1)
            
        } else if (frameType == 11) // 2 inners on the same side
        {

            if (rotation == 0)
            {
                frameBlocks[0] = CreateBlock(parent, 2, 0); // top-left inner (offset: 0, 0)
                frameBlocks[1] = CreateBlock(parent, 2, 1, 1, 0); // top-right inner (offset: 1, 0)
            }
            else if (rotation == 1)
            {
                frameBlocks[0] = CreateBlock(parent, 2, 1, 1, 0); // top-right inner (offset: 1, 0)
                frameBlocks[1] = CreateBlock(parent, 2, 2, 1, 1); // bottom-right inner (offset: 1, 1)
            }
            else if (rotation == 2)
            {
                frameBlocks[0] = CreateBlock(parent, 2, 2, 1, 1); // bottom-right inner (offset: 1, 1)
                frameBlocks[1] = CreateBlock(parent, 2, 3, 0, 1); // bottom-left inner (offset: 0, 1)
            }
            else if (rotation == 3)
            {
                frameBlocks[0] = CreateBlock(parent, 2, 3, 0, 1); // bottom-left inner (offset: 0, 1)
                frameBlocks[1] = CreateBlock(parent, 2, 0, 0, 0); // top-left inner (offset: 0, 0)
            }

        }

        // remove null values
        var frameBlocksList = new List<FrameBlock>();
        foreach (var frameBlock in frameBlocks)
            if (frameBlock != null)
                frameBlocksList.Add(frameBlock);
        
        return frameBlocksList.ToArray();
    }

    private static GameObject prefab;
    private static FrameBlock CreateBlock(GameObject parent, int type, int rotation, float x = 0, float y = 0)
    {
        if (prefab == null)
            prefab = Resources.Load<GameObject>("FrameBlock");
        
        var frameBlock = Instantiate(prefab, parent.transform).GetComponent<FrameBlock>();
        frameBlock.frameType = type;
        frameBlock.rotation = rotation;
        frameBlock.UpdateSprite("bricks", Constants.GetBlockColorById(0)); 
        frameBlock.transform.localPosition = new Vector3(x * 0.05f - 0.05f, y * - 0.05f + 0.05f, 0);
        return frameBlock;
    }
}