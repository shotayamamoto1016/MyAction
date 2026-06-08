using UnityEngine;

public class GoldenMushroom : MonoBehaviour, IResettable
{
    private Vector3 startPosition;
    private bool isCollected = false;

    void Start()
    {
        startPosition = transform.position;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        // ‚Û‚ñ‚½‚ªG‚ê‚½‚©”»’è
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                isCollected = true;
                player.StartInvincibility();
                gameObject.SetActive(false); // Destroy‚Ì‘ã‚í‚è‚É”ñ•\¦
            }

            // ƒLƒmƒR‚ğÁ‚·
            //Destroy(gameObject);
        }
    }

    public void ResetObject()
    {
        isCollected = false;
        transform.position = startPosition;
        gameObject.SetActive(true);
    }
}