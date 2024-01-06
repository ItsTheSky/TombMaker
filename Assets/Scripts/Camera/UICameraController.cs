using System;
using Unity.VisualScripting;
using UnityEngine;

public class UICameraController : MonoBehaviour
{
    
    public LogicScript logic;
    public Camera cam;

    private void Update()
    {
        if (logic.editor)
        {
            if (Input.GetKey(LogicScript.KeyCodeForMoving) && !BlockPlacerScript.overlapUI)
                transform.position += new Vector3(Input.GetAxisRaw("Mouse X") * Time.deltaTime * GetSpeed(),
                    Input.GetAxisRaw("Mouse Y") * Time.deltaTime * GetSpeed(), 0f);
        }
    }

    public float GetSpeed()
    {
        float camZoom = cam.orthographicSize;
        /*
         * Some values got by hand:
         * # zoom -> needed speed
            3 -> -80
            5 -> -120
            7.5 -> -200
            13 -> -390
            But this is not linear, so we need to find a function that fits this data.
         */
        
        return Mathf.Log(camZoom) * -50;
    }
}
