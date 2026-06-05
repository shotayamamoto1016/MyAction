using UnityEngine;
using UnityEngine.SceneManagement; 

public class TitleManager : MonoBehaviour
{
    // 開始ボタンから呼ばれる関数
    public void OnClickStartButton()
    {
        // シーンを読み込む
        SceneManager.LoadScene("02_Stage1");
    }

    // オプションボタン用
    public void OnClickOptionButton()
    {
        Debug.Log("オプションボタンが押されました");
    }
}