using UnityEngine;
using System.Collections;

public class ItemBlock : MonoBehaviour
{
    [Header("ブロック設定")]
    public Color usedBlockColor = new Color(0.49f, 0.25f, 0f, 1f);
    public float hitAnimationHeight = 0.2f;
    public float hitAnimationDuration = 0.1f;

    
    public enum ItemType
    {
        Freeze,     // 凍るキノコ
        Confused,      // 混乱するキノコ
        Golden,     // 小さくなるキノコ
        Poison,     // 毒キノコ
    }

    [Header("アイテム設定")]
    public ItemType itemType;
    public float itemEmergeSpeed = 1f;

    [Header("各アイテムのPrefab")]
    public GameObject freezeMushroomPrefab;
    public GameObject confusedMushroomPrefab;
    public GameObject GoldenMushroom;
    public GameObject poisonMushroomPrefab;

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
                    StartCoroutine(HitAnimation());
                    break;
                }
            }
        }
    }

    IEnumerator HitAnimation()
    {
        isUsed = true;

        // 上に跳ねるアニメーション
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

        // 使用済みブロックを茶色に変更
        spriteRenderer.color = usedBlockColor;

        // 選択したアイテムを出現させる
        GameObject selectedItem = GetSelectedItem();
        if (selectedItem != null)
        {
            StartCoroutine(EmergeItem(selectedItem));
        }
    }

    GameObject GetSelectedItem()
    {
        switch (itemType)
        {
            case ItemType.Freeze:
                return freezeMushroomPrefab;
            case ItemType.Confused:
                return confusedMushroomPrefab;
            case ItemType.Golden:
                return GoldenMushroom;
            case ItemType.Poison:
                return poisonMushroomPrefab;
            default:
                return null;
        }
    }

    IEnumerator EmergeItem(GameObject itemPrefab)
    {
        Vector3 hidePos = transform.position;
        Vector3 showPos = transform.position + Vector3.up * 0.9f;

        GameObject item = Instantiate(
            itemPrefab, hidePos, Quaternion.identity);

        float elapsed = 0f;
        float duration = 1f / itemEmergeSpeed;

        while (elapsed < duration)
        {
            item.transform.position = Vector3.Lerp(
                hidePos, showPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        item.transform.position = showPos;
    }
}