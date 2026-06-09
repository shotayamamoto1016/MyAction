using UnityEngine;
using DG.Tweening;

public class CrumbleBlock : MonoBehaviour, IResettable
{
    [Header("設定")]
    public float crumbleDelay = 1.0f;  // 乗ってから崩れるまでの時間
    public float respawnTime = 3.0f;   // 復活までの時間

    private bool isCrumbling = false;
    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // ぽんたが上から乗った時だけ崩れる
        if (collision.gameObject.CompareTag("Player") && !isCrumbling)
        {
            // 接触点がブロックの上側かチェック
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

        // 揺れるアニメーション
        transform.DOShakePosition(crumbleDelay, 0.1f, 20)
            .SetLink(gameObject)
            .OnComplete(() => Crumble());
    }

    void Crumble()
    {
        GSound.Instance.PlaySe(SoundData.SeType.Block_Break.ToString());

        // ブロックを非表示・Colliderを無効化
        spriteRenderer.enabled = false;
        col.enabled = false;

        // 一定時間後に復活
        Invoke("Respawn", respawnTime);
    }

    void Respawn()
    {
        spriteRenderer.enabled = true;
        col.enabled = true;
        isCrumbling = false;
        transform.position = startPosition;
    }

    //public void ResetBlock()
    //{
    //    isCrumbling = false;
    //    spriteRenderer.enabled = true;
    //    col.enabled = true;
    //    transform.position = startPosition;
    //}

    public void ResetObject()
    {
        isCrumbling = false;
        spriteRenderer.enabled = true;
        col.enabled = true;
        transform.position = startPosition;
    }
}