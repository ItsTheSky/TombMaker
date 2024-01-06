using System;
using Unity.VisualScripting;
using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public GameObject collideAnimationPrefab;
        
    public ArcherBlock archerBlock;
    public Rigidbody2D rigidbody2D;
    public Collider2D collider2D;
    
    private bool _shouldMove;
    private Vector2Int _direction;
    private int _speed;

    private void Start()
    {
        Physics2D.IgnoreLayerCollision(9, 9);
    }

    private void FixedUpdate()
    {
        if (true)
            return;
        
        var pos = transform.position;
        var offset = _direction;
        var newPos = new Vector3(pos.x + offset.x * 0.05f * _speed, pos.y + offset.y * 0.05f * _speed, gameObject.transform.position.z);
        
        transform.position = newPos;
        var toCheck = newPos + new Vector3(
            offset.x * 0.2f,
            offset.y * 0.2f,
            0);
        var mapped = archerBlock._playerScript.logic.MapToGrid(toCheck);
        var blockThere = archerBlock._playerScript.logic.GetBlock(new Vector2(mapped.x, mapped.y));
        if (blockThere != null && blockThere.GetComponent<Block>().isSolid && blockThere.GetComponent<ArcherBlock>() == null)
        {
            _shouldMove = false;
            
            // spawn the collide animation
            var collideAnimation = Instantiate(collideAnimationPrefab);
            collideAnimation.transform.position = toCheck + new Vector3(0, 0, -5) 
                - (new Vector3(
                    offset.x * 0.2f,
                    offset.y * 0.2f,
                    0) * 2);
            collideAnimation.transform.rotation = gameObject.transform.rotation; 
            
            Destroy(gameObject);
        }
    }

    public void DestroySelf()
    {
        if (_shouldMove)
            Destroy(gameObject);
    }

    public void StartMoving(long numDirection, Vector2Int direction, int speed)
    {
        _shouldMove = true;
        _direction = direction;
        _speed = speed;
        
        if (numDirection == 1) {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 270);
            gameObject.GetComponent<SpriteRenderer>().flipY = false;
        } else if (numDirection == 2) {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            gameObject.GetComponent<SpriteRenderer>().flipY = false;
        } else if (numDirection == 3) {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 90);
            gameObject.GetComponent<SpriteRenderer>().flipY = false;
        } else if (numDirection == 4) {
            gameObject.transform.rotation = Quaternion.Euler(0, 0, 180);
            gameObject.GetComponent<SpriteRenderer>().flipY = true;
        }
        
        rigidbody2D.velocity = new Vector2(direction.x * speed, direction.y * speed);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<Block>() != null && other.gameObject.GetComponent<Block>().isSolid && other.gameObject != archerBlock.gameObject)
        {
            _shouldMove = false;
            
            // spawn the collide animation
            var collideAnimation = Instantiate(collideAnimationPrefab);
            collideAnimation.transform.position = other.transform.position + new Vector3(0, 0, -5) 
                - (new Vector3(
                    _direction.x * 0.4f,
                    _direction.y * 0.4f,
                    0) * 2);
            var o = gameObject;
            collideAnimation.transform.rotation = o.transform.rotation; 
            
            Destroy(o);
        }
        
        var player = other.gameObject.GetComponent<PlayerScript>();
        if (player != null) {
            player.Die();
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!archerBlock.IsDestroyed() && other.gameObject == archerBlock.gameObject) 
            collider2D.isTrigger = false;
    }
}