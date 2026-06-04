using UnityEngine;
using System.Collections;

public class PoisonMushroomItem : MonoBehaviour
{
    [Header("毒エフェクト設定")]
    public float poisonDuration = 1.5f; // 毒になってからゲームオーバーまでの時間

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.isDead)
            {
                player.PoisonAndDie(poisonDuration);
                Destroy(gameObject);
            }
        }
    }
}