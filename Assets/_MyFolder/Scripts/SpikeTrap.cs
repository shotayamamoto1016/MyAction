using UnityEngine;
using System.Collections;

public class SpikeTrap : MonoBehaviour, IResettable
{
    [Header("å©ÇΩñ⁄ê›íË")]
    public SpriteRenderer blockRenderer;
    public Color warningColor = Color.red;
    public GameObject spikeVisual;

    [Header("ïˆâÛê›íË")]
    public Sprite fragmentSprite;
    public int fragmentCount = 4;
    public float fragmentForce = 5f;
    public float fragmentLifeTime = 0.8f;

    private Color originalColor;
    private Collider2D spikeCollider;
    private bool isCollapsed = false;
    private Vector3 startPosition;

    void Start()
    {
        if (blockRenderer != null) originalColor = blockRenderer.color;
        spikeCollider = spikeVisual != null ?
            spikeVisual.GetComponent<Collider2D>() : null;
        startPosition = transform.position;

        if (spikeVisual != null) spikeVisual.SetActive(false);
        if (spikeCollider != null) spikeCollider.enabled = false;
    }

    public IEnumerator ActivateSequence(float warningTime, float spikeTime)
    {
        if (isCollapsed) yield break;

        if (blockRenderer != null) blockRenderer.color = warningColor;
        yield return new WaitForSeconds(warningTime);

        if (isCollapsed) yield break;

        if (spikeVisual != null) spikeVisual.SetActive(true);
        if (spikeCollider != null) spikeCollider.enabled = true;

        yield return new WaitForSeconds(spikeTime);

        if (spikeVisual != null) spikeVisual.SetActive(false);
        if (spikeCollider != null) spikeCollider.enabled = false;
        if (blockRenderer != null) blockRenderer.color = originalColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.isDead && !player.isInvincible)
            {
                player.Die();
            }
        }
    }

    // ÉuÉçÉbÉNé©ëÃÇ™ïˆÇÍÇÈèàóù
    public void CollapseBlock()
    {
        isCollapsed = true;
        StopAllCoroutines();
        SpawnFragments();
        gameObject.SetActive(false);
    }

    void SpawnFragments()
    {
        if (fragmentSprite == null) return;

        for (int i = 0; i < fragmentCount; i++)
        {
            GameObject fragment = new GameObject("Fragment");
            fragment.transform.position = transform.position;
            fragment.transform.localScale = Vector3.one * 0.4f;

            SpriteRenderer sr = fragment.AddComponent<SpriteRenderer>();
            sr.sprite = fragmentSprite;

            Rigidbody2D rb = fragment.AddComponent<Rigidbody2D>();

            float angle = 45f + (i * 90f);
            float randomAngle = angle + Random.Range(-30f, 30f);
            Vector2 direction = new Vector2(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                Mathf.Sin(randomAngle * Mathf.Deg2Rad));

            rb.linearVelocity = direction * fragmentForce;
            rb.angularVelocity = Random.Range(-360f, 360f);

            Destroy(fragment, fragmentLifeTime);
        }
    }

    public void ResetObject()
    {
        isCollapsed = false;
        transform.position = startPosition;
        gameObject.SetActive(true);

        if (blockRenderer != null) blockRenderer.color = originalColor;
        if (spikeVisual != null) spikeVisual.SetActive(false);
        if (spikeCollider != null) spikeCollider.enabled = false;
    }
}