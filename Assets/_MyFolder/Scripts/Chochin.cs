using UnityEngine;
using System.Collections;

public class Chochin : MonoBehaviour
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

        // 1. 左右の動き：これを極端に遅くすることで、上下の波が目立ちます
        float x = startPosition.x + Mathf.Sin(timeCounter * moveSpeed) * moveRange;

        // 2. 上下の動き：Amplitude（高さ）を大きくし、Frequency（速さ）を抑える
        // ここで「激しいけれどゆっくり」を表現します
        float y = startPosition.y + Mathf.Sin(timeCounter * waveFrequency) * waveAmplitude;

        transform.position = new Vector3(x, y, transform.position.z);

        // 3. 視覚的演出：波の頂点で少し「タメ」を作るような回転
        // 波の動き（Sin）に合わせて、提灯の角度を大きく揺らします
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
        // ぽんたを非表示にする（または死亡演出へ）
        playerObj.SetActive(false);
        Debug.Log("ぽんたが提灯にやられた！");

        // ここでリスタート処理などを呼ぶとゲームらしくなります
    }

    IEnumerator AscendAnimation()
    {
        isDead = true;
        GetComponent<Collider2D>().enabled = false;
        transform.rotation = Quaternion.identity;

        for (int i = 0; i < ascendSprites.Length; i++)
        {
            spriteRenderer.sprite = ascendSprites[i];
            yield return new WaitForSeconds(ascendInterval);
        }
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}