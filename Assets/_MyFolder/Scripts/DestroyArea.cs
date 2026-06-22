using UnityEngine;

public class DestroyArea : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.gameObject.GetComponentInParent<PlayerController>() != null)
        {
            PlayerController pc = collision.gameObject.GetComponentInParent<PlayerController>();
            if (pc != null)
            {
                if (!pc.isDead)
                {
                    Debug.Log("ぽんたのDie()を呼びます");
                    pc.Die();
                }
            }
            return; // プレイヤーの処理をしたらここで終了
        }

        IResettable resettable = collision.GetComponent<IResettable>();

        if (resettable != null)
        {
            // 破壊せず非表示にする
            collision.gameObject.SetActive(false);
        }
        else
        {
            // 弾などのリセット不要なものは破壊する
            Destroy(collision.gameObject);
        }
    }
}