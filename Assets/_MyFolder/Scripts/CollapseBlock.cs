using UnityEngine;
using DG.Tweening;

public class CollapseBlock : MonoBehaviour, IResettable
{
    [Header("•ö‚ê‚éİ’è")]
    public float crumbleDelay = 1.0f;  // æ‚Á‚Ä‚©‚ç•ö‚ê‚é‚Ü‚Å‚ÌŠÔ

    [Header("”j•Ğİ’è")]
    public Sprite fragmentSprite;      // ”j•Ğ‚Ì‰æ‘œ
    public int fragmentCount = 4;      // ”j•Ğ‚Ì”
    public float fragmentForce = 5f;   // ”j•Ğ‚ª”ò‚Ô‹­‚³
    public float fragmentLifeTime = 0.8f; // ”j•Ğ‚ªÁ‚¦‚é‚Ü‚Å‚ÌŠÔ

    private bool isCrumbling = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && !isCrumbling)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    StartCrumble();
                    break;
                }
            }
        }
    }

    void StartCrumble()
    {
        isCrumbling = true;

        // —h‚ê‚éƒAƒjƒ[ƒVƒ‡ƒ“
        transform.DOShakePosition(crumbleDelay, 0.1f, 20)
            .SetLink(gameObject)
            .OnComplete(() => Crumble());
    }

    void Crumble()
    {
        // ”j•Ğ‚ğ”ò‚Î‚·
        SpawnFragments();

        // Destroy‚Ì‘ã‚í‚è‚É”ñ•\¦‚É‚·‚é
        gameObject.SetActive(false);
    }

    void SpawnFragments()
    {
        for (int i = 0; i < fragmentCount; i++)
        {
            GameObject fragment = new GameObject("Fragment");
            fragment.transform.position = transform.position;
            fragment.transform.localScale = Vector3.one * 0.4f;

            SpriteRenderer sr = fragment.AddComponent<SpriteRenderer>();
            sr.sprite = fragmentSprite != null ?
                fragmentSprite : spriteRenderer.sprite;
            sr.sortingOrder = spriteRenderer.sortingOrder;

            Rigidbody2D rb = fragment.AddComponent<Rigidbody2D>();

            // 4•ûŒü‚Éƒ‰ƒ“ƒ_ƒ€‚É”ò‚Î‚·
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

    // ResetBlock‚ğ’Ç‰Á
    //public void ResetBlock()
    //{
    //    isCrumbling = false;
    //    gameObject.SetActive(true);
    //    spriteRenderer.enabled = true;
    //    col.enabled = true;
    //    transform.position = startPosition;
    //    transform.rotation = Quaternion.identity;
    //}

    public void ResetObject()
    {
        isCrumbling = false;
        gameObject.SetActive(true);
        spriteRenderer.enabled = true;
        col.enabled = true;
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;
    }
}