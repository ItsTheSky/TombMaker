using System;
using UnityEngine;
using UnityEngine.UI;

public class ToolboxController : MonoBehaviour
{
        
    public GameObject[] tools;
    public LogicScript logicScript;

    private void Start()
    {
        var tools = 4;
        
        if (!this.tools.Length.Equals(tools))
            throw new Exception("ToolboxController: tools array length is not equal to " + tools);

        for (int i = 0; i < tools; i++)
        {
            var obj = this.tools[i];
            var btn = obj.GetComponent<Button>();
            var i1 = i;
            btn.onClick.AddListener(() => SelectTool(i1));
        }
        
        SelectTool(0);
    }
    
    private int _prevToolIndex = -1;

    public void SelectTool(int toolIndex)
    {
        logicScript.ChangeTool(toolIndex);
        
        if (_prevToolIndex != -1)
            tools[_prevToolIndex].transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.yellow;
        
        tools[toolIndex].transform.GetChild(0).gameObject.GetComponent<Image>().color = Color.cyan;
        _prevToolIndex = toolIndex;
    }
    
}