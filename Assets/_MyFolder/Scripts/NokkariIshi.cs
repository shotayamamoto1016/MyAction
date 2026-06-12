using UnityEngine;
using System.Collections;

public class NokkariIshi : MonoBehaviour, IResettable
{
    [Header("アニメーション設定（4枚）")]
    public Sprite[] blockSprites;       // ブロック→針が出る画像4枚
    public float animInterval = 0.1f;   // アニメーション速度

    [Header("クールタイム設定")]
    public float cooldownDuration = 3f; // 針が出てから戻るまでの時間

    private SpriteRenderer spriteRenderer;
    private Collider2D col;
    private bool isActivated = false;
    //private bool isCooldown = false;
    private Vector3 startPosition;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        startPosition = transform.position;

        // 最初はブロックの画像
        if (blockSprites.Length > 0)
        {
            spriteRenderer.sprite = blockSprites[0];
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;
        if (isActivated) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player == null) return;

        // 無敵中は何もしない
        if (player.isInvincible) return;

        // 上に乗った・横に触れた両方で発動
        StartCoroutine(ActivateSpike(player));
    }

    IEnumerator ActivateSpike(PlayerController player)
    {
        isActivated = true;

        // 針が出るアニメーション（1枚目→4枚目）
        for (int i = 0; i < blockSprites.Length; i++)
        {
            spriteRenderer.sprite = blockSprites[i];
            yield return new WaitForSeconds(animInterval);
        }

        // ぽんたを死亡させる
        if (player != null && !player.isDead)
        {
            player.Die();
        }

        // クールタイム待機
        yield return new WaitForSeconds(cooldownDuration);

        // 元のブロック画像に戻す
        if (blockSprites.Length > 0)
        {
            spriteRenderer.sprite = blockSprites[0];
        }

        isActivated = false;
    }

    public void ResetObject()
    {
        StopAllCoroutines();
        isActivated = false;
        //isCooldown = false;
        transform.position = startPosition;

        if (blockSprites.Length > 0)
        {
            spriteRenderer.sprite = blockSprites[0];
        }
    }
}