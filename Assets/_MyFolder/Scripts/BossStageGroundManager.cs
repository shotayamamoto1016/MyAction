using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossStageGroundManager : MonoBehaviour, IResettable
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
        Debug.Log("TriggerCollapse開始");
        StartCoroutine(CollapseSequence());   
    }

    IEnumerator CollapseSequence()
    {
        // 16個目から1個目に向かって順番に崩す 
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

        Debug.Log("CollapseSequence完了。remainingBlocks数: " + remainingBlocks.Count);
    }

    // ランダムでspikeActiveCount個を選んでトゲ攻撃を実行
    public List<SpikeTrap> GetRandomSpikeTargets()
    {
        Debug.Log("GetRandomSpikeTargets呼ばれた。remainingBlocks数: " + remainingBlocks.Count);

        List<SpikeTrap> pool;
        if (remainingBlocks.Count == 0)
        {
            Debug.LogWarning("remainingBlocksが空！allBlocksから直接取得します");
            pool = new List<SpikeTrap>();
            for (int i = blocksToCollapse; i < allBlocks.Count; i++)
            {
                if (allBlocks[i] != null && allBlocks[i].gameObject.activeSelf)
                {
                    pool.Add(allBlocks[i]);
                }
            }
        }
        else
        {
            pool = new List<SpikeTrap>(remainingBlocks);
        }

        List<SpikeTrap> selected = new List<SpikeTrap>();
        int count = Mathf.Min(spikeActiveCount, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int randIndex = Random.Range(0, pool.Count);
            selected.Add(pool[randIndex]);
            pool.RemoveAt(randIndex);
        }

        Debug.Log("選ばれたトゲの数: " + selected.Count);
        return selected;

        //List<SpikeTrap> pool = new List<SpikeTrap>(remainingBlocks);
        //List<SpikeTrap> selected = new List<SpikeTrap>();

        //int count = Mathf.Min(spikeActiveCount, pool.Count);

        //for (int i = 0; i < count; i++)
        //{
        //    int randIndex = Random.Range(0, pool.Count);
        //    selected.Add(pool[randIndex]);
        //    pool.RemoveAt(randIndex);
        //}

        //return selected;
    }

    // チェックポイント復活時にすべてのブロックを元に戻す
    public void ResetObject()
    {
        StopAllCoroutines();
        hasCollapsed = false;
        remainingBlocks.Clear();

        // 全ブロックを復活させる
        foreach (var block in allBlocks)
        {
            if (block != null)
            {
                block.ResetObject();
            }
        }

        // BossTriggerZoneもリセット
        BossTriggerZone triggerZone = FindFirstObjectByType<BossTriggerZone>();
        if (triggerZone != null)
        {
            triggerZone.ResetTrigger();
        }

        Debug.Log("BossStageGroundManager リセット完了");
    }
}