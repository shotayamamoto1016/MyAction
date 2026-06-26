using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GoalDoor : MonoBehaviour
{
    [Header("扉アニメーション")]
    public Sprite[] doorSprites;
    public float doorAnimInterval = 0.1f;

    [Header("シーン設定")]
    public string nextSceneName;        // 次のシーン名
    public int currentStageNum = 1;     // このステージの番号

    [Header("停止時間設定")]
    public float stopDuration = 1.5f;   // 停止する時間

    [Header("ぽんたの後ろ姿画像")]
    public Sprite backSprite;           // ぽんたの後ろ姿画像

    private SpriteRenderer doorRenderer;
    private bool isActivated = false;
    private bool playerInside = false;
    private float stopTimer = 0f;
    private GameObject player;
    private SpriteRenderer playerRenderer;
    private Rigidbody2D playerRb;
    private PlayerController playerController;

    void Start()
    {
        doorRenderer = GetComponent<SpriteRenderer>();

        // ぽんたを取得
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerRenderer = player.GetComponent<SpriteRenderer>();
            playerRb = player.GetComponent<Rigidbody2D>();
            playerController = player.GetComponent<PlayerController>();
        }
    }

    void Update()
    {
        if (isActivated || player == null) return;

        if (playerInside)
        {
            // ぽんたが止まっているか確認
            if (Mathf.Abs(playerRb.linearVelocity.x) < 0.1f)
            {
                stopTimer += Time.deltaTime;

                if (stopTimer >= stopDuration)
                {
                    StartCoroutine(GoalSequence());
                }
            }
            else
            {
                // 動いたらタイマーリセット
                stopTimer = 0f;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;
            stopTimer = 0f;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
            stopTimer = 0f;
        }
    }

    IEnumerator GoalSequence()
    {
        isActivated = true;

        // ぽんたの動きを止める
        playerController.enabled = false;
        playerRb.linearVelocity = Vector2.zero;

        // アニメーターを無効化して後ろ姿画像に切り替えられるようにする
        Animator playerAnim = player.GetComponent<Animator>();
        if (playerAnim != null)
        {
            playerAnim.enabled = false;
        }

        // 扉のアニメーション
        for (int i = 0; i < doorSprites.Length; i++)
        {
            doorRenderer.sprite = doorSprites[i];
            yield return new WaitForSeconds(doorAnimInterval);
        }

        // ぽんたを後ろ姿に変更
        if (backSprite != null && playerRenderer != null)
        {
            playerRenderer.sprite = backSprite;
        }

        // 少し待ってからシーン移行
        yield return new WaitForSeconds(0.5f);

        // 現在の開放済みステージ数を読み込む
        int savedStage = PlayerPrefs.GetInt("StageCleared", 1);

        // もしクリアしたステージが、これまでの記録と同じなら次のステージを開放する
        if (currentStageNum >= savedStage)
        {
            PlayerPrefs.SetInt("StageCleared", currentStageNum + 1);
            PlayerPrefs.Save(); // 確実に保存
            Debug.Log("次のステージを開放しました！ 現在の進行度: " + (currentStageNum + 1));
        }

        // 次のステージの扉を表示するためにフラグを立てる
        if (GameManager.instance != null)
        {
            GameManager.instance.isComingFromGoal = true;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}