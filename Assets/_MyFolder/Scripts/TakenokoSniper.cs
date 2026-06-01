using UnityEngine;

public class TakenokoSniper : MonoBehaviour
{
    [Header("出現設定")]
    public float detectRange = 6f;
    public float emergeSpeed = 0.1f;
    public float hideOffsetY = -1f; // 地面に隠れる深さ

    [Header("射撃設定")]
    public float shootRange = 5f;       // 射撃する距離
    public float shootInterval = 2f;    // 射撃間隔
    public float bulletSpeed = 6f;      // 弾の速度
    public float bulletHeight = 3f;     // 弾の山なりの高さ
    public GameObject bulletPrefab;     // 弾のPrefab

    [Header("出現アニメーション（3枚）")]
    public Sprite[] emergeSprites;
    public float emergeInterval = 0.1f;

    [Header("射撃アニメーション（4枚）")]
    public Sprite[] shootSprites;
    public float shootAnimInterval = 0.1f;

    [Header("トゲアニメーション（3枚）")]
    public Sprite[] spikeSprites;
    public float spikeAnimInterval = 0.1f;

    [Header("クールタイム設定")]
    public float spikeCooldown = 5f;    // トゲ後の弾を撃たない時間
    private bool isCoolingDown = false; // クールタイム中かどうか
    private float cooldownTimer = 0f;   // クールタイム残り時間計測用


    private SpriteRenderer spriteRenderer;
    private Transform player;
    //private bool isEmerged = false;
    //private bool isEmerging = false;
    //private bool isShooting = false;
    //private bool isSpiking = false;
    private float shootTimer = 0f;

    private Vector3 hidePosition;
    private Vector3 showPosition;

    private enum State { Hiding, Emerging, Idle, Shooting, Spiking }
    private State currentState = State.Hiding;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player")?.transform;

        // 最初は地面に隠れた位置に移動
        hidePosition = transform.position + Vector3.up * hideOffsetY;
        showPosition = transform.position;
        transform.position = hidePosition;
    }

    void Update()
    {
        if (player == null) return;

        // クールタイムのカウントダウン
        if (isCoolingDown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                isCoolingDown = false;
                Debug.Log("竹の子スナイパー：攻撃再開！");
            }
        }

        float distanceToPlayer = Vector2.Distance(
            transform.position, player.position);

        switch (currentState)
        {
            case State.Hiding:
                if (distanceToPlayer < detectRange)
                {
                    // 出現前に向きを設定
                    FacePlayer();
                    StartCoroutine(EmergeAnimation());
                }
                break;

            case State.Idle:
                shootTimer += Time.deltaTime;

                // 常にぽんたの方向を向く
                FacePlayer();

                if (distanceToPlayer < shootRange && shootTimer >= shootInterval && !isCoolingDown)
                {
                    shootTimer = 0f;
                    StartCoroutine(ShootAnimation());
                }
                break;
        }
    }

    void FacePlayer()
    {
        if (player == null) return;

        if (player.position.x < transform.position.x)
        {
            // ぽんたが左にいる → 左向き（正面）
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z);
        }
        else
        {
            // ぽんたが右にいる → 右向き（反転）
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z);
        }
    }

    System.Collections.IEnumerator EmergeAnimation()
    {
        currentState = State.Emerging;

        for (int i = 0; i < emergeSprites.Length; i++)
        {
            spriteRenderer.sprite = emergeSprites[i];

            // 少しずつ上に移動
            float elapsed = 0f;
            while (elapsed < emergeInterval)
            {
                transform.position = Vector3.Lerp(
                    hidePosition,
                    showPosition,
                    (float)i / emergeSprites.Length +
                    (elapsed / emergeInterval) / emergeSprites.Length);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // 出現完了
        transform.position = showPosition;
        currentState = State.Idle;
    }

    // 射撃アニメーション
    System.Collections.IEnumerator ShootAnimation()
    {
        currentState = State.Shooting;

        for (int i = 0; i < shootSprites.Length; i++)
        {
            spriteRenderer.sprite = shootSprites[i];
            yield return new WaitForSeconds(shootAnimInterval);
        }

        // 弾を発射
        ShootBullet();

        // 最後の画像に少し待機してからIdleに戻る
        yield return new WaitForSeconds(0.3f);
        currentState = State.Idle;
    }

    

    void ShootBullet()
    {
        if (bulletPrefab == null || player == null) return;

        // 【修正】向きに合わせて発射位置を調整
        // プレイヤーが左にいればマイナス方向、右にいればプラス方向にオフセットをかける
        float directionX = (player.position.x < transform.position.x) ? -1f : 1f;

        // 元の 0.8f から -0.3f して、高さ 0.5f に調整
        Vector3 spawnPos = transform.position + new Vector3(directionX * 1.0f, 0.5f, 0f);

        GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        // 弾が自分（竹の子）に当たらないように設定
        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        Collider2D sniperCollider = GetComponent<Collider2D>();
        if (bulletCollider != null && sniperCollider != null)
        {
            Physics2D.IgnoreCollision(bulletCollider, sniperCollider);
        }

        Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
        if (bulletRb == null) return;

        // 放物線の計算
        float dx = player.position.x - spawnPos.x;
        float dy = player.position.y - spawnPos.y;

        float timeToReach = Mathf.Abs(dx) / bulletSpeed;
        if (timeToReach < 0.5f) timeToReach = 0.5f;

        float vx = dx / timeToReach;
        float gravity = bulletRb.gravityScale * 9.81f;
        float vy = (dy / timeToReach) + (0.5f * gravity * timeToReach);

        vy += bulletHeight * 0.5f;

        bulletRb.linearVelocity = new Vector2(vx, vy);

        // 弾の向きを発射方向に合わせる
        float angle = Mathf.Atan2(vy, vx) * Mathf.Rad2Deg;
        bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // トゲアニメーション（踏まれそうになった時）
    System.Collections.IEnumerator SpikeAnimation(GameObject pontaObj)
    {
        currentState = State.Spiking;

        for (int i = 0; i < spikeSprites.Length; i++)
        {
            spriteRenderer.sprite = spikeSprites[i];
            yield return new WaitForSeconds(spikeAnimInterval);
        }

        // ぽんたを死亡させる
        pontaObj.SetActive(false);

        yield return new WaitForSeconds(0.5f);

        //クールタイム5秒間開始
        isCoolingDown = true;
        cooldownTimer = spikeCooldown;
        Debug.Log("竹の子スナイパー：クールタイム開始（5秒間）");

        currentState = State.Idle;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (currentState == State.Hiding || currentState == State.Emerging) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                // 上から踏もうとした → トゲで返り討ち
                if (currentState != State.Spiking)
                {
                    StartCoroutine(SpikeAnimation(collision.gameObject));
                }
                return;
            }
            else
            {
                // 横から当たった → ぽんた死亡
                collision.gameObject.SetActive(false);
                return;
            }
        }
    }
}