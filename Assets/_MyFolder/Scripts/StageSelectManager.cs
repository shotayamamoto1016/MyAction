using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class StageSelectManager : MonoBehaviour
{
    [Header("ステージボタン設定")]
    public Button[] stageButtons;

    [Header("巻物背景のCanvasGroup")]
    public CanvasGroup scrollCanvasGroup;

    [Header("演出設定")]
    public float shakeDuration = 0.6f;　　// 演出時間
    public float swingAngle = 5f;      // 左右に揺れる角度
    public float elementFadeDuration = 0.4f; // 他の要素が消えるまでの時間

    private int clearedStage;

    void Start()
    {
        // クリア状況を読み込む
        clearedStage = PlayerPrefs.GetInt("StageCleared", 1);

        // ボタンの状態を更新
        for (int i = 0; i < stageButtons.Length; i++)
        {
            int stageNum = i + 1;
            Image btnImage = stageButtons[i].GetComponent<Image>();
            stageButtons[i].transition = Selectable.Transition.None;

            if (stageNum <= clearedStage)
            {
                // 開放済み：通常の色
                stageButtons[i].interactable = true;
                btnImage.color = Color.white;
            }
            else
            {
                // 暗くする
                stageButtons[i].interactable = false;
                btnImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            }
        }

        // タイトルと同じBGMを流す
        GSound.Instance.PlayBgm(SoundData.BgmType.Title.ToString(), true);
    }

    // 各ボタンから呼ばれる関数
    public void OnClickStage(string sceneName)
    {
        GSound.Instance.PlaySe(SoundData.SeType.Button_Click.ToString());

        // 押されたボタンを特定して揺らす
        GameObject clickedBtn = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (clickedBtn != null)
        {
            // 全てのボタンを一時的に押せなくする
            foreach (var b in stageButtons) b.interactable = false;
            clickedBtn.GetComponent<Button>().interactable = false;

            StartCoroutine(SelectSequence(clickedBtn, sceneName));
        }
        else
        {
            // 万が一ボタンが取得できなかった時のフォールバック
            SceneManager.LoadScene("00_Title");
        }
    }

    // 揺れ ➔ フェード ➔ 遷移 の流れ
    IEnumerator SelectSequence(GameObject selectedBtn, string sceneName)
    {
        //木札を揺らす
        Quaternion originalRot = selectedBtn.transform.localRotation;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            // サイン波を使って左右に滑らかに振る
            float angle = Mathf.Sin(elapsed * 20f) * swingAngle;
            selectedBtn.transform.localRotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }
        selectedBtn.transform.localRotation = originalRot;

        Canvas btnCanvas = selectedBtn.GetComponent<Canvas>();
        if (btnCanvas != null)
        {
            btnCanvas.overrideSorting = true;
            btnCanvas.sortingOrder = 100;
        }
        float t = 0;

        while (t < elementFadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / elementFadeDuration);

            // 巻物を薄くする
            if (scrollCanvasGroup != null) scrollCanvasGroup.alpha = alpha;

            // 他のボタンを薄くする
            foreach (var b in stageButtons)
            {
                if (b.gameObject != selectedBtn)
                {
                    CanvasGroup cg = b.GetComponent<CanvasGroup>();
                    if (cg == null) cg = b.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = alpha;
                }

                GameObject backBtnObj = GameObject.Find("ReturnTitleButton"); 
                if (backBtnObj != null && backBtnObj != selectedBtn)
                {
                    CanvasGroup cg = backBtnObj.GetComponent<CanvasGroup>();
                    if (cg == null) cg = backBtnObj.AddComponent<CanvasGroup>();
                    cg.alpha = alpha;
                }

            }
            yield return null;
        }

        // フェードアウト開始
        if (GameManager.instance != null)
        {
            // GameManagerのisComingFromGoalをfalseにしてワープを防ぐ
            GameManager.instance.isComingFromGoal = false;
            // 画面を暗くする
            yield return GameManager.instance.StartCoroutine("Fade", 1f);
        }

        // シーン遷移
        SceneManager.LoadScene(sceneName);
    }

    public void OnClickBackToTitle()
    {
        GSound.Instance.PlaySe(SoundData.SeType.Button_Click.ToString());
        SceneManager.LoadScene("00_Title");
    }
}