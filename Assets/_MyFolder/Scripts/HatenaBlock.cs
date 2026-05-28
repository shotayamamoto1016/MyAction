using UnityEngine;

public class HatenaBlock : MonoBehaviour
{
    [Header("ê›íË")]
    public Color usedBlockColor = new Color(0.49f, 0.25f, 0f, 1f);
    public float hitAnimationHeight = 0.2f;
    public float hitAnimationDuration = 0.1f;

    private bool isUsed = false;
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        // è„Ç…à⁄ìÆ
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

        // â∫Ç…ñﬂÇÈ
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

        // êFÇíÉêFÇ…ïœçX
        spriteRenderer.color = usedBlockColor;
    }
}