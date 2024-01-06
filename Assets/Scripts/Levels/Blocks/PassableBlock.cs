using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassableBlock : Block
{

    public new Collider2D collider2D;

    private readonly List<FrameBlock> _frameBlocks = new();
    
    public override void Enable()
    {
        CheckInit();
        
        collider2D.isTrigger = true;
        SwitchToPassable();
        
        foreach (var frame in _frameBlocks)
            Destroy(frame.gameObject);
        _frameBlocks.Clear();
    }
    
    public override void Disable()
    {
        CheckInit();
        
        SwitchToPassable();

        if (_frameBlocks.Count == 0)
        {
            _frameBlocks.AddRange(FrameBlock.GenerateBlocks(gameObject, 10, 0));
            foreach (var frame in _frameBlocks) {
                frame.gameObject.SetActive(false);
                frame.UpdateSprite(EditorValues.LogicScript.levelSettings.GetStyle(), EditorValues.LogicScript.GetColor());
            }
        }
    }

    public override void UpdateBlockColor(Color color)
    {
        base.UpdateBlockColor(color);
        
        foreach (var frame in _frameBlocks)
            frame.UpdateSprite(EditorValues.LogicScript.levelSettings.GetStyle(), color);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name != "Player")
            return;
        
        collider2D.isTrigger = false;
        SwitchToSolid();
    }

    public override void OnPlayerDeath()
    {
        base.OnPlayerDeath();
        
        SwitchToPassable();
    }

    public void SwitchToSolid()
    {
        _renderer.enabled = false;
        collider2D.isTrigger = false;
        
        foreach (var frame in _frameBlocks)
            frame.gameObject.SetActive(true);
    }
    
    public void SwitchToPassable()
    {
        _renderer.enabled = true;
        collider2D.isTrigger = true;
        
        foreach (var frame in _frameBlocks)
            frame.gameObject.SetActive(false);
    }
}