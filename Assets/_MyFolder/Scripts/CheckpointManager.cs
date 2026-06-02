using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager instance;

    private Vector3 checkpointPosition;
    private bool hasCheckpoint = false;

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
}