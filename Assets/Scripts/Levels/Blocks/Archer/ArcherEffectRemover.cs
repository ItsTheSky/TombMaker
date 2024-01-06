using System;
using UnityEngine;
public class ArcherEffectRemover : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(RemoveEffect());
    }
    
    private System.Collections.IEnumerator RemoveEffect()
    {
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }
}