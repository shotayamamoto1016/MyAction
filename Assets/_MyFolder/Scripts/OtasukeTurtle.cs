using System.Collections;
using UnityEngine;

public class OtasukeTurtle : MonoBehaviour, IResettable
{
    [Header("移動設定")]
    public float moveSpeed = 1.5f;
    public float chaseSpeed = 4.0f;
    public Vector2 detectRange = new Vector2(10f, 1f);

    [Header("投げ設定")]
    public float throwForce = 25f;
    public float deathYHeight = 5.0f;

    [Header("判定用")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public Transform wallCheckFront;
    public float checkRadius = 0.1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private Transform player;

    private bool isChasing = false;
    private bool isThrowing = false;
    private bool movingLeft = true;

    private float flipCoolTime = 0f;
    private float flipCoolDuration = 0.5f; 

    private Vector3 startPosition;

    // 前フレームのground/wall状態を記録
    private bool wasGrounded = true;
    private bool wasWall = false;

   

    void Start()
    {
        startPosition = transform.position;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;

        movingLeft = true;
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z);
    }

    void Update()
    {
        if (isThrowing) return;

        if (flipCoolTime > 0f) flipCoolTime -= Time.deltaTime;

        CheckPlayerRange();
        Move();
    }

    void CheckPlayerRange()
    {
        if (player == null) return;
        float diffX = Mathf.Abs(player.position.x - transform.position.x);
        float diffY = Mathf.Abs(player.position.y - transform.position.y);

        bool newChasing = (diffX <= detectRange.x && diffY <= detectRange.y);

        // 追跡状態が変わった時だけアニメーターを更新
        if (newChasing != isChasing)
        {
            isChasing = newChasing;
            if (anim != null) anim.SetBool("isChasing", isChasing);
        }
    }

    void Move()
    {
        float currentSpeed = isChasing ? chaseSpeed : moveSpeed;

        if (flipCoolTime <= 0f)
        {
            bool isGrounded = Physics2D.OverlapCircle(
                groundCheck.position, checkRadius, groundLayer);
            bool isWall = wallCheckFront != null && Physics2D.OverlapCircle(
                wallCheckFront.position, checkRadius, groundLayer);

            // 状態が変化した時だけ反転する
            bool shouldFlip = false;

            if (!isGrounded && wasGrounded) shouldFlip = true;
            if (isWall && !wasWall) shouldFlip = true;

            if (shouldFlip)
            {
                Flip();
            }

            wasGrounded = isGrounded;
            wasWall = isWall;
        }

        // 追跡中はぽんたの方向に向く
        if (isChasing && player != null)
        {
            bool playerIsLeft = player.position.x < transform.position.x;
            if (playerIsLeft != movingLeft)
            {
                movingLeft = playerIsLeft;
                Vector3 scale = transform.localScale;
                scale.x = movingLeft ?
                    Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }

        rb.linearVelocity = new Vector2(
            movingLeft ? -currentSpeed : currentSpeed,
            rb.linearVelocity.y);

        if (anim != null)
            anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
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
        if (isThrowing || !collision.gameObject.CompareTag("Player")) return;

        PlayerController pc = collision.gameObject.GetComponent<PlayerController>();
        if (pc != null && !pc.isDead)
        {
            if (pc.isInvincible)
            {
                gameObject.SetActive(false);
                return;
            }
            StartCoroutine(ThrowRoutine(pc));
        }
    }

    IEnumerator ThrowRoutine(PlayerController pc)
    {
        isThrowing = true;
        rb.linearVelocity = Vector2.zero;

        Rigidbody2D playerRb = pc.GetComponent<Rigidbody2D>();
        playerRb.linearVelocity = Vector2.zero;
        playerRb.bodyType = RigidbodyType2D.Kinematic;

        // 入力を無効化 
        pc.isInputDisabled = true;

        // 投げアニメーション
        if (anim != null) anim.SetTrigger("Throw");

        yield return new WaitForSeconds(0.4f);

        // ぽんたを少し上に移動させてから投げる 
        pc.transform.position = new Vector3(
            pc.transform.position.x,
            pc.transform.position.y + 1.0f, // 高さを調整
            pc.transform.position.z);

        // Groundとの当たり判定を無効化 
        pc.DisableGroundCollision();

        playerRb.bodyType = RigidbodyType2D.Dynamic;
        playerRb.linearVelocity = new Vector2(0, throwForce);

        GSound.Instance.PlaySe(SoundData.SeType.Enemy_Takenoko.ToString());

        yield return new WaitForSeconds(0.5f);
        isThrowing = false;
        StartCoroutine(MonitorHeight(pc));
    }

    IEnumerator MonitorHeight(PlayerController pc)
    {
        // 投げられた直後は少し待つ
        yield return new WaitForSeconds(0.3f);

        while (pc != null && !pc.isDead)
        {
            // Y座標が5以上になったら死亡 
            if (pc.transform.position.y >= 5f)
            {
                pc.EnableGroundCollision();
                // 入力を有効化 
                pc.isInputDisabled = false;
                pc.Die();
                yield break;
            }
            yield return null;
        }

        // ループを抜けた場合もGround判定を戻す 
        if (pc != null)
        {
            pc.EnableGroundCollision();

            // 入力を有効化 
            pc.isInputDisabled = false;
        }
    }

    public void ResetObject()
    {
        StopAllCoroutines();
        isChasing = false;
        isThrowing = false;
        movingLeft = true;
        flipCoolTime = 0f;
        wasGrounded = true;
        wasWall = false;
        transform.position = startPosition;
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z);
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(true);
        if (anim != null) anim.Play("Idle");
    }
}