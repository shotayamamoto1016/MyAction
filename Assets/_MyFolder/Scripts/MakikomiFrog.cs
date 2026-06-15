using UnityEngine;
using System.Collections;

public class MakikomiFrog : MonoBehaviour, IResettable
{
    [Header("移動設定")]
    public float jumpForce = 8f;
    public float moveSpeed = 3f;

    [Header("地面・壁判定")]
    public LayerMask groundLayer;
    public Transform groundCheck;　//地面の判定
    public Transform wallCheckFront; // 全法の壁判定
    public Transform wallCheckBack; // 後方の壁判定
    public float checkRadius = 0.1f;

    [Header("舌攻撃設定")]
    public float tongueDuration = 0.5f;　
    public float tongueRange = 4f;
    public Vector3 tongue1Offset = Vector3.zero;

    [Header("待機アニメーション（4枚）")]
    public Sprite[] idleSprites;
    public float idleAnimInterval = 0.2f;
    public float idleWaitTime = 1.5f;

    [Header("ジャンプアニメーション（4枚）")]
    public Sprite[] jumpSprites;
    public float jumpAnimInterval = 0.1f;
    public int jumpFrameIndex = 1; // 何枚目からジャンプするか

    [Header("舌アニメーション（2枚）")]
    public Sprite[] tongueSprites;
    public float tongueAnimInterval = 0.15f;
    public float tongue1MoveSpeed = 1.5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Transform player;
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
        player = GameObject.FindWithTag("Player")?.transform;
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
            yield return StartCoroutine(IdleSequence());
            yield return StartCoroutine(JumpSequence());
            yield return new WaitUntil(() => isGrounded);
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(JumpSequence());
            yield return new WaitUntil(() => isGrounded);
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(TongueAttack());
        }
    }

    IEnumerator IdleSequence()
    {
        float timer = 0f;
        int animIndex = 0;

        while (timer < idleWaitTime)
        {
            if (idleSprites.Length > 0)
            {
                spriteRenderer.sprite = idleSprites[animIndex];
                animIndex = (animIndex + 1) % idleSprites.Length;
            }
            yield return new WaitForSeconds(idleAnimInterval);
            timer += idleAnimInterval;
        }
    }

    IEnumerator JumpSequence()
    {
        // ジャンプ前のアニメーション
        for (int i = 0; i < jumpFrameIndex && i < jumpSprites.Length; i++)
        {
            spriteRenderer.sprite = jumpSprites[i];
            yield return new WaitForSeconds(jumpAnimInterval);
        }

        // 壁・端判定してから向きを決定
        CheckFlip();

        // jumpFrameIndex枚目でジャンプ実行
        if (jumpFrameIndex < jumpSprites.Length)
        {
            spriteRenderer.sprite = jumpSprites[jumpFrameIndex];
        }

        rb.linearVelocity = new Vector2(
            movingLeft ? -moveSpeed : moveSpeed, jumpForce);

        // ④ 残りのアニメーションを空中で再生
        for (int i = jumpFrameIndex + 1; i < jumpSprites.Length; i++)
        {
            spriteRenderer.sprite = jumpSprites[i];
            yield return new WaitForSeconds(jumpAnimInterval);
        }

        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TongueAttack()
    {
        float directionX = movingLeft ? -1f : 1f;

        // 舌攻撃中は落下しないようにする
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // そのまま表示
        if (tongueSprites.Length > 0)
        {
            spriteRenderer.sprite = tongueSprites[0];
            yield return new WaitForSeconds(tongueAnimInterval);
        }

        // 位置をオフセットして表示 
        if (tongueSprites.Length > 1)
        {
            spriteRenderer.sprite = tongueSprites[1];

            // 2枚目でX座標をずらす
            Vector3 offsetPos = new Vector3(
                -directionX * tongue1Offset.x,
                tongue1Offset.y,
                0f);
            transform.position += offsetPos;

            yield return new WaitForSeconds(tongueAnimInterval);
        }

        // 当たり判定
        CheckTongueHit();

        yield return new WaitForSeconds(tongueDuration);

        // 元の位置に戻す
        transform.position -= new Vector3(
            -directionX * tongue1Offset.x,
            tongue1Offset.y,
            0f);

        // Dynamicに戻す
        rb.bodyType = RigidbodyType2D.Dynamic;

        if (idleSprites.Length > 0)
        {
            spriteRenderer.sprite = idleSprites[0];
        }
    }

    void CheckTongueHit()
    {
        if (player == null) return;

        float directionX = movingLeft ? -1f : 1f;
        Vector2 startPos = transform.position;
        Vector2 endPos = startPos + new Vector2(directionX * tongueRange, 0f);

        Debug.DrawLine(startPos, endPos, Color.green, 2f);
        Debug.Log("舌判定開始 startPos: " + startPos + " endPos: " + endPos);

        // OverlapAreaで範囲判定に変更 
        Vector2 boxCenter = (startPos + endPos) / 2f;
        Vector2 boxSize = new Vector2(tongueRange, 1.5f);

        Collider2D hit = Physics2D.OverlapBox(
            boxCenter, boxSize, 0f, LayerMask.GetMask("Player"));

        if (hit != null)
        {
            Debug.Log("舌ヒット！: " + hit.gameObject.name);
            PlayerController playerController =
                hit.GetComponent<PlayerController>();
            if (playerController != null && !playerController.isDead &&
                !playerController.isInvincible)
            {
                playerController.Die();
            }
        }
        else
        {
            Debug.Log("舌ミス");

            
            Collider2D hitNoMask = Physics2D.OverlapBox(
                boxCenter, boxSize, 0f);
            if (hitNoMask != null)
            {
                Debug.Log("LayerMaskなしでヒット: " + hitNoMask.gameObject.name +
                    " Layer: " + hitNoMask.gameObject.layer);
            }
        }
    }
    void CheckFlip()
    {
        if (flipCoolTime > 0f) return;

        // 前方の壁判定
        bool isFrontWall = wallCheckFront != null && Physics2D.OverlapCircle(
            wallCheckFront.position, checkRadius, groundLayer);

        // 後方の壁判定 
        bool isBackWall = wallCheckBack != null && Physics2D.OverlapCircle(
            wallCheckBack.position, checkRadius, groundLayer);

        // 地面端の判定
        bool noGround = groundCheck != null && !Physics2D.OverlapCircle(
            groundCheck.position, checkRadius, groundLayer);

        if (isFrontWall || noGround)
        {
            Flip();
        }
        else if (isBackWall)
        {
            // 後方に壁がある場合も反転
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
            gameObject.SetActive(false);
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                isDead = true;
                gameObject.SetActive(false);

                Rigidbody2D playerRb =
                    collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector2(
                        playerRb.linearVelocity.x, 5f);
                }
                return;
            }
            else
            {
                if (!playerController.isDead)
                {
                    playerController.Die();
                    rb.linearVelocity = new Vector2(
                        movingLeft ? -moveSpeed : moveSpeed,
                        rb.linearVelocity.y);
                }
                return;
            }
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

        if (idleSprites.Length > 0)
        {
            spriteRenderer.sprite = idleSprites[0];
        }

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

        float dir = Application.isPlaying ? (movingLeft ? -1f : 1f) : -1f;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(
            transform.position,
            transform.position + new Vector3(dir * tongueRange, 0f, 0f));
    }
}