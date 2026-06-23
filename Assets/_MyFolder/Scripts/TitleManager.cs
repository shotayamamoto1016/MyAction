using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TitleManager : MonoBehaviour
{
    [Header("UIパネルのリファレンス")]
    public GameObject optionPopup;

    [Header("SEサウンド登録（タイトル用）")]
    public AudioClip[] seClips;

    //BGM名
    string bgmName;

    [Header("スライダーと数値テキスト")]
    public Slider bgmSlider;
    public TextMeshProUGUI bgmValueText; // BGMの数値を表示するテキスト
    public Slider seSlider;
    public TextMeshProUGUI seValueText;  // SEの数値を表示するテキスト

    void Awake()
    {
        // セットされた各音源ファイルの名前を使って自動登録
        foreach (var clip in seClips)
        {
            if (clip != null)
            {
                // ファイル名をそのまま登録名にする
                GSound.Instance.SetSe(clip.name, clip);
                Debug.Log("SE登録完了: " + clip.name);
            }
        }
    }

    void Start()
    {
        // ゲーム開始時はポップアップを確実に閉じておく
        if (optionPopup != null)
        {
            optionPopup.SetActive(false);
        }


        // タイトルBGMを再生
        string bgmName = SoundData.BgmType.Title.ToString();
        GSound.Instance.PlayBgm(bgmName, true);
    }

    void Update()
    {
        GSound.Instance.CheckSeQueue();
    }


    // 開始ボタン 
    public void OnClickStartButton()
    {
        GSound.Instance.PlaySe(SoundData.SeType.Button_Click.ToString());
        // 02_Stage1 シーンを読み込む
        SceneManager.LoadScene("02_Stage1");
    }

    // 設定ボタン
    public void OnClickOptionButton()
    {
        if (optionPopup != null)
        {
            GSound.Instance.PlaySe(SoundData.SeType.Button_Click.ToString());
            optionPopup.SetActive(true);
            bgmSlider.value = GSound.Instance.bgmVolume;
            seSlider.value = GSound.Instance.seVolume;
            UpdateVolumeText();
            Debug.Log("設定画面を開きました");
        }
    }

    // 閉じるボタン
    public void OnClickCloseOptionButton()
    {
        if (optionPopup != null)
        {
            GSound.Instance.PlaySe(SoundData.SeType.Button_Click.ToString());
            optionPopup.SetActive(false);
            Debug.Log("設定画面を閉じました");
        }
    }

    // BGM用
    public void SetBgmVolume(float value)
    {
        GSound.Instance.bgmVolumeChange(value);
        UpdateVolumeText(); // 数値表示を更新
    }

    // SE用
    public void SetSeVolume(float value)
    {
        GSound.Instance.seVolumeChange(value);
        UpdateVolumeText(); // 数値表示を更新
    }

    // 数値テキストを書き換える関数
    void UpdateVolumeText()
    {
        if (bgmValueText != null) bgmValueText.text = bgmSlider.value.ToString("F1");
        if (seValueText != null) seValueText.text = seSlider.value.ToString("F1");
    }
}