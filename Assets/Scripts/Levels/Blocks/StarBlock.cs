using JetBrains.Annotations;
using UnityEngine;

public class StarBlock : Block
{

    [CanBeNull]
    public override string CanPlace()
    {
        //return _playerScript.logic.GetStarCount() <= 3 ? null : "Max 3 stars";
        return null;
    }
    
    public void OnTriggerEnter2D(Collider2D col)
    {
        CheckInit();
        
        if (col.gameObject.CompareTag("player") && !_playerScript.logic.editor)
        {
            AudioManager.instance.Play("StarCollect");
            gameObject.SetActive(false);
            _playerScript.logic.stars++;
            
            _playerScript.logic.AddLevelProgression(10);
        }
    }

    public override void Enable()
    {
        base.Enable();
        gameObject.SetActive(true);
        
        _animator.Play("StarStatic");
    }

    public override void Disable()
    {
        base.Disable();
        
        _animator.Play("StarShin");
    }

    public override void RenderSprite()
    {
        CheckInit();
        _renderer.sprite = _defaultSprite;
    }

    public override void OnPlayerDeath()
    {
        base.OnPlayerDeath();
        gameObject.SetActive(true);
        
        _animator.Play("StarShin");
    }
}