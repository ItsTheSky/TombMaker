using System;
using UnityEngine;

public class ArcherBlockChecker : MonoBehaviour
{

    public ArcherBlock archerBlock;

    private void Update()
    {
        // check if there's a solid collider on us
        var colliders = Physics2D.OverlapCircleAll(transform.position, 0.1f);
        foreach (var collider in colliders)
        {
            if (collider.gameObject == gameObject)
                continue;
            
            if (!collider.isTrigger && collider.gameObject.name != "Player")
            { 
                archerBlock.canShoot = false;
                return;
            }
        }
        
        archerBlock.canShoot = true;
    }
}