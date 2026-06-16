using UnityEngine;
using System.Collections;

public class KoromochiLeft : MonoBehaviour, IResettable
{
    [Header("移動設定")]
    public float moveSpeed = 2f;

    [Header("つぶれ設定")]
    public Sprite flatSprite;
    public float flatDuration = 0.5f;
    public float flatOffsetY = -0.2f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator anim;
    private bool isDead = false;
    private Vector3 startPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        startPosition = transform.position;

        // 常に左に移動
        rb.linearVelocity = new Vector2(-moveSpeed, 0f);
    }

    void Update()
    {
        if (isDead) return;

        // 常に左に移動し続ける
        rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (isDead) return;

        PlayerController controller =
            collision.gameObject.GetComponent<PlayerController>();
        if (controller == null) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                // 上から踏まれた → ころもち死亡
                GetStomp();

                Rigidbody2D playerRb =
                    collision.gameObject.GetComponent<Rigidbody2D>();
                if (playerRb != null)
                {
                    playerRb.linearVelocity = new Vector2(
                        playerRb.linearVelocity.x, 5f);
                }
                return;
            }
            else
            {
                // 横から当たった
                if (controller.isInvincible)
                {
                    GetStomp();
                    return;
                }
                else
                {
                    controller.Die();
                    return;
                }
            }
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
        StartCoroutine(HideAfterDelay(flatDuration));
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);
    }

    public void ResetObject()
    {
        // スポーナーから生成されるので個別リセットは不要
        gameObject.SetActive(false);
    }
}