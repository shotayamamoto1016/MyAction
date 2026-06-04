using UnityEngine;

public class ConfusedMushroomItem : MonoBehaviour
{
    [Header("¬—İ’è")]
    public float confusedDuration = 20f; // ¬—‚·‚éŠÔ

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.isDead)
            {
                player.StartConfused(confusedDuration);
                Destroy(gameObject);
            }
        }
    }
}