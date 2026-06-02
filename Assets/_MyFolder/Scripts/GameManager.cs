using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("ゲーム状態")]
    public int lives = 3;             // 初期残機
    public float restartDelay = 2.0f; // 暗転後の待機時間
    public float fadeDuration = 0.5f; // フェードにかける時間

    [Header("UIリファレンス")]
    public CanvasGroup fadeCanvasGroup;
    public TextMeshProUGUI lifeText;
    public GameObject lifeUI;        // 残機を表示するUI

    // 初回起動かどうかの判定フラグ 
    private bool isFirstLoad = true;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ステージが変わっても残機を維持
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // シーンが読み込まれた直後に呼ばれる
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //最初のゲーム開始時は何もしない
        if (isFirstLoad)
        {
            if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 0;
            if (lifeUI != null) lifeUI.SetActive(false);
            isFirstLoad = false; // 次からは「死んだ後のリロード」として扱う
            return;
        }

        // 死んだ後のリロード時は、真っ黒な状態からフェードアウト
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 1;
            if (lifeUI != null) lifeUI.SetActive(false);
            StartCoroutine(Fade(0)); // 徐々に明るくする
        }
    }

    // プレイヤーが死んだ時にPlayerControllerから呼ばれる
    public void OnPlayerDie()
    {
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // 演出待ち
        yield return new WaitForSeconds(0.6f);

        // j徐々に画面を暗くする
        yield return StartCoroutine(Fade(1));

        // 残機を減らす
        lives--;
        UpdateLifeText();

        // 残機表示画面を出す
        if (lifeUI != null)
        {
            lifeUI.SetActive(true);
        }

        yield return new WaitForSeconds(restartDelay);

        // 常に復活
        // livesの数に関わらず、Respawn()を呼び出すように変更する
        Respawn();
    }

    

   

    void Respawn()
    {
        // 【修正ポイント】まだ黒いパネル(fadePanel)は消さない！
        if (lifeUI != null) lifeUI.SetActive(false);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            if (CheckpointManager.instance.HasCheckpoint())
            {
                // チェックポイントへワープ
                player.transform.position = CheckpointManager.instance.GetCheckpointPosition();
                player.GetComponent<PlayerController>().ResetPlayer();

                StartCoroutine(Fade(0));
            }
            else
            {
                // シーンを最初から読み直す場合
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                // ※シーンを読み直す場合は、Start()の中でfadePanelを消す処理が必要になります
            }
        }
    }

    // カメラが追いつくまで待ってから画面を明るくする
    //IEnumerator RemoveFadePanel()
    //{
    //    yield return new WaitForSeconds(0.3f); // 0.3秒だけ黒いまま待機

    //    if (fadePanel != null) fadePanel.SetActive(false);
    //}

    //フェード処理
    IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    //残機テキストの更新
    void UpdateLifeText()
    {
        if (lifeText != null)
        {
            // 指定通りの空白を入れたフォーマット
            lifeText.text = "     ×   " + lives;
        }
    }
}