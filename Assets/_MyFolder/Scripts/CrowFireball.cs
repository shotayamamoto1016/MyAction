using UnityEngine;

public class CrowFireball : MonoBehaviour, IResettable
{
    private Vector3 startPosition;
    private bool isHit = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isHit) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.isDead)
            {
                if (player.isInvincible)
                {
                    // –³“G’†‚Í’e‚¾‚¯Á‚¦‚é
                }
                else
                {
                    player.Die();
                }
            }
            isHit = true;
            gameObject.SetActive(false);
        }
        else if (!other.CompareTag("Enemy"))
        {
            // ’n–Ê‚â•Ç‚É“–‚½‚Á‚½‚çÁ‚¦‚é
            isHit = true;
            gameObject.SetActive(false);
        }
    }

    public void ResetObject()
    {
        // ’e‚Í•œŠˆ‚³‚¹‚¸”ñ•\¦‚Ì‚Ü‚Ü
    }
}