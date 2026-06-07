using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [Header("UIパネルのリファレンス")]
    public GameObject optionPopup;

    //BGM名
    string bgmName;

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

    // 開始ボタン 
    public void OnClickStartButton()
    {
        // 02_Stage1 シーンを読み込む
        SceneManager.LoadScene("02_Stage1");
    }

    // 設定ボタン
    public void OnClickOptionButton()
    {
        if (optionPopup != null)
        {
            optionPopup.SetActive(true);
            Debug.Log("設定画面を開きました");
        }
    }

    // 閉じるボタン
    public void OnClickCloseOptionButton()
    {
        if (optionPopup != null)
        {
            optionPopup.SetActive(false);
            Debug.Log("設定画面を閉じました");
        }
    }
}