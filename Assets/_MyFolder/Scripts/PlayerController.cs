using DG.Tweening.Core.Easing;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;      //移動スピード
    public float jumpForce = 10f;     //ジャンプ力

    [Header("地面判定用")]
    public LayerMask groundLayer;      //何を地面とするか
    public Transform groundCheck;     //足元の判定ポイント
    public float checkRadius = 0.2f;  //地面判定の範囲

    [Header("死亡時用画像")]
    public Sprite deathSprite;

    [Header("凍る画像")]
    public Sprite freezeSprite;

    [Header("毒画像")]
    public Sprite poisonSprite;

    [Header("混乱時の画像")]
    public Sprite confusedWalkSprite;  // 混乱歩き画像
    public Sprite confusedJumpSprite;  // 混乱ジャンプ画像
    public Sprite confusedIdleSprite;  // 混乱待機画像

    [Header("無敵モード設定")]
    public bool isInvincible = false;
    public float invincibilityDuration = 15f;
    public GameObject sparkleEffectPrefab;

    // 混乱フラグ
    public bool isConfused = false;
    private float confusedTimer = 0f;

    private Rigidbody2D rb;
    private Animator anim; // アニメーター用
    private bool isGrounded;
    private bool facingRight = true;

    public bool isDead = false;

    //private float originalMoveSpeed;
    //private float originalJumpForce;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        anim = GetComponent<Animator>(); // 開始時にAnimatorを取得

        // 元の値を保存
        //originalMoveSpeed = moveSpeed;
        //originalJumpForce = jumpForce;
    }

    

    void Update()
    {
        // 死んでいる間は以下の操作を全て無視する
        if (isDead) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // デバッグ用に追加
        //Debug.Log("isGrounded: " + isGrounded);

        //GetAxis ではなく GetAxisRaw を使う
        float moveInput = Input.GetAxisRaw("Horizontal");

        //入力が非常に小さい場合は、完全に0として扱う
        if (Mathf.Abs(moveInput) < 0.05f)
        {
            moveInput = 0;
        }

        // 混乱中は左右反転
        if (isConfused)
        {
            moveInput = -moveInput;

            // 混乱タイマー
            confusedTimer -= Time.deltaTime;
            if (confusedTimer <= 0f)
            {
                StopConfused();
            }

            // 混乱中のアニメーション管理
           // UpdateConfusedAnimation(moveInput);
        }

        //else
        //{
        //    // 通常のアニメーション
        //    float speed = Mathf.Abs(moveInput) > 0.1f ? Mathf.Abs(moveInput) : 0f;
        //    anim.SetFloat("Speed", speed);
        //    anim.SetBool("isGrounded", isGrounded);
        //}


        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        if (anim != null && anim.enabled)
        {
            anim.SetBool("isGrounded", isGrounded);
            anim.SetFloat("Speed", Mathf.Abs(moveInput));
            anim.SetBool("isConfused", isConfused); // Animatorに混乱中か伝える
        }

        //float speed = Mathf.Abs(moveInput) > 0.1f ? Mathf.Abs(moveInput) : 0f;
        //anim.SetFloat("Speed", speed);

        //anim.SetBool("isGrounded", isGrounded);


        if (moveInput > 0 && !facingRight) Flip();
        else if (moveInput < 0 && facingRight) Flip();

        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    //向きを反転させる関数
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    //デバッグ用に地面判定の円を表示
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }

    public void Die()
    {
        if (isDead ) return;
        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        isDead = true;

        // 画像を死んだ時のものに変える
        if (deathSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = deathSprite;
        }

        // 物理と当たり判定を無効化
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        // アニメーションを止める
        if (anim != null)
        {
            anim.enabled = false;
        }
            

        // 上にピョーンと跳ねる
        rb.AddForce(Vector2.up * 12f, ForceMode2D.Impulse);

        // GameManagerに通知
        GameManager.instance.OnPlayerDie();

        yield return null;
    }

    // 復活時にGameManagerから呼ばれる
    public void ResetPlayer()
    {
        isDead = false;
        GetComponent<Collider2D>().enabled = true;
        anim.enabled = true;
        // Animatorを再起動
        anim.Play("Ponta_Idle", 0, 0);
        rb.linearVelocity = Vector2.zero;
        // 必要なら、ここで「待機ポーズ」のアニメに強制的に戻す
    }

    // 敵に触れた時の判定
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return; // すでに死んでいたら何もしない

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (isInvincible)
            {
                // 無敵なら敵を即死させる
                Destroy(collision.gameObject);
                Debug.Log("無敵パワーで敵を倒した！");
                return;
            }
            else
            {
                // 通常時は上から踏んだか判定
                foreach (ContactPoint2D contact in collision.contacts)
                {
                    if (contact.normal.y > 0.5f)
                    {  
                        return;
                    }
                }

                Die();
            }
        }
    }

    // 弾に触れた時の判定
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (isInvincible)
            {
                // トリガー接触した敵（弾など）も消滅させる
                Destroy(collision.gameObject);
                Debug.Log("無敵パワーでトリガー接触した敵を倒した！");
            }
            else
            {
                // 即死
                Die();
            }
        }
    }

    public void FreezeAndDie(float freezeDuration)
    {
        if (isDead) return;
        StartCoroutine(FreezeDeathSequence(freezeDuration));
    }

    IEnumerator FreezeDeathSequence(float freezeDuration)
    {
        isDead = true;

        // 移動を止める
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // アニメーターを無効化
        if (anim != null) anim.enabled = false;

        // 凍る画像に変更
        if (freezeSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = freezeSprite;
        }

        // 少し上にずらす 
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + 0.5f, 
            transform.position.z);

        // 凍った状態で待機
        yield return new WaitForSeconds(freezeDuration);

        // 死亡画像に変更
        if (deathSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = deathSprite;
        }

        // Colliderを無効化
        GetComponent<Collider2D>().enabled = false;

        // 跳ねずにGameManagerに通知
        GameManager.instance.OnPlayerDie();
    }

    public void PoisonAndDie(float poisonDuration)
    {
        if (isDead) return;
        StartCoroutine(PoisonDeathSequence(poisonDuration));
    }

    IEnumerator PoisonDeathSequence(float poisonDuration)
    {
        isDead = true;

        // 移動を止める
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // アニメーターを無効化
        if (anim != null) anim.enabled = false;

        // 毒画像に変更
        if (poisonSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = poisonSprite;
        }

        // 少し上にずらす
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + 0.7f,
            transform.position.z);

        // 毒状態で待機
        yield return new WaitForSeconds(poisonDuration);

        // 死亡画像に変更
        if (deathSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = deathSprite;
        }

        // Colliderを無効化
        GetComponent<Collider2D>().enabled = false;

        // 跳ねずにGameManagerに通知
        GameManager.instance.OnPlayerDie();
    }

    // 混乱中のアニメーション管理
    
　　　void UpdateConfusedAnimation(float moveInput)
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (!isGrounded)
        {
            // ジャンプ中→ジャンプ画像
            if (confusedJumpSprite != null)
                sr.sprite = confusedJumpSprite;
        }
        else if (Mathf.Abs(moveInput) > 0.1f)
        {
            // 歩き中→混乱アニメーター
            if (anim != null)
            {
                anim.enabled = true;
                anim.Play("Ponta_Confused_Walk");
            }
        }
        else
        {
            // 待機中→気を付け画像
            if (anim != null) anim.enabled = false;
            if (confusedIdleSprite != null)
                sr.sprite = confusedIdleSprite;
        }
    }

    // 混乱開始
    public void StartConfused(float duration)
    {
        isConfused = true;
        confusedTimer = duration;

        // アニメーターを混乱用に切り替え
        if (anim != null)
        {
            //anim.enabled = true;

            anim.SetBool("isConfused", true);
        }
    }

    // 混乱終了
    void StopConfused()
    {
        isConfused = false;
        if (anim != null) anim.SetBool("isConfused", false);
        //confusedTimer = 0f;

        //// 通常アニメーターに戻す
        //SpriteRenderer sr = GetComponent<SpriteRenderer>();
        //if (anim != null)
        //{
        //    anim.enabled = true;
        //    anim.Play("Ponta_Idle");
        //}
    }

    // 金色キノコを取った時に呼ばれる
    public void StartInvincibility()
    {
        // すでに無敵なら一度リセットして時間を上書き
        StopCoroutine("InvincibilityRoutine");
        StartCoroutine("InvincibilityRoutine");
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        moveSpeed = 6.0f;
        jumpForce = 17.5f;

        // キラキラエフェクトを表示（ぽんたの子オブジェクトとして生成）
        GameObject sparkles = null;
        if (sparkleEffectPrefab != null)
        {
            sparkles = Instantiate(sparkleEffectPrefab, transform.position, Quaternion.identity, transform);
        }

        // 無敵中の演出：ぽんたをチカチカさせる、または金色っぽくする
        float timer = 0;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        while (timer < invincibilityDuration)
        {
            timer += Time.deltaTime;

            // 無敵中の色変化
            float lerp = Mathf.PingPong(Time.time * 15f, 1f);
            sr.color = Color.Lerp(Color.white, new Color(1f, 0.9f, 0f), lerp);
            yield return null;
        }

        // 元に戻す
        sr.color = Color.white;
        isInvincible = false;
        moveSpeed = 4.5f; // 元の速度に戻す
        jumpForce = 15.0f; // 元のジャンプ力に戻す
        if (sparkles != null) Destroy(sparkles);
    }

    // 向きをリセットするメソッド
    public void ResetFacing()
    {
        facingRight = true;
        transform.localScale = new Vector3(
            Mathf.Abs(transform.localScale.x),
            transform.localScale.y,
            transform.localScale.z);
    }
}