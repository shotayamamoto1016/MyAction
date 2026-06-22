using UnityEngine;
using System.Collections;

public class KageBoshi : MonoBehaviour, IResettable
{
    [Header("追跡設定")]
    public float chaseSpeed = 5f;       // 追跡速度
    public float activateDistance = 1f; // ぽんたを追い越したと判断する距離

    [Header("アニメーション設定")]
    public RuntimeAnimatorController kageBoshiAnimator; // 影法師用Animatorコントローラー

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Transform player;
    private PlayerController playerController;
    private Vector3 startPosition;

    private bool isChasing = false;
    private bool isDead = false;
    //private bool facingRight = false; // 最初は左向き


    // ぽんたの入力を模倣するための変数
    private float moveInput = 0f;
   // private bool isJumping = false;
    private bool isGrounded = false;

    [Header("地面判定")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public float jumpForce = 10f;
    public float moveSpeed = 5f;

    Color kageBoshiColor = new Color(20f / 255f, 20f / 255f, 20f / 255f, 1f);

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        startPosition = transform.position;

        player = GameObject.FindWithTag("Player")?.transform;
        if (player != null)
        {
            playerController = player.GetComponent<PlayerController>();
        }

        // R,G,B全て20に設定
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(
                20f / 255f, 20f / 255f, 20f / 255f, 1f);

        // 初期向きを明示的に設定
        //facingRight = false;
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z);
    }

    void Update()
    {
        if (isDead) return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, checkRadius, groundLayer);

        if (!isChasing)
        {
            // ぽんたが追い越したか確認
            CheckPlayerPassed();
        }
        else
        {
            // ぽんたの動きを模倣
            MimicPlayer();
        }

        // アニメーターを更新
        UpdateAnimation();

        // ScaleXを1に固定する
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void CheckPlayerPassed()
    {
        if (player == null) return;

        // ぽんたが影法師より右に行ったら追跡開始
        if (player.position.x > transform.position.x + activateDistance)
        {
            isChasing = true;
            Debug.Log("影法師：追跡開始！");
        }
    }

    void MimicPlayer()
    {
        if (player == null || playerController == null) return;

        // ぽんたとの位置差を計算
        float diffX = player.position.x - transform.position.x;
        float diffY = player.position.y - transform.position.y;

        // 横移動：ぽんたの方向に移動
        if (Mathf.Abs(diffX) > 0.5f)
        {
            moveInput = diffX > 0 ? 1f : -1f;
        }
        else
        {
            moveInput = 0f;
        }

        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y);


        // ジャンプ：ぽんたが上にいて地面にいる場合はジャンプ
        if (diffY > 1f && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x, jumpForce);
        }
    }

    void UpdateAnimation()
    {
        if (anim == null) return;

        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isConfused", false);
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

        
        // 横から当たった → ぽんた死亡
        if (!playerController.isDead)
        {
            playerController.Die();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (!other.CompareTag("Player")) return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        if (pc.isInvincible)
        {
            isDead = true;
            gameObject.SetActive(false);
            return;
        }

        // トリガーでも死亡判定 
        if (!pc.isDead)
        {
            pc.Die();
        }
    }

    public void ResetObject()
    {
        StopAllCoroutines();
        isDead = false;
        isChasing = false;
        moveInput = 0f;
        transform.position = startPosition;
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z);
        rb.linearVelocity = Vector2.zero;
        // R,G,B全て20に設定 
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(
                20f / 255f, 20f / 255f, 20f / 255f, 1f);

        gameObject.SetActive(true);

        if (anim != null)
        {
            anim.enabled = true;
            anim.Play("Ponta_Idle");
        }
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}