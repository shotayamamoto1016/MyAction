using UnityEngine;
using System.Collections;

public class CheckpointFlag : MonoBehaviour
{
    [Header("旗アニメーション（3枚）")]
    public Sprite[] flagSprites;
    public float flagAnimInterval = 0.1f;

    [Header("チェックポイント設定")]
    public Transform respawnPoint; // ぽんたが復活する位置

    private SpriteRenderer flagRenderer;
    private bool isActivated = false;

    void Start()
    {
        flagRenderer = GetComponent<SpriteRenderer>();

        // 最初は閉じた状態（1枚目）
        if (flagSprites.Length > 0)
        {
            flagRenderer.sprite = flagSprites[0];
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            StartCoroutine(ActivateFlag());
        }
    }

    IEnumerator ActivateFlag()
    {
        isActivated = true;

        // 旗が開くアニメーション
        for (int i = 0; i < flagSprites.Length; i++)
        {
            flagRenderer.sprite = flagSprites[i];
            yield return new WaitForSeconds(flagAnimInterval);
        }

        // 最後の画像に固定
        flagRenderer.sprite = flagSprites[flagSprites.Length - 1];

        // チェックポイントを保存
        if (respawnPoint != null)
        {
            CheckpointManager.instance.SetCheckpoint(respawnPoint.position);
        }
    }
}