using System.Collections.Generic;
using UnityEngine;

/**
 * Base class for every decorative block, which is a block that doesn't have any
 * gameplay effect but is just there for decoration.
 */
public class DecorationBlock : Block
{
    [Header("Decoration Block Settings")]
    public float xOffset = 0;
    public float yOffset = 0;
    
    [HideInInspector] public int rotation = 1; // 1: down, 2: right, 3: up, 4: left
    
    private Color _lastColor = Color.red;
    
    public override void Init()
    {
        base.Init();
        
        options.Add(new BlockSettingController.DropdownOption("rotation", "Rotation", "Rotate the decoration",
            new [] {"Down", "Right", "Up", "Left"}, (int) rotation - 1, value =>
            {
                rotation = value + 1;
                UpdateShownRotation();
            }));
    }

    public override void SetPos(Vector2 vector2)
    {
        _pos = vector2;
        var transform1 = transform;
        
        transform1.position = new Vector3(vector2.x + xOffset, vector2.y + yOffset, transform1.position.z);
    }

    public override void UpdateBlockColor(Color color)
    {
        base.UpdateBlockColor(color);
        
        _lastColor = color;
        _renderer.color = new Color(color.r, color.g, color.b, 0.3f);
    }
    
    public override void Enable()
    {
        CheckInit();
        
        UpdateShownRotation();
        UpdateBlockColor(_lastColor);
    }

    public override void Disable()
    {
        UpdateShownRotation();
        UpdateBlockColor(_lastColor);
    }

    private void UpdateShownRotation()
    {
        if (rotation == 1) {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 270);
        } else if (rotation == 2) {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        } else if (rotation == 3) {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 90);
        } else if (rotation == 4) {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 180);
        }
    }

    public override Dictionary<string, object> SaveSettings()
    {
        var settings = base.SaveSettings();
        settings.Add("rotation", rotation);
        return settings;
    }
    
    public override void RotorRotation(int degrees)
    {
        rotation = rotation - 1;
        if (rotation == 0)
            rotation = 4;
    }
    
    public override void SetupRotation(int blockPlacerRotation)
    {
        if (blockPlacerRotation == 0)
            rotation = 2;
        else if (blockPlacerRotation == 1)
            rotation = 1;
        else if (blockPlacerRotation == 2)
            rotation = 4;
        else rotation = 3;
        
        UpdateShownRotation();
        GetOptionById("rotation").SetValue((long) rotation - 1);
    }
    
    public override void LoadSettings(Dictionary<string, object> settings)
    {
        base.LoadSettings(settings);
        rotation = settings.TryGetValue("rotation", out var value) ? int.Parse(value.ToString()) : 1;
        UpdateShownRotation();
    }

    public override bool CanRotate()
    {
        return true;
    }
}