using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;

    private Vector3 checkpointPosition;
    private bool hasCheckpoint = false;

    // 倒したうらめし提灯のIDを記録
    private HashSet<int> defeatedChochinIds = new HashSet<int>();

    // チェックポイント通過時の提灯のIDを記録
    private HashSet<int> chochinIdsAtCheckpoint = new HashSet<int>();

    private bool isBossCheckpoint = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // チェックポイントを設定
    public void SetCheckpoint(Vector3 position, bool isBoss = false)
    {
        checkpointPosition = position;
        hasCheckpoint = true;
        isBossCheckpoint = isBoss; 
        chochinIdsAtCheckpoint = new HashSet<int>(defeatedChochinIds);
    }

    public bool IsBossCheckpoint()
    {
        return isBossCheckpoint;
    }

    // チェックポイントがあるか確認
    public bool HasCheckpoint()
    {
        return hasCheckpoint;
    }

    // チェックポイントの位置を取得
    public Vector3 GetCheckpointPosition()
    {
        return checkpointPosition;
    }

    // うらめし提灯が倒された時に呼ばれる
    public void RegisterDefeatedChochin(int id)
    {
        defeatedChochinIds.Add(id);
    }

    // うらめし提灯が倒されているか確認
    public bool IsChochinDefeated(int id)
    {
        // チェックポイント通過時点で倒済みのIDのみtrue
        return chochinIdsAtCheckpoint.Contains(id);
    }

    // チェックポイントをリセット
    public void ResetCheckpoint()
    {
        hasCheckpoint = false;
        defeatedChochinIds.Clear();
        chochinIdsAtCheckpoint.Clear();
    }
}