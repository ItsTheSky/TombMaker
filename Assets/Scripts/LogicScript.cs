using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static AudioManager;

public class LogicScript : MonoBehaviour
{
    public static bool UseArrowsForLayers = false;
    public static KeyCode KeyCodeForDeleting = KeyCode.Mouse1;
    public static KeyCode KeyCodeForEditing = KeyCode.Mouse2;
    public static KeyCode KeyCodeForMoving = KeyCode.Mouse2;
    public static bool ShowEditorGuide = true;
    public static bool ShowEditorGrid = true;
        
    public static GameLevelSettings settings;
    
    // It's the whole system that is enabled/disabled!!! (not just the editor)
    [FormerlySerializedAs("enabled")] public bool _enabled = true;
    
    // Definitions
    public float gridSize;
    public BlockPlacerScript blockPlacer;
    public PlayerScript player;
    public GameObject particles;
    public GameObject UI;
    public GameObject pauseMenu;
    public GameObject pauseMenuPanel;
    public TMP_Text pauseMenuText;
    public GameObject entranceDoor;
    public GameObject pauseButton;
    public GameObject[] editorToolbox;
    public GameObject popupPrefab;
    public GameObject canvas;
    public bool hasUIOpen;
    
    public TMP_Text debugText;
    public TMP_Text layerText;
    
    public List<SpriteRenderer> coloredRenderers;

    public Sprite pauseSprite;
    public GameObject settingsPanel;
    
    public BlockSettingController blockSettings;
    public LevelFinishController levelFinishController; // also holds the UI game object
    
    public List<GameObject> UIObjects;
    public List<GameObject> disabledPauseObjects;

    public TMP_Text deathStatText;
    public TMP_Text coinsStatText;

    [Header("Editor Data's")]
    public int localLevelId = 99999;
    public bool editor = true;
    public int blockType = 12;
    public int tool; // 0: place, 1: remove, 2: edit, 3: select
    public SaveData.LevelSettings levelSettings = new ();
    public bool hasSavedYet = true;

    public GameObject selects;
    
    // Playing data
    [Header("Playing data")]
    public int stars;
    public int deaths;
    public int dots;

    [Header("Loading Screen")]
    public TMP_Text loadingText;
    public Slider loadingSlider;
    public GameObject loadingScreenForeground;
    
    [Header("Controllers")]
    public Notifications notifications;
    public LevelSettingsController levelSettingsController;
    public HelpTextsManager helpTextsManager;
    public ColorLogic colorLogic;
    public GridController gridController;

    [Header("Shield")] 
    public Button shieldButton;
    public Slider shieldTimer;
    public GameObject shieldObject;
    public TMP_Text shieldAmountText;
    
    [Header("Other")]
    
    public bool defaultCamera = true;
    public bool StickyCamera => defaultCamera ? levelSettings.stickyCamera : _stickyCamera;
    public float CameraZoom => defaultCamera ? levelSettings.cameraZoom : _cameraZoom;
    public double CameraX => defaultCamera ? levelSettings.cameraX : _cameraX;
    public double CameraY => defaultCamera ? levelSettings.cameraY : _cameraY;
    public bool UnlockedX => defaultCamera ? levelSettings.unlockedX : _unlockedX;
    public bool UnlockedY => defaultCamera ? levelSettings.unlockedY : _unlockedY;

    // Internal data
    public readonly Dictionary<int, Dictionary<Vector2, GameObject>> layers = new();
    private Camera _camera;
    [HideInInspector] public int layer;
    private int _lastLayer;
    
    private int _lastNonTriggerLayer;
    private float _previousZoom = 5f;
    private bool _allowEditor;
    private HistoryManager _historyManager;
    private Dictionary<int, Block> _selectedBlocks = new();
    [HideInInspector] public long collectedExperience = 0;
    
    [HideInInspector] public bool _stickyCamera;
    [HideInInspector] public float _cameraZoom;
    [HideInInspector] public double _cameraX;
    [HideInInspector] public double _cameraY;
    [HideInInspector] public bool _unlockedX;
    [HideInInspector] public bool _unlockedY;
    
    public Dictionary<Vector2, GameObject> GetLayerByIndex(int index)
    {
        if (!layers.ContainsKey(index))
            layers.Add(index, new Dictionary<Vector2, GameObject>());
        
        return layers[index];
    }

    public List<Block> GetAllBlocks(List<int> excludedLayers = null)
    {
        excludedLayers ??= new List<int>();
        
        List<Block> blocks = new();
        foreach (var layer in layers)
        {
            if (excludedLayers.Contains(layer.Key))
                continue;
            
            foreach (var block in layer.Value)
                if (block.Value != null && !block.Value.IsDestroyed()) 
                    blocks.Add(block.Value.GetComponent<Block>());
        }

        return blocks;
    }

    private void Awake()
    {
        EditorValues.LogicScript = this;
    }

    private void Start()
    {
        helpTextsManager.Init();
        settings ??= new GameLevelSettings(new SaveData(), true, -1);
        _historyManager = new HistoryManager(this);

        if (settings.saveData.version == 0) // unsupported level version or unsupported level type
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        instance?.StopAll();
        PlaySound("LevelTheme");
        
        localLevelId = settings.localLevelId;
        
        _allowEditor = settings.allowEditor;
        
        EditorValues.PlayerScript = player;
        player.SetupTrail(Constants.Trails[PlayerStatsManager.GetSelectedTrail()]);
        _camera = Camera.main;
        var spawnPos = settings.saveData.GetSpawnPos();
        _camera.transform.position = new Vector3(spawnPos.x, spawnPos.y, _camera.transform.position.z);
        UpdateScales();
        shieldAmountText.text = PlayerStatsManager.GetShields().ToString();

        // Adding layers
        layers.Clear();
        layers.AddRange(settings.saveData.GetLayers(this));
        
        if (layers.Count == 0)
            layers.Add(0, new Dictionary<Vector2, GameObject>());
        
        if (!layers.ContainsKey(-1)) // -1 is the triggers layer
            layers.Add(-1, new Dictionary<Vector2, GameObject>());
        
        if (settings.allowEditor && !settings.saveData.HasSpawnPoint())
            PlaceBlock(new Vector2(0, 0), blockPlacer.spawnPrefab, true);
        
        levelSettings = settings.saveData.GetSettings(this);
        colorLogic.colorMode = levelSettings.levelType == LevelType.Color;
        colorLogic.Init();
        player.speed = levelSettings.playerSpeed;
        UpdateBlocksColor();

        if (settings.allowEditor)
        {
            gridController.gameObject.SetActive(ShowEditorGrid);
            levelSettingsController.Init(this);
            shieldObject.SetActive(false);
            
            if (_camera != null) 
                _camera.transform.position = new Vector3(0, 0, -10);
            particles.SetActive(false);
            UI.SetActive(true);
            
            deathStatText.transform.parent.gameObject.SetActive(false);
            coinsStatText.transform.parent.gameObject.SetActive(false);
            debugText.gameObject.SetActive(true);
            
            if (ShowEditorGuide) 
                helpTextsManager.ShowHelpTextInternal("EditorWelcome");
        }
        else
        {
            gridController.gameObject.SetActive(false);
            shieldObject.SetActive(true);
            
            DisableEditor();
            Destroy(UI);
            
            pauseButton.GetComponent<Image>().sprite = pauseSprite;
            
            foreach (var obj in disabledPauseObjects)
                obj.SetActive(false);
            
            deathStatText.transform.parent.gameObject.SetActive(true);
            coinsStatText.transform.parent.gameObject.SetActive(true);
            debugText.gameObject.SetActive(false);
            
            deathStatText.text = "0";
            coinsStatText.text = PlayerStatsManager.GetCoins().ToString();
            
            PlaySound("EnterLevel");
        }
        
        RefreshBlocksDisplayedColor();
    }

    public void UpdateScales()
    {
        EditorValues.UpdateScale(pauseMenu.transform, false);
        foreach (var obj in UIObjects)
            if (obj != null) 
                EditorValues.UpdateScale(obj.transform, false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeybindManager.ToggleEditor.value) && _allowEditor && !hasUIOpen)
            ToggleEditor();
        
        if (editor && _allowEditor) 
            UpdateDebugText();
        
        if (Input.GetKeyDown(KeybindManager.ActivateShield.value) && !_allowEditor && !editor && !hasUIOpen)
            UseShield();
        
        if (Input.GetKeyDown(KeyCode.P))
            throw new Exception("Exception test");

        if (UseArrowsForLayers && !hasUIOpen)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) && _allowEditor && editor && !hasUIOpen)
                NextLayer();
            if (Input.GetKeyDown(KeyCode.DownArrow) && _allowEditor && editor && !hasUIOpen)
                PreviousLayer();
        }
        else
        {
            if (_allowEditor && editor && tool == 3 && !hasUIOpen)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow)) MoveSelectedBlocks(1);
                if (Input.GetKeyDown(KeyCode.RightArrow)) MoveSelectedBlocks(2);
                if (Input.GetKeyDown(KeyCode.DownArrow)) MoveSelectedBlocks(3);
                if (Input.GetKeyDown(KeyCode.LeftArrow)) MoveSelectedBlocks(4);
            }
        }
    }

    public void OpenLevelSettings()
    {
        levelSettingsController.transform.parent.gameObject.SetActive(true);
    }
    
    public void CloseLevelSettings()
    {
        levelSettingsController.transform.parent.gameObject.SetActive(false);
    }

    public void PlaceBlock(Vector2 pos, [CanBeNull] GameObject block, bool force = false, bool log = true)
    {
        if (!_enabled || !_allowEditor)
            return;

        var script = block.GetBlock();

        if (block.GetComponent<Trigger>() == null && layer == -1)
                return; // only triggers can be placed in the triggers layer
        if (block.GetComponent<Trigger>() != null && layer != -1)
            return; // triggers can only be placed in the triggers layer
            
        if (GetSpawnLocation() == pos && !force)
        {
            // UIController.ShowError("Cannot replace spawn block!");
            return; // don't place block on spawn block
        }

        string canPlace = block.GetComponent<Block>().CanPlace();
        if (canPlace != null && !force)
        {
            // UIController.ShowError(canPlace);
            return;
        }

        if (script.blocksLimit != -1 && GetBlockCount(script.blockType) >= script.blocksLimit)
        {
            notifications.ShowError("Block limit reached!");
            return;
        }
            
        if (GetBlock(pos) != null)
            RemoveBlock(pos);
        
        if (block.GetComponent<Block>().uniqueLayer)
            RemoveBlockAtAllLayers(pos);
        
        Achievements.IncrementAchievementGroup(5);

        hasSavedYet = false;
        GameObject newBlock = Instantiate(block, new Vector3(pos.x, pos.y, block.transform.position.z), Quaternion.identity);
        var blockScript = newBlock.GetBlock();
        blockScript.SetPos(pos);

        if (newBlock.IsBlock(3) && GetSpawnScript() != null) // spawn block, we'll remove the previous one BUT copy its settings before
        {
            print("Found spawn block, removing previous one.");
            RemoveBlockAtAllLayers(GetSpawnLocation());
            
            var spawnBlock = newBlock.GetComponent<SpawnBlock>();
            _spawnScript = spawnBlock;
        }

        if (newBlock.GetComponent<FullFrameBlock>() != null) { // frame blocks
            var frameBlock = newBlock.GetComponent<FullFrameBlock>();
            frameBlock.frameType = blockPlacer.frameType;
            frameBlock.rotation = blockPlacer.rotation;
            frameBlock.ResetSprite();
        }
            
        blockScript.UpdateBlockColor(GetColor());
        blockScript.blockPrefab = block;
        blockScript.GetComponent<Block>().Init();
        if (blockScript.CanRotate())
            blockScript.SetupRotation(blockPlacer.rotation);

        blockScript.ready = true;
        GetLayerByIndex(layer).Add(pos, newBlock);
    }

    struct MovingRequest
    {
        public Vector2 _from;
        public Vector2 _to;
        public Block _block;

        public MovingRequest(Vector2 from, Vector2 to, Block block)
        {
            _from = from;
            _to = to;
            _block = block;
        }
    }
    
    public void MoveSelectedBlocks(int direction)
    {
        var vector = new Vector2Int(0, 0);
        if (direction == 1)
            vector.y = 1;
        else if (direction == 2)
            vector.x = 1;
        else if (direction == 3)
            vector.y = -1;
        else
            vector.x = -1;

        var toMove = new List<MovingRequest>();
        foreach (var i in _selectedBlocks)
        {
            var block = i.Value;
            var pos = block.GetPos();
            var newPos = pos + vector;
            var blockLayer = block.layer;
            

            if (layers[blockLayer].ContainsKey(newPos))
                //&& !layers[blockLayer][newPos].GetComponent<Block>().CanBeMoveReplaced())
            {
                var newPosBlock = layers[blockLayer][newPos].GetComponent<Block>();
                if (!_selectedBlocks.ContainsKey(newPosBlock.GetInstanceID()))
                {
                    print("block at " + newPos.ToString() + " make impossible moving of block at " + pos.ToString());
                    return;
                }
            }

            toMove.Add(new MovingRequest(pos, newPos, block));
        }

        if (direction == 1) // moving to top
            toMove.Sort((first, second) => first._to.y > second._to.y ? -1 : 1);
        else if (direction == 3)
            toMove.Sort((first, second) => first._to.y > second._to.y ? 1 : -1);
        else if (direction == 2)
            toMove.Sort((first, second) => first._to.x > second._to.x ? -1 : 1);
        else if (direction == 4)
            toMove.Sort((first, second) => first._to.x > second._to.x ? 1 : -1);

        foreach (var move in toMove)
        {
            if (move._block == null || move._block.IsDestroyed())
                continue;
            
            var block = move._block;
            var blockLayer = block.layer;
            var newPos = move._to;
            var pos = move._from;
            
            layers[blockLayer].Remove(pos);
            layers[blockLayer][newPos] = block.gameObject;
            block.SetPos(newPos);
        }
    }

    public void DeleteSelectedBlocks()
    {
        foreach (var i in _selectedBlocks)
        {
            var block = i.Value;
            if (!block.CanBeDeletedManually())
                continue;
            
            RemoveBlock(block.GetPos());
        }
        
        DeselectAllBlocks();
    }

    public void EditSelectedBlocks()
    {
        List<int> alreadyStoredSettings = new();
        Block lastSettings = null;
        
        foreach (var i in _selectedBlocks)
        {
            var block = i.Value;
            if (block.ShouldShowSettings())
            {
                if (!alreadyStoredSettings.Contains(block.blockType))
                {
                    alreadyStoredSettings.Add(block.blockType);
                    lastSettings = block;
                }
            }
        }
        if (alreadyStoredSettings.Count <= 0 || alreadyStoredSettings.Count > 1)
            return;
        
        EditBlock(lastSettings);
    }

    public void RemoveBlock(Vector2 pos, int desiredLayer = -10, bool log = false)
    {
        desiredLayer = desiredLayer == -10 ? layer : desiredLayer;
        
        if (!_enabled || !_allowEditor)
            return;
        if (!HasAnyBlockAt(pos))
            return;
        
        if (log)
            _historyManager.AddAction(new HistoryAction.BlockDeleteAction()
            {
                position = pos,
                block = GetBlock(pos).GetComponent<Block>()
            });

        Destroy(GetLayerByIndex(desiredLayer)[pos]);
        GetLayerByIndex(desiredLayer).Remove(pos);
    }
    
    public void RemoveBlockAtAllLayers(Vector2 pos)
    {
        if (!_enabled || !_allowEditor)
            return;
        if (!HasAnyBlockAt(pos))
            return;

        foreach (var layer in layers)
        {
            if (layer.Value.ContainsKey(pos))
            {
                Destroy(layer.Value[pos]);
                layer.Value.Remove(pos);
            }
        }
    }

    public void ChangeBlockType(int type)
    {
        if (!_enabled || !_allowEditor)
            return;
        
        blockType = type;
        if (type == -1)
        {
            blockPlacer.currentPrefab = null;
            blockPlacer.UseEditSprite();
        }
        else
        {
            var previous = blockPlacer.currentPrefab;
            
            blockPlacer.currentPrefab = blockPlacer.BlockPrefabs[type];
            blockPlacer.UpdatePrefab();
            
            if (blockPlacer.currentPrefab.GetComponent<Trigger>() != null)
                SetLayer(-1);
            
            if (previous != null && previous.GetComponent<Trigger>() != null && blockPlacer.currentPrefab.GetComponent<Trigger>() == null)
                SetLayer(_lastNonTriggerLayer);
        }
    }

    public void ClearPlayingData(bool includeDeaths = true)
    {
        stars = 0;
        dots = 0;
        collectedExperience = 0;
        if (includeDeaths)
            deaths = 0;
    }
    
    public void AddLevelProgression(long amount)
    {
        if (ShouldIncreaseLevelProgression())
            collectedExperience += amount;
    }

    public Vector2 MapToGrid(Vector2 pos)
    {
        return new Vector2(Mathf.RoundToInt(pos.x / gridSize), Mathf.RoundToInt(pos.y / gridSize));
    }

    public Vector2 GridToMap(Vector2 pos)
    {
        return new Vector2(pos.x * gridSize, pos.y * gridSize);
    }
    
    public Vector2 MapToGridAbs(Vector2 pos)
    {
        return new Vector2(Mathf.RoundToInt(pos.x / gridSize) * gridSize, Mathf.RoundToInt(pos.y / gridSize) * gridSize);
    }
    
    public Vector2 MapToHalfTheGrid(Vector2 pos)
    {
        pos.x -= 0.25f;
        pos.y += 0.25f;
        Vector2 vector2 = new Vector2(Mathf.RoundToInt(pos.x / (gridSize / 2)) * (gridSize / 2),
            Mathf.RoundToInt(pos.y / (gridSize / 2)) * (gridSize / 2));
        return vector2;
    }

    public KeyValuePair<Vector2, GameObject>? GetSpawnInfos()
    {
        foreach (var aLayer in layers)
            foreach (var block in aLayer.Value)
                if (!block.Value.IsDestroyed())
                    if (block.Value.GetComponent<SpawnBlock>() != null) 
                        return block;
    
        return null;
    }
    
    public Vector2 GetSpawnLocation()
    {
        return GetSpawnInfos()?.Key ?? new Vector2(99999999, 99999999);
    }

    public GameObject GetSpawnObject()
    {
        return GetSpawnInfos()?.Value;
    }

    public bool EditorMode()
    {
        return editor;
    }

    public void ToggleEditor()
    {
        if (!_enabled || !_allowEditor)
            return;
        
        if (editor) // disable the editor
        {
            DisableEditor();
        }
        else // enable the editor
        {
           EnableEditor();
        }
    }
    
    public void RefreshBlocksDisplayedColor()
    {
        foreach (var layerData in layers)
        {
            var loopLayer = layerData.Key;
            foreach (var block in layerData.Value)
            {
                if (block.Value.IsDestroyed())
                    continue;
                
                if (block.Value.GetComponent<DecorationBlock>() != null)
                    continue;
                
                var bl = block.Value.GetComponent<Block>();
                var trigger = bl.IsTrigger();

                float alpha;
                
                if (layer == -1) 
                    alpha = trigger ? 1f : 0.4f;
                else
                    alpha = trigger ? 0.4f : (loopLayer == layer ? 1f : 0.4f);
                
                if (bl.uniqueLayer)
                    alpha = 1f;
                
                var color = bl._renderer.color;
                color.a = alpha;
                
                bl._renderer.color = color;
            }
        }
    }

    public void NextLayer()
    {
        SetLayer(layer + 1);
    }

    public void PreviousLayer()
    {
        if (GetLayerByIndex(layer).Count == 0)
            layers.Remove(layer); // remove empty layer

        SetLayer(layer - 1);
    }

    public void TriggerLayer()
    {
        SetLayer(-1);
    }

    public void SetLayer(int layer)
    {
        if (layer < -1) layer = -1;
        if (layer > 100) layer = 100;
        
        this.layer = layer;
        RefreshBlocksDisplayedColor();
        
        layerText.text = layer == -1 ? "Triggers" :  "Layer " + (layer);
        if (layer != -1) _lastNonTriggerLayer = layer;
    }

    public void SelectBlock(Block block)
    {
        if (block == null)
            return;

        if (_selectedBlocks.ContainsKey(block.GetInstanceID()))
        {
            _selectedBlocks.Remove(block.GetInstanceID());
            block.OnDeselect();
        }
        else
        {
            _selectedBlocks.Add(block.GetInstanceID(), block);
            block.OnSelect();
        }
    }
    
    public void DeselectAllBlocks()
    {
        foreach (var block in _selectedBlocks)
            block.Value.OnDeselect();
        
        _selectedBlocks.Clear();
    }

    public void EnableEditor()
    {
        if (!CheckValidLevel() || !_allowEditor)
            return;
     
        gridController.gameObject.SetActive(true);
        colorLogic.EnableEditor();
        layer = _lastLayer;
        foreach (var obj in editorToolbox) obj.SetActive(true);
        ClearPlayingData();
        editor = true;
        GetSpawnObject()?.SetActive(true);
        blockPlacer.Enable();
        player.Disable();

        List<object[]> blocksToRemove = new ();
        foreach (var layer in layers)
            foreach (var block in layer.Value)
                if (!block.Value.IsDestroyed())
                {
                    var bl = block.Value.GetBlock();
                    if (bl.ShouldBeDesactivatedInCurrentState()) 
                        bl.Desactivate();
                    else 
                        bl.Enable();
                }
                else
                {
                    // remove destroyed blocks
                    blocksToRemove.Add(new object[] {layer.Key, block.Key});
                }
        
        foreach (var block in blocksToRemove)
            GetLayerByIndex((int) block[0]).Remove((Vector2) block[1]);
        
        RefreshBlocksDisplayedColor();
            
        // camera
        _camera.orthographicSize = _previousZoom;
        particles.SetActive(false);
        entranceDoor.SetActive(false);
        entranceDoor.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    public void DisableEditor()
    {
        if (GetSpawnObject() == null)
        {
            notifications.ShowError("a spawn block is missing.");
            return;
        }
        
        gridController.gameObject.SetActive(false);
        colorLogic.DisableEditor();
        editor = false;
        GetSpawnObject()?.SetActive(false);
        blockPlacer.Disable();
        player.Enable();
        player.speed = levelSettings.playerSpeed;
        
        foreach (var obj in editorToolbox) obj.SetActive(false);
        DeselectAllBlocks();
        
        var blocks = new List<KeyValuePair<Vector2, GameObject>>();
        List<object[]> blocksToRemove = new ();
        foreach (var block in layers)
        {
            foreach (var block2 in block.Value)
            {
                blocks.Add(block2);
            }
        }
        
        blocks.RemoveAll(block => block.Value.IsDestroyed());
        
        // sort 'blocks' by id
        blocks.Sort((a, b) => 
            a.Value.GetComponent<Block>().blockType.CompareTo(b.Value.GetComponent<Block>().blockType));
        foreach (var block2 in blocks)
        {
            if (block2.Value.IsDestroyed())
            {
                // remove destroyed blocks
                // block.Value.Remove(block2.Key);
                blocksToRemove.Add(new object[] {block2.Key, block2.Key});
                continue;
            }
            var bl = block2.Value.GetBlock();
            if (bl.ShouldBeDesactivatedInCurrentState()) 
                bl.Activate();
            else 
                bl.Disable();
        }
        
        foreach (var block in blocksToRemove)
            GetLayerByIndex((int) block[0]).Remove((Vector2) block[1]);
        
        Dictionary<long, PortalBlock> stored = new();
        foreach (var _blocks in layers)
        {
            foreach (var block in _blocks.Value.Values)
            {
                if (block == null || block.IsDestroyed())
                    continue;
                
                var portal = block.GetComponent<PortalBlock>();
                if (portal == null)
                    continue;

                long id = portal.portalId;
                if (stored.ContainsKey(id))
                {
                    portal.linked = stored[id];
                    portal.linked.linked = portal;
                    stored.Remove(id);
                }
                else
                {
                    stored.Add(id, portal);
                }
            }
        }
        
        // Restore opacity of all blocks
        foreach (var bl in GetAllBlocks())
        {
            if (bl.GetComponent<DecorationBlock>() != null)
                continue;
            var color = bl._renderer.color;
            color.a = 1f;
            bl._renderer.color = color;
        }

        // camera
        _previousZoom = _camera.orthographicSize;
        _camera.orthographicSize = CameraZoom;
        particles.SetActive(true);

        if (levelSettings.showDoor)
        {
            entranceDoor.transform.position = new Vector3(GetSpawnLocation().x + 0.75f, GetSpawnLocation().y + 0.4f, 0);
            entranceDoor.SetActive(true);
            entranceDoor.GetComponent<EntranceDoorController>().PlayAnimation();

            var degrees = (levelSettings.spawnRotation - 1) * 90;
            // 1: down, 2: right, 3: up, 4: left
            entranceDoor.transform.RotateAround(GetSpawnLocation(), Vector3.forward, degrees);
        }
        
        _lastLayer = layer;
        SetLayer(0);
    }

    public void WinLevel()
    {
        
        if (_allowEditor) // don't win in editor mode
        {
            DataManager.CustomLevelInfo cli = DataManager.GetLocalLevel(localLevelId);
            if (cli != null) {
                cli.verified = true;
                if (levelSettings.levelType == LevelType.Color) 
                    cli.coloredBlocksCount = colorLogic.GetColoredBlockCount();
                DataManager.SetLocalLevel(cli);
                print($"local level with id {localLevelId} is now verified. (colored blocks: {cli.coloredBlocksCount})");
            }
            
            player.Die(true);
            return;
        }

        player.Disable();
        StopShieldStuff();

        /* MainController.IsFromPlayingLevel = true;
        SceneManager.LoadScene("MainMenu"); */
        
        levelFinishController.collectedDots = dots;
        levelFinishController.collectedStars = stars;
        levelFinishController.collectedExperience = collectedExperience;
        
        levelFinishController.RefreshScore(settings.levelIndex == -1, deaths, (int) collectedExperience);
        levelFinishController.gameObject.SetActive(true);
        _enabled = false; // disable the level

        if (settings.GetLevelId() != -1)
        {
            var gli = PlayerStatsManager.GetLevelProgress(settings.GetLevelId(), settings.IsOfficial());
            gli.collectedStars = Mathf.Max(stars, gli.collectedStars);
            gli.collectedDots = Mathf.Max(dots, gli.collectedDots);
            gli.AddDeaths(deaths);
            gli.completed = true;
            PlayerStatsManager.SaveLevelProgress(gli);   
        }

        PlayerStatsManager.AddCollectedDots(collectedExperience);

        if (ShouldIncreaseLevelProgression())
        {
            print("increasing level progression");
            Achievements.IncrementAchievementGroup(1);
            Achievements.IncrementAchievementGroup(2, dots);
        }
    }

    public void UpdateBlocksColor()
    {
        var color = GetColor();
        foreach (var layer in layers)
        {
            foreach (var block in layer.Value)
            {
                block.Value.GetComponent<Block>().UpdateBlockColor(color);
                if (block.Value.GetComponent<FullFrameBlock>() != null)
                    block.Value.GetComponent<FullFrameBlock>().ResetSprite();
            }
        }
        
        foreach (var rend in coloredRenderers)
            rend.color = new Color(color.r, color.g, color.b, rend.color.a);
        
        blockPlacer.UpdateSprite();
        
        foreach (var renderer in coloredRenderers)
            renderer.color = new Color(color.r, color.g, color.b, renderer.color.a);
    }

    public void UpdateCamera()
    {
        _camera.orthographicSize = CameraZoom;
    }

    public void RestartLevel()
    {
        if (_allowEditor)
            return;
        
        player.Enable();
        player.Die(true); // kill the player (it'll also clear the data, see OnPlayerDeath)
        ClearPlayingData(true); // killing the player will NOT reset its death count, we do it manually
        _enabled = true;
        EnableEditor();
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }
    
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void OnPlayerDeath(bool restart = false)
    {
        if (!player.enabled)
            return;
        
        colorLogic.OnPlayerDeath();
        
        foreach (var _blocks in layers)
        {
            foreach (var block in _blocks.Value)
                if (block.Value != null && !block.Value.IsDestroyed()) 
                    block.Value.GetComponent<Block>().OnPlayerDeath();
        }
        ClearPlayingData(false);
        deaths++;
        deathStatText.text = deaths.ToString();

        if (!restart) 
        {
            if (ShouldIncreaseLevelProgression())
                Achievements.IncrementAchievementGroup(0);
            
            PlaySound("Death");
        }
        
        player.UseNormalSkin();
    }
    
    public bool CheckValidLevel()
    {
        // must have spawn block
        if (GetSpawnInfos() == null)
        {
            // UIController.ShowError("Must have spawn block!");
            return false;
        }

        return true;
    }
    
    public GameObject GetBlock(Vector2 pos)
    {
        if (GetLayerByIndex(layer).TryGetValue(pos, out var block))
            return block;
        foreach (var layer in layers)
        {
            if (layer.Value.TryGetValue(pos, out var anyLayer))
                if (anyLayer.GetComponent<Block>().uniqueLayer)
                    return anyLayer;
        }
        return null;
    }
    
    public List<GameObject> GetBlockAtAllLayers(Vector2 pos)
    {
        List<GameObject> blocks = new();
        foreach (var layer in layers)
        {
            if (layer.Value.ContainsKey(pos))
                blocks.Add(layer.Value[pos]);
        }

        return blocks;
    }
    
    public GameObject GetFirstBlockAtAnyLayer(Vector2 pos)
    {
        foreach (var layer in layers)
        {
            if (layer.Value.TryGetValue(pos, out var anyLayer))
                return anyLayer;
        }

        return null;
    }

    [CanBeNull]
    public Block SearchBlock(Predicate<Block> predicate)
    {
        foreach (var _blocks in layers)
        {
            foreach (var obj in _blocks.Value.Values)
            {
                var block = obj.GetComponent<Block>();
                if (predicate.Invoke(block))
                    return block;
            }
        }

        return null;
    }
    
    public GameObject CreateBlock(int type)
    {
        if (!blockPlacer.BlockPrefabs.ContainsKey(type))
        {
            Debug.LogError("Block type " + type + " doesn't exist!");
            return null;
        }
        return Instantiate(blockPlacer.BlockPrefabs[type], new Vector3(0, 0, 0), Quaternion.identity);
    }
    
    public bool HasBlockAt(Vector2 pos)
    {
        return GetLayerByIndex(layer).ContainsKey(pos);
    }
    
    public bool HasAnyBlockAt(Vector2 pos)
    {
        return GetLayerByIndex(layer).ContainsKey(pos);
    }
    
    public List<GameObject> GetBlockAtRelativeAtAllLayers(Block block, Vector2 pos)
    {
        List<GameObject> blocks = new();
        foreach (var layer in layers)
        {
            if (layer.Value.ContainsKey(block.GetPos() + pos))
                blocks.Add(layer.Value[block.GetPos() + pos]);
        }

        return blocks;
    }
    
    public bool AllowEditor()
    {
        return _allowEditor;
    }

    public void SaveLevel()
    {
        if (GetSpawnObject() == null)
        {
            notifications.ShowError("can't save: a spawn block is missing.");
            ClosePause();
            return;
        }
        
        ClosePause();
        notifications.ShowNotification("saving...");
        DataManager.CustomLevelInfo cli = DataManager.GetLocalLevel(localLevelId);
        if (cli == null)
        {
            notifications.ShowError("local level not found, creating a new one.");
            cli = new DataManager.CustomLevelInfo()
            {
                name = "# Editor Level",
                saveData = new SaveData(this),
                lastModified = DateTime.Now.Ticks,
                creationDate = DateTime.Now.Ticks,
                id = localLevelId
            };
        }
        
        cli.saveData = new SaveData(this);
        cli.lastModified = DateTime.Now.Ticks;
        cli.verified = false;
        cli.coloredBlocksCount = -1;
        
        DataManager.SetLocalLevel(cli);
        DataManager.SaveLocalLevels();
        
        notifications.ShowNotification("level saved!");
        hasSavedYet = true;
    }
    
    public void ClosePause()
    {
        pauseMenu.SetActive(false);
        hasUIOpen = false;
    }

    public bool ShouldIncreaseLevelProgression()
    {
        if (settings.levelIndex != -1)
            return true;
        if (_allowEditor)
            return false;
        
        return settings.IsVerified();
    }

    public bool HasBlockAtRelative(Block block, Vector2 pos)
    {
        return HasBlockAt(block.GetPos() + pos);
    }
    
    public bool HasSolidBlockAtRelative(Block block, Vector2 pos)
    {
        return HasBlockAtRelative(block, pos) && GetBlock(block.GetPos() + pos).GetComponent<Block>().isSolid;
    }
    
    public Block GetBlockAtRelative(Block block, Vector2 pos)
    {
        return GetBlock(block.GetPos() + pos)?.GetComponent<Block>();
    }

    public void UpdateDebugText()
    {
        var pos = blockPlacer.GetMousePos();
        debugText.text = "X: " + Mathf.RoundToInt(pos.x) + ", Y: " + Mathf.RoundToInt(pos.y)
                         + "\nLayer: " + layer
                         + "\nBlocks Sel. : " + _selectedBlocks.Count
                         + "\nRotation (" + KeybindManager.RotateBlock.value.ToString() + "): " + blockPlacer.rotation;
    }

    private SpawnBlock _spawnScript;
    public SpawnBlock GetSpawnScript()
    {
        if (_spawnScript != null)
            return _spawnScript;
        
        _spawnScript = GetSpawnObject()?.GetComponent<SpawnBlock>();
        return _spawnScript;
    }

    public void PlayLevel()
    {
        pauseMenu.SetActive(false);
        ToggleEditor();
        hasUIOpen = false;
    }

    public class GameLevelSettings
    {

        public SaveData saveData;
        public int localLevelId;
        public bool allowEditor;
        /**
         * -1 means custom level
         */
        public int levelIndex;

        public NewDB.Level level;
        public NewDB.LevelStats levelStats;
        public NewDB.LevelOnlineSettings levelOnlineSettings;
        
        public DataManager.CustomLevelInfo customLevelInfo;
        
        public bool wasFromSaved;

        public GameLevelSettings(SaveData saveData, bool allowEditor, int levelIndex)
        {
            this.saveData = saveData;
            this.localLevelId = -1;
            this.allowEditor = allowEditor;
            this.levelIndex = levelIndex;
        }

        public GameLevelSettings(DataManager.CustomLevelInfo cli, bool allowEditor)
        {
            this.saveData = cli.saveData;
            this.localLevelId = cli.id;
            this.allowEditor = allowEditor;
            this.customLevelInfo = cli;
            this.levelIndex = -1;
        }
        
        public GameLevelSettings(NewDB.Level level, NewDB.LevelStats levelStats, NewDB.LevelOnlineSettings levelOnlineSettings, SaveData saveData, bool allowEditor, bool wasFromSaved)
        {
            this.saveData = saveData;
            this.localLevelId = -1;
            this.allowEditor = allowEditor;
            this.levelIndex = -1;
            
            this.level = level;
            this.levelStats = levelStats;
            this.levelOnlineSettings = levelOnlineSettings;
            this.wasFromSaved = wasFromSaved;
        } 
        
        public bool IsVerified()
        {
            return level is { verified: true };
        }
        
        public bool IsOfficial()
        {
            return levelIndex != -1;
        }
        
        public int GetLevelId()
        {
            return levelIndex != -1 ? levelIndex : level?.id ?? customLevelInfo.publishId;
        }

        public int GetColoredBlockCount()
        {
            return level?.coloredBlocks ?? customLevelInfo.coloredBlocksCount;
        }
        
    }
    
    public void LeaveLevel(bool toMainMenu)
    {
        if (_allowEditor && !hasSavedYet)
        {
            var newPopup = Instantiate(popupPrefab, canvas.transform);
            var popupController = newPopup.GetComponent<PopupController>();
            
            popupController.Init("Unsaved", "Are you sure you want to leave without saving?",
                () =>
                {
                    SceneManager.LoadScene("MainMenu");
                },
                () =>
                {
                    Destroy(newPopup);
                });
            
            return;
        }
        SceneManager.LoadScene("MainMenu");
    }
    
    public void OpenPause()
    {
        pauseMenuText.text = editor ? "Play" : "Edit";
        ((RectTransform) pauseMenuPanel.transform).sizeDelta = new Vector2(400, _allowEditor ? 600 : 400);
        pauseMenu.SetActive(true);
        hasUIOpen = true;
    }

    public void NextLevel()
    {
        settings = new GameLevelSettings(LevelsUIManager.levels[settings.levelIndex], false, settings.levelIndex + 1);
        TransitionManager.SwitchToLevelScene();
    }

    public void EditBlock(Block block)
    {
        if (block == null)
            return;
        
        blockSettings.LoadBlockSettings(block);
        blockSettings.gameObject.SetActive(true);
        hasUIOpen = true;
    }
    
    public int GetBlockCount(int type)
    {
        int count = 0;
        foreach (var _blocks in layers)
        {
            foreach (var block in _blocks.Value)
            {
                if (block.Value.GetComponent<Block>().blockType == type)
                    count++;
            }
        }

        return count;
    }
    
    public void CloseBlockSettings()
    {
        blockSettings.gameObject.SetActive(false);
        hasUIOpen = false;
    }

    public void ChangeTool(int tool)
    {
        if (tool < 0 || tool > 3)
            return;
        
        this.tool = tool;
        blockPlacer.UpdateSprite();
        
        selects.SetActive(tool == 3);
    }
    
    // ############################

    public static bool CanLoadLevel(SaveData saveData)
    {
        return saveData.HasSpawnPoint();
    }

    public void CollectCoin()
    {
        dots++; // coins = enhanced dots
        
        PlayerStatsManager.CollectCoin();
        coinsStatText.text = PlayerStatsManager.GetCoins().ToString();
    }

    public Color GetColor()
    {
        return levelSettings.GetColor();
    }

    public void UseShield()
    {
        if (player.HasShield)
            return;
        
        var shields = PlayerStatsManager.GetShields();
        if (shields < 1)
            return;
        
        PlayerStatsManager.SetShields(shields - 1);
        shieldAmountText.text = (shields - 1).ToString();
        player.SetShield(true);
        Achievements.IncrementAchievementGroup(3);
    }

    public void StopShieldStuff()
    {
        shieldObject.SetActive(false);
        player.SetShield(false);
    }

    public void UpdateGrid()
    {
        gridController.gameObject.SetActive(ShowEditorGrid);
    }

    public void OpenWelcomeBook()
    {
        helpTextsManager.ShowHelpTextInternal("EditorWelcome");
    }

}
