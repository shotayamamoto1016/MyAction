using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;

    private Vector3 checkpointPosition;
    private bool hasCheckpoint = false;

    // 座標で記録する
    private HashSet<Vector3> defeatedChochinPositions = new HashSet<Vector3>();
    private HashSet<Vector3> positionsAtCheckpoint = new HashSet<Vector3>();

    private bool isBossCheckpoint = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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
        positionsAtCheckpoint = new HashSet<Vector3>(defeatedChochinPositions);
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

    // 提灯が倒された時に座標を登録
    public void RegisterDefeatedChochin(Vector3 pos)
    {
        defeatedChochinPositions.Add(pos);
    }

    // うらめし提灯が倒されているか確認
    public bool IsChochinDefeated(Vector3 pos)
    {
        return positionsAtCheckpoint.Contains(pos);
    }

    // チェックポイントをリセット
    public void ResetCheckpoint()
    {
        hasCheckpoint = false;
        defeatedChochinPositions.Clear();
        positionsAtCheckpoint.Clear();
    }
}