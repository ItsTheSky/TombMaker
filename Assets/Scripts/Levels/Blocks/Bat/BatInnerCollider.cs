using UnityEngine;

public class BatInnerCollider : MonoBehaviour
{
    
    public BatBlock batBlock;
    
    private void OnTriggerEnter2D(Collider2D col)
    {
        var player = col.gameObject.GetComponent<PlayerScript>();
        if (player != null)
            player.Die();
    }
    
}