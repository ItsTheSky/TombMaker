using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ReleaseSpikes : MonoBehaviour
{

    public Animator animator;
    public FullFrameBlock tempSpike;
    
    public bool isReleased;

    private Vector2Int _pos;
    private long _releaseStay;
    private int _rotationId;
    
    public void Init(FullFrameBlock tempSpike, 
        long spikeRelease, long releaseStay, 
        Vector2Int _pos, int _rotationId)
    {
        this.tempSpike = tempSpike;
        this._pos = _pos;
        _releaseStay = releaseStay;
        this._rotationId = _rotationId;

        isReleased = false;
        StartCoroutine(Release(spikeRelease));
    }
    
    private IEnumerator Release(long delay)
    {
        yield return new WaitForSeconds(delay / 1000f);
        animator.Play("ReleasedSpike");
        
        StartCoroutine(FinishRelease(_releaseStay));
    }
    
    private IEnumerator FinishRelease(long delay)
    {
        yield return new WaitForSeconds(delay / 1000f);
        tempSpike.ClearTempSpike(_rotationId);
    }

    public void EnableReleased()
    {
        isReleased = true;
        // check if the player is on the spike

        var playerPos = tempSpike._playerScript.GetPos();
        if (playerPos.x == _pos.x && playerPos.y == _pos.y)
            tempSpike._playerScript.Die();
    }

    public bool ShouldHurt()
    {
        return isReleased;
    }
}