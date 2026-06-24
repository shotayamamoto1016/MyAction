using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;

[System.Serializable]
public struct SoundDataEntry
{
    public string Name;
    public AudioClip Clip;
}

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

    [Header("カメラ設定")]
    public CinemachineCamera vcam;
    public float cameraVerticalOffset = 1.5f;

    [Header("ステージ遷移フラグ")]
    public bool isComingFromGoal = false; // ゴール扉から来たかどうか

    // 初回起動かどうかの判定フラグ 
    private bool isFirstLoad = true;

    //BGM名
    string bgmName;

    [Header("サウンドデータ登録")]
    public SoundDataEntry[] seSounds;  // SEのリスト

    private bool isBossCheckpoint = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ステージが変わっても残機を維持
            // ゲーム開始時に音源をGSoundに登録する
            InitializeSounds();
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

    void Start()
    {
        //// ステージ1のBGMを再生
        //PlayStageBGM();
    }

    void InitializeSounds()
    {
        foreach (var entry in seSounds)
        {
            GSound.Instance.SetSe(entry.Name, entry.Clip);
        }
    }

    void Update()
    {
        
        GSound.Instance.CheckSeQueue();
    }



    // シーンが読み込まれた直後に呼ばれる
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "02_Stage1" || scene.name == "03_Stage2" || scene.name == "04_Stage3" || scene.name == "05_Stage4" || scene.name == "06_Stage5")
        {
            PlayStageBGM();
        }

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
        // 死んだ瞬間にポーズボタンを消す
        if (PauseManager.instance != null)
        {
            PauseManager.instance.SetMenuButtonActive(false);
        }

        // 演出待ち
        yield return new WaitForSeconds(0.6f);

        // カラスの弾を全て非表示にする 
        CrowFireball[] fireballs = FindObjectsByType<CrowFireball>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var fireball in fireballs)
        {
            fireball.gameObject.SetActive(false);
        }

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
        if (lifeUI != null) lifeUI.SetActive(false);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Vector3 targetPos;
            if (CheckpointManager.instance != null && CheckpointManager.instance.HasCheckpoint())
            {
                targetPos = CheckpointManager.instance.GetCheckpointPosition();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }

            // ステージをリセット
            if (StageResetManager.instance != null)
            {
                StageResetManager.instance.ResetStage();
            }

            // ぽんたをワープさせる
            player.transform.position = targetPos;
            player.GetComponent<PlayerController>().ResetPlayer();

            // 復活したらポーズボタンを再び表示する(条件分岐)
            if (PauseManager.instance != null)
            {
                bool isBossCheckpoint = CheckpointManager.instance != null &&
                    CheckpointManager.instance.IsBossCheckpoint();

                PauseManager.instance.SetMenuButtonActive(!isBossCheckpoint);
            }

            // カメラ処理をコルーチンで実行 
            StartCoroutine(ResetCamera(player.transform, targetPos));

        }
    }

    

    IEnumerator ResetCamera(Transform playerTransform, Vector3 targetPos)
    {
        Debug.Log("ResetCamera開始。targetPos: " + targetPos);
        if (vcam == null)
        {
            StartCoroutine(Fade(0));
            yield break;
        }

        // Followを一度外す
        vcam.Follow = null;

        // カメラをぽんたの位置に強制移動
        Vector3 cameraTargetPos = new Vector3(
            targetPos.x,
            targetPos.y + cameraVerticalOffset,
            vcam.transform.position.z);

        Debug.Log("カメラ目標位置: " + cameraTargetPos);

        vcam.transform.position = cameraTargetPos;
        vcam.ForceCameraPosition(cameraTargetPos, Quaternion.identity);

        // 1フレーム待つ
        yield return null;

        // Followを再セット
        vcam.Follow = playerTransform;

        // もう1フレーム待つ
        yield return null;
        yield return null;

        // カメラを再度強制移動
        vcam.ForceCameraPosition(cameraTargetPos, Quaternion.identity);
        yield return null;
        vcam.ForceCameraPosition(cameraTargetPos, Quaternion.identity);

        Debug.Log("ResetCamera完了。カメラ位置: " + vcam.transform.position);

        // フェードイン開始
        StartCoroutine(Fade(0));
    }

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

    //BGMを再生するための関数
    public void PlayStageBGM()
    {
        string bgmName = SoundData.BgmType.Start.ToString();
        GSound.Instance.PlayBgm(bgmName, true);
    }


}