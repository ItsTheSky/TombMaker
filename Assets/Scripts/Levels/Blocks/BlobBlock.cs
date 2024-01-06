using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BlobBlock : Block
{
    public Animator animator;
    public long delay = 5;
    
    private bool _shouldAttack;
    private Color _normalColor;

    public override void UpdateBlockColor(Color color)
    {
        base.UpdateBlockColor(color);
        _normalColor = color;
        _renderer.color = color;
    }

    public override void Init()
    {
        base.Init();
        options.Add(new BlockSettingController.SliderOption("delay",
            "Attack Delay", "Change the amount of time between each attack.",
            5, 15, true, delay, value => delay = (int) value));
    }

    public override void Enable()
    {
        CheckInit();
        animator.Play("BlobStatic");
        _renderer.color = _normalColor;
        gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        _shouldAttack = false;
    }

    public override void Disable()
    {
        CheckInit();
        animator.Play("BlobIdle");
        _renderer.color = _normalColor;

        _shouldAttack = true;
    }

    // ReSharper disable Unity.PerformanceAnalysis
    public void Attack()
    {
        CheckPlayerAround();
        animator.Play("BlobAttack");
        _renderer.color = Color.cyan;
        StartCoroutine(ResetColor());
    }
    
    private IEnumerator ResetColor()
    {
        yield return new WaitForSeconds(0.70f);
        _renderer.color = _normalColor;
    }

    // Check if there's the player in 3x3 area around the block.
    public void CheckPlayerAround()
    {
        Vector2 playerPos = _playerScript.GetPos();
        Vector2 blockPos = _pos;
        
        if (playerPos.x >= blockPos.x - 1 && playerPos.x <= blockPos.x + 1 &&
            playerPos.y >= blockPos.y - 1 && playerPos.y <= blockPos.y + 1)
        {
            _playerScript.Die();
        }
        
    }
    
    public bool IsAttacking()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("BlobAttack");
    }

    private int _occuredLoops;
    private void FixedUpdate()
    {
        if (!_shouldAttack)
            return;
        
        _occuredLoops++;
        if (_occuredLoops < delay * 10)
            return;
        
        _occuredLoops = 0;
        Attack();
    }
    public override bool isSpike => IsAttacking();

    public override void LoadSettings(Dictionary<string, object> settings)
    {
        delay = (settings.ContainsKey("delay") ? (long) settings["delay"] : 2);
    }
    
    public override Dictionary<string, object> SaveSettings()
    {
        var dict = new Dictionary<string, object>();
        
        dict.Add("delay", delay);
        
        return dict;
    }

    public override bool IsRotatableByRotor()
    {
        return false;
    }
}