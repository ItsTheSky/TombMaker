using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Rotor : Block
{

    // delay in seconds
    public double delay = 1.5f;
    
    [HideInInspector] public bool hasPlayer;
    [HideInInspector] public Color _normalColor;

    public override void Init()
    {
        base.Init();

        options.Add(new BlockSettingController.SliderOption("delay", "Rotate Delay", "Delay between each rotation.",
            0.3f, 2f, false, (float) delay, value =>
            {
                delay = value;
            }));
    }

    public override void UpdateBlockColor(Color color)
    {
        base.UpdateBlockColor(color);
        _normalColor = color;

        /* var dict = new Dictionary<string, float>()
        {
            { "r", color.r },
            { "g", color.g },
            { "b", color.b },
            { "a", color.a }
        };

        foreach (var c in dict)
        {
            rotorIdleAnimation.SetCurve(
                "",
                typeof(SpriteRenderer),
                "m_Color." + c.Key,
                new AnimationCurve(new Keyframe(0, c.Value, 0, 0))
            );

            rotorTurnAnimation.SetCurve(
                "",
                typeof(SpriteRenderer),
                "m_Color." + c.Key,
                new AnimationCurve(new Keyframe(0, c.Key == "r" ? 0 : 1, 0, 0),
                    new Keyframe(0.25f, c.Value, 0, 0))
            );
            
            rotorInvalidAnimation.SetCurve(
                "",
                typeof(SpriteRenderer),
                "m_Color." + c.Key,
                new AnimationCurve(new Keyframe(0, c.Key == "a" ? 1 : (c.Key != "r" ? 0 : 1), 0, 0),
                    new Keyframe(0.5f, c.Value, 0, 0))
            );
        } */
    }

    public override Dictionary<string, object> SaveSettings()
    {
        var settings = base.SaveSettings();
        settings.Add("delay", delay);
        return settings;
    }
    
    public override void LoadSettings(Dictionary<string, object> settings)
    {
        base.LoadSettings(settings);
        
        try
        {
            delay = settings.ContainsKey("delay") ? (double)settings["delay"] : delay;
        }
        catch (Exception e)
        {
            delay = settings.ContainsKey("delay") ? (long) settings["delay"] : delay;
        }
    }

    private bool _shouldRotate;
    private List<GameObject> _attachedBlocks;
    private readonly Dictionary<GameObject, float> _attachedBlocksRotation = new ();
    private int _lastRotation;
    private bool _waitingFirst = true;
    private Coroutine _rotateCoroutine;
    
    public IEnumerator<WaitForSeconds> Rotate()
    {
        while (_shouldRotate)
        {
            if (!_waitingFirst)
            {
                NormalFlash();
                AudioManager.PlaySound("RotorRotate");
                
                
                transform.Rotate(0, 0, 90);
                _lastRotation += 90;
                if (_lastRotation >= 360) _lastRotation = 0;
                
                // add player
                if (hasPlayer)
                {
                    _playerScript.DisableTrail();
                    var currentRotation = _playerScript.rotation;
                    _playerScript.transform.RotateAround(transform.position, Vector3.forward, -90);
                    currentRotation = currentRotation == 1 ? 4 : currentRotation - 1;
                    
                    _playerScript.RotateAndMove(currentRotation);
                    _playerScript.attachedRotor = this;
                    hasPlayer = true;
                    _playerScript.EnableTrail();
                }
            
                // rotate attached blocks
                foreach (var block in _attachedBlocks) {
                    block.transform.RotateAround(transform.position, Vector3.forward, -90);
                    var blockScript = block.GetComponent<Block>();
                    if (blockScript != null)
                        blockScript.RotorRotation(_lastRotation);
                }
            } else 
                _waitingFirst = false;
            
            yield return new WaitForSeconds((float) delay);
        }
    }
    
    private static readonly int Limit = 30;
    private Coroutine _invalidFlashCoroutine;
    public override void Disable()
    {
        _shouldRotate = true;
        _lastRotation = 0;
        _attachedBlocks = GetBlockAttachedTo(Limit);
        _attachedBlocks.Remove(gameObject);

        _attachedBlocksRotation.Clear();
        foreach (var block in _attachedBlocks)
        {
            if (block.GetComponent<Rotor>() != null)
            {
                if (_invalidFlashCoroutine is not null)
                    StopCoroutine(_invalidFlashCoroutine);
                
                _invalidFlashCoroutine = StartCoroutine(InvalidFlash());
                return;
            }
            var blockScript = block.GetComponent<Block>();
            if (blockScript is null)
                continue;
            
            if (!blockScript.IsRotatableByRotor())
                continue;

            _attachedBlocksRotation.Add(block, block.transform.rotation.z);
            blockScript.attachedRotor = this;
        }
        
        if (_rotateCoroutine is not null)
            StopCoroutine(_rotateCoroutine);
        _rotateCoroutine = StartCoroutine(Rotate());
    }

    public override void Enable()
    {
        _shouldRotate = false;
        _waitingFirst = true;
        transform.rotation = Quaternion.identity;
        
        if (_invalidFlashCoroutine is not null)
            StopCoroutine(_invalidFlashCoroutine);

        if (_rotateCoroutine != null)
        {
            StopCoroutine(_rotateCoroutine);
            _rotateCoroutine = null;
        }
        
        // rotate back attached blocks
        foreach (var block in _attachedBlocks) {
            block.transform.RotateAround(transform.position, Vector3.forward, _lastRotation);
            if (_attachedBlocksRotation.ContainsKey(block)) 
                block.transform.rotation = Quaternion.Euler(0, 0, _attachedBlocksRotation[block]);
            
            var blockScript = block.GetComponent<Block>();
            if (blockScript != null) {
                blockScript.attachedRotor = null;
                blockScript.ResetRotorRotation();
            }
        }
        
        _attachedBlocks.Clear();
        _attachedBlocksRotation.Clear();
        
        _renderer.color = _normalColor;
    }

    public override void OnCollisionEnter2D(Collision2D col)
    {
        base.OnCollisionEnter2D(col);
        
        if (col.gameObject.GetComponent<PlayerScript>() != null)
            hasPlayer = true;
    }
    
    // function to instantly switch from _normalColor to cyan color, and fade back to _normalColor in 0.5s (WITHOUT ANIMATOR!!!)
    private IEnumerator<WaitForSeconds> Flash(Color to, float duration,
        Action callback = null)
    {
        // first instant switch
        _renderer.color = to;
        // then fade back
        float time = 0;
        Color startColor = to;
        while (time < duration)
        {
            _renderer.color = Color.Lerp(startColor, _normalColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        _renderer.color = _normalColor;
        callback?.Invoke();
    }
    
    public void NormalFlash()
    {
        StartCoroutine(Flash(Color.cyan, 0.4f));
    }
    
    // invalid flash coroutine repeating infinitely
    public IEnumerator InvalidFlash()
    {
        var duration = 0.5f;
        // first instant switch
        _renderer.color = Color.red;
        // then fade back
        float time = 0;
        Color startColor = Color.red;
        while (time < duration)
        {
            _renderer.color = Color.Lerp(startColor, _normalColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        _renderer.color = _normalColor;
        yield return new WaitForSeconds(0.5f);
        _invalidFlashCoroutine = StartCoroutine(InvalidFlash());
    }
}