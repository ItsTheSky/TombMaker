using UnityEngine;
using UnityEngine.UI;
 
public class MultiImageTargetGraphics : MonoBehaviour
{
    [SerializeField] private Graphic[] targetImages;
 
    public Graphic[] GetTargetImages => targetImages;
}