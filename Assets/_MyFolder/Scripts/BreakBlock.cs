using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class BreakBlock : MonoBehaviour, IResettable
{
    [Header("破片設定")]
    public Sprite fragmentSprite; // 破片の画像
    public int fragmentCount = 4; // 破片の数
    public float fragmentForce = 5f; // 破片が飛ぶ強さ
    public float fragmentLifeTime = 0.8f; // 破片が消えるまでの時間

    [Header("炎で壊れる設定")]
    public float respawnTime = 3f; // 炎で壊れた後の復活時間

    private Vector3 startPosition;
    private bool isBroken = false;

    // コルーチン用の別オブジェクト 
    private static GameObject coroutineRunner;

    void Start()
    {
        startPosition = transform.position;

        // コルーチン実行用の永続オブジェクトを作成
        if (coroutineRunner == null)
        {
            coroutineRunner = new GameObject("BreakBlockCoroutineRunner");
            coroutineRunner.AddComponent<CoroutineRunner>();
            DontDestroyOnLoad(coroutineRunner);
        }
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
                    GSound.Instance.PlaySe(SoundData.SeType.Block_Break.ToString(), 1.5f);
                    SpawnFragments();
                    gameObject.SetActive(false); // Destroyの代わりに非表示
                    break;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isBroken) return;

        // 爆炎だるまの弾に当たった場合
        if (other.GetComponent<BakuenFlame>() != null)
        {
            isBroken = true;
            SpawnFragments();
            gameObject.SetActive(false);

            // 別オブジェクトでコルーチンを実行 
            CoroutineRunner.Instance.StartRespawn(this, respawnTime);
        }
    }

    IEnumerator RespawnAfterDelay()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(respawnTime);
        ResetObject();
    }

    void SpawnFragments()
    {
        for (int i = 0; i < fragmentCount; i++)
        {
            // 破片を生成
            GameObject fragment = new GameObject("Fragment");
            fragment.transform.position = transform.position;
            fragment.transform.localScale = Vector3.one * 0.4f;

            // 画像を設定
            SpriteRenderer sr = fragment.AddComponent<SpriteRenderer>();
            sr.sprite = fragmentSprite != null ? fragmentSprite : GetComponent<SpriteRenderer>().sprite;
            sr.sortingOrder = GetComponent<SpriteRenderer>().sortingOrder;

            // 物理演算を追加
            Rigidbody2D rb = fragment.AddComponent<Rigidbody2D>();

            // ランダムな方向に飛ばす
            float angle = 45f + (i * 90f); // 4方向に飛ばす
            float randomAngle = angle + Random.Range(-30f, 30f);
            Vector2 direction = new Vector2(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                Mathf.Sin(randomAngle * Mathf.Deg2Rad)
            );
            rb.linearVelocity = direction * fragmentForce;

            // 回転を加える
            rb.angularVelocity = Random.Range(-360f, 360f);

            // 一定時間後に破片を削除
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