using System;
using UnityEngine;

public class Switcher : Block
{
        
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer retainedSprite;

    public override void Init()
    {
        base.Init();
        
        uniqueLayer = true;
    }

    public override void Enable()
    {
        CheckInit();
        
        animator.Play("SwitcherStatic");
        retainedSprite.gameObject.SetActive(false);
        spriteRenderer.color = new Color(1, 1, 0, 1f);
    }
    
    public override void Disable()
    {
        CheckInit();
        
        animator.Play("Switcher");
    }

    private bool _hasPlayer;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.gameObject.GetComponent<PlayerScript>();
        if (player == null)
            return;

        if (_hasPlayer)
            return;
        
        _hasPlayer = true;
        spriteRenderer.color = new Color(0, 1, 1, 1f);
        player.Hide();
        player.StopMoving();
        player.rotation = 0;
        player.Teleport(transform.position);
        player.canMove = false;
        
        retainedSprite.gameObject.SetActive(true);
        
        StartCoroutine(EnablePlayerAfterDelay(player));
    }
    
    private System.Collections.IEnumerator EnablePlayerAfterDelay(PlayerScript player)
    {
        yield return new WaitForSeconds(0.1f);
        player.canMove = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.gameObject.GetComponent<PlayerScript>();
        if (player == null)
            return;

        if (!_hasPlayer)
            return;
        
        _hasPlayer = false;
        spriteRenderer.color = new Color(1, 1, 0, 1f);
        player.Show();
        
        retainedSprite.gameObject.SetActive(false);
    }
}