using UnityEngine;

public class Coin : MonoBehaviour
{
    public GameObject coinCollectEffectPrefab;

    [HideInInspector] public DotBlock dotBlock;

    public void OnCollect(PlayerScript playerScript)
    {
        playerScript.logic.CollectCoin();
        Instantiate(coinCollectEffectPrefab, transform.position, Quaternion.identity);

        dotBlock._isCoin = false;
        Destroy(gameObject);
        AudioManager.instance.Play("CoinCollect");
    }
    
}