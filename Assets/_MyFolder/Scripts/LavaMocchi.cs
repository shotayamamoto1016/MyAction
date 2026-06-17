using UnityEngine;
using System.Collections;

public class LavaMocchi : MonoBehaviour, IResettable
{
    [Header("移動設定")]
    public float jumpForce = 8f;
    public float normalSpeed = 2f;      // 通常速度
    public float fastSpeed = 5f;        // 速い速度
    public float crawlDuration = 2f;    // 地面を這う時間

    [Header("地面・壁判定")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public Transform wallCheckFront;
    public Transform wallCheckBack;
    public float checkRadius = 0.1f;

    [Header("ジャンプアニメーション（4枚）")]
    public Sprite[] jumpSprites;
    public Sprite landingSprite;       
    public float jumpAnimInterval = 0.1f;
    public float landingDuration = 0.2f;

    [Header("通常速度這いアニメーション（4枚）")]
    public Sprite[] normalCrawlSprites;
    public float normalCrawlAnimInterval = 0.2f;

    [Header("速い速度這いアニメーション（3枚）")]
    public Sprite[] fastCrawlSprites;
    public float fastCrawlAnimInterval = 0.1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;

    private bool isGrounded = false;
    private bool movingLeft = true;
    private bool isDead = false;

    private float flipCoolTime = 0f;
    private float flipCoolDuration = 0.5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;

        movingLeft = true;
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z);

        StartCoroutine(MainLoop());
    }

    void Update()
    {
        if (isDead) return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, checkRadius, groundLayer);

        if (flipCoolTime > 0f) flipCoolTime -= Time.deltaTime;
    }

    IEnumerator MainLoop()
    {
        while (!isDead)
        {
            // 1回目ジャンプ
            yield return StartCoroutine(JumpSequence());
            yield return new WaitUntil(() => isGrounded);
            yield return new WaitForSeconds(0.1f);

            // 2回目ジャンプ
            yield return StartCoroutine(JumpSequence());
            yield return new WaitUntil(() => isGrounded);
            yield return new WaitForSeconds(0.1f);

            // 這いながら進む（確率で通常or速い）
            yield return StartCoroutine(CrawlSequence());
        }
    }

    IEnumerator JumpSequence()
    {
        // 壁・端判定
        CheckFlip();

        // ジャンプアニメーション
        for (int i = 0; i < jumpSprites.Length; i++)
        {
            spriteRenderer.sprite = jumpSprites[i];

            // 2枚目でジャンプ実行
            if (i == 1)
            {
                rb.linearVelocity = new Vector2(
                    movingLeft ? -normalSpeed : normalSpeed,
                    jumpForce);
            }

            yield return new WaitForSeconds(jumpAnimInterval);
        }

        // 着地まで待つ
        yield return new WaitUntil(() => isGrounded);

        // 着地画像に差し替え
        if (landingSprite != null)
        {
            spriteRenderer.sprite = landingSprite;
        }

        // 着地画像を少し表示してから次へ
        yield return new WaitForSeconds(landingDuration);
    }

    IEnumerator CrawlSequence()
    {
        // 確率で通常(2/3)か速い(1/3)かを決定
        float rand = Random.Range(0f, 1f);
        bool isFast = rand < (1f / 3f);

        float currentSpeed = isFast ? fastSpeed : normalSpeed;
        Sprite[] currentSprites = isFast ? fastCrawlSprites : normalCrawlSprites;
        float currentInterval = isFast ? fastCrawlAnimInterval : normalCrawlAnimInterval;

        float elapsed = 0f;
        int animIndex = 0;
        float animTimer = 0f;

        while (elapsed < crawlDuration && !isDead)
        {
            // 這いアニメーション
            animTimer += Time.deltaTime;
            if (animTimer >= currentInterval && currentSprites.Length > 0)
            {
                animTimer = 0f;
                spriteRenderer.sprite = currentSprites[animIndex];
                animIndex = (animIndex + 1) % currentSprites.Length;
            }

            // 移動
            rb.linearVelocity = new Vector2(
                movingLeft ? -currentSpeed : currentSpeed,
                rb.linearVelocity.y);

            // 壁・端判定
            if (flipCoolTime <= 0f)
            {
                bool isFrontWall = wallCheckFront != null &&
                    Physics2D.OverlapCircle(
                        wallCheckFront.position, checkRadius, groundLayer);
                bool isBackWall = wallCheckBack != null &&
                    Physics2D.OverlapCircle(
                        wallCheckBack.position, checkRadius, groundLayer);
                bool noGround = groundCheck != null &&
                    !Physics2D.OverlapCircle(
                        groundCheck.position, checkRadius, groundLayer);

                if (isFrontWall || noGround || isBackWall)
                {
                    Flip();
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 這い終わったら止まる
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    void CheckFlip()
    {
        if (flipCoolTime > 0f) return;

        bool isFrontWall = wallCheckFront != null &&
            Physics2D.OverlapCircle(
                wallCheckFront.position, checkRadius, groundLayer);
        bool isBackWall = wallCheckBack != null &&
            Physics2D.OverlapCircle(
                wallCheckBack.position, checkRadius, groundLayer);
        bool noGround = groundCheck != null &&
            !Physics2D.OverlapCircle(
                groundCheck.position, checkRadius, groundLayer);

        if (isFrontWall || noGround || isBackWall)
        {
            Flip();
        }
    }

    void Flip()
    {
        movingLeft = !movingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        flipCoolTime = flipCoolDuration;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerController playerController =
            collision.gameObject.GetComponent<PlayerController>();
        if (playerController == null) return;

        if (playerController.isInvincible)
        {
            isDead = true;
            StopAllCoroutines();
            gameObject.SetActive(false);
            return;
        }

        // どこから触れても死亡
        if (!playerController.isDead)
        {
            playerController.Die();
        }
    }

    public void ResetObject()
    {
        StopAllCoroutines();
        isDead = false;
        movingLeft = true;
        flipCoolTime = 0f;
        transform.position = startPosition;
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z);
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(true);

        if (jumpSprites.Length > 0)
            spriteRenderer.sprite = jumpSprites[0];

        StartCoroutine(MainLoop());
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
        if (wallCheckFront != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wallCheckFront.position, checkRadius);
        }
        if (wallCheckBack != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(wallCheckBack.position, checkRadius);
        }
    }
}