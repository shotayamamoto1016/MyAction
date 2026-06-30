using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using TMPro;

public class PauseManager : MonoBehaviour
{
    public static PauseManager instance;

    [Header("表示・非表示にする親オブジェクト")]
    public GameObject menuButton;
    public GameObject menuPopup;    // メインのポーズ画面
    public GameObject confirmPopup; // 「本当に戻りますか？」画面

    [Header("音量設定UI")]
    public Slider bgmSlider;
    public TextMeshProUGUI bgmValueText;
    public Slider seSlider;
    public TextMeshProUGUI seValueText;

    private bool isPaused = false;

    void Awake()
    {
        // シングルトンの設定
        if (instance == null) instance = this;
    }

    void Start()
    {
        // 新しいシーンが始まったとき、GSoundにある現在の音量をスライダーに反映させる
        if (bgmSlider != null && seSlider != null)
        {
            bgmSlider.value = GSound.Instance.bgmVolume;
            seSlider.value = GSound.Instance.seVolume;
            UpdateVolumeText();
        }
    }

    void Update()
    {
        if (!menuButton.activeSelf) return;


        // EscキーやPキーでもポーズの開閉ができるようにする
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused) Resume();
            else PressMenuButton();
        }
    }

    public void SetBgmVolume(float value)
    {
        GSound.Instance.bgmVolumeChange(value);
        UpdateVolumeText();
    }

    public void SetSeVolume(float value)
    {
        GSound.Instance.seVolumeChange(value);
        UpdateVolumeText();
    }

    void UpdateVolumeText()
    {
        if (bgmValueText != null) bgmValueText.text = GSound.Instance.bgmVolume.ToString("F1");
        if (seValueText != null) seValueText.text = GSound.Instance.seVolume.ToString("F1");
    }

    public void SetMenuButtonActive(bool active)
    {
        if (menuButton != null)
        {
            menuButton.SetActive(active);
        }
    }


    // 左上のメニューボタンを押した時
    public void PressMenuButton()
    {
        GSound.Instance.PlaySe(SoundData.SeType.Button_Click.ToString());
        isPaused = true;

        // ポーズを開いた瞬間にも数値を最新にする
        bgmSlider.value = GSound.Instance.bgmVolume;
        seSlider.value = GSound.Instance.seVolume;
        UpdateVolumeText();

        menuPopup.SetActive(true);
        confirmPopup.SetActive(false); // 確認画面は閉じる
        Time.timeScale = 0f;           // ゲームを停止
    }

    // ゲームに戻る
    public void Resume()
    {
        GSound.Instance.PlaySe(SoundData.SeType.Button_Click.ToString());
        isPaused = false;
        menuPopup.SetActive(false);
        confirmPopup.SetActive(false);
        Time.timeScale = 1f;           // ゲームを再始動
    }

    

    // 「巻物を確認する」ボタンを押した時
    public void PressReturnTitle()
    {
        GSound.Instance.PlaySe(SoundData.SeType.Button_Click.ToString());
        // メインポーズ画面を隠して確認画面を出す
        menuPopup.SetActive(false);
        confirmPopup.SetActive(true);
    }

    // 確認画面で「いいえ」を押した時
    public void CancelReturn()
    {
        GSound.Instance.PlaySe(SoundData.SeType.Button_Click.ToString());
        confirmPopup.SetActive(false);
        menuPopup.SetActive(true); // メインポーズ画面に戻す
    }

    // 確認画面で「はい」を押した時
    public void ConfirmReturn()
    {
        GSound.Instance.PlaySe(SoundData.SeType.Button_Click.ToString());
        Time.timeScale = 1f; // 時間を戻してからシーン移動
        SceneManager.LoadScene("01_Select"); 
    }
}