using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fusuma : MonoBehaviour
{
    [Header("ふすまアニメーション（4枚）")]
    public Sprite[] fusumaSprites;
    public float fusumaAnimInterval = 0.1f;

    [Header("転送設定")]
    public Transform destinationFusuma; // 転送先のふすま
    public float groundOffsetY = 0f; // 地面の高さ調整


    [Header("停止時間設定")]
    public float stopDuration = 1.5f;

    [Header("クールタイム設定")]
    public float cooldownAfterArrival = 3f; // 転移後のクールタイム

    [Header("ぽんたの画像設定")]
    public Sprite backSprite;    // ぽんたの後ろ姿画像
    public Sprite frontSprite;   // ぽんたの前向き画像

    [Header("シーン遷移設定")]
    public bool useSceneTransition = false; // trueにするとシーン遷移
    public string transitionSceneName = "01_Select";

    private SpriteRenderer fusumaRenderer;
    private bool isActivated = false;
    private bool playerInside = false;
    private float stopTimer = 0f;
    private GameObject player;
    private SpriteRenderer playerRenderer;
    private Rigidbody2D playerRb;
    private PlayerController playerController;

    // クールタイム管理
    private bool isCooldown = false;
    private float cooldownTimer = 0f;

    void Start()
    {
        fusumaRenderer = GetComponent<SpriteRenderer>();

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

        // クールタイムのカウントダウン
        if (isCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isCooldown = false;
                stopTimer = 0f;
            }
            return;
        }

        if (playerInside)
        {
            if (Mathf.Abs(playerRb.linearVelocity.x) < 0.1f)
            {
                stopTimer += Time.deltaTime;
                if (stopTimer >= stopDuration)
                {
                    StartCoroutine(FusumaSequence());
                }
            }
            else
            {
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

    IEnumerator FusumaSequence()
    {
        isActivated = true;

        // ぽんたの動きを止める
        playerController.enabled = false;
        playerRb.linearVelocity = Vector2.zero;

        // アニメーターを無効化
        Animator playerAnim = player.GetComponent<Animator>();
        if (playerAnim != null) playerAnim.enabled = false;

        // ふすまのアニメーション
        for (int i = 0; i < fusumaSprites.Length; i++)
        {
            fusumaRenderer.sprite = fusumaSprites[i];
            yield return new WaitForSeconds(fusumaAnimInterval);
        }

        // ぽんたを後ろ姿に変更
        if (backSprite != null && playerRenderer != null)
        {
            playerRenderer.sprite = backSprite;
        }

        yield return new WaitForSeconds(0.5f);

        if (useSceneTransition)
        {
            // シーン遷移の場合
            Debug.Log("シーン遷移を開始します: " + transitionSceneName);

            // GameManagerのフラグをセット
            if (GameManager.instance != null)
            {
                GameManager.instance.isComingFromGoal = true;
            }

            // フェードアウトさせてからシーンを読み込む
            SceneManager.LoadScene(transitionSceneName);
            yield break; // ここでコルーチンを完全に終了させる
        }

        else if (destinationFusuma != null)
        {
            // 同じシーン内でのワープの場合
            WarpToDestination(playerAnim);
        }

        isActivated = false;

        //// 転送先のふすまへ移動
        //if (destinationFusuma != null)
        //{
        //    Vector3 spawnPos = new Vector3(
        //        destinationFusuma.position.x,
        //        destinationFusuma.position.y + groundOffsetY,
        //        player.transform.position.z);

        //    player.transform.position = spawnPos;

        //    // カメラを転送先に移動
        //    Unity.Cinemachine.CinemachineCamera vcam =
        //        FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        //    if (vcam != null)
        //    {
        //        vcam.ForceCameraPosition(
        //            new Vector3(
        //                spawnPos.x,
        //                spawnPos.y,
        //                vcam.transform.position.z),
        //            Quaternion.identity);
        //    }

        //    // 転送先のふすまにクールタイムを設定
        //    Fusuma destinationScript =
        //        destinationFusuma.GetComponent<Fusuma>();
        //    if (destinationScript != null)
        //    {
        //        destinationScript.StartCooldown(cooldownAfterArrival);
        //    }
        //}

        //// ふすまを最初の画像に戻す 
        //if (fusumaSprites.Length > 0)
        //{
        //    fusumaRenderer.sprite = fusumaSprites[0];
        //}

        //yield return new WaitForSeconds(0.3f);

        ////// ぽんたの向きを必ず前向きにリセット 
        ////player.transform.localScale = new Vector3(
        ////    Mathf.Abs(player.transform.localScale.x),
        ////    player.transform.localScale.y,
        ////    player.transform.localScale.z);

        //// ぽんたの向きを必ず前向きにリセット
        //playerController.ResetFacing();

        //// ぽんたを前向き画像に変更
        //if (frontSprite != null && playerRenderer != null)
        //{
        //    playerRenderer.sprite = frontSprite;
        //}

        //// ぽんたの操作を再開
        //playerController.enabled = true;

        //// キー入力があるまで前向き画像を固定
        //StartCoroutine(WaitForInput(playerAnim));

        //isActivated = false;

        //// シーン遷移の場合
        //if (useSceneTransition)
        //{
        //    yield return new WaitForSeconds(0.5f);
        //    UnityEngine.SceneManagement.SceneManager.LoadScene(transitionSceneName);
        //    yield break;
        //}
    }

    // シーン内ワープ用の処理を分離
    void WarpToDestination(Animator playerAnim)
    {
        Vector3 spawnPos = new Vector3(
            destinationFusuma.position.x,
            destinationFusuma.position.y + groundOffsetY,
            player.transform.position.z);

        player.transform.position = spawnPos;

        // カメラ強制移動
        Unity.Cinemachine.CinemachineCamera vcam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        if (vcam != null) vcam.ForceCameraPosition(new Vector3(spawnPos.x, spawnPos.y, vcam.transform.position.z), Quaternion.identity);

        // クールタイム設定
        Fusuma destScript = destinationFusuma.GetComponent<Fusuma>();
        if (destScript != null) destScript.StartCooldown(cooldownAfterArrival);

        // ふすまを閉じる
        if (fusumaSprites.Length > 0) fusumaRenderer.sprite = fusumaSprites[0];

        // ぽんたを前向きに戻して操作再開
        playerController.ResetFacing();
        if (frontSprite != null) playerRenderer.sprite = frontSprite;
        playerController.enabled = true;
        StartCoroutine(WaitForInput(playerAnim));
    }

    // 外部から呼び出すクールタイム開始メソッド
    public void StartCooldown(float duration)
    {
        isCooldown = true;
        cooldownTimer = duration;
        playerInside = false;
        stopTimer = 0f;
    }

    IEnumerator WaitForInput(Animator playerAnim)
    {
        yield return new WaitUntil(() =>
            Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f ||
            Input.GetButtonDown("Jump"));

        if (playerAnim != null)
        {
            playerAnim.enabled = true;
        }
    }
}