using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSettingsController : MonoBehaviour
{

    [Header("Block Styles")] 
    public GameObject blockStyleContainer;
    public GameObject blockStylePrefab;
    [HideInInspector] public Button lastSelectedBlockStyleButton;
    [HideInInspector] public List<Button> blockStyleButtons;
    
    [Header("Block Colors")]
    public GameObject blockColorContainer;
    public GameObject blockColorPrefab;
    [HideInInspector] public Button lastSelectedBlockColorButton;

    [Header("Camera Settings")] 
    public Toggle stickyCameraToggle;
    public TMP_InputField cameraZoomInput;
    public TMP_InputField cameraXInput;
    public TMP_InputField cameraYInput;
    public Toggle unlockedXCameraToggle;
    public Toggle unlockedYCameraToggle;

    [Header("Gameplay Settings")] 
    public Toggle showDoorToggle;
    public Button startingRotationButton;
    public Image startingRotationImage;
    public TMP_InputField playerSpeedInput;
    public TMP_Dropdown levelTypeDropdown;

    public void Init(LogicScript logic)
    {
        InitBlockStyles(logic);
        InitBlockColors(logic);
        InitCameraSettings(logic);
        InitGameplaySettings(logic);
    }
    
    private void InitBlockStyles(LogicScript logic)
    {
        foreach (Transform child in blockStyleContainer.transform)
            Destroy(child.gameObject);

        
        int[] styles = new[] { 0, 1, 2, 3, 6 };
        var selectedColor = logic.GetColor();
        var colors = new ColorBlock()
        {
            normalColor = selectedColor,
            highlightedColor = selectedColor,
            pressedColor = selectedColor,
            selectedColor = selectedColor,
            disabledColor = Color.cyan,
            colorMultiplier = 1,
            fadeDuration = 0.1f
        };
        
        foreach (var style in styles)
        {
            var button = Instantiate(blockStylePrefab, blockStyleContainer.transform).GetComponent<Button>();
            button.colors = colors;
            button.onClick.AddListener(() =>
            {
                logic.levelSettings.style = style;
                lastSelectedBlockStyleButton.interactable = true;
                
                lastSelectedBlockStyleButton = button;
                lastSelectedBlockStyleButton.interactable = false;
            });
            
            button.GetComponent<Image>().sprite = Constants.GetBlockStyleIcon(style);
            
            if (style == logic.levelSettings.style)
            {
                lastSelectedBlockStyleButton = button;
                lastSelectedBlockStyleButton.interactable = false;
            }
            
            blockStyleButtons.Add(button);
        }
    }

    private void RefreshBlockStylesColor(Color selectedColor)
    {
        var colors = new ColorBlock()
        {
            normalColor = selectedColor,
            highlightedColor = selectedColor,
            pressedColor = selectedColor,
            selectedColor = selectedColor,
            disabledColor = Color.cyan,
            colorMultiplier = 1,
            fadeDuration = 0.1f
        };
        
        foreach (var button in blockStyleButtons)
            button.colors = colors;
    }
    
    private void InitBlockColors(LogicScript logic)
    {
        foreach (Transform child in blockColorContainer.transform)
            Destroy(child.gameObject);
        
        foreach (var color in Constants.COLORS)
        {
            var button = Instantiate(blockColorPrefab, blockColorContainer.transform).GetComponent<Button>();
            button.colors = new ColorBlock()
            {
                normalColor = color.Value.color,
                highlightedColor = color.Value.color,
                pressedColor = color.Value.color,
                selectedColor = color.Value.color,
                disabledColor = color.Value.color,
                colorMultiplier = 1,
                fadeDuration = 0.1f
            };
            button.onClick.AddListener(() =>
            {
                logic.levelSettings.color = color.Key;
                logic.UpdateBlocksColor();
                
                lastSelectedBlockColorButton.transform.Find("Selected").gameObject.SetActive(false);
                lastSelectedBlockColorButton.interactable = true;
                
                lastSelectedBlockColorButton = button;
                lastSelectedBlockColorButton.transform.Find("Selected").gameObject.SetActive(true);
                lastSelectedBlockColorButton.interactable = false;
                
                RefreshBlockStylesColor(color.Value.color);
            });
            
            if (color.Key == logic.levelSettings.color)
            {
                lastSelectedBlockColorButton = button;
                lastSelectedBlockColorButton.transform.Find("Selected").gameObject.SetActive(true);
                lastSelectedBlockColorButton.interactable = false;
                
                RefreshBlockStylesColor(color.Value.color);
            }
        }
    }
    
    private void InitCameraSettings(LogicScript logic)
    {
        stickyCameraToggle.isOn = logic.levelSettings.stickyCamera;
        cameraZoomInput.text = logic.levelSettings.cameraZoom.ToString();
        cameraXInput.text = logic.levelSettings.cameraX.ToString();
        cameraYInput.text = logic.levelSettings.cameraY.ToString();
        unlockedXCameraToggle.isOn = logic.levelSettings.unlockedX;
        unlockedYCameraToggle.isOn = logic.levelSettings.unlockedY;
        
        if (logic.levelSettings.stickyCamera)
        {
            cameraXInput.interactable = false;
            cameraYInput.interactable = false;
                
            unlockedXCameraToggle.interactable = false;
            unlockedYCameraToggle.interactable = false;
        } else
        {
            cameraXInput.interactable = true;
            cameraYInput.interactable = true;
                
            unlockedXCameraToggle.interactable = true;
            unlockedYCameraToggle.interactable = true;
        }
        
        stickyCameraToggle.onValueChanged.AddListener((value) =>
        {
            logic.levelSettings.stickyCamera = value;

            if (value)
            {
                cameraXInput.interactable = false;
                cameraYInput.interactable = false;
                
                unlockedXCameraToggle.interactable = false;
                unlockedYCameraToggle.interactable = false;
            } else
            {
                cameraXInput.interactable = true;
                cameraYInput.interactable = true;
                
                unlockedXCameraToggle.interactable = true;
                unlockedYCameraToggle.interactable = true;
            }
        });
        
        cameraZoomInput.onValueChanged.AddListener((value) =>
        {
            if (value == "")
                return;

            if (long.TryParse(value, out var result))
                logic.levelSettings.cameraZoom = (long) Mathf.Clamp(result, 1, 15); 
            
            cameraZoomInput.text = logic.levelSettings.cameraZoom.ToString();
        });
        
        cameraXInput.onValueChanged.AddListener((value) =>
        {
            if (value == "")
                return;
            
            if (float.TryParse(value, out var result))
                logic.levelSettings.cameraX = result;
            else
                cameraXInput.text = logic.levelSettings.cameraX.ToString();
        });
        
        cameraYInput.onValueChanged.AddListener((value) =>
        {
            if (value == "")
                return;
            
            if (float.TryParse(value, out var result))
                logic.levelSettings.cameraY = result;
            else
                cameraYInput.text = logic.levelSettings.cameraY.ToString();
        });
        
        unlockedXCameraToggle.onValueChanged.AddListener((value) =>
        {
            logic.levelSettings.unlockedX = value;
        });
        
        unlockedYCameraToggle.onValueChanged.AddListener((value) =>
        {
            logic.levelSettings.unlockedY = value;
        });
    }

    private void InitGameplaySettings(LogicScript logic)
    { 
        showDoorToggle.isOn = logic.levelSettings.showDoor;
        startingRotationImage.transform.rotation = Quaternion.Euler(0, 0, (logic.levelSettings.spawnRotation - 1) * 90);
        playerSpeedInput.text = logic.levelSettings.playerSpeed.ToString();
        levelTypeDropdown.value = (int) logic.levelSettings.levelType;
        
        showDoorToggle.onValueChanged.AddListener((value) =>
        {
            logic.levelSettings.showDoor = value;
        });
        
        startingRotationButton.onClick.AddListener(() =>
        {
            logic.levelSettings.spawnRotation = (logic.levelSettings.spawnRotation + 1);
            if (logic.levelSettings.spawnRotation > 4)
                logic.levelSettings.spawnRotation = 1;
            
            startingRotationImage.transform.rotation = Quaternion.Euler(0, 0, (logic.levelSettings.spawnRotation - 1) * 90);
            print(logic.levelSettings.spawnRotation);
        });
        
        playerSpeedInput.onValueChanged.AddListener((value) =>
        {
            if (value == "")
                return;

            if (long.TryParse(value, out var result))
                logic.levelSettings.playerSpeed = ((long) Mathf.Clamp(result, 1, 5)) * 10;
            
            playerSpeedInput.text = logic.levelSettings.playerSpeed/10 + "";
        });
        
        levelTypeDropdown.onValueChanged.AddListener((value) =>
        {
            var type = (LevelType)value;
            logic.levelSettings.levelType = type;
            
            logic.colorLogic.colorMode = type == LevelType.Color;
        });
    }
}