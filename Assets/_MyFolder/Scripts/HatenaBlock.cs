using UnityEngine;

public class HatenaBlock : MonoBehaviour, IResettable
{
    [Header("設定")]
    public Color usedBlockColor = new Color(0.49f, 0.25f, 0f, 1f);
    public float hitAnimationHeight = 0.2f;
    public float hitAnimationDuration = 0.1f;

    private bool isUsed = false;
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color; // 元の色を保存
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isUsed)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    HitFromBelow();
                    break;
                }
            }
        }
    }

    void HitFromBelow()
    {
        isUsed = true;
        StartCoroutine(HitAnimation());
    }

    System.Collections.IEnumerator HitAnimation()
    {
        // 上に移動
        float elapsed = 0f;
        while (elapsed < hitAnimationDuration)
        {
            transform.position = Vector3.Lerp(
                startPosition,
                startPosition + Vector3.up * hitAnimationHeight,
                elapsed / hitAnimationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 下に戻る
        elapsed = 0f;
        while (elapsed < hitAnimationDuration)
        {
            transform.position = Vector3.Lerp(
                startPosition + Vector3.up * hitAnimationHeight,
                startPosition,
                elapsed / hitAnimationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startPosition;

        // 色を茶色に変更
        spriteRenderer.color = usedBlockColor;
    }

    public void ResetObject()
    {
        isUsed = false;
        transform.position = startPosition;
        spriteRenderer.color = originalColor; // 元の色に戻す
    }
}