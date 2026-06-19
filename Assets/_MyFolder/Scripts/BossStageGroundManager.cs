using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossStageGroundManager : MonoBehaviour
{
    [Header("ボス戦ブロック設定")]
    public List<SpikeTrap> allBlocks; // 28個のブロック全て登録
    public int blocksToCollapse = 16; // 崩れる数
    public int remainingBlockCount = 12; // 崩れた後に残る数
    public int spikeActiveCount = 8;  // とげが生えるブロック数

    [Header("崩壊演出設定")]
    public float collapseInterval = 0.15f; // ブロック間の崩れる間隔

    private List<SpikeTrap> remainingBlocks = new List<SpikeTrap>();
    private bool hasCollapsed = false;

    // ぽんたが一定数進んだら呼ぶ
    public void TriggerCollapse()
    {
        if (hasCollapsed) return;
        hasCollapsed = true;

        StartCoroutine(CollapseSequence());

        // 最初のblocksToCollapse個を崩す
        //for (int i = 0; i < blocksToCollapse && i < allBlocks.Count; i++)
        //{
        //    if (allBlocks[i] != null)
        //    {
        //        allBlocks[i].CollapseBlock();
        //    }
        //}

        //// 残りをボス戦使用ブロックとして記録
        //remainingBlocks.Clear();
        //for (int i = blocksToCollapse; i < allBlocks.Count; i++)
        //{
        //    if (allBlocks[i] != null)
        //    {
        //        remainingBlocks.Add(allBlocks[i]);
        //    }
        //}
    }

    IEnumerator CollapseSequence()
    {
        // 16個目から1個目に向かって順番に崩す ← 逆順
        for (int i = blocksToCollapse - 1; i >= 0; i--)
        {
            if (allBlocks[i] != null)
            {
                allBlocks[i].CollapseBlock();
            }
            yield return new WaitForSeconds(collapseInterval);
        }

        // 残りをボス戦使用ブロックとして記録
        remainingBlocks.Clear();
        for (int i = blocksToCollapse; i < allBlocks.Count; i++)
        {
            if (allBlocks[i] != null)
            {
                remainingBlocks.Add(allBlocks[i]);
            }
        }
    }

    // ランダムでspikeActiveCount個を選んでトゲ攻撃を実行
    public List<SpikeTrap> GetRandomSpikeTargets()
    {
        List<SpikeTrap> pool = new List<SpikeTrap>(remainingBlocks);
        List<SpikeTrap> selected = new List<SpikeTrap>();

        int count = Mathf.Min(spikeActiveCount, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int randIndex = Random.Range(0, pool.Count);
            selected.Add(pool[randIndex]);
            pool.RemoveAt(randIndex);
        }

        return selected;
    }
}