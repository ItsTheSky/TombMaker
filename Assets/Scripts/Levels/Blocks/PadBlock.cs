using System.Collections.Generic;
using UnityEngine;

public class PadBlock : Block
{

    public Animator animator;
 
    [HideInInspector] public long rotation = 1; // 1: top-right (default sprite rotation), 2: top-right, 3: bottom-right, 4: bottom-left

    public override void Init()
    {
        base.Init();
        
        options.Add(new BlockSettingController.DropdownOption("rotation",
            "Rotation", "Change the rotation of the pad.",
            new []{"Top-Right", "Top-Left", "Bottom-Right", "Bottom-Left"}, (int) rotation - 1, value =>
            {
                rotation = value + 1;
                gameObject.transform.rotation = Quaternion.Euler(0, 0, GetRotationInDegrees());
            }));
    }
    
    public override void Enable()
    {
        CheckInit();
        gameObject.transform.rotation = Quaternion.Euler(0, 0, GetRotationInDegrees());
    }
    
    public override void Disable()
    {
        CheckInit();
        gameObject.transform.rotation = Quaternion.Euler(0, 0, GetRotationInDegrees());
    }

    public float GetRotationInDegrees()
    {
        if (rotation == 1)
            return 0;
        if (rotation == 3)
            return -90;
        if (rotation == 4)
            return -180;
        return 90;
    }

    public override bool CanRotate()
    {
        return true;
    }

    public override void SetupRotation(int blockPlacerRotation)
    {
        if (blockPlacerRotation == 0)
            rotation = 1;
        else if (blockPlacerRotation == 1)
            rotation = 3;
        else if (blockPlacerRotation == 2)
            rotation = 4;
        else rotation = 2;
        
        gameObject.transform.rotation = Quaternion.Euler(0, 0, GetRotationInDegrees());
        GetOptionById("rotation").SetValue((long) rotation - 1);
    }

    public void CollideWithPlayer()
    {
        int playerRotation = _playerScript.rotation;
        // we have to use RotateAndMove() so we push the player in perpendicular direction.
        
        if (rotation == 1)
        {
            if (playerRotation == 1)
                _playerScript.RotateAndMove(2);
            else if (playerRotation == 4)
                _playerScript.RotateAndMove(3);
        }
        else if (rotation == 2)
        {
            if (playerRotation == 1)
                _playerScript.RotateAndMove(4);
            else if (playerRotation == 2)
                _playerScript.RotateAndMove(3);
        }
        else if (rotation == 3) // bottom-right
        {
            if (playerRotation == 3)
                _playerScript.RotateAndMove(2);
            else if (playerRotation == 4)
                _playerScript.RotateAndMove(1);
        }
        else if (rotation == 4) // bottom-left
        {
            if (playerRotation == 2)
                _playerScript.RotateAndMove(1);
            else if (playerRotation == 3)
                _playerScript.RotateAndMove(4);
        }

        if (_playerScript.rotation != playerRotation)
        {
            animator.Play("PadExtend");
            AudioManager.PlaySound("Tramplin");
        }
    }

    public override Dictionary<string, object> SaveSettings()
    {
        return new Dictionary<string, object>()
        {
            {"rotation", rotation}
        };
    }

    public override void LoadSettings(Dictionary<string, object> settings)
    {
        rotation = (settings.ContainsKey("rotation") ? (long)settings["rotation"] : 1);
        
        gameObject.transform.rotation = Quaternion.Euler(0, 0, GetRotationInDegrees());
    }
}