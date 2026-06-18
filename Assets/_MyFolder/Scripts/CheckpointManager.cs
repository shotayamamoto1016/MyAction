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
    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
        hasCheckpoint = true;

        // チェックポイント通過時点での倒済み提灯IDを記録
        chochinIdsAtCheckpoint = new HashSet<int>(defeatedChochinIds);

        Debug.Log("チェックポイント設定: " + position);
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