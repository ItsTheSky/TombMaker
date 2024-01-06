using System;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlockSettingController : MonoBehaviour
{
    public static BlockSettingController Instance { get; private set; }
    
    public LogicScript logic;
    public GameObject blockSettingsContainer;
    
    public GameObject popupPrefab;
    public GameObject canvas;
    
    public TMP_Text helpTextContent;
    public GameObject helpButton;
    
    private Block _block;

    public BlockSettingController()
    {
        Instance = this;
    }

    private void Start()
    {
        popupPrefab = Resources.Load<GameObject>("Popup");
    }

    public void LoadBlockSettings(Block block)
    {
        _block = block;
        RefreshOptions();

        helpButton.SetActive(!string.IsNullOrEmpty(block.help));
    }

    public void ClearBlockSettings()
    {
        foreach (Transform child in blockSettingsContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    public void RefreshOptions(bool force = false)
    {
        ClearBlockSettings();
        foreach (var option in _block.options)
        {
            option.attachedController = this;

            if (_block.ShouldShowOption(option)) 
                option.AddSelf(blockSettingsContainer);
        }
    }

    public void ShowHelp(BlockOption option)
    {
        var popup = Instantiate(popupPrefab, canvas.transform).GetComponent<PopupController>();
        popup.Init("Help", option.helpText, null, null, true, false);
    }
    
    public void ShowHelpText()
    {
        ShowHelpTextStatic(_block);
    }

    public static void ShowHelpTextStatic(Block block)
    {
        HelpTextsManager.Instance.ShowHelpTextInternal("BlockSettings");
        var helpText = block.help;
        var lines = helpText.Split('\n').Length;
        
        Instance.helpTextContent.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 25 * lines);
        Instance.helpTextContent.text = block.help;
    }

    // ####################################################################################################
    // ###
    // ### Block options
    // ###
    // ####################################################################################################
    
    public abstract class BlockOption
    {

        public BlockSettingController attachedController;
        public string id;
        public string name;
        public string helpText;
        public BlockOptionType type;
        
        [CanBeNull] public OptionController attachedOptionController;
        
        public void AddSelf(GameObject container)
        {
            type.AddOption(container, this);
        }

        public virtual void SetValue(object value)
        {
            attachedOptionController?.SetValue(value);
        }
    }

    public abstract class BlockOptionType
    {

        public string name;
        
        public abstract void AddOption(GameObject container, BlockOption option);
    }
    
    // ### Default option types
    
    public class SliderOption : BlockOption
    {
        
        private float min;
        private float max;
        private bool wholeNumbers;
        private float value;
        private System.Action<float> onValueChanged;

        public SliderOption(string id, string name, string helpText, float min, float max, bool wholeNumbers, float value, System.Action<float> onValueChanged)
        {
            this.id = id;
            this.name = name;
            this.helpText = helpText;
            this.min = min;
            this.max = max;
            this.wholeNumbers = wholeNumbers;
            this.value = value;
            this.onValueChanged = onValueChanged;
            type = new SliderOptionType();
        }
        
        public override void SetValue(object value)
        {
            base.SetValue(value);
            this.value = float.Parse(value.ToString());
        }
        
        private class SliderOptionType : BlockOptionType
        {
            public override void AddOption(GameObject container, BlockOption option)
            {
                var sliderOption = (SliderOption) option;
                
                var slider = Instantiate(Resources.Load<GameObject>("SliderOption"), container.transform).GetComponent<SliderOptionController>();
                option.attachedOptionController = slider;

                HoverWindowed hoverWindowed = slider.GetComponentInChildren<HoverWindowed>();
                hoverWindowed.Text = option.helpText;

                slider.attachedOption = option;
                slider.attachedController = option.attachedController;
                
                slider.Init(option.name, sliderOption.min, sliderOption.max, sliderOption.wholeNumbers, sliderOption.value, new Action<float>(
                    (v) =>
                    {
                        sliderOption.onValueChanged(v);
                        sliderOption.value = v;
                        
                        // if needed, round up to 2 decimal places like '0.00'
                        slider.valueText.text = sliderOption.wholeNumbers ? v.ToString() : v.ToString("0.00");
                        slider.SetValue(v);
                    }));
            }
            
        }
    }
    
    public class ToggleOption : BlockOption
    {
        
        private bool value;
        private System.Action<bool> onValueChanged;

        public ToggleOption(string id, string name, string helpText, bool value, System.Action<bool> onValueChanged)
        {
            this.id = id;
            this.name = name;
            this.helpText = helpText;
            this.value = value;
            this.onValueChanged = onValueChanged;
            type = new ToggleOptionType();
        }
        
        public override void SetValue(object value)
        {
            base.SetValue(value);
            this.value = bool.Parse(value.ToString());
        }
        
        private class ToggleOptionType : BlockOptionType
        {
            public override void AddOption(GameObject container, BlockOption option)
            {
                var toggleOption = (ToggleOption) option;
                var toggle = Instantiate(Resources.Load<GameObject>("ToggleOption"), container.transform).GetComponent<ToggleOptionController>();
                option.attachedOptionController = toggle;
                
                HoverWindowed hoverWindowed = toggle.GetComponentInChildren<HoverWindowed>();
                hoverWindowed.Text = option.helpText;

                toggle.attachedOption = option;
                toggle.attachedController = option.attachedController;
                
                toggle.Init(option.name, toggleOption.value, new Action<bool>(
                    (v) =>
                    {
                        toggleOption.value = v;
                        toggleOption.onValueChanged(v);
                        toggle.RefreshIcon();
                        option.attachedController.RefreshOptions();
                    }));
                
            }
            
        }
    }
    
    public class InputOption : BlockOption
    {
        
        private string value;
        private TMP_InputField.ContentType contentType;
        private System.Action<string> onValueChanged;

        public InputOption(string id, string name, string helpText, string value, TMP_InputField.ContentType contentType, System.Action<string> onValueChanged)
        {
            this.id = id;
            this.name = name;
            this.helpText = helpText;
            this.value = value;
            this.contentType = contentType;
            this.onValueChanged = onValueChanged;
            type = new InputOptionType();
        }
        
        public override void SetValue(object value)
        {
            base.SetValue(value);
            this.value = string.IsNullOrEmpty(value.ToString()) ? "0" : value.ToString();
        }
        
        private class InputOptionType : BlockOptionType
        {
            public override void AddOption(GameObject container, BlockOption option)
            {
                var inputOption = (InputOption) option;
                var input = Instantiate(Resources.Load<GameObject>("InputOption"), container.transform).GetComponent<InputOptionController>();
                option.attachedOptionController = input;
                
                HoverWindowed hoverWindowed = input.GetComponentInChildren<HoverWindowed>();
                hoverWindowed.Text = option.helpText;

                input.attachedOption = option;
                input.attachedController = option.attachedController;

                input.Init(option.name, inputOption.value, inputOption.contentType, new Action<string>(
                    (v) =>
                    {
                        inputOption.value = v;
                        inputOption.onValueChanged(v);
                        // option.attachedController.RefreshOptions();
                    }));
           
            }
            
        }
    }
    
    public class DropdownOption : BlockOption
    {
        
        private string[] options;
        private int value;
        private System.Action<int> onValueChanged;

        public DropdownOption(string id, string name, string helpText, string[] options, int value, System.Action<int> onValueChanged)
        {
            this.id = id;
            this.name = name;
            this.helpText = helpText;
            this.options = options;
            this.value = value;
            this.onValueChanged = onValueChanged;
            type = new DropdownOptionType();
        }
        
        public override void SetValue(object value)
        {
            base.SetValue(value);
            this.value = int.Parse(value.ToString());
        }
        
        private class DropdownOptionType : BlockOptionType
        {
            public override void AddOption(GameObject container, BlockOption option)
            {
                var dropdownOption = (DropdownOption) option;
                var dropdown = Instantiate(Resources.Load<GameObject>("DropdownOption"), container.transform).GetComponent<DropdownOptionController>();
                option.attachedOptionController = dropdown;
                
                HoverWindowed hoverWindowed = dropdown.GetComponentInChildren<HoverWindowed>();
                hoverWindowed.Text = option.helpText;

                dropdown.attachedOption = option;
                dropdown.attachedController = option.attachedController;

                dropdown.Init(option.name, dropdownOption.value, dropdownOption.options, new Action<int>(
                    (v) =>
                    {
                        dropdownOption.value = v;
                        dropdownOption.onValueChanged(v); 
                        option.attachedController.RefreshOptions();
                    }));
                
            }
            
        }
    }
}