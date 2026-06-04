using UnityEngine;
using System.Collections;

public class FreezeMushroomItem : MonoBehaviour
{
    [Header("凍るエフェクト")]
    public float freezeDuration = 1.5f; // 凍ってからゲームオーバーまでの時間

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.isDead)
            {
                player.FreezeAndDie(freezeDuration);
                Destroy(gameObject);
            }
        }
    }
}