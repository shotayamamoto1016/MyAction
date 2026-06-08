using UnityEngine;

public class HiddenBlock : MonoBehaviour, IResettable
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private bool isRevealed = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer.enabled = false;
        boxCollider.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isRevealed) return;
        if (!collision.CompareTag("Player")) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // 上昇中かチェック
        if (rb.linearVelocity.y <= 0.01f) return;

        Bounds playerBounds = collision.bounds;
        Bounds blockBounds = boxCollider.bounds;

        // ぽんたの頭がブロックの底より下にあるかチェック
        if (playerBounds.max.y <= blockBounds.center.y)
        {
            // ぽんたの横位置がブロックと少しでも重なっているかチェック
            bool horizontalOverlap =
                playerBounds.max.x > blockBounds.min.x &&
                playerBounds.min.x < blockBounds.max.x;

            if (horizontalOverlap)
            {
                Reveal(rb);
            }
        }
    }

    void Reveal(Rigidbody2D playerRb)
    {
        isRevealed = true;
        spriteRenderer.enabled = true;
        boxCollider.isTrigger = false;

        // 叩いた瞬間に上昇を止める
        playerRb.linearVelocity = new Vector2(
            playerRb.linearVelocity.x, 0);

        StartCoroutine(HitAnimation());
    }

    System.Collections.IEnumerator HitAnimation()
    {
        Vector3 origin = transform.position;
        Vector3 peek = origin + Vector3.up * 0.2f;
        float duration = 0.05f;
        float t = 0;

        while (t < duration)
        {
            transform.position = Vector3.Lerp(origin, peek, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        t = 0;
        while (t < duration)
        {
            transform.position = Vector3.Lerp(peek, origin, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = origin;
    }

    //public void ResetBlock()
    //{
    //    isRevealed = false;
    //    spriteRenderer.enabled = false;
    //    boxCollider.isTrigger = true;
    //    transform.position = transform.position; // 位置リセット
    //}

    public void ResetObject()
    {
        isRevealed = false;
        spriteRenderer.enabled = false;
        boxCollider.isTrigger = true;
       
    }
}