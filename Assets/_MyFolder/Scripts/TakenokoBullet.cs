using UnityEngine;

public class TakenokoBullet : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
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

            Destroy(gameObject);
        }
        // 地面（Groundタグ）またはそれ以外の障害物に当たった
        else if (collision.gameObject.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
        // タグ設定が漏れている場合のために、Enemy以外なら消える設定も入れておくと安全です
        else if (!collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}