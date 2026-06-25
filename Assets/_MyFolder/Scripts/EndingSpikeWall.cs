using UnityEngine;
using System.Collections;

public class EndingSpikeWall : MonoBehaviour
{
    [Header("移動設定")]
    public float startSpeed = 1f;       // 初期速度
    public float maxSpeed = 8f;         // 最大速度
    public float acceleration = 0.3f;   // 加速度

    [Header("崩壊ブロック設定")]
    public EndingBreakBlock[] breakBlocks; // 崩壊するブロック

    [Header("ぽんたのリスポーン設定")]
    public Vector3 respawnPosition = new Vector3(510f, 4.5f, 0f);

    private float currentSpeed;
    private bool isActive = false;
    private Vector3 initialPosition; // 壁の初期位置

    void Start()
    {
        currentSpeed = startSpeed;
        initialPosition = transform.position; // 開始時の位置を記録
    }

    public void StartMoving()
    {
        isActive = true;
    }

    void Update()
    {
        if (!isActive) return;

        // 右に移動しながら加速
        currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, maxSpeed);
        transform.position += Vector3.right * currentSpeed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null && !pc.isDead)
            {
                // 復活地点を 06_Stage5 の座標(510, 4.5)に設定
                if (CheckpointManager.instance != null)
                {
                    CheckpointManager.instance.SetCheckpoint(new Vector3(510f, 4.5f, 0f));
                }

                // GameManagerに次は前のシーンを読み込むように指示
                if (GameManager.instance != null)
                {
                    GameManager.instance.nextRespawnScene = "06_Stage5";
                }

                pc.Die();
            }
        }

        // 崩壊ブロックに当たったら崩す
        EndingBreakBlock block = other.GetComponent<EndingBreakBlock>();
        if (block != null)
        {
            block.Collapse();
        }
    }

    public void ResetWall()
    {
        isActive = false;
        transform.position = initialPosition;
        currentSpeed = startSpeed;
    }
}