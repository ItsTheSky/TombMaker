using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public LogicScript logic;
    
    private readonly Vector3 _offset = new (0, 0, -10);
    private readonly float _smoothTime = 0.1f;
    private Vector3 _velocity = Vector3.zero;
    
    private float zoom; 
    private float zoomMultiplier = 6f; 
    private float minZoom = 1f; 
    private float maxZoom = 40f; 
    private float velocity; 
    private float smoothTime = 0.1f;
    
    [SerializeField] private Transform target;
    [SerializeField] private Camera cam;

    private void Start()
    {
        zoom = cam.orthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        if (logic.editor)
        {
            if (logic.hasUIOpen || BlockPlacerScript.overlapUI)
                return;
            
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            zoom -= scroll * zoomMultiplier;
            zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
            cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, zoom, ref velocity, smoothTime);

            var moveSpeed = 0.3f;
            if (Input.GetKey(KeybindManager.MoveCameraLeft.value)) transform.position = new Vector3(transform.position.x - moveSpeed, transform.position.y, transform.position.z);
            if (Input.GetKey(KeybindManager.MoveCameraDown.value)) transform.position = new Vector3(transform.position.x, transform.position.y - moveSpeed, transform.position.z);
            if (Input.GetKey(KeybindManager.MoveCameraUp.value)) transform.position = new Vector3(transform.position.x, transform.position.y + moveSpeed, transform.position.z);
            if (Input.GetKey(KeybindManager.MoveCameraRight.value)) transform.position = new Vector3(transform.position.x + moveSpeed, transform.position.y, transform.position.z);
        }
        else
        {

            if (logic.StickyCamera)
            {
                Vector3 targetPosition = target.position + _offset;
                transform.position = Vector3.SmoothDamp(transform.position,
                    targetPosition, ref _velocity, _smoothTime);
                /*var targetPosition = target.position;
                targetPosition.z = transform.position.z;
                if (logic.UnlockedX)
                    targetPosition.x = logic.player.transform.position.x;
                if (logic.UnlockedY)
                    targetPosition.y = logic.player.transform.position.y;
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _velocity, _smoothTime);*/
            }
            else
            {
                var targetX = (float) (logic.UnlockedX ? logic.player.transform.position.x : logic.CameraX);
                var targetY = (float) (logic.UnlockedY ? logic.player.transform.position.y : logic.CameraY);
                
                Vector3 targetPosition = new Vector3(targetX, targetY, 0) + _offset;
                transform.position = Vector3.SmoothDamp(transform.position,
                    targetPosition, ref _velocity, _smoothTime);
            }
        }
    }
}
