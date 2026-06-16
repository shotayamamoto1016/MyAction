using UnityEngine;

public class DokanBlock : MonoBehaviour, IResettable
{
    [Header("設定")]
    public Sprite blockOnSprite;    // ONの時の画像
    public Sprite blockOffSprite;   // OFFの時の画像
    public DokanSpawner spawner;    // 連携するスポーナー

    private SpriteRenderer spriteRenderer;
    private bool isOn = true;
    private Vector3 startPosition;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        UpdateSprite();

        // 最初からONにする 
        if (spawner != null)
        {
            spawner.SetOn(true);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 下から頭突き
            if (contact.normal.y > 0.5f)
            {
                ToggleBlock();
                break;
            }
        }
    }

    void ToggleBlock()
    {
        isOn = !isOn;
        UpdateSprite();

        // スポーナーに状態を伝える
        if (spawner != null)
        {
            spawner.SetOn(isOn);
        }
    }

    void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        spriteRenderer.sprite = isOn ? blockOnSprite : blockOffSprite;
    }

    public void ResetObject()
    {
        isOn = true;
        UpdateSprite();

        if (spawner != null)
        {
            spawner.SetOn(true);
        }
    }
}