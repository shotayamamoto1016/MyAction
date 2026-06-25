using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PostBossGroundReveal : MonoBehaviour
{
    [Header("出現させる地面ブロックの親オブジェクト")]
    public Transform hiddenGroundParent; //

    [Header("演出設定")]
    public float revealInterval = 0.05f; // ブロックごとの出現間隔
    public bool playSound = true;

    private List<GameObject> hiddenGroundBlocks = new List<GameObject>();

    void Start()
    {
        // 親オブジェクトの子オブジェクトを自動的に全取得
        if (hiddenGroundParent != null)
        {
            foreach (Transform child in hiddenGroundParent)
            {
                hiddenGroundBlocks.Add(child.gameObject);
            }
        }

        // もし既にボスを倒しているなら、最初から全て表示する
        if (GameManager.instance != null && GameManager.instance.isBossDefeated)
        {
            foreach (var block in hiddenGroundBlocks)
            {
                if (block != null) block.SetActive(true);
            }
        }
        else
        {
            // まだ倒していないなら非表示
            foreach (var block in hiddenGroundBlocks)
            {
                if (block != null) block.SetActive(false);
            }
        }
    }

    public void RevealGround()
    {
        // ボスを倒したフラグを立てる
        if (GameManager.instance != null) GameManager.instance.isBossDefeated = true;
        StartCoroutine(RevealSequence());
    }

    IEnumerator RevealSequence()
    {
        foreach (var block in hiddenGroundBlocks)
        {
            if (block != null)
            {
                block.SetActive(true);

                if (playSound && GSound.Instance != null)
                {
                    GSound.Instance.PlaySe(SoundData.SeType.Block_Break.ToString());
                }
            }
            yield return new WaitForSeconds(revealInterval);
        }
    }
}