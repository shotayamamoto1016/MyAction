using UnityEngine;
using System.Collections;
using TMPro;

public class EndingClearText : MonoBehaviour
{
    [Header("テキスト設定")]
    public TextMeshProUGUI clearText;
    public float typingSpeed = 0.1f;        // 一文字が出る速さ
    public float displayDuration = 3f;      // 全文字表示後の待機時間
    public float fadeOutDuration = 1f;      // フェードアウト時間
    public float delayBeforeStart = 1f;     // 演出開始までの遅延

    [Header("とげブロック連動")]
    public EndingSpikeWall[] spikeWalls;
    public float spikeStartDelay = 2f;      // テキスト表示後にとげブロックが動き出すまでの時間

    void Start()
    {
        if (clearText != null)
        {
            // 最初は文字をゼロにする
            clearText.maxVisibleCharacters = 0;

            // 透明度は1にしておく
            Color c = clearText.color;
            clearText.color = new Color(c.r, c.g, c.b, 1f);
        }

        StartCoroutine(ClearTextSequence());
    }

    IEnumerator ClearTextSequence()
    {
        // 最初に少し待つ
        yield return new WaitForSeconds(delayBeforeStart);

        // 文字を左から一文字ずつ出す演出
        StartCoroutine(TriggerSpikeWallsAfterDelay());
        yield return StartCoroutine(TypeText());

        // 全文字表示後の待機
        yield return new WaitForSeconds(displayDuration);

        // フェードアウト
        yield return StartCoroutine(FadeOutText());
    }

    // 文字を一文字ずつ表示するコルーチン
    IEnumerator TypeText()
    {
        if (clearText == null) yield break;

        // 全文字数を取得
        int totalCharacters = clearText.textInfo.characterCount;
        if (totalCharacters == 0) totalCharacters = clearText.text.Length;

        for (int i = 0; i <= totalCharacters; i++)
        {
            clearText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    // 指定時間後にとげブロックを一斉に動かす
    IEnumerator TriggerSpikeWallsAfterDelay()
    {
        yield return new WaitForSeconds(spikeStartDelay);

        if (spikeWalls != null)
        {
            foreach (EndingSpikeWall wall in spikeWalls)
            {
                if (wall != null)
                {
                    wall.StartMoving();
                }
            }
            Debug.Log("全てのとげブロックが動き出しました！");
        }
    }

    IEnumerator FadeOutText()
    {
        if (clearText == null) yield break;

        float elapsed = 0f;
        Color c = clearText.color;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            clearText.color = new Color(c.r, c.g, c.b, alpha);
            yield return null;
        }

        clearText.color = new Color(c.r, c.g, c.b, 0f);
    }
}