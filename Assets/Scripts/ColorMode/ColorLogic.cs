using System;
using System.Collections.Generic;
using UnityEngine;

public class ColorLogic : MonoBehaviour
{
    
    [Header("References")]
    public LogicScript logic;
    public PlayerScript player;
    public GameObject colorBlockPrefab;
    public GameObject finishLevelButton;

    [Header("Values")]
    public bool colorMode;
    public bool paintPlayer;
    
    public readonly Dictionary<Vector2Int, ColoredBlock> colorBlocks = new();
    public void EnableEditor()
    {
        if (!colorMode) return;
        paintPlayer = false;
        finishLevelButton.SetActive(false);
        
        RemoveAllColorBlocks();
    }

    public void Init()
    {
        finishLevelButton.SetActive(false);
    }
    
    public void DisableEditor()
    {
        if (!colorMode) return;

        RemoveAllColorBlocks();
        paintPlayer = true;
        finishLevelButton.SetActive(logic.AllowEditor());
    }

    private void Update()
    {
        if (!colorMode) return;
        if (!paintPlayer) return;

        var playerPos = player.transform.position;
        var playerPosInt = new Vector2Int((int) playerPos.x, (int) playerPos.y);
        if (!colorBlocks.ContainsKey(playerPosInt))
        {
            var colliders = Physics2D.OverlapCircleAll(playerPosInt, 0.005f);
            foreach (var collider2D1 in colliders)
            {
                if (collider2D1.gameObject == gameObject) continue;
                if (collider2D1.gameObject.name == "Player") continue;
                if (collider2D1.gameObject.GetComponent<FullFrameBlock>() == null) continue;

                return;
            }
            
            var colorBlock = Instantiate(colorBlockPrefab, playerPos, Quaternion.identity, transform).GetComponent<ColoredBlock>();
            
            colorBlock.Init(playerPosInt, logic.GetColor(), this);
            colorBlocks.Add(playerPosInt, colorBlock);
        }

        if (GetColoredBlockCount() >= LogicScript.settings.GetColoredBlockCount() && !logic.AllowEditor())
        {
            logic.WinLevel();
        }
    }

    public void RemoveAllColorBlocks()
    {
        foreach (var block in colorBlocks)
            Destroy(block.Value.gameObject);
        colorBlocks.Clear();
    }

    public void OnPlayerDeath()
    {
        if (!colorMode) return;
        
        RemoveAllColorBlocks();
    }

    public int GetColoredBlockCount()
    {
        return colorBlocks.Count;
    }

    public void FinishLevel()
    {
        var blocks = GetColoredBlockCount();
        if (blocks <= 1) {
            logic.notifications.ShowError("You need to color at least 2 blocks!");
            return;
        }
        
        logic.WinLevel();
        logic.EnableEditor();
        logic.notifications.ShowNotification($"Level verified with {blocks} colored blocks!");
    }
}