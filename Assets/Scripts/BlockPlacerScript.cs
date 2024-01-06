using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class BlockPlacerScript : MonoBehaviour
{
    
    public LogicScript logic;
    public SelectorZoneController selectorZoneController;
    public Transform blockIndicator;
    public new bool enabled = true;
    public SpriteRenderer indicatorSprite;
    
    public Sprite[] blockSprites;
    public Sprite[] spikeSprites;
    public Sprite[] tempSpikeSprites;

    //public Animator wheelAnimator;
    //public GameObject frameWheel;

    public GameObject currentPrefab;
    
    [Header("Frame Block Selector")]
    public FrameBlockSelectorController frameBlockSelectorController;
    public Animator frameSelectorAnimator;
    
    [Header("Toolbox Sprites")]
    public Sprite editSprite;
    public Sprite removeSprite;
    public Sprite selectSprite;
    
    [Header("Blocks")]
    public GameObject blockPrefab;
    public GameObject spikePrefab;
    public GameObject spawnPrefab;
    public GameObject starPrefab;
    public GameObject dotPrefab;
    public GameObject goalPrefab;
    public GameObject batPrefab;
    public GameObject archerPrefab;
    public GameObject blobPrefab;
    public GameObject tempSpikePrefab;
    public GameObject padPrefab;
    public GameObject frameBlockPrefab;
    public GameObject spikeFrameBlockPrefab;
    public GameObject tempSpikeFrameBlockPrefab;
    public GameObject portalPrefab;
    public GameObject tempBlockPrefab;
    public GameObject rotorPrefab;
    public GameObject switcherPrefab;
    public GameObject passablePrefab;
    public GameObject textPrefab;
    
    [Header("Triggers")]
    public GameObject cameraTrigger;
    public GameObject speedTrigger;
    public GameObject lavaTrigger;
    
    public readonly Dictionary<int, GameObject> BlockPrefabs = new();

    private Camera _camera;
    // frame blocks
    public static int PossibleFullFrameTypes = 11;
    public int frameType = 10; // 0: corner, 1: wall, 2: inner
    [HideInInspector] public int rotation; // 0: u-l, 1: u-r, 2: d-l, 3: d-r

    private void Awake()
    {
        BlockPrefabs.Add(1, blockPrefab);
        BlockPrefabs.Add(2, spikePrefab);
        BlockPrefabs.Add(3, spawnPrefab);
        BlockPrefabs.Add(4, starPrefab);
        BlockPrefabs.Add(5, dotPrefab);
        BlockPrefabs.Add(6, goalPrefab);
        BlockPrefabs.Add(7, batPrefab);
        BlockPrefabs.Add(8, archerPrefab);
        BlockPrefabs.Add(9, blobPrefab);
        BlockPrefabs.Add(10, tempSpikePrefab);
        BlockPrefabs.Add(11, padPrefab);
        BlockPrefabs.Add(12, frameBlockPrefab);
        BlockPrefabs.Add(13, spikeFrameBlockPrefab);
        BlockPrefabs.Add(14, tempSpikeFrameBlockPrefab);
        BlockPrefabs.Add(15, portalPrefab);
        BlockPrefabs.Add(16, tempBlockPrefab);
        BlockPrefabs.Add(17, rotorPrefab);
        BlockPrefabs.Add(18, switcherPrefab);
        BlockPrefabs.Add(19, passablePrefab);
        BlockPrefabs.Add(20, textPrefab);
        
        // Decorations (loaded dynamically), starting at 500
        foreach (var decoration in Resources.LoadAll<GameObject>("Decorations")) 
            BlockPrefabs.Add(int.Parse(decoration.name), decoration);
        
        // Triggers
        BlockPrefabs.Add(1000, cameraTrigger);
        BlockPrefabs.Add(1001, speedTrigger);
        BlockPrefabs.Add(1002, lavaTrigger);
    }

    void Start()
    {
        _camera = Camera.main;
        UpdatePrefab();
    }

    public Vector2 GetMousePos()
    {
        return _camera.ScreenToWorldPoint(Input.mousePosition);
    }

    private Vector2 _lastSelectedBlock = Vector2.positiveInfinity;
    
    // Bunch select variables
    private Vector2 _startingPos = Vector2.positiveInfinity;
    private bool _isSelecting = false;
    private bool _isHolding = false;
    private float _holding = 0;
    
    void Update()
    {
        if (!enabled || !LogicScript.settings.allowEditor)
            return;
        var tool = logic.tool;
        
        Vector2 mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mapped = logic.MapToGrid(mousePos);

        #region Bunch Selection

        if (logic.tool == 3)
        {
            if (_isHolding && Input.GetMouseButtonUp(0))
            {
                _isHolding = false;
                _holding = 0;
                _isSelecting = false;
            } else if (_isHolding)
            {
                _holding += Time.deltaTime;
                if (_holding > 0.5f)
                {
                    _isSelecting = true;
                    print("selecting");
                    _holding = 0;
                    _isHolding = false;
                    _startingPos = mapped;
                }
            } else if (!_isHolding && Input.GetMouseButtonDown(0))
            {
                _isHolding = true;
            }
        
            if (_isSelecting)
            {
                print("drawing");
                Vector2 min = new Vector2(Mathf.Min(mapped.x, _startingPos.x), Mathf.Min(mapped.y, _startingPos.y));
                Vector2 max = new Vector2(Mathf.Max(mapped.x, _startingPos.x), Mathf.Max(mapped.y, _startingPos.y));
                Vector2 size = max - min;
                Vector2 center = min + size / 2;
            
                if (!Input.GetMouseButton(0) && !overlapUI)
                {
                    for (int x = (int) min.x; x <= max.x; x++)
                    {
                        for (int y = (int) min.y; y <= max.y; y++)
                        {
                            SelectBlock(new Vector2(x, y));
                        }
                    }
                    
                    _isSelecting = false;
                    _holding = 0;
                    _isHolding = false;
                }
                return;
            }   
        }
        
        #endregion
        

        if (Input.GetMouseButton(0) && !overlapUI && tool != 3)
        {
            
            if (tool == 0)
                PlaceBlock(mapped);
            else if (tool == 1)
                RemoveBlock(mapped);
            else if (tool == 2)
                EditBlock(mapped);
            
        } else if (Input.GetMouseButton(0) && !overlapUI && tool == 3)
        {
            SelectBlock(mapped);
        }
        else if (Input.GetKey(LogicScript.KeyCodeForDeleting) && !overlapUI)
        {
            RemoveBlock(mapped);
        } else if (Input.GetKey(LogicScript.KeyCodeForEditing) && !overlapUI)
        {
            EditBlock(mapped);
        } else if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift) && !overlapUI)
        {
            SelectBlock(mapped);
        }
        
        // Check for 'R' key
        if (Input.GetKeyDown(KeyCode.R) && tool == 0)
            NextRotation();

        if (tool == 3 && Input.GetMouseButtonUp(0))
            _lastSelectedBlock = Vector2.positiveInfinity;

        var fullFrame = currentPrefab.GetComponent<FullFrameBlock>();
        var tPressed = Input.GetKey(KeyCode.E) && fullFrame != null && tool == 0;
        if (_wasTPressed && !tPressed)
        {
            _wasTPressed = false;

            var input = frameBlockSelectorController.GetSelectedFrameBlock();
            frameType = input == -1 ? frameType : input;
            frameBlockSelectorController.OnPanelClose();
            UpdateSprite();
            
            frameSelectorAnimator.Play("WheelHide");
            frameBlockSelectorController.gameObject.SetActive(false);
        } else if (!_wasTPressed && tPressed)
        {
            _wasTPressed = true;
            frameBlockSelectorController.Init();
            frameBlockSelectorController.OnPanelOpen(frameType);
            frameBlockSelectorController.RecolorAll(fullFrame.GetColor());
            
            frameBlockSelectorController.gameObject.SetActive(true);
            frameSelectorAnimator.Play("WheelShow");
        }

        blockIndicator.position = new Vector3(mapped.x, mapped.y, 0);
    }
    
    // methods extracted from above:
    // ReSharper disable Unity.PerformanceAnalysis
    private void PlaceBlock(Vector2 mapped)
    {
        /* if (currentPrefab != null && currentPrefab.GetComponent<SpawnBlock>() != null && logic.GetSpawnScript() != null)
            logic.RemoveBlock(logic.GetSpawnLocation()); */
        
        if (currentPrefab.GetComponent<SpawnBlock>() == null && logic.GetSpawnLocation() == mapped)
        {
            logic.notifications.ShowError("Can't place block on spawn block");
            return;
        }

        logic.PlaceBlock(mapped, currentPrefab, true);
    }
    
    private void RemoveBlock(Vector2 mapped)
    {
        if (logic.GetSpawnLocation() == mapped) // don't remove spawn block
        {
            logic.notifications.ShowError("Can't remove spawn block");
            return;
        }
        
        logic.RemoveBlock(mapped);
    }

    private void EditBlock(Vector2 mapped)
    {
        Block block = logic.GetFirstBlockAtAnyLayer(mapped)?.GetComponent<Block>();
        if (block != null)
        {
            if (block.ShouldShowSettings())
            {
                logic.EditBlock(block);
            } else if (!string.IsNullOrEmpty(block.help))
            {
                BlockSettingController.ShowHelpTextStatic(block);
            }
        }
    }
    
    private void SelectBlock(Vector2 mapped)
    {
        if (_lastSelectedBlock == mapped)
            return;

        _lastSelectedBlock = mapped;
        Block block = logic.GetBlock(mapped)?.GetComponent<Block>();
        if (block != null)
        {
            logic.SelectBlock(block);
        }
    }

    private Utilities.POINT _lastMousePos;
    private bool _wasTPressed;

    public void UpdatePrefab()
    {
        var block = currentPrefab.GetComponent<Block>();
        block.RefreshSprite();
        
        SpriteRenderer blockIndicatorRenderer = blockIndicator.GetComponent<SpriteRenderer>();
        Color desiredColor = block.indicatorColor;
        blockIndicatorRenderer.sprite = currentPrefab.GetComponent<SpriteRenderer>().sprite;
        blockIndicatorRenderer.color = new Color(desiredColor.r, desiredColor.g, desiredColor.b, 0.5f);
        
        if (currentPrefab.GetComponent<FrameBlock>() != null)
            UpdateSprite();
        else
        {
            // reset rotation and frame type
            frameType = currentPrefab.GetComponent<FullFrameBlock>() != null ? frameType : 10;
            rotation = currentPrefab.GetComponent<Block>().CanRotate() ? rotation : 0; // reset rotation if can't rotate
            UpdateSprite();
        }
    }

    public void UseEditSprite()
    {
        SpriteRenderer blockIndicatorRenderer = blockIndicator.GetComponent<SpriteRenderer>();
        blockIndicatorRenderer.sprite = editSprite;
        blockIndicatorRenderer.color = new Color(1, 1, 1, 0.5f);
    }
    
    public void ChangePrefab(GameObject prefab)
    {
        currentPrefab = prefab;
        UpdatePrefab();
    }

    public void Disable()
    {
        enabled = false;
        indicatorSprite.enabled = false;
    }
    
    public void Enable()
    {
        enabled = true;
        indicatorSprite.enabled = true;
    }
    
    public static bool overlapUI
    {
        get
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);
            
            // check if any of the results are UI elements (by checking if they have a canvas)
            bool hasCanvas = false;
            foreach (var result in results)
            {
                // we check their parent until it's null
                var parent = result.gameObject.transform.parent;
                while (parent != null)
                {
                    if (parent.GetComponent<Canvas>() != null)
                    {
                        hasCanvas = true;
                        break;
                    }
                    parent = parent.parent;
                }
            }
            
            return hasCanvas;
        }
    }
    
    public void NextType()
    {
        var frame = currentPrefab.GetComponent<FullFrameBlock>();
        if (frame == null)
            return;
        
        
        frameType = (frameType + 1) % PossibleFullFrameTypes;
        UpdateSprite();
    }
    
    public void NextRotation()
    { 
        var block = currentPrefab.GetComponent<Block>();
        if (block == null || !block.CanRotate())
            return;
        
        
        rotation = (rotation + 1) % 4;
        UpdateSprite();
    }

    public void UpdateSprite()
    {
        var tool = logic.tool;
        if (tool == 0)
        {
            // place

            var block = currentPrefab.GetComponent<Block>();
            var frame = currentPrefab.GetComponent<FullFrameBlock>();
            if (frame == null && block == null)
                return;

            if (frame == null)
            {
                var degrees = rotation * (-90);
                indicatorSprite.transform.rotation = Quaternion.Euler(0, 0, degrees);
                return;
            }

            var sprites = Resources.LoadAll<Sprite>("blocks/" + (frame.type == 2 ? "temp" : "full"));
            indicatorSprite.sprite = sprites[frameType];
            indicatorSprite.transform.rotation = Quaternion.Euler(0, 0, rotation * (-90));
            var color = frame == null ? logic.GetColor() : frame.GetColor();
            indicatorSprite.color = new Color(color.r, color.g, color.b, 0.5f);
        } else if (tool == 1) // remove
        {
            indicatorSprite.sprite = removeSprite;
            indicatorSprite.transform.rotation = Quaternion.Euler(0, 0, 0);
            indicatorSprite.color = new Color(1, 0, 0, 0.6f);
        } else if (tool == 2) // edit
        {
            indicatorSprite.sprite = editSprite;
            indicatorSprite.transform.rotation = Quaternion.Euler(0, 0, 0);
            indicatorSprite.color = new Color(1, 0, 1, 0.6f);
        } else if (tool == 3) // select
        {
            indicatorSprite.sprite = selectSprite;
            indicatorSprite.transform.rotation = Quaternion.Euler(0, 0, 0);
            indicatorSprite.color = new Color(0, 1, 0, 0.6f);
        } else
        {
            throw new System.Exception("BlockPlacerScript: invalid tool index " + tool);
        }
    }

}
