using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GridController : MonoBehaviour
{
    public MeshFilter filter;

    public int gridSize = 100;
    [Range(0, 1)]
    public float bendX = 1;
    [Range(0, 1)]
    public float bendY = 0;

    Mesh mesh;
    List<Vector3> verticies;
    List<int> indicies;
    private Camera _camera;

    float gridSpacing;
    float xMin;
    float yMin;
    float zMin;
    float x1;
    float x2;
    float y1;
    float y2;
    float zx1;
    float zx2;
    float zy1;
    float zy2;

    void Start()
    {
        //MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        mesh = new Mesh();
        MeshRenderer meshRenderer = filter.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.material.color = new Color(1, 1, 1, 0.2f);
        Rebuild();
        
        _camera = Camera.main;
    }

    private Vector2Int _lastCameraPos = Vector2Int.zero;
    private void Update()
    {
        var cameraPos = _camera.transform.position;
        var cameraPosInt = new Vector2Int((int)cameraPos.x, (int)cameraPos.y);
        
        // difference of 5 to prevent rebuilding every frame
        if (cameraPosInt != _lastCameraPos && Mathf.Abs(cameraPosInt.x - _lastCameraPos.x) > 5 || Mathf.Abs(cameraPosInt.y - _lastCameraPos.y) > 5)
        {
            Rebuild();
            _lastCameraPos = cameraPosInt;
            transform.position = new Vector3(cameraPosInt.x + 0.5f, cameraPosInt.y + 0.5f, 0);
        }
    }

    void Rebuild()
    {
        verticies = new List<Vector3>();
        indicies = new List<int>();
        gridSpacing = (1 / (float)gridSize);

        xMin = gridSpacing * gridSize / 2;
        yMin = gridSpacing * gridSize / 2;
        zMin = (.25f * bendX + .25f * bendY)/2;

        for (int i = 0; i <= gridSize; i++)
        {
            for (int j = 0; j<=gridSize; j++)
            {
                x1 = i * gridSpacing - xMin;
                x2 = (i+1) * gridSpacing - xMin;
                y1 = j * gridSpacing - yMin;
                y2 = (j+1) * gridSpacing - yMin;

                zx1 = Mathf.Abs(Mathf.Pow(x1, 2)) * bendX * 2 - zMin;
                zx2 = Mathf.Abs(Mathf.Pow(x2, 2)) * bendX * 2 - zMin;
                zy1 = Mathf.Abs(Mathf.Pow(y1, 2)) * bendY * 2 - zMin;
                zy2 = Mathf.Abs(Mathf.Pow(y2, 2)) * bendY * 2 - zMin;

                if (i != gridSize)
                {
                    verticies.Add(new Vector3(x1, y1, -zx1 - zy1));
                    verticies.Add(new Vector3(x2, y1, -zx2 - zy1));
                }

                if (j != gridSize)
                {
                    verticies.Add(new Vector3(x1, y1, -zx1 - zy1));
                    verticies.Add(new Vector3(x1, y2, -zx1 - zy2));
                }
            }
        }
        int indiciesCount = 4*((int)Mathf.Pow(gridSize, 2) + gridSize);
        for (int i = 0; i < indiciesCount; i++)
        {
            indicies.Add(i);
        }

        mesh.vertices = verticies.ToArray();
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Lines, 0);
        filter.mesh = mesh;
    }
}
