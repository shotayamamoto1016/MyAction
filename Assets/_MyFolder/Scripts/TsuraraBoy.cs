using UnityEngine;
using System.Collections;

public class TsuraraBoy : MonoBehaviour, IResettable
{
    [Header("画像設定")]
    public Sprite idleSprite;           // 通常時の画像
    public Sprite[] fallSprites;        // 落下アニメーション画像3枚
    public Sprite breakSprite;          // バラバラになる画像4枚目

    [Header("検知設定")]
    public float detectRangeX = 2f;     // 横の検知範囲
    public float detectRangeY = 5f;     // 下の検知範囲
    public LayerMask groundLayer;       // 地面レイヤー

    [Header("落下設定")]
    public float fallAnimInterval = 0.1f; // 落下アニメーション速度
    public float fallSpeed = 10f;         // 落下速度

    [Header("復活設定")]
    public float respawnTime = 2.5f;    // 復活までの時間

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D col;
    private Vector3 startPosition;
    private bool isActivated = false;
    private Transform player;

    private enum State { Idle, Falling, Broken }
    private State currentState = State.Idle;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        startPosition = transform.position;
        player = GameObject.FindWithTag("Player")?.transform;

        // 最初は通常画像
        if (idleSprite != null)
            spriteRenderer.sprite = idleSprite;

        // 重力を無効化（落下まで待機）
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void Update()
    {
        if (isActivated || player == null) return;
        if (currentState != State.Idle) return;

        // ぽんたがつららの下にいるか検知
        float distanceX = Mathf.Abs(
            player.position.x - transform.position.x);
        float distanceY = transform.position.y - player.position.y;

        // ぽんたが真下にいる場合のみ落下
        if (distanceX < detectRangeX && distanceY > 0 &&
            distanceY < detectRangeY)
        {
            StartCoroutine(FallSequence());
        }
    }

    IEnumerator FallSequence()
    {
        isActivated = true;
        currentState = State.Falling;

        // 落下アニメーション（3枚）
        for (int i = 0; i < fallSprites.Length; i++)
        {
            spriteRenderer.sprite = fallSprites[i];
            yield return new WaitForSeconds(fallAnimInterval);
        }

        // 落下開始
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;
        rb.linearVelocity = new Vector2(0f, -fallSpeed);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Falling)
        {
            // 地面に当たった
            if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            {
                StartCoroutine(BreakAndRespawn());
                return;
            }
        }

        // ぽんたに当たった
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController =
                collision.gameObject.GetComponent<PlayerController>();
            if (playerController != null && !playerController.isDead)
            {
                if (playerController.isInvincible)
                {
                    StartCoroutine(BreakAndRespawn());
                    return;
                }
                playerController.Die();
            }
        }
    }

    IEnumerator BreakAndRespawn()
    {
        currentState = State.Broken;

        // 落下を止める
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        // バラバラ画像に変更
        if (breakSprite != null)
            spriteRenderer.sprite = breakSprite;

        // Colliderを無効化
        if (col != null) col.enabled = false;

        // 復活まで待機
        yield return new WaitForSeconds(respawnTime);

        // 元の位置に復活
        Respawn();
    }

    void Respawn()
    {
        transform.position = startPosition;
        currentState = State.Idle;
        isActivated = false;

        if (idleSprite != null)
            spriteRenderer.sprite = idleSprite;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;

        if (col != null) col.enabled = true;
    }

    public void ResetObject()
    {
        StopAllCoroutines();
        Respawn();
    }

    // Sceneビューで検知範囲を表示
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            transform.position - new Vector3(0, detectRangeY / 2f, 0),
            new Vector3(detectRangeX * 2f, detectRangeY, 0));
    }
}