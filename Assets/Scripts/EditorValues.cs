using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class EditorValues
{
    
    public static LogicScript LogicScript;
    public static PlayerScript PlayerScript;
    public static CategoryManager CategoryManager;
    public static float UIScaleMultiplier = 1.5f;
    public static void UpdateScale(Transform element, bool onlyChildren)
    {
        if (!onlyChildren)
        {
            element.localScale = Vector3.one * UIScaleMultiplier;
        }
        else
        {
            // only children at one level
            foreach (Transform child in element)
            {
                UpdateScale(child, false);
            }
        }
    }
}