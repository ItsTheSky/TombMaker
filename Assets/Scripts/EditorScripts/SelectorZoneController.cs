using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class SelectorZoneController : MonoBehaviour
{

    public Color MeshColor;
    public MeshFilter filter;
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    
    Mesh mesh;
    List<Vector3> verticies;
    List<int> indicies;
    
    void Start()
    {
        //MeshFilter filter = gameObject.GetComponent<MeshFilter>();
        mesh = new Mesh();
        MeshRenderer meshRenderer = filter.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.material.color = MeshColor;
        Rebuild();
    }
    
    public void Rebuild()
    {
        // Make a rectangle (empty inside) from the two points
        transform.position = new Vector3(StartPosition.x, StartPosition.y, 0);
        
        verticies = new List<Vector3>();
        indicies = new List<int>();
        
        // We look for a rectangle, so we need 4 points
        verticies.Add(new Vector3(0, 0, 0));
        verticies.Add(new Vector3(EndPosition.x - StartPosition.x, 0, 0));
        verticies.Add(new Vector3(EndPosition.x - StartPosition.x, EndPosition.y - StartPosition.y, 0));
        verticies.Add(new Vector3(0, EndPosition.y - StartPosition.y, 0));
        
        indicies.Add(0);
        indicies.Add(1);
        indicies.Add(1);
        indicies.Add(2);
        indicies.Add(2);
        indicies.Add(3);
        indicies.Add(3);
        indicies.Add(0);
        
        mesh.Clear();
        mesh.vertices = verticies.ToArray();
        mesh.SetIndices(indicies.ToArray(), MeshTopology.Lines, 0);
        filter.mesh = mesh;
    }
    
    public void ScaleAround(Transform target, Vector3 worldPivot, Vector3 newScale)
    {
        //Seemed to work, except when under a parent that has a non uniform scale and rotation it was a bit off.
        //This might be due to transform.lossyScale not being accurate under those conditions, or possibly something else is wrong...
        //Maybe things can work if we can find a way to convert the "newPosition = ..." line to use Matrix4x4 for possibly more scale accuracy.
        //However, I have tried and tried and have no idea how to do that kind of math =/
 
        Vector3 localOffset = target.InverseTransformPoint(worldPivot);
 
        Vector3 localScale = target.localScale;
        Vector3 scaleRatio = new Vector3(SafeDivide(newScale.x, localScale.x), SafeDivide(newScale.y, localScale.y), SafeDivide(newScale.z, localScale.z));
        Vector3 scaledLocalOffset = localOffset;
        scaledLocalOffset.Scale(scaleRatio);
        Vector3 newPosition = target.rotation * Vector3.Scale(localOffset - scaledLocalOffset, target.lossyScale) + target.position;
 
        target.localScale = newScale;
        target.position = newPosition;
    }
 
    float SafeDivide(float value, float divider)
    {
        if(divider == 0) return 0;
        return value / divider;
    }

    public void SetPosition(Vector2 startPosition, Vector2 endPosition)
    {
        var transform1 = transform;
        
        transform1.position = new Vector3(startPosition.x, startPosition.y, 0);
        // we update the local scale instead
        ScaleAround(transform1, startPosition, new Vector3(endPosition.x - startPosition.x, endPosition.y - startPosition.y, 1));
    }
    
}