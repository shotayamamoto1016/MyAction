using UnityEngine;

public class TakenokoBullet : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        // プレイヤーに当たった
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.SetActive(false);
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