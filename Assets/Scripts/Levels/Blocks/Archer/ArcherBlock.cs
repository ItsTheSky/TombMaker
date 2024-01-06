using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ArcherBlock : Block
{
    public GameObject arrowPrefab;
    public Animator animator;
   

    public long direction = 2; // 1: down, 2: right, 3: up, 4: left
    public long shootSpeed = 5;
    public long arrowSpeed = 4;
    
    [Header("Animations")]
    public AnimationClip archerIdleAnimation;
    public AnimationClip archerShootAnimation;

    private bool _shouldShoot;
    private int _lastRotorDegrees = 0;
    
    private List<GameObject> _arrows = new ();
    private Color _normalColor;
    
    public ArcherBlockChecker blockInFrontCollider;
    [HideInInspector] public bool canShoot = true;
 
    public override void UpdateBlockColor(Color color)
    {
        base.UpdateBlockColor(color);
        _normalColor = color;
        _renderer.color = color;
    }

    public override void Init()
    {
        base.Init();
        uniqueLayer = true;

        options.Add(new BlockSettingController.DropdownOption("direction",
            "Direction", "Change the direction the archer will shoot int.",
            new []{"Down", "Right", "Up", "Left"}, (int) direction - 1, value =>
            {
                direction = value + 1;
                UpdateShownRotation();
            }));
        options.Add(new BlockSettingController.SliderOption("shootSpeed",
            "Shoot Speed", "Change the speed the archer will shoot at.",
            1, 15, true, shootSpeed, value => shootSpeed = (int) value));
        options.Add(new BlockSettingController.SliderOption("arrowSpeed",
            "Arrow Speed", "Change the speed the arrow will fly at.",
            1, 15, true, arrowSpeed, value => arrowSpeed = (int) value));
    }

    public override void Enable()
    {
        CheckInit();

        _shouldShoot = false;
        foreach (var arrow in _arrows)
            if (arrow != null && arrow.activeSelf && !arrow.IsDestroyed())
                Destroy(arrow);
        _arrows.Clear();
        
        _renderer.color = _normalColor;
    }

    public override bool CanRotate()
    {
        return true;
    }

    public override void SetupRotation(int blockPlacerRotation)
    {
        if (blockPlacerRotation == 0)
            direction = 2;
        else if (blockPlacerRotation == 1)
            direction = 1;
        else if (blockPlacerRotation == 2)
            direction = 4;
        else direction = 3;
        
        UpdateShownRotation();
        GetOptionById("direction").SetValue((long) direction - 1);
    }

    public override void Disable()
    {
        CheckInit();
        _lastRotorDegrees = 0;

        animator.Play("ArcherIdle");
        _renderer.color = _normalColor;
        
        _shouldShoot = true;

        UpdateShownRotation();
    }

    public void UpdateShownRotation()
    {
        if (direction == 1) {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 270);
            gameObject.GetComponent<SpriteRenderer>().flipY = false;
        } else if (direction == 2) {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            gameObject.GetComponent<SpriteRenderer>().flipY = false;
        } else if (direction == 3) {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 90);
            gameObject.GetComponent<SpriteRenderer>().flipY = false;
        } else if (direction == 4) {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 180);
            gameObject.GetComponent<SpriteRenderer>().flipY = true;
        }
    }

    public void Shoot()
    {
        if (!canShoot)
        {
            return;
        }
        
        AudioManager.PlaySound("CannonShoot");
        animator.Play("ArcherShoot");
        var arrow = Instantiate(arrowPrefab, transform.position, Quaternion.identity);
        var position = arrow.transform.position;
        position = new Vector3(position.x, position.y, 1);
        arrow.transform.position = position;

        var controller = arrow.GetComponent<ArrowController>();
        
        controller.archerBlock = this;
        controller.StartMoving(direction, GetDirection(direction), (int) arrowSpeed);
        
        _arrows.Add(arrow);
        
        _renderer.color = Color.cyan;
        if (_resetColorCoroutine != null)
            StopCoroutine(_resetColorCoroutine);
        _resetColorCoroutine = StartCoroutine(ResetColor());
    }
    
    private Coroutine _resetColorCoroutine;
    private System.Collections.IEnumerator ResetColor()
    {
        yield return new WaitForSeconds(0.15f);
        _renderer.color = _normalColor;
    }

    private int _occuredLoops;
    private void FixedUpdate()
    {
        if (!_shouldShoot)
            return;
        
        _occuredLoops++;
        if (_occuredLoops < shootSpeed * 10)
            return;
        
        _occuredLoops = 0;
        Shoot();
    }

    private Vector2Int GetDirection(long direction)
    {
        switch (direction)
        {
            case 1:
                return Vector2Int.down;
            case 2:
                return Vector2Int.right;
            case 3:
                return Vector2Int.up;
            case 4:
                return Vector2Int.left;
        }

        return Vector2Int.down;
    }

    public override bool isSpike => false;

    public override void LoadSettings(Dictionary<string, object> settings)
    {
        direction = (settings.ContainsKey("direction") ? (long) settings["direction"] : 2);
        shootSpeed = (settings.ContainsKey("shootSpeed") ? (long) settings["shootSpeed"] : 5);
        arrowSpeed = (settings.ContainsKey("arrowSpeed") ? (long) settings["arrowSpeed"] : 4);
    }
    
    public override Dictionary<string, object> SaveSettings()
    {
        var dict = new Dictionary<string, object>();
        
        dict.Add("direction", direction);
        dict.Add("shootSpeed", shootSpeed);
        dict.Add("arrowSpeed", arrowSpeed);
        
        return dict;
    }

    #region Rotor bahaviour

    public override bool IsRotatableByRotor()
    {
        return true;
    }

    public override void RotorRotation(int degrees)
    {
        direction = direction - 1;
        if (direction == 0)
            direction = 4;
    }

    #endregion
}