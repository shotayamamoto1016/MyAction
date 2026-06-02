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
}