using UnityEngine;

public class TakenokoBullet : MonoBehaviour, IResettable
{
    private Vector3 startPosition;
    private bool isHit = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isHit) return;

        // プレイヤーに当たった
        if (collision.gameObject.CompareTag("Player"))
        {
            // PlayerControllerスクリプトを取得
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();

            if (player != null)
            {
                // 無敵状態なら弾だけ消える、そうでなければ死亡演出
                if (player.isInvincible)
                {
                    Debug.Log("無敵パワーで弾を弾き飛ばした！");
                }
                else
                {
                    player.Die(); // 死亡演出を開始
                }
            }

            isHit = true;
            gameObject.SetActive(false); // Destroyの代わりに非表示
        }
        // 地面（Groundタグ）またはそれ以外の障害物に当たった
        else if (collision.gameObject.CompareTag("Ground"))
        {
            isHit = true;
            gameObject.SetActive(false);
        }
        // タグ設定が漏れている場合のために、Enemy以外なら消える設定も入れておくと安全です
        else if (!collision.gameObject.CompareTag("Enemy"))
        {
            isHit = true;
            gameObject.SetActive(false);
        }
    }

    public void ResetObject()
    {
        // 弾は復活させず非表示のまま
        // チェックポイント復活時に弾は消えたままにする
    }
}