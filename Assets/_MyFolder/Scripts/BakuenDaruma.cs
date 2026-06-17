using UnityEngine;
using System.Collections;

public class BakuenDaruma : MonoBehaviour, IResettable
{
    [Header("検知設定")]
    public float detectRangeX = 40f;

    [Header("待機アニメーション（4枚）")]
    public Sprite[] idleSprites;
    public float idleAnimInterval = 0.2f;

    [Header("攻撃アニメーション（6枚）")]
    public Sprite[] attackSprites;
    public float attackAnimInterval = 0.1f;

    [Header("炎設定")]
    public GameObject flamePrefab;
    public Transform flameSpawnPoint;
    public float attackCooldown = 2f;

    [Header("爆発アニメーション（5枚）")]
    public Sprite[] explosionSprites;
    public float explosionAnimInterval = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Vector3 startPosition;

    private bool isAttacking = false;
    private bool isDead = false;
    private bool isCooldown = false;
    private float cooldownTimer = 0f;

    private int idleAnimIndex = 0;
    private float idleAnimTimer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player")?.transform;
        startPosition = transform.position;

        if (idleSprites.Length > 0)
            spriteRenderer.sprite = idleSprites[0];
    }

    void Update()
    {
        if (isDead) return;

        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isCooldown = false;
            }
        }

        if (!isAttacking)
        {
            UpdateIdleAnimation();

            if (player != null)
            {
                float distX = Mathf.Abs(
                    player.position.x - transform.position.x);

                if (distX <= detectRangeX && !isCooldown)
                {
                    StartCoroutine(AttackSequence());
                }
            }
        }
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

    IEnumerator AttackSequence()
    {
        isAttacking = true;

        // 攻撃アニメーション（6枚）
        for (int i = 0; i < attackSprites.Length; i++)
        {
            spriteRenderer.sprite = attackSprites[i];
            yield return new WaitForSeconds(attackAnimInterval);
        }

        // 炎を発射
        ShootFlame();

        // クールタイム
        isCooldown = true;
        cooldownTimer = attackCooldown;
        isAttacking = false;

        // 待機画像に戻す
        if (idleSprites.Length > 0)
            spriteRenderer.sprite = idleSprites[0];
    }

    void ShootFlame()
    {
        if (flamePrefab == null) return;

        Vector3 spawnPos = flameSpawnPoint != null ?
            flameSpawnPoint.position : transform.position;

        GameObject flame = Instantiate(
            flamePrefab, spawnPos, Quaternion.identity);
        flame.transform.SetParent(null);
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
            StartCoroutine(ExplosionSequence());
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                Rigidbody2D playerRb =
                    collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector2(
                        playerRb.linearVelocity.x, 5f);
                }
                StartCoroutine(ExplosionSequence());
                return;
            }
            else
            {
                if (!playerController.isDead)
                {
                    playerController.Die();
                }
                return;
            }
        }
    }

    IEnumerator ExplosionSequence()
    {
        isDead = true;
        StopCoroutine(nameof(AttackSequence));

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 爆発アニメーション（5枚）
        for (int i = 0; i < explosionSprites.Length; i++)
        {
            spriteRenderer.sprite = explosionSprites[i];
            yield return new WaitForSeconds(explosionAnimInterval);
        }

        gameObject.SetActive(false);
    }

    public void ResetObject()
    {
        StopAllCoroutines();
        isDead = false;
        isAttacking = false;
        isCooldown = false;
        cooldownTimer = 0f;
        idleAnimIndex = 0;
        idleAnimTimer = 0f;
        transform.position = startPosition;
        gameObject.SetActive(true);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        if (idleSprites.Length > 0)
            spriteRenderer.sprite = idleSprites[0];
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            transform.position - new Vector3(detectRangeX / 2f, 0f, 0f),
            new Vector3(detectRangeX, 2f, 0f));
    }
}