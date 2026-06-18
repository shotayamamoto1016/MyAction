using UnityEngine;
using UnityEngine.Tilemaps;

public class BakuenFlame : MonoBehaviour, IResettable
{
    [Header("設定")]
    public float moveSpeed = 5f;
    public float maxDistance = 50f;     // 最大移動距離

    [Header("破片設定")]
    public Sprite fragmentSprite;       // 破片画像
    public int fragmentCount = 4;
    public float fragmentForce = 5f;
    public float fragmentLifeTime = 0.8f;

    private bool isHit = false;
    private Vector3 startPosition;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;

        // 左方向に飛ばす
        rb.linearVelocity = new Vector2(-moveSpeed, 0f);
    }

    void Update()
    {
        if (isHit) return;

        // 最大移動距離を超えたら消滅
        float distanceMoved = Mathf.Abs(
            transform.position.x - startPosition.x);
        if (distanceMoved >= maxDistance)
        {
            isHit = true;
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Enemyタグには反応しない
        if (other.CompareTag("Enemy")) return;

        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.isDead)
            {
                if (!player.isInvincible)
                {
                    player.Die();
                }
            }
            // 弾は消去しない 
            return;
        }
        // 地面ブロックに当たったら壊す
        else if (other.CompareTag("Ground"))
        {
            // Tilemapには当たらないようにする
            if (other.GetComponent<Tilemap>() != null) return;

            SpawnFragments(other.transform.position);
            Destroy(other.gameObject);
            return;
        }

    }

    void SpawnFragments(Vector3 position)
    {
        for (int i = 0; i < fragmentCount; i++)
        {
            GameObject fragment = new GameObject("Fragment");
            fragment.transform.position = position;
            fragment.transform.localScale = Vector3.one * 0.4f;

            SpriteRenderer sr = fragment.AddComponent<SpriteRenderer>();
            sr.sprite = fragmentSprite;

            Rigidbody2D fragRb = fragment.AddComponent<Rigidbody2D>();

            float angle = 45f + (i * 90f);
            float randomAngle = angle + Random.Range(-30f, 30f);
            Vector2 direction = new Vector2(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                Mathf.Sin(randomAngle * Mathf.Deg2Rad));

            fragRb.linearVelocity = direction * fragmentForce;
            fragRb.angularVelocity = Random.Range(-360f, 360f);

            Destroy(fragment, fragmentLifeTime);
        }
    }

    public void ResetObject()
    {
        gameObject.SetActive(false);
    }
}