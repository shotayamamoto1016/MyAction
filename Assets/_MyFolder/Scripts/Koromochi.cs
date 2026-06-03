using UnityEngine;

public class Koromochi : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 2f;

    [Header("地面判定")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float checkRadius = 0.1f;

    [Header("壁判定")]
    public Transform wallCheckFront;
    public float wallCheckRadius = 0.1f;

    [Header("つぶれ設定")]
    public Sprite flatSprite;
    public float flatDuration = 0.5f;
    public float flatOffsetY = -0.2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private bool isDead = false;
    private bool movingLeft = true;

    // 折り返しのクールタイム
    private float flipCoolTime = 0f;
    private float flipCoolDuration = 0.5f; // 折り返し後0.5秒は折り返さない

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb.linearVelocity = new Vector2(-moveSpeed, 0f);
    }

    void Update()
    {
        if (isDead) return;

        // クールタイムを減らす
        if (flipCoolTime > 0f)
        {
            flipCoolTime -= Time.deltaTime;
            return;
        }

        // 地面判定
        bool isGrounded = Physics2D.OverlapCircle(
            groundCheck.position, checkRadius, groundLayer);

        // 壁判定
        bool isWall = wallCheckFront != null && Physics2D.OverlapCircle(
            wallCheckFront.position, wallCheckRadius, groundLayer);

        // 地面がなくなったまたは壁に当たったら折り返す
        if (!isGrounded || isWall)
        {
            Flip();
        }
    }

    void Flip()
    {
        movingLeft = !movingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
        rb.linearVelocity = new Vector2(
            movingLeft ? -moveSpeed : moveSpeed,
            rb.linearVelocity.y);

        // クールタイムをセット
        flipCoolTime = flipCoolDuration;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (isDead) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                GetStomp();

                Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector2(
                        playerRb.linearVelocity.x, 5f);
                }
                return;
            }
            else
            {
                PlayerDie(collision.gameObject);
                return;
            }
        }
    }

    void PlayerDie(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();

        if (controller != null)
        {
            // ぽんたの死亡演出を呼ぶ
            controller.Die();
        }
    }

    void GetStomp()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (anim != null) anim.enabled = false;

        if (flatSprite != null)
        {
            spriteRenderer.sprite = flatSprite;
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y + flatOffsetY,
                transform.position.z);
            transform.rotation = Quaternion.identity;
        }

        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, flatDuration);
    }
}