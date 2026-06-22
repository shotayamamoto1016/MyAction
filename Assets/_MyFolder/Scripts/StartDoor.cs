using UnityEngine;
using System.Collections;

public class StartDoor : MonoBehaviour
{
    [Header("扉アニメーション")]
    public Sprite[] closeSprites;
    public float animInterval = 0.1f;
    public float fadeDuration = 1.0f; // 消えるまでの時間

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // ゴールから来た時以外、または2回目以降は非表示
        if (GameManager.instance == null || !GameManager.instance.isComingFromGoal)
        {
            gameObject.SetActive(false);
            return;
        }

        // 演出開始
        StartCoroutine(StartDoorSequence());
    }

    IEnumerator StartDoorSequence()
    {
        // 扉が閉まるアニメーション
        for (int i = 0; i < closeSprites.Length; i++)
        {
            sr.sprite = closeSprites[i];
            yield return new WaitForSeconds(animInterval);
        }

        // フラグを戻す
        GameManager.instance.isComingFromGoal = false;

        // 徐々に消える
        float elapsed = 0f;
        Color startColor = sr.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // 完全に消去
        Destroy(gameObject);
    }
}