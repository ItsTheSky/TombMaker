using System;
using System.Collections.Generic;
using UnityEngine;

public class BatBlock : Block
{ 
    private static readonly int Speed = Animator.StringToHash("Speed");
    
    public Animator animator;

    public long direction = 2; // 1: down, 2: right, 3: up, 4: left
    public long speed = 3;
    public long configuredDirection = 2;
    private bool _shouldMove;
    public GameObject indicator;
    public Rigidbody2D rigidbody2D;
    public Collider2D collider2D;

    private void Start()
    {
        Physics2D.IgnoreCollision(_playerScript.collider2D, collider2D, true);
    }

    public override void Init()
    {
        base.Init();
        
        options.Add(new BlockSettingController.DropdownOption("direction",
            "Direction", "Change the direction the bat will fly.",
            new []{"Down", "Right", "Up", "Left"}, (int) direction - 1, value =>
            {
                direction = value + 1;
                UpdateIndicator();
            }));
        
        options.Add(new BlockSettingController.SliderOption("speed",
            "Speed", "Change the speed of the bat.",
            1, 10, true, 3, value =>
            {
                speed = (long) value;
            }));
    }

    public override void Enable()
    {
        CheckInit();
        
        animator.Play("BatIdle");
        
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        rigidbody2D.velocity = Vector2.zero;
        rigidbody2D.angularVelocity = 0;
        
        GameObject o;
        (o = gameObject).transform.rotation = Quaternion.Euler(0, 0, 0);
        o.transform.position = new Vector3(_pos.x, _pos.y, 0);
        
        _shouldMove = false;
        direction = configuredDirection;
        indicator.SetActive(true);
    }

    public override void Disable()
    {
        CheckInit();

        transform.rotation = Quaternion.Euler(0, 0, 0);
        animator.Play("BatFlying");
        
        animator.SetFloat(Speed, speed - 0.5f);
        _shouldMove = true;
        configuredDirection = direction;
        indicator.SetActive(false);
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation & (direction == 1 || direction == 3 ? RigidbodyConstraints2D.FreezePositionX : RigidbodyConstraints2D.FreezePositionY);
        
        Fly((int) direction);
    }

    public void Fly(int direction) // now using rigidbody
    {
        if (direction == 1)
        {
            rigidbody2D.velocity = Vector2.down * speed;
            rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX & RigidbodyConstraints2D.FreezeRotation;
        }
        else if (direction == 2)
        {
            rigidbody2D.velocity = Vector2.right * speed;
            rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionY & RigidbodyConstraints2D.FreezeRotation;
        }
        else if (direction == 3)
        {
            rigidbody2D.velocity = Vector2.up * speed;
            rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionX & RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            rigidbody2D.velocity = Vector2.left * speed;
            rigidbody2D.constraints = RigidbodyConstraints2D.FreezePositionY & RigidbodyConstraints2D.FreezeRotation;
        }
        
        _shouldMove = true;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
            _playerScript.Die();
        
        var block = other.gameObject.GetComponent<Block>();
        if (block != null)
        {
            if (block.IsSolid())
            {
                direction = direction == 1 ? 3 : direction == 2 ? 4 : direction == 3 ? 1 : 2;
                AudioManager.instance?.Play("Bat");
                
                rigidbody2D.velocity = Vector2.zero;
                rigidbody2D.angularVelocity = 0;
                
                Fly((int) direction);
            }
        }
    }

    public void UpdateIndicator()
    {
        var transform = indicator.transform;
        var degrees = direction == 1 ? 0 : direction == 2 ? 90 : direction == 3 ? 180 : 270;
        
        transform.rotation = Quaternion.Euler(0, 0, degrees);
    }

    public override bool isSpike => true;

    public override void LoadSettings(Dictionary<string, object> settings)
    {
        direction = (settings.ContainsKey("direction") ? (long)settings["direction"] : 2);
        speed = settings.TryGetValue("speed", out var setting) ? long.Parse(setting.ToString()) : 3;
        
        UpdateIndicator();
    }
    
    public override Dictionary<string, object> SaveSettings()
    {
        var dict = new Dictionary<string, object>();
        
        dict.Add("direction", direction);
        dict.Add("speed", speed);
        
        return dict;
    }

    public override bool IsRotatableByRotor()
    {
        return false;
    }
}