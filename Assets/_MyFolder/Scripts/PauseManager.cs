using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("表示・非表示にする親オブジェクト")]
    public GameObject menuPopup;    // メインのポーズ画面
    public GameObject confirmPopup; // 「本当に戻りますか？」画面

    private bool isPaused = false;

    void Update()
    {
        // Escキーでもポーズの開閉ができるように
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) Resume();
            else PressMenuButton();
        }
    }

    

    // 左上のメニューボタンを押した時
    public void PressMenuButton()
    {
        isPaused = true;
        menuPopup.SetActive(true);
        confirmPopup.SetActive(false); // 確認画面は念のため閉じる
        Time.timeScale = 0f;           // ゲームを停止
    }

    // ゲームに戻る
    public void Resume()
    {
        isPaused = false;
        menuPopup.SetActive(false);
        confirmPopup.SetActive(false);
        Time.timeScale = 1f;           // ゲームを再始動
    }

    

    // 「タイトルへ戻る」ボタンを押した時
    public void PressReturnTitle()
    {
        // メインポーズ画面を隠して確認画面を出す
        menuPopup.SetActive(false);
        confirmPopup.SetActive(true);
    }

    // 確認画面で「いいえ」を押した時
    public void CancelReturn()
    {
        confirmPopup.SetActive(false);
        menuPopup.SetActive(true); // メインポーズ画面に戻す
    }

    // 確認画面で「はい」を押した時
    public void ConfirmReturn()
    {
        Time.timeScale = 1f; // 重要：時間を戻してからシーン移動
        SceneManager.LoadScene("00_Title"); // タイトルのシーン名に合わせて変更
    }
}