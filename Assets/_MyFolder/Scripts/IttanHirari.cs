using UnityEngine;
using System.Collections;

public class IttanHirari : MonoBehaviour, IResettable
{
    [Header("待機アニメーション（4枚）")]
    public Sprite[] idleSprites;
    public float idleAnimInterval = 0.2f;

    [Header("連れ去り設定")]
    public Sprite carrySprite;          // 連れ去り時の画像
    public Transform playerHoldPoint;   // ぽんたを配置する位置
    public float ascendSpeed = 3f;      // 上昇速度
    public Vector2 ascendDirection = new Vector2(1f, 1f); // 斜め右上方向
    public float deathYHeight = 15f;    // この高さで死亡

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private bool isCarrying = false;
    private int idleAnimIndex = 0;
    private float idleAnimTimer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        if (idleSprites.Length > 0)
        {
            spriteRenderer.sprite = idleSprites[0];
        }
    }

    void Update()
    {
        if (isCarrying)
        {
            // 斜め右上に上昇させる
            Vector2 dir = ascendDirection.normalized;
            transform.position += new Vector3(
                dir.x, dir.y, 0f) * ascendSpeed * Time.deltaTime;
            return;
        }

        // 待機アニメーション
        UpdateIdleAnimation();
    }

    void UpdateIdleAnimation()
    {
        if (idleSprites.Length == 0) return;

        idleAnimTimer += Time.deltaTime;
        if (idleAnimTimer >= idleAnimInterval)
        {
            idleAnimTimer = 0f;
            idleAnimIndex = (idleAnimIndex + 1) % idleSprites.Length;
            spriteRenderer.sprite = idleSprites[idleAnimIndex];
        }
    }

    //void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (isCarrying) return;
    //    if (!collision.gameObject.CompareTag("Player")) return;

    //    PlayerController player = collision.gameObject.GetComponent<PlayerController>();
    //    if (player == null || player.isDead) return;

    //    if (player.isInvincible)
    //    {
    //        gameObject.SetActive(false);
    //        return;
    //    }

    //    StartCoroutine(CarryPlayer(player));
    //}

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerContact(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerContact(other.gameObject);
    }

    void HandlePlayerContact(GameObject obj)
    {
        if (isCarrying) return;
        if (!obj.CompareTag("Player")) return;

        PlayerController player = obj.GetComponent<PlayerController>();
        if (player == null || player.isDead) return;

        if (player.isInvincible)
        {
            gameObject.SetActive(false);
            return;
        }

        StartCoroutine(CarryPlayer(player));
    }

    IEnumerator CarryPlayer(PlayerController player)
    {
        isCarrying = true;

        // 連れ去り画像に変更
        if (carrySprite != null)
        {
            spriteRenderer.sprite = carrySprite;
        }

        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.bodyType = RigidbodyType2D.Kinematic;
        }

        // 入力を無効化
        player.isInputDisabled = true;

        // Groundとの当たり判定を無効化
        player.DisableGroundCollision();

        // ぽんたをくぼみの位置に配置
        if (playerHoldPoint != null)
        {
            player.transform.position = playerHoldPoint.position;
            player.transform.SetParent(transform); // 子オブジェクト化
        }

        // 上空に行くまで監視
        while (player.transform.position.y < deathYHeight)
        {
            yield return null;
        }

        // 死亡演出
        player.transform.SetParent(null);
        //player.DisableGroundCollision(); 
        player.EnableGroundCollision();
        player.isInputDisabled = false;

        if (playerRb != null)
        {
            playerRb.bodyType = RigidbodyType2D.Dynamic;
        }

        player.Die();
    }

    public void ResetObject()
    {
        StopAllCoroutines();
        isCarrying = false;
        idleAnimIndex = 0;
        idleAnimTimer = 0f;
        transform.position = startPosition;
        gameObject.SetActive(true);

        if (idleSprites.Length > 0)
        {
            spriteRenderer.sprite = idleSprites[0];
        }
    }
}