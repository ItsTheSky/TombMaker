using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

public class Block : MonoBehaviour
{
    public static Block CurrentBlockPlayerIsOn;
    public static GameObject tempSpikeReleasedPrefab;

    public Color indicatorColor = Color.magenta;
    public int blockType;
    public bool isSolid;
    public bool uniqueLayer;
    public int blocksLimit = -1; // -1 = infinite

    [TextArea(1, 10)]
    public string help;
    
    [HideInInspector] public bool isPlayerGlued;
    [HideInInspector] public bool canEdit;
    [HideInInspector] public GameObject blockPrefab;
    [HideInInspector] public bool ready;
    
    [HideInInspector] public List<BlockSettingController.BlockOption> options = new ();
    [HideInInspector] public int layer;

    [HideInInspector] public Animator _animator;
    [HideInInspector] public PlayerScript _playerScript;
    [HideInInspector] public SpriteRenderer _renderer;
    protected Sprite _defaultSprite;
    protected bool _init;
    protected Vector2 _pos;
    [CanBeNull] [HideInInspector] public Rotor attachedRotor;
    
    // settings
    public bool isTemporary;

    // Start is called before the first frame update
    public virtual void Init()
    {
        _playerScript = EditorValues.PlayerScript;
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();

        _init = true;
        _defaultSprite = _renderer.sprite;

        var position = transform.position;
        _pos = new Vector2(position.x, position.y);

        if (tempSpikeReleasedPrefab == null)
            tempSpikeReleasedPrefab = Resources.Load<GameObject>("ReleasedSpikes");

        layer = EditorValues.LogicScript.layer;
    }

    public virtual void SetPos(Vector2 vector2)
    {
        _pos = vector2;
        var transform1 = transform;
        
        transform1.position = new Vector3(vector2.x, vector2.y, transform1.position.z);
    }

    protected BlockSettingController.BlockOption GetOptionById(string id)
    {
        CheckInit();
        
        foreach (var option in options)
        {
            if (option.id == id)
                return option;
        }
        
        return null;
    }

    protected void CheckInit()
    {
        if (!_init) 
            Init();
    }

    public void RefreshSprite()
    {
        CheckInit();
    }

    public virtual void OnCollisionEnter2D(Collision2D col)
    {
        if (!ready)
            return;

        if (IsPlayer(col))
        {
            CurrentBlockPlayerIsOn = this;
            isPlayerGlued = true;
            if (CurrentBlockPlayerIsOn != null)
                CurrentBlockPlayerIsOn.isPlayerGlued = false;
            
            _playerScript.StopMoving();
            if (isSpike)
                _playerScript.Die();
        }
    }

    public bool IsPlayer(Collision2D col)
    {
        return col.gameObject.CompareTag("player");
    }
    
    public virtual bool isSpike
    {
        get 
        {
            CheckInit();
            return blockType == 2;
        }
    }

    public virtual void Desactivate()
    {
        gameObject.SetActive(true);
    }
    
    public virtual void Activate()
    {
        gameObject.SetActive(false);
    }
    
    public int GetBlockType()
    {
        CheckInit();
        return blockType;
    }
    
    public Vector2 GetPos()
    {
        CheckInit();
        return _pos;
    }

    private bool _isEnabled; // Check if it's enabled IN THE GAME, not in the editor
    public virtual void Enable()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        CheckInit();
        GenerateTexture();

        _isEnabled = false;
        
        if (isTemporary) 
            _animator.Play("TempSpikeStatic");

        CurrentBlockPlayerIsOn = null;
        isPlayerGlued = false;
    }

    public virtual void Disable()
    {
        CheckInit();
        RenderSprite();
        
        _isEnabled = true;
        
        if (isTemporary) 
            _animator.Play("TempSpikeShining");
    }
    
    private void FixedUpdate()
    {
        if (!_isEnabled || !ready)
            return;
    }

    public virtual bool ShouldShowOption(BlockSettingController.BlockOption option)
    {
        return true;
    }

    public virtual void RenderSprite()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        
        var hasAbove = _playerScript.logic.HasSolidBlockAtRelative(this, Vector2Int.up);
        var hasBelow = _playerScript.logic.HasSolidBlockAtRelative(this, Vector2Int.down);
        var hasLeft = _playerScript.logic.HasSolidBlockAtRelative(this, Vector2Int.left);
        var hasRight = _playerScript.logic.HasSolidBlockAtRelative(this, Vector2Int.right);
        var currentArray = new Sprite[0];

        if (isSpike)
            currentArray = _playerScript.logic.blockPlacer.spikeSprites;
        if (isTemporary)
            currentArray = _playerScript.logic.blockPlacer.tempSpikeSprites;
        if (!isSpike && !isTemporary)
            currentArray = _playerScript.logic.blockPlacer.blockSprites;
        
        Sprite sprite = currentArray[0]; // empty block

        // SLABS & FULL BLOCKS

        if (!hasAbove && !hasBelow && !hasLeft && !hasRight)
            sprite = currentArray[3]; // full block
        if ((!hasAbove && !hasBelow && hasRight && hasLeft)
            || (hasAbove && hasBelow && !hasRight && !hasLeft)) {
            sprite = currentArray[2]; // horizontal block
            if (!hasRight)
                transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        
        // CORNERS

        var cornerChecks = new[]
        {
            !hasAbove && !hasLeft && hasBelow && hasRight, // top left
            !hasBelow && !hasLeft && hasAbove && hasRight, // bottom left
            !hasBelow && !hasRight && hasAbove && hasLeft, // bottom right
            !hasAbove && !hasRight && hasBelow && hasLeft, // top right
        };

        var baseRotation = 0;
        for (var i = 0; i < cornerChecks.Length; i++)
        {
            if (cornerChecks[i])
            {
                sprite = currentArray[1]; // corner block
                baseRotation = i * 90;
                break;
            }
        }
        if (baseRotation != 0)
            transform.rotation = Quaternion.Euler(0, 0, baseRotation);
        
        // POINTS
        
        baseRotation = 0;
        var pointChecks = new[]
        {
            !hasAbove && !hasLeft && !hasRight && hasBelow, // top point
            !hasAbove && !hasLeft && !hasBelow && hasRight, // left point
            !hasBelow && !hasLeft && !hasRight && hasAbove, // bottom point
            !hasAbove && !hasRight && !hasBelow && hasLeft, // right point
        };
        
        for (var i = 0; i < pointChecks.Length; i++)
        {
            if (pointChecks[i])
            {
                sprite = currentArray[0]; // point block
                baseRotation = i * 90;
                break;
            }
        }
        if (baseRotation != 0)
            transform.rotation = Quaternion.Euler(0, 0, baseRotation);
        
        // T BLOCKS
        
        baseRotation = 0;
        var tChecks = new[]
        {
            !hasAbove && hasLeft && hasRight && hasBelow, // top t
            hasAbove && !hasLeft && hasRight && hasBelow, // left t
            hasAbove && hasLeft && hasRight && !hasBelow, // right t
            hasAbove && hasLeft && !hasRight && hasBelow, // bottom t
        };
        
        for (var i = 0; i < tChecks.Length; i++)
        {
            if (tChecks[i])
            {
                sprite = currentArray[5]; // t block
                baseRotation = i * 90;
                break;
            }
        }
        if (baseRotation != 0)
            transform.rotation = Quaternion.Euler(0, 0, baseRotation);
        
        // everywhere :o
        if (hasAbove && hasBelow && hasLeft && hasRight)
            sprite = null;

        _renderer.sprite = sprite;
    }
    
    public void GenerateTexture()
    {
        CheckInit();
        _renderer.sprite = _defaultSprite;
    }

    public bool CanEdit()
    {
        return canEdit;
    }

    public virtual bool CanRotate()
    {
        return false;
    }

    [CanBeNull]
    public virtual string CanPlace()
    {
        return null;
    }

    public virtual void OnPlayerDeath()
    {
        
    }

    public virtual Dictionary<string, object> SaveSettings()
    {
        return new();
    }
    
    public virtual void LoadSettings(Dictionary<string, object> settings)
    {
        
    }
    
    public virtual bool ShouldShowSettings()
    {
        return canEdit || options.Count > 0;
    }
    
    public virtual bool IsSolid()
    {
        return isSolid;
    }

    public virtual void UpdateBlockColor(Color color)
    {
        CheckInit();
        if (blockType == 12) 
            _renderer.color = color;
    }

    public virtual void SetupRotation(int blockPlacerRotation)
    {
        throw new NotImplementedException("SetupRotation not implemented for this block type");
    }

    public virtual bool IsTrigger()
    {
        return false;
    }

    #region Block utilities

    public List<GameObject> GetBlocksAround(bool allLayers = false)
    {
        var blocks = new List<GameObject>();

        if (allLayers)
        {
            // GetBlockAtRelativeAtAllLayers
            
            var tops = EditorValues.LogicScript.GetBlockAtRelativeAtAllLayers(this, Vector2Int.up);
            var bottoms = EditorValues.LogicScript.GetBlockAtRelativeAtAllLayers(this, Vector2Int.down);
            var lefts = EditorValues.LogicScript.GetBlockAtRelativeAtAllLayers(this, Vector2Int.left);
            var rights = EditorValues.LogicScript.GetBlockAtRelativeAtAllLayers(this, Vector2Int.right);
            
            blocks.AddRange(tops);
            blocks.AddRange(bottoms);
            blocks.AddRange(lefts);
            blocks.AddRange(rights);
            
            blocks.RemoveAll(block => block == null);
        }
        else
        {
            var top = EditorValues.LogicScript.GetBlockAtRelative(this, Vector2Int.up);
            var bottom = EditorValues.LogicScript.GetBlockAtRelative(this, Vector2Int.down);
            var left = EditorValues.LogicScript.GetBlockAtRelative(this, Vector2Int.left);
            var right = EditorValues.LogicScript.GetBlockAtRelative(this, Vector2Int.right);

            if (top != null) blocks.Add(top.gameObject);
            if (bottom != null) blocks.Add(bottom.gameObject);
            if (left != null) blocks.Add(left.gameObject);
            if (right != null) blocks.Add(right.gameObject);
        }

        // also check for player
        var playerPos = EditorValues.PlayerScript.GetPos();
        if (playerPos == _pos + Vector2.up) blocks.Add(EditorValues.PlayerScript.gameObject);
        if (playerPos == _pos + Vector2.down) blocks.Add(EditorValues.PlayerScript.gameObject);
        if (playerPos == _pos + Vector2.left) blocks.Add(EditorValues.PlayerScript.gameObject);
        if (playerPos == _pos + Vector2.right) blocks.Add(EditorValues.PlayerScript.gameObject);
        
        return blocks;
    }

    /**
     * Using recursion, get all blocks attached to this one. limit is 5
     */
    public List<GameObject> GetBlockAttachedTo(int limit)
    {
        var blocks = new List<GameObject>();
        GetBlockAttachedTo(limit, blocks, 0);
        return blocks;
    }
    
    private void GetBlockAttachedTo(int limit, List<GameObject> array, int depth)
    {
        if (depth >= limit)
            return;
        
        var around = GetBlocksAround(true);
        if (around.Count == 0)
            return;
        
        foreach (var block in around)
        {
            if (array.Contains(block.gameObject))
                continue;

            var script = block.GetComponent<Block>();
            if (script == null || !script.IsRotatableByRotor())
                continue;
            array.Add(block.gameObject);
            script.GetBlockAttachedTo(limit, array, depth + 1);
        }
    }
    
    #endregion
    
    #region Selection

    private Color _defaultColor = Color.white;
    public virtual void OnSelect()
    {
        _defaultColor = _renderer.color;
        _renderer.color = Color.green;
    }
    
    public virtual void OnDeselect()
    {
        _renderer.color = _defaultColor;
    }
    
    #endregion
    
    #region Rotor stuff
    
    public virtual bool IsRotatableByRotor()
    {
        return true;
    }

    public virtual void RotorRotation(int degrees)
    {
        // do nothing by default
    }
    
    public virtual void ResetRotorRotation()
    {
        // do nothing by default
    }
    
    #endregion

    public virtual bool CanBeMoveReplaced()
    {
        return true;
    }

    public virtual bool CanBeDeletedManually()
    {
        return true;
    }
    
    public bool IsDeadly()
    {
        return isSpike || blockType == 13 || blockType == 7;
    }

    public virtual bool ShouldBeDesactivatedInCurrentState()
    {
        return false;
    }
}
