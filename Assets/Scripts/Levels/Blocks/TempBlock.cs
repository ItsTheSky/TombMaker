using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempBlock : Block
{

    public new Collider2D collider2D;
    public Animator animator;
    private Color _normalColor;

    [Header("Animations")] public AnimationClip tempBlockConstructedAnimation;

    [HideInInspector] public long delay;

    public override void UpdateBlockColor(Color color)
    {
        base.UpdateBlockColor(color);
        _normalColor = color;
        /* 
        var dict = new Dictionary<string, float>()
        {
            { "r", color.r },
            { "g", color.g },
            { "b", color.b },
            { "a", color.a }
        };
        
        foreach (var c in dict)
        {
            tempBlockConstructedAnimation.SetCurve(
                "",
                typeof(SpriteRenderer),
                "m_Color." + c.Key,
                new AnimationCurve(new Keyframe(0, c.Value, 0, 0))
            );
        } */
    }

    public override void Init()
    {
        base.Init();
        
        options.Add(new BlockSettingController.SliderOption("portal_id",
            "Portal ID", "This portal will lead to the other portal with the same ID.",
            10, 30, true, 10, value =>
            {
                delay = (long) value;
            }));
    }
    
    public override void Enable()
    {
        CheckInit();
        
        collider2D.isTrigger = true;
        _wasSolid = false;
        _waitedFrames = 0;
        
        SwitchToSolidTempBlock();
    }
    
    public override void Disable()
    {
        CheckInit();
        
        SwitchToTempBlock();
    }

    private int _waitedFrames;
    private bool _wasSolid;
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name != "Player")
            return;
        _wasSolid = true;
        _waitedFrames = 0;
        
        SwitchToSolidTempBlock();
        collider2D.isTrigger = false;
    }

    public override bool IsSolid()
    {
        return _wasSolid;
    }

    public void FixedUpdate()
    {
        if (!_wasSolid)
            return;
        
        _waitedFrames++;
        
        if (_waitedFrames >= delay * 500 * Time.deltaTime)
        {
            if (isPlayerGlued)
                return;
            if (HasPlayerSticked())
                return;
            
            _waitedFrames = 0;
            _wasSolid = false;
            collider2D.isTrigger = true;
            SwitchToTempBlock();
        }
    }

    public bool HasPlayerSticked()
    {
        Vector2 playerPos = _playerScript.GetPos();
        int rotation = _playerScript.rotation;

        // check below
        if (playerPos.x == _pos.x && playerPos.y == _pos.y - 1 && rotation == 3)
            return true;
        // check above
        if (playerPos.x == _pos.x && playerPos.y == _pos.y + 1 && rotation == 1)
            return true;
        // check left
        if (playerPos.x == _pos.x - 1 && playerPos.y == _pos.y && rotation == 2)
            return true;
        // check right
        if (playerPos.x == _pos.x + 1 && playerPos.y == _pos.y && rotation == 4)
            return true;
        
        return false;
    }

    public override Dictionary<string, object> SaveSettings()
    {
        return new Dictionary<string, object>()
        {
            {"delay", delay}
        };
    }

    public override void LoadSettings(Dictionary<string, object> settings)
    {
        delay = (settings.ContainsKey("delay") ? (long)settings["delay"] : 10);
    }
    
    public void SwitchToSolidTempBlock()
    {
        animator.Play("TempBlockStatic");
        _renderer.color = _normalColor;
    }
    
    public void SwitchToTempBlock()
    {
        animator.Play("TempBlock");
        _renderer.color = new Color(1, 1, 0, 1);
    }
}