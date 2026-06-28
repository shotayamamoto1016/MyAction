using System.Collections;
using UnityEngine;
//using static Unity.Cinemachine.InputAxisControllerBase<T>;

public class Chochin : MonoBehaviour, IResettable
{
    [Header("通常移動設定（大きく波打つ）")]
    public float moveSpeed = 0.5f;      // 左右移動の速さ
    public float moveRange = 8.0f;      // 左右の幅
    public float horizontalWaveFreq = 2.0f; // 左右の細かな揺れ
    public float waveAmplitude = 1.0f;  // 上下の波の高さ
    public float waveFrequency = 8.0f;  // 上下の波の速さ

    [Header("追跡設定")]
    public float chaseSpeed = 7f;

    [Header("スプライト設定")]
    public Sprite normalSprite;
    public Sprite[] changeSprites;
    public float changeInterval = 0.15f;
    public Sprite[] ascendSprites;
    public float ascendInterval = 0.15f;
    public float ascendSpeed = 2.5f;

    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Vector3 startPosition;
    private float timeCounter = 0f;

    private enum State { Normal, Changing, Chasing, Ascending }
    private State currentState = State.Normal;
    private bool isDead = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        // タグでプレイヤーを探す
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void Update()
    {
        if (isDead)
        {
            // 昇天：ゆらゆら揺れながら上昇
            transform.position += Vector3.up * ascendSpeed * Time.deltaTime;
            transform.position += Vector3.right * Mathf.Sin(Time.time * 10f) * 0.05f;
            return;
        }

        switch (currentState)
        {
            case State.Normal:
                WavyMove();
                CheckPlayerPassed();
                break;
            case State.Chasing:
                ChasePlayer();
                break;
        }
    }

    void WavyMove()
    {
        timeCounter += Time.deltaTime;
        
        float x = startPosition.x + Mathf.Sin(timeCounter * moveSpeed) * moveRange;

        float y = startPosition.y + Mathf.Sin(timeCounter * waveFrequency) * waveAmplitude;

        transform.position = new Vector3(x, y, transform.position.z);

        float tilt = Mathf.Cos(timeCounter * waveFrequency) * 20f; // 20度くらい大きく揺らす
        transform.rotation = Quaternion.Euler(0, 0, tilt);
    }

    void CheckPlayerPassed()
    {
        if (player == null) return;
        // プレイヤーが提灯を1ユニット分追い越したら追跡開始
        if (player.position.x > transform.position.x + 1f)
        {
            StartCoroutine(ChangeAnimation());
        }
    }

    IEnumerator ChangeAnimation()
    {
        currentState = State.Changing;
        for (int i = 0; i < changeSprites.Length; i++)
        {
            spriteRenderer.sprite = changeSprites[i];
            yield return new WaitForSeconds(changeInterval);
        }
        currentState = State.Chasing;
    }

    void ChasePlayer()
    {
        if (player == null) return;
        // 回転をリセット（追跡時は傾かない）
        transform.rotation = Quaternion.identity;

        Vector3 targetPos = new Vector3(player.position.x, player.position.y + 0.3f, transform.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, chaseSpeed * Time.deltaTime);

        // 向きの反転
        Vector3 scale = transform.localScale;
        scale.x = (player.position.x > transform.position.x) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // 衝突相手がPlayerタグを持っているか
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 提灯の「上側」が触れたかどうかを判定 (Normal.yが負の値なら、相手が上にいる)
                if (contact.normal.y < -0.7f)
                {
                    // 頭を踏まれた！
                    StartCoroutine(AscendAnimation());
                    Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                    if (playerRb != null) playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 10f); // ぽんたを跳ね上げる
                    return;
                }
                else
                {
                    // 頭以外に当たった！
                    KillPlayer(collision.gameObject);
                    return;
                }
            }
        }
    }

    void KillPlayer(GameObject playerObj)
    {
        PlayerController player = playerObj.GetComponent<PlayerController>();
        if (player != null)
        {
            if (player.isInvincible)
            {
                Debug.Log("ぽんたが無敵なので、敵側で自爆処理を実行");
                StartCoroutine(AscendAnimation()); // 踏まれた時と同じ演出で消える
                return;
            }

            player.Die(); // ぽんたの死亡演出を呼び出す
        }
    }

    IEnumerator AscendAnimation()
    {
        isDead = true;

        // アニメーションを待たずに、倒された瞬間に座標を登録する
        if (CheckpointManager.instance != null)
        {
            CheckpointManager.instance.RegisterDefeatedChochin(startPosition);
        }

        GSound.Instance.PlaySe(SoundData.SeType.Enemy_Chouchin.ToString());
        GetComponent<Collider2D>().enabled = false;
        transform.rotation = Quaternion.identity;

        for (int i = 0; i < ascendSprites.Length; i++)
        {
            spriteRenderer.sprite = ascendSprites[i];
            yield return new WaitForSeconds(ascendInterval);
        }
        yield return new WaitForSeconds(0.5f);

        //// CheckpointManagerに登録 
        //if (CheckpointManager.instance != null)
        //{
        //    CheckpointManager.instance.RegisterDefeatedChochin(
        //        gameObject.GetInstanceID());
        //}

        // Destroyの代わりに非表示
        gameObject.SetActive(false);
    }

    // 公開用の初期座標取得プロパティ
    public Vector3 GetStartPosition()
    { 
        return startPosition;
    }


    // 倒された時にCheckpointManagerに登録
    //void OnDestroy()
    //{
    //    if (isDead && CheckpointManager.instance != null)
    //    {
    //        CheckpointManager.instance.RegisterDefeatedChochin(
    //            gameObject.GetInstanceID());
    //    }
    //}

    // 初期化処理
    public void ResetChochin()
    {
        StopAllCoroutines();
        currentState = State.Normal;
        isDead = false;
        timeCounter = 0f;
        transform.position = startPosition;

        if (spriteRenderer != null && normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }

        // Colliderを有効化
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        // Rigidbody2Dをリセット
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }

        gameObject.SetActive(true);
    }

    public void ResetObject()
    {
        //ResetChochin();
    }
}