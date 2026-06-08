using UnityEngine;

public class BreakBlock : MonoBehaviour, IResettable
{
    [Header("”j•Ğİ’è")]
    public Sprite fragmentSprite; // ”j•Ğ‚Ì‰æ‘œ
    public int fragmentCount = 4; // ”j•Ğ‚Ì”
    public float fragmentForce = 5f; // ”j•Ğ‚ª”ò‚Ô‹­‚³
    public float fragmentLifeTime = 0.8f; // ”j•Ğ‚ªÁ‚¦‚é‚Ü‚Å‚ÌŠÔ

    private Vector3 startPosition;
    private bool isBroken = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBroken) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isBroken = true;
                    SpawnFragments();
                    gameObject.SetActive(false); // Destroy‚Ì‘ã‚í‚è‚É”ñ•\¦
                    break;
                }
            }
        }
    }

    void SpawnFragments()
    {
        for (int i = 0; i < fragmentCount; i++)
        {
            // ”j•Ğ‚ğ¶¬
            GameObject fragment = new GameObject("Fragment");
            fragment.transform.position = transform.position;
            fragment.transform.localScale = Vector3.one * 0.4f;

            // ‰æ‘œ‚ğİ’è
            SpriteRenderer sr = fragment.AddComponent<SpriteRenderer>();
            sr.sprite = fragmentSprite != null ? fragmentSprite : GetComponent<SpriteRenderer>().sprite;
            sr.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;

            // •¨—‰‰Z‚ğ’Ç‰Á
            Rigidbody2D rb = fragment.AddComponent<Rigidbody2D>();

            // ƒ‰ƒ“ƒ_ƒ€‚È•ûŒü‚É”ò‚Î‚·
            float angle = 45f + (i * 90f); // 4•ûŒü‚É”ò‚Î‚·
            float randomAngle = angle + Random.Range(-30f, 30f);
            Vector2 direction = new Vector2(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            );
            rb.linearVelocity = direction * fragmentForce;

            // ‰ñ“]‚ğ‰Á‚¦‚é
            rb.angularVelocity = Random.Range(-360f, 360f);

            // ˆê’èŠÔŒã‚É”j•Ğ‚ğíœ
            Destroy(fragment, fragmentLifeTime);
        }
    }

    public void ResetObject()
    {
        isBroken = false;
        transform.position = startPosition;
        gameObject.SetActive(true);
    }
}