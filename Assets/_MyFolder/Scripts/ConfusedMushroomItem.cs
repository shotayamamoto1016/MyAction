using UnityEngine;

public class ConfusedMushroomItem : MonoBehaviour, IResettable
{
    [Header("¬—İ’è")]
    public float confusedDuration = 20f; // ¬—‚·‚éŠÔ

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
                player.StartConfused(confusedDuration);
                gameObject.SetActive(false); // Destroy‚Ì‘ã‚í‚è‚É”ñ•\¦
            }
        }
    }

    public void ResetObject()
    {
        isCollected = false;
        gameObject.SetActive(true);
        transform.position = startPosition;
    }
}