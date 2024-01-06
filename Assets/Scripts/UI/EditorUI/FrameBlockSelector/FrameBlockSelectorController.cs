using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FrameBlockSelectorController : MonoBehaviour
{
    
    [Header("References")]
    public GameObject frameBlockUnitPrefab;
    public Transform frameBlockUnitContainer;
    public int[] desiredFrameBlocks = new int[9];

    [Header("Values")] 
    public readonly Dictionary<int, FrameBlockUnit> frameBlockUnits = new ();
    public FrameBlockUnit lastHoveredFrameBlockUnit;
    public FrameBlockUnit selectedFrameBlockUnit;
    
    private bool _initialized = false;
    public void Init()
    {
        if (_initialized)
            return;
        _initialized = true;
        
        
        var sprites = Resources.LoadAll<Sprite>("blocks/full");
        
        for (var i = 0; i < desiredFrameBlocks.Length; i++)
        {
            var frameBlockUnit = Instantiate(frameBlockUnitPrefab, frameBlockUnitContainer).GetComponent<FrameBlockUnit>();
            if (frameBlockUnit == null)
            {
                Debug.LogError("FrameBlockUnit prefab does not have FrameBlockUnit component!");
                return;
            }
            
            frameBlockUnit.frameBlock = desiredFrameBlocks[i];
            frameBlockUnit.RefreshSprite(sprites);
            frameBlockUnit.frameBlockSelectorController = this;
            frameBlockUnits[i] = frameBlockUnit;
        }
    }
    
    public void FrameBlockEnter(FrameBlockUnit frameBlockUnit)
    {
        lastHoveredFrameBlockUnit = frameBlockUnit;
    }
    
    public int GetSelectedFrameBlock()
    {
        if (lastHoveredFrameBlockUnit == null)
            return selectedFrameBlockUnit == null ? -1 : selectedFrameBlockUnit.frameBlock;
        
        return lastHoveredFrameBlockUnit.frameBlock;
    }

    public void OnPanelClose()
    {
        selectedFrameBlockUnit?.Deselect();
        
        selectedFrameBlockUnit = lastHoveredFrameBlockUnit;
        selectedFrameBlockUnit?.Select();
        
        lastHoveredFrameBlockUnit = null;
        lastHoveredFrameBlockUnit?.OnPointerExit(null);
    }

    public void OnPanelOpen(int frameBlock)
    {
        if (selectedFrameBlockUnit == null)
        {
            var frameBlockUnit = frameBlockUnits.First(pair => pair.Value.frameBlock == frameBlock).Value;
            frameBlockUnit.Select();
            selectedFrameBlockUnit = frameBlockUnit;
        }
        
        selectedFrameBlockUnit?.OnPointerExit(null);
    }
    
    public void RecolorAll(Color color)
    {
        foreach (var frameBlockUnit in frameBlockUnits.Values)
        {
            frameBlockUnit.tintedSprite.color = color;
        }
    }
}