using UnityEngine;
using System.Collections;

public class PoisonMushroomItem : MonoBehaviour, IResettable
{
    [Header("毒エフェクト設定")]
    public float poisonDuration = 1.5f; // 毒になってからゲームオーバーまでの時間

    private Vector3 startPosition;
    private bool isCollected = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.isDead)
            {
                isCollected = true;
                player.PoisonAndDie(poisonDuration);
                gameObject.SetActive(false); // Destroyの代わりに非表示
            }
        }
    }

    public void ResetObject()
    {
        isCollected = false;
        transform.position = startPosition;
        gameObject.SetActive(true);
    }
}