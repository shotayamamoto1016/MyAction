using UnityEngine;

public class EndingBreakBlock : MonoBehaviour
{
    [Header("îjï–ê›íË")]
    public Sprite fragmentSprite;
    public int fragmentCount = 4;
    public float fragmentForce = 5f;
    public float fragmentLifeTime = 0.8f;

    private bool isCollapsed = false;

    public void Collapse()
    {
        if (isCollapsed) return;
        isCollapsed = true;
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
}