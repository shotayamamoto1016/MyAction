using UnityEngine;

public class HiddenBlock : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private bool isRevealed = false;

    [Header("判定の厳密さ設定")]
    [Range(0.1f, 1.0f)]
    public float detectionWidthPercent = 0.8f; // 横幅の何割を許容するか

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

        if (collision.CompareTag("Player"))
        {
            Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();

            // 1. 上昇中かチェック
            if (rb != null && rb.linearVelocity.y > 0.01f)
            {
                // Colliderの境界線(Bounds)を使って計算（これでピボット位置に左右されなくなります）
                Bounds playerBounds = collision.bounds;
                Bounds blockBounds = boxCollider.bounds;

                // 横方向のズレを確認
                float horizontalDistance = Mathf.Abs(playerBounds.center.x - blockBounds.center.x);
                float allowedWidth = (blockBounds.size.x * 0.5f) * detectionWidthPercent;

                // 【ここがポイント】
                // A. プレイヤーの横位置がブロックの範囲内か
                // B. プレイヤーの「足元」が、ブロックの「底」よりも下にあるか
                if (horizontalDistance < allowedWidth && playerBounds.min.y < blockBounds.min.y)
                {
                    Reveal(rb);
                }
                else
                {
                    // デバッグ用（うまくいかない理由をコンソールに出す）
                    // Debug.Log($"判定外: 距離={horizontalDistance}, 許容={allowedWidth}, 足元位置={playerBounds.min.y}");
                }
            }
        }
    }

    void Reveal(Rigidbody2D playerRb)
    {
        isRevealed = true;
        spriteRenderer.enabled = true;
        boxCollider.isTrigger = false;

        // 叩いた瞬間に落下させる（理不尽演出）
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0);

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
}