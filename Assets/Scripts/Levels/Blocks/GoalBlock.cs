using UnityEngine;

public class GoalBlock : Block
{
    public Animator Animator;
    
    public override void OnCollisionEnter2D(Collision2D col)
    {
        // should never be called tho
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        CheckInit();
        
        if (col.gameObject.CompareTag("player"))
        {
            _playerScript.logic.WinLevel();
        }
    }

    public override void RenderSprite()
    {
        CheckInit();
        _renderer.sprite = _defaultSprite;
    }

    public override void Enable()
    {
        base.Enable();
        
        Animator.Play("GoalIdle");
    }
    
    public override void Disable()
    {
        base.Disable();
        
        Animator.Play("Goal");
    }

    public override bool ShouldBeDesactivatedInCurrentState()
    {
        return EditorValues.LogicScript.levelSettings.levelType == LevelType.Color;
    }
}