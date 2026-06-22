using UnityEngine;

public class BossTriggerZone : MonoBehaviour
{
    public BossStageGroundManager groundManager;
    private bool triggered = false;

    // IResettableを実装してリセット時にtriggeredをfalseに戻す
    public void ResetTrigger()
    {
        triggered = false;
        Debug.Log("BossTriggerZoneリセット");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;
            Debug.Log("ボストリガーゾーン発動！");

            if (groundManager != null)
            {
                groundManager.TriggerCollapse();
            }
            else
            {
                Debug.LogError("GroundManagerが設定されていません！");
            }
        }
    }
}