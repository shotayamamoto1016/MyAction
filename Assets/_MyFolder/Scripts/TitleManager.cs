using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("UIパネルのリファレンス")]
    public GameObject optionPopup;

    [Header("SEサウンド登録（タイトル用）")]
    public AudioClip[] seClips;

    //BGM名
    string bgmName;

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
}