using System;
using Unity.VisualScripting;
using UnityEngine;

public class DotBlock : Block
{ 
    public Animator Animator;
    public GameObject coinPrefab;
    
    // Coins
    [HideInInspector] public bool _isCoin;
    [HideInInspector] public Coin _coinScript;
    
    public override void OnCollisionEnter2D(Collision2D col)
    {
        // should never be called tho
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        CheckInit();
        
        if (col.gameObject.CompareTag("player") && !_playerScript.logic.editor)
        {
            gameObject.SetActive(false);
            _playerScript.logic.dots++;
            
            _playerScript.logic.AddLevelProgression(1);
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
        gameObject.SetActive(true);
        Animator.Play("DotIdle");

        if (_isCoin) {
            Destroy(_coinScript.gameObject);
            _isCoin = false;
        }
    }

    public override void Disable()
    {
        base.Disable();
        
        Animator.Play("Dot");
        
        CheckForCoin();
    }

    public override void OnPlayerDeath()
    {
        base.OnPlayerDeath();
        gameObject.SetActive(true);
        
        if (_isCoin) {
            Destroy(_coinScript.gameObject);
            _isCoin = false;
        }
        
        Animator.Play("Dot");
        CheckForCoin();
    }

    private void CheckForCoin()
    {
        var chance = Constants.BONUSES[0].GetValue(PlayerStatsManager.GetBonusLevel(0));
        var random = UnityEngine.Random.Range(0, 100);
        var isCoin = random < chance;
        
        if (isCoin && !_playerScript.logic.AllowEditor() && LogicScript.settings.levelIndex != -1)
        {
            _isCoin = true;
            gameObject.SetActive(false);
            
            var coin = Instantiate(coinPrefab, transform.position, Quaternion.identity);
            _coinScript = coin.GetComponent<Coin>();
            _coinScript.dotBlock = this;
        }
    }

    public override bool IsRotatableByRotor()
    {
        return false;
    }
}