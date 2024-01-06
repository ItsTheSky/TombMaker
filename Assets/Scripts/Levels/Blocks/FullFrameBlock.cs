using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FullFrameBlock : Block
{
    public SpriteRenderer renderer;
    
    // Def of those in BlockPlacerScript.cs
    public int frameType;
    public int rotation;

    public int type; // 0: blocks, 1: spikes, 2: temp spikes

    private Color NormalColor => EditorValues.LogicScript.GetColor();
    private bool _shouldShine = false;

    public Color GetColor()
    {
        return type == 0 ? NormalColor : Color.cyan;
    }

    public List<FrameBlock> createdFramesBlock = new ();

    public override void Disable()
    {
        base.Disable();

        foreach (var frameBlock in createdFramesBlock)
        {
            var skin = "default";
            if (type == 0) 
                skin = _playerScript.logic.levelSettings.GetStyle();
            else if (type == 1)
                skin = "spikes";
            else if (type == 2)
                skin = "temp_spikes";
            
            frameBlock.UpdateSprite(skin, _playerScript.logic.GetColor());
        }
        renderer.enabled = false;
        renderer.transform.rotation = Quaternion.Euler(0, 0, 0);
        
        _shouldShine = type == 2;
    }

    public override Dictionary<string, object> SaveSettings()
    {
        var settings = base.SaveSettings();
        settings.Add("frameType", frameType);
        settings.Add("rotation", rotation);
        return settings;
    }

    public override bool isSpike => type == 1;

    public override void LoadSettings(Dictionary<string, object> settings)
    {
        base.LoadSettings(settings);
        try
        {
            frameType = (int)(settings.ContainsKey("frameType") ? (long)settings["frameType"] : 0);
            rotation = (int)(settings.ContainsKey("rotation") ? (long)settings["rotation"] : 0);

        }
        catch (Exception e)
        {
            frameType = (int)(settings.ContainsKey("frameType") ? (int)settings["frameType"] : 0);
            rotation = (int)(settings.ContainsKey("rotation") ? (int)settings["rotation"] : 0);
        }
    }

    public override void Enable()
    {
        base.Enable();
        
        foreach (var frameBlock in createdFramesBlock) Destroy(frameBlock.gameObject);
        renderer.enabled = true;
        
        foreach (var releaseSpikes in _releaseSpikesDict) Destroy(releaseSpikes.Value.gameObject);
        _releaseSpikesDict.Clear();

        _shouldShine = false;
        ResetSprite();
        if (FrameBlock.cached.Count > 0)
            FrameBlock.cached.Clear();
    }

    public override void RenderSprite()
    {
        foreach (var frameBlock in createdFramesBlock)
            if (frameBlock != null && !frameBlock.IsDestroyed()) 
                Destroy(frameBlock.gameObject);
        createdFramesBlock.Clear();
        
        var frameBlocksPrefab = FrameBlock.GenerateBlocks(gameObject, frameType, rotation);
        foreach (var frameBlock in frameBlocksPrefab)
            createdFramesBlock.Add(frameBlock);
    }

    public override bool CanRotate()
    {
        return true;
    }

    public override void SetupRotation(int blockPlacerRotation)
    {
        
    }

    private static Dictionary<string, Sprite[]> _cachedSpritesDict = new();
    public void ResetSprite()
    {
        var path = "blocks/" + (type == 2 ? "temp" : "full");
        if (!_cachedSpritesDict.ContainsKey(path))
            _cachedSpritesDict.Add(path, Resources.LoadAll<Sprite>(path));
        
        var sprites = _cachedSpritesDict[path];
        renderer.sprite = sprites[frameType];
        renderer.transform.rotation = Quaternion.Euler(0, 0, rotation * (-90));
        
        renderer.color = GetColor();
    }
    
    // Release spikes only ---------------------------------------------
    public Dictionary<int, ReleaseSpikes> _releaseSpikesDict = new(); // 0: top, 1: left, 2: bottom, 3: right
    private long tempSpikeDelay = 500L;
    private long releaseSpikeDelay = 500L;

    private void CheckForReleasedSpikes()
    {
        var playerPos = _playerScript.GetPos();
        var pos = GetPos();
        var desiredOffset = new Vector2Int(0, 0);
        
        if (playerPos.x == pos.x && playerPos.y == pos.y - 1)
            desiredOffset = new Vector2Int(0, -1);
        else if (playerPos.x == pos.x && playerPos.y == pos.y + 1)
            desiredOffset = new Vector2Int(0, 1);
        else if (playerPos.x == pos.x - 1 && playerPos.y == pos.y)
            desiredOffset = new Vector2Int(-1, 0);
        else if (playerPos.x == pos.x + 1 && playerPos.y == pos.y)
            desiredOffset = new Vector2Int(1, 0);

        if (desiredOffset is { x: 0, y: 0 })
            return;
        // Also find the correct rotation with the desired offset. By default the sprike is made for (1, 0), aka facing right
        var rotation = Quaternion.Euler(0, 0, 0);
        if (desiredOffset.x == -1)
            rotation = Quaternion.Euler(0, 0, -180);
        else if (desiredOffset.x == 1)
            rotation = Quaternion.Euler(0, 0, 0);
        else if (desiredOffset.y == 1)
            rotation = Quaternion.Euler(0, 0, 90);
        else if (desiredOffset.y == -1)
            rotation = Quaternion.Euler(0, 0, -90);
        
        int rotationId = 0; // 0: top, 1: left, 2: bottom, 3: right
        if (desiredOffset is { x: 0, y: -1 })
            rotationId = 0;
        else if (desiredOffset is { x: -1, y: 0 })
            rotationId = 1;
        else if (desiredOffset is { x: 0, y: 1 })
            rotationId = 2;
        else if (desiredOffset is { x: 1, y: 0 })
            rotationId = 3;
        
        if (_releaseSpikesDict.ContainsKey(rotationId))
            return;
        
        var finalPosition = transform.position + new Vector3(desiredOffset.x, desiredOffset.y, 0);
        
        var createdSpikes = Instantiate(tempSpikeReleasedPrefab, finalPosition, rotation);
        createdSpikes.GetComponent<BoxCollider2D>().isTrigger = true;
        
        var releaseSpikes = createdSpikes.GetComponent<ReleaseSpikes>();
        releaseSpikes.Init(this, releaseSpikeDelay, tempSpikeDelay, new Vector2Int((int) finalPosition.x, (int) finalPosition.y),
            rotationId);
        
        _releaseSpikesDict.Add(rotationId, releaseSpikes);
        StartCoroutine(PlayAttackSound());
    }
    
    public void ClearTempSpike(int rotationId)
    {
        if (!_releaseSpikesDict.ContainsKey(rotationId))
            return;
        
        var releaseSpikes = _releaseSpikesDict[rotationId];
        _releaseSpikesDict.Remove(rotationId);
        Destroy(releaseSpikes.gameObject);
        AudioManager.PlaySound("SpikeHide");
    }
    
    public IEnumerator PlayAttackSound()
    {
        yield return new WaitForSeconds(0.60f);
        AudioManager.PlaySound("SpikeAttack");
    }
    
    public void FixedUpdate()
    {
        if (type != 2)
            return;
        
        CheckForReleasedSpikes();
    }

    public override void Init()
    {
        base.Init();
        
        ResetSprite();
        
        if (isTemporary)
        {
            options.Add(new BlockSettingController.SliderOption("release_delay", "Release Delay", "The amount of time before the spikes are released", 100, 995, true, releaseSpikeDelay,
                v => releaseSpikeDelay = (long) v));
            options.Add(new BlockSettingController.SliderOption("delay", "Release Time", "The amount of time the spikes will be released for", 100, 995, true, tempSpikeDelay,
                v => tempSpikeDelay = (long) v));
        }
    }

    public override void ResetRotorRotation()
    {
        ResetSprite();
    }
    
    /* private IEnumerator FadeToColor0(Color color, float duration) 
    {
        Color[] startColors = new Color[createdFramesBlock.Count];
    
        for(int i = 0; i < createdFramesBlock.Count; i++) {
            startColors[i] = createdFramesBlock[i].GetComponent<SpriteRenderer>().color;
        }
    
        float time = 0;
    
        while(time < duration) {
            for(int i = 0; i < createdFramesBlock.Count; i++) {
                createdFramesBlock[i].GetComponent<SpriteRenderer>().color = 
                    Color.Lerp(startColors[i], color, time / duration); 
            }
        
            time += Time.deltaTime;
            yield return null;
        }
    
        foreach (var t in createdFramesBlock)
        {
            t.GetComponent<SpriteRenderer>().color = color;
        }
    } */
    // We know use the fade effect via the update method and not a coroutine

    private void LateUpdate()
    {
        if (!_shouldShine)
            return;
        
        var color = Color.Lerp(EditorValues.LogicScript.GetColor(), Color.cyan, Mathf.PingPong(Time.time * 3, 1));
        foreach (var frameBlock in createdFramesBlock)
            frameBlock.spriteRenderer.color = color;
    }
}