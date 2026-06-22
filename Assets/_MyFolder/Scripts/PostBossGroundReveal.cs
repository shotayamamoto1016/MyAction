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

        // 最初は全て非表示にしておく
        foreach (var block in hiddenGroundBlocks)
        {
            if (block != null)
            {
                block.SetActive(false);
            }
        }
    }

    public void RevealGround()
    {
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