using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaController : MonoBehaviour
{

    public SpriteRenderer renderer;
    
    [HideInInspector] public Color themeColor;
    [HideInInspector] public float bps = 0; // how many blocks moved per second
    [HideInInspector] public int side = 0; // 0 = bottom, 1 = left, 2
    
    private IEnumerator FadeToColor0(Color color, float duration)
    {
        float time = 0;
        Color startColor = renderer.color;
        while (time < duration)
        {
            renderer.color = Color.Lerp(startColor, color, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        renderer.color = color;
    }

    private void Start()
    {
        renderer.color = themeColor;
        // start the loop
        StartCoroutine(FadeToColor());
    }
    
    public IEnumerator FadeToColor()
    {
        yield return FadeToColor0(Color.cyan, 0.5f);
        yield return FadeToColor0(themeColor, 0.5f);
        StartCoroutine(FadeToColor());
    }

    private void Update()
    {
        var transform1 = transform;
        var position = transform1.position;
        var moving = new Vector3(0, 0, 0);

        if (side == 0 || side == 2)
        {
            position.x = EditorValues.PlayerScript.GetPos().x;
            moving.y = bps * Time.deltaTime * (side == 0 ? 1 : -1);
        }
        else if (side == 1 || side == 3)
        {
            position.y = EditorValues.PlayerScript.GetPos().y;
            moving.x = bps * Time.deltaTime * (side == 1 ? 1 : -1);
        }
        
        transform1.position = position + moving;
    }
}