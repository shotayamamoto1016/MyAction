using UnityEngine;

public class DestroyArea : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().Die();
        }
        else
        {
            // ’e‚È‚Ç‚Í•’Ê‚ÉÁ‹
            Destroy(collision.gameObject);
        }
    }
}