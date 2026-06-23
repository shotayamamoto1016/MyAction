using UnityEngine;
using System.Collections;

public class FireCrow : MonoBehaviour, IResettable
{
    [Header("上下移動設定")]
    public float moveRange = 1.5f;
    public float moveSpeed = 1.5f;

    [Header("検知設定")]
    public float attackRange = 6f;

    [Header("攻撃設定")]
    public int fireCount = 3;
    public float fireInterval = 0.6f;
    public float attackCooldown = 2.5f;
    public GameObject fireballPrefab;
    public float fireballSpeed = 6f;
    public float fireballOffsetY = 0f;

    [Header("アニメーション（2枚）")]
    public Sprite[] flySprites;
    public float flyAnimInterval = 0.15f;

    [Header("吐息エフェクト（2枚）")]
    public Sprite[] breathSprites;
    public float breathAnimInterval = 0.15f;

    [Header("死亡アニメーション（3枚）")]
    public Sprite[] deathSprites;
    public float deathAnimInterval = 0.15f;
    public float ascendSpeed = 2.5f;

    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Vector3 startPosition;
    private float waveTimer = 0f;
    private bool isAttacking = false;
    private bool isCooldown = false;
    private bool isDead = false;
    private float cooldownTimer = 0f;
    private int flyAnimIndex = 0;
    private float flyAnimTimer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player")?.transform;
        startPosition = transform.position;
    }

    void Update()
    {
        if (player == null || isDead) return;

        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isCooldown = false;
            }
        }

        // 上下に波を描く移動は常に行う
        waveTimer += Time.deltaTime * moveSpeed;
        float y = startPosition.y + Mathf.Sin(waveTimer) * moveRange;
        transform.position = new Vector3(
            transform.position.x, y, transform.position.z);

        if (!isAttacking)
        {
            UpdateFlyAnimation();
           

            float distance = Vector2.Distance(
                transform.position, player.position);

            if (distance < attackRange && !isCooldown)
            {
                StartCoroutine(AttackSequence());
            }
        }
    }

    void UpdateFlyAnimation()
    {
        if (flySprites.Length == 0) return;

        flyAnimTimer += Time.deltaTime;
        if (flyAnimTimer >= flyAnimInterval)
        {
            flyAnimTimer = 0f;
            flyAnimIndex = (flyAnimIndex + 1) % flySprites.Length;
            spriteRenderer.sprite = flySprites[flyAnimIndex];
        }
    }

    

    IEnumerator AttackSequence()
    {
        isAttacking = true;

        for (int i = 0; i < fireCount; i++)
        {
            // 飛行アニメーション表示
            if (flySprites.Length > 0)
            {
                spriteRenderer.sprite = flySprites[0];
            }
            yield return new WaitForSeconds(fireInterval);

            // 吐息アニメーション
            if (breathSprites.Length > 0)
            {
                spriteRenderer.sprite = breathSprites[0];
                yield return new WaitForSeconds(breathAnimInterval);
            }

            if (breathSprites.Length > 1)
            {
                spriteRenderer.sprite = breathSprites[1];
                yield return new WaitForSeconds(breathAnimInterval);
            }

            // 火の弾を発射
            ShootFireball();

            // 飛行画像に戻す
            if (flySprites.Length > 0)
            {
                spriteRenderer.sprite = flySprites[0];
            }

            yield return new WaitForSeconds(fireInterval);
        }

        isAttacking = false;
        isCooldown = true;
        cooldownTimer = attackCooldown;
    }

    void ShootFireball()
    {
        if (fireballPrefab == null || player == null) return;

        // 左方向に発射
        float direction = -1f;

        Vector3 spawnPos = transform.position +
            new Vector3(direction * 0.5f, 0f, 0f);

        GameObject fireball = Instantiate(
            fireballPrefab, spawnPos, Quaternion.identity);

        fireball.transform.localScale = Vector3.one;

        Rigidbody2D fireballRb = fireball.GetComponent<Rigidbody2D>();
        if (fireballRb != null)
        {
            fireballRb.linearVelocity = new Vector2(
                direction * fireballSpeed, 0f);
        }

       
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player == null) return;

        if (player.isInvincible)
        {
           // isDead = true;
            HandleDeath();
            //StopCoroutine(nameof(AttackSequence)); // AttackSequenceだけ止める
            //StartCoroutine(DeathAnimation());
            return;
        }

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 上から踏まれた
            if (contact.normal.y < -0.5f)
            {
                //isDead = true; 
                //StopCoroutine(nameof(AttackSequence));

                // カラスを倒す
                //StartCoroutine(DeathAnimation());

                // ぽんたを跳ね上げる
                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector2(
                        playerRb.linearVelocity.x, 5f);
                }

                HandleDeath(); // すべてのコルーチンを止めて昇天
                return;
            }
        }

        // 上以外から当たった場合は死亡
        if (!player.isDead)
        {
            player.Die();
        }

        // 攻撃を止めて死亡演出開始
        void HandleDeath()
        {
            isDead = true;
            isAttacking = false; // 攻撃中フラグを下ろす

            StopAllCoroutines();

            // 死亡アニメーションを開始
            StartCoroutine(DeathAnimation());
        }
    }

    // 死亡アニメーション
    IEnumerator DeathAnimation()
    {
        // Colliderを無効化
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        transform.localScale = new Vector3(
        transform.localScale.x * 0.7f,
        transform.localScale.y * 0.7f,
        transform.localScale.z);

        for (int i = 0; i < deathSprites.Length; i++)
        {
            spriteRenderer.sprite = deathSprites[i];

            // 上に上昇
            float elapsed = 0f;
            while (elapsed < deathAnimInterval)
            {
                transform.position += Vector3.up * ascendSpeed * Time.deltaTime;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // 3枚目まで表示したら消える
        gameObject.SetActive(false);
    }

    public void ResetObject()
    {
        StopAllCoroutines();
        isDead = false;
        isAttacking = false;
        isCooldown = false;
        cooldownTimer = 0f;
        waveTimer = 0f;
        flyAnimTimer = 0f;
        // 当たり判定と見た目を復活させる
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        transform.position = startPosition;
        

        if (flySprites.Length > 0)
        {
            spriteRenderer.sprite = flySprites[0];
        }

        gameObject.SetActive(true);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}