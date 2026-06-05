using UnityEngine;

public class GoldenMushroom : MonoBehaviour
{
   
    void OnTriggerEnter2D(Collider2D other)
    {
        // ぽんたが触れたか判定
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // ぽんたの無敵メソッドを呼び出す
                player.StartInvincibility();
            }

            // キノコを消す
            Destroy(gameObject);
        }
    }
}