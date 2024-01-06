using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public static readonly long DefaultPlayerSpeed = 35;
    
    public float speed = DefaultPlayerSpeed;
    public int rotation; // 1: down, 2: right, 3: up, 4: left
    public LogicScript logic;
    public new bool enabled;
    public bool noClip;
    public Collider2D collider2D;
    public bool canMove = true;
    
    public bool plateformerMode = false;
    
    [Header("Shield Control")]
    public GameObject shieldEffect;
    
    public bool HasShield { get; private set; }
    public void SetShield(bool value)
    {
        HasShield = value;
        shieldEffect.SetActive(value);

        if (value)
        {
            logic.shieldTimer.maxValue = 1f;
            logic.shieldTimer.value = 1f;   
        }
        
        logic.shieldTimer.gameObject.SetActive(value);
        logic.shieldButton.interactable = !value;
    }
    
    [Header("Effects")]
    public GameObject landingEffectPrefab;
    
    private Vector2 _lastSafePos;
    private int _lastSafeRot;
    
    private Rigidbody2D _rb;
    private Transform _transform;
    private TrailRenderer _trailRenderer;
    private Animator _animator;
    private Rigidbody2D _rigidbody2D;
    private SpriteRenderer _spriteRenderer;
    private bool _isFlying;
    [CanBeNull] [HideInInspector] public Rotor attachedRotor;

    private bool _isPaused;
    private Vector3 _velocityBeforePause;
    private int _rotationBeforePause;
    
    // Events
    private void Awake()
    {
        GameStateManager.Instance.OnGameStateChanged += OnGameStateChanged;
        
        Physics2D.IgnoreLayerCollision(3, 10, true);
    }

    private void OnDestroy()
    {
        GameStateManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }
    
    private void OnGameStateChanged(GameState newGameState)
    {
        ((Behaviour) this).enabled = newGameState == GameState.Gameplay;
        if (newGameState == GameState.Gameplay)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _transform = GetComponent<Transform>();
        _trailRenderer = GetComponent<TrailRenderer>();
        _animator = GetComponent<Animator>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<Collider2D>();

        Disable();
    }

    public void EnableTrail()
    {
        _trailRenderer.Clear();
        _trailRenderer.enabled = true;
    }

    public void DisableTrail()
    {
        _trailRenderer.Clear();
        _trailRenderer.enabled = false;
    }

    public void SetupTrail(Constants.Trail trail)
    {
        _trailRenderer.material = trail.GetMaterial();
        
        _trailRenderer.startColor = trail.color;
        _trailRenderer.endColor = trail.color;
    }

    // Update is called once per frame
    void Update()
    {
        _transform.rotation = Quaternion.Euler(0, 0, (rotation - 1) * 90);
        /* if (_isFlying && !IsMoving())
            UseNormalSkin(); */
        
        // check for arrow input
        if (!IsMoving() && enabled)
        {
            if (Input.GetKey(KeybindManager.MovePlayerLeft.value))
                RotateAndMove(4);
            else if (Input.GetKey(KeybindManager.MovePlayerRight.value))
                RotateAndMove(2);
            else if (Input.GetKey(KeybindManager.MovePlayerUp.value))
                RotateAndMove(3);
            else if (Input.GetKey(KeybindManager.MovePlayerDown.value))
                RotateAndMove(1);
            
            // Check if there's empty block under the player
            Vector2 pos = logic.MapToGrid(_transform.position);
            if (logic.GetBlock(new Vector2(pos.x - 1, pos.y)) == null && rotation == 4)
                RotateAndMove(4);
            else if (logic.GetBlock(new Vector2(pos.x + 1, pos.y)) == null && rotation == 2)
                RotateAndMove(2);
            else if (logic.GetBlock(new Vector2(pos.x, pos.y - 1)) == null && rotation == 1)
                RotateAndMove(1);
            else if (logic.GetBlock(new Vector2(pos.x, pos.y + 1)) == null && rotation == 3)
                RotateAndMove(3);
            
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 touchPos = touch.position;
                Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
                Vector2 diff = touchPos - screenCenter;
                if (diff.magnitude > 100)
                {
                    if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y))
                    {
                        RotateAndMove(diff.x > 0 ? 2 : 4);
                    }
                    else
                    {
                        RotateAndMove(diff.y > 0 ? 3 : 1);
                    }
                }
            }

        }
        
        if (_rigidbody2D.velocity == Vector2.zero)
            UseNormalSkin();

        var shieldDuration = Constants.GetBonusValue(1);
        // shield timer
        if (HasShield)
        {
            logic.shieldTimer.value -= Time.deltaTime / shieldDuration;
            if (logic.shieldTimer.value <= 0)
            {
                SetShield(false);
            }
        }
        else
        {
            logic.shieldTimer.value += Time.deltaTime / shieldDuration;
            if (logic.shieldTimer.value >= 1)
            {
                logic.shieldButton.interactable = true;
            }
        }
    }

    public void StopMoving()
    {
        _rb.velocity = Vector2.zero;
    }
    
    void Rotate(int rot)
    {
        rotation = rot;
    }

    public void RotateAndMove(int rot, bool force = false)
    {
        if ((rot == rotation || !canMove) && !force)
            return;
        
        Rotate(rot);

        // velocity according to rotation
        Vector2 velocity;
        if (rotation == 1)
            velocity = Vector2.down;
        else if (rotation == 4)
            velocity = Vector2.left;
        else if (rotation == 3)
            velocity = Vector2.up;
        else
            velocity = Vector2.right;

        _rb.velocity = velocity * speed;
        _transform.position = logic.MapToGridAbs(_transform.position);
        StartFlight();
        
        if (attachedRotor != null)
            attachedRotor.hasPlayer = false;
    }
    
    public void Hide()
    {
        _trailRenderer.enabled = false;
        _spriteRenderer.enabled = false;
    }
    
    public void Show()
    {
        _trailRenderer.Clear();
        _trailRenderer.enabled = true;
        _spriteRenderer.enabled = true;
    }
    
    public bool IsMoving()
    {
        return _rb.velocity != Vector2.zero;
    }

    public void Enable()
    {
        TeleportToSpawn();
        gameObject.SetActive(true);
        enabled = true;
        
        _trailRenderer.enabled = true;
        _trailRenderer.Clear();
        _spriteRenderer.enabled = true;
        _rigidbody2D.simulated = true;
        
        UseNormalSkin();
    }

    public void TeleportToSpawn()
    {
        var spawnInfos = logic.GetSpawnInfos();
        if (spawnInfos != null)
        {
            Vector2 spawnPoint = spawnInfos.Value.Key;
            int rot = (int)logic.levelSettings.spawnRotation;
        
            gameObject.transform.position = new Vector3(spawnPoint.x, spawnPoint.y, 0);
            RotateAndMove(rot, true);
        }
    }
    
    public void Teleport(Vector2 pos)
    {
        gameObject.transform.position = new Vector3(pos.x, pos.y, 0);
    }
    
    public void OnTriggerEnter2D(Collider2D col)
    {
        var block = col.gameObject.GetComponent<Block>();
        if (block != null && block.isSpike)
            Die();
        
        if (col.gameObject.GetComponent<ArrowController>() != null)
            Die();
        if (col.gameObject.GetComponent<LavaController>() != null)
            Die();

        var tempSpikes = col.gameObject.GetComponent<ReleaseSpikes>();
        if (tempSpikes != null && tempSpikes.ShouldHurt())
            Die();
        
        if (col.gameObject.GetComponent<Coin>() != null)
            col.gameObject.GetComponent<Coin>().OnCollect(this);
        
        var padBlock = col.gameObject.GetComponent<PadBlock>();
        if (padBlock != null)
            padBlock.CollideWithPlayer();

        var portalBlock = col.gameObject.GetComponent<PortalBlock>();
        if (portalBlock != null)
            portalBlock.CollideWithPlayer();
        
    }
    
    public void Disable()
    {
        //gameObject.SetActive(false);
        enabled = false;
        _trailRenderer.enabled = false;
        _spriteRenderer.enabled = false;
        _rigidbody2D.simulated = false;
        
        StopMoving();
        
        foreach (var obj in _landingEffects)
            if (obj != null && !obj.IsDestroyed()) 
                Destroy(obj);
        _landingEffects.Clear();
    }

    public void Pause()
    {
        _velocityBeforePause = _rigidbody2D.velocity;
        _rotationBeforePause = rotation;
        
        _rigidbody2D.simulated = false;
    }
    
    public void Resume()
    {
        _rigidbody2D.simulated = true;
        _rigidbody2D.velocity = _velocityBeforePause;
        
        RotateAndMove(_rotationBeforePause);
    }

    public void Die(bool restart = false)
    {
        if (noClip || !enabled)
            return;

        if (HasShield && !((int) _lastSafePos.x == (int) _transform.position.x && (int) _lastSafePos.y == (int) _transform.position.y))
        {
            SetShield(false);
            _transform.position = _lastSafePos;
            RotateAndMove(_lastSafeRot);
            return;
        }
        
        if (HasShield)
            SetShield(false);
        
        _trailRenderer.Clear();
        _trailRenderer.enabled = false;
        logic.OnPlayerDeath(restart);
        TeleportToSpawn();
        _trailRenderer.enabled = true;
        
        enabled = false;
        StartCoroutine(ResetAnimation());
        StartCoroutine(UnlockMovement());
        
        UseNormalSkin();
    }
    
    private IEnumerator ResetAnimation() {
        yield return new WaitForSeconds(0.05f);
        UseNormalSkin();
    }
    
    private IEnumerator UnlockMovement() {
        yield return new WaitForSeconds(0.1f);
        enabled = true;
    }
    
    public Vector2 GetPos()
    {
        return logic.MapToGrid(_transform.position);
    }

    public void SetPos(Vector2 pos)
    {
        var real = logic.GridToMap(pos);
        _transform.position = real;
    }

    private List<GameObject> _landingEffects = new();

    private void OnCollisionEnter2D(Collision2D other) // CollisionEnter is only called for solid objects (aka walls)
    {
        AudioManager.PlaySound("PlayerHit");
        //UseNormalSkin();
        
        var block = other.gameObject.GetComponent<Block>();
        if (block == null)
            return;
        
        _lastSafePos = _transform.position;
        _lastSafeRot = rotation;

        if (block.attachedRotor != null)
        {
            attachedRotor = block.attachedRotor;
            attachedRotor.hasPlayer = true;
        }
        
        // landing effect
        var landingEffect = Instantiate(landingEffectPrefab, _transform.position, Quaternion.identity);
        landingEffect.transform.rotation = Quaternion.Euler(0, 0, (rotation - 1) * 90);
        landingEffect.GetComponent<SpriteRenderer>().color = logic.GetColor();
        _landingEffects.Add(landingEffect);
        
        StartCoroutine(DeleteLandingEffect(landingEffect));
    }
    
    private IEnumerator DeleteLandingEffect(GameObject landingEffect)
    {
        yield return new WaitForSeconds(0.58f);
        Destroy(landingEffect);
        _landingEffects.Remove(landingEffect);
    }
    
    public void UseNormalSkin()
    {
        _animator.Play(Constants.GetPlayerIdleAnimationFromSkin(PlayerStatsManager.GetSelectedSkin()));
        _isFlying = false;
    }
    
    public void StartFlight()
    {
        _animator.Play("PlayerFlight");
        _isFlying = true;
    }

    public static Vector2Int DirectionToVector(int direction)
    {
        if (direction == 1)
            return Vector2Int.down;
        if (direction == 2)
            return Vector2Int.right;
        if (direction == 3)
            return Vector2Int.up;
        if (direction == 4)
            return Vector2Int.left;
        return Vector2Int.zero;
    }
    
    public static Vector2Int InversedDirectionToVector(int direction)
    {
        if (direction == 1)
            return Vector2Int.up;
        if (direction == 2)
            return Vector2Int.left;
        if (direction == 3)
            return Vector2Int.down;
        if (direction == 4)
            return Vector2Int.right;
        return Vector2Int.zero;
    }
}
