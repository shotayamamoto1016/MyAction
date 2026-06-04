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

    private Rigidbody2D rb;
    private Animator anim; // アニメーター用
    private bool isGrounded;
    private bool facingRight = true;

    public bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        anim = GetComponent<Animator>(); // 開始時にAnimatorを取得
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

        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        float speed = Mathf.Abs(moveInput) > 0.1f ? Mathf.Abs(moveInput) : 0f;
        anim.SetFloat("Speed", speed);

        anim.SetBool("isGrounded", isGrounded);

       
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
        if (isDead) return;
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
            // 提灯などの「踏める敵」の場合、上から踏んだ時は死なないようにする判定
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // 下からの衝撃なら死亡
                if (contact.normal.y < 0.5f)
                {
                    Die();
                    return;
                }
            }
        }
    }

    // 弾に触れた時の判定
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Die();
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
}