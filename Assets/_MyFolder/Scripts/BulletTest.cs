using UnityEngine;

public class BulletTest : MonoBehaviour
{
    public GameObject bulletPrefab;

    void Update()
    {
        // スペースキーをRキーに変更
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameObject bullet = Instantiate(
                bulletPrefab, transform.position, Quaternion.identity);

            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, 10f);
                Debug.Log("発射！速度: " + rb.linearVelocity);
            }
        }
    }
}