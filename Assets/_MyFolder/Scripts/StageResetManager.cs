using UnityEngine;

public class StageResetManager : MonoBehaviour
{
    public static StageResetManager instance;

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

    
    public void ResetStage()
    {
        // カラスの弾を全て非表示にする 
        CrowFireball[] fireballs = FindObjectsByType<CrowFireball>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var fireball in fireballs)
        {
            fireball.gameObject.SetActive(false);
        }

        
        // うらめし提灯の個別処理
        Chochin[] chochins = FindObjectsByType<Chochin>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var chochin in chochins)
        {
            if (CheckpointManager.instance != null &&
                CheckpointManager.instance.IsChochinDefeated(
                    chochin.GetInstanceID()))
            {
                // チェックポイント通過前に倒済み(復活させない)
                Destroy(chochin.gameObject);
            }
            else
            {
                // チェックポイント以降に倒したらリセットして復活
                chochin.gameObject.SetActive(true);
                chochin.ResetChochin();
            }
        }

        KageBoshi[] allKageBoshi = FindObjectsByType<KageBoshi>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var kage in allKageBoshi)
        {
            kage.ResetObject(); // 影法師を初期位置に戻す
        }

        // IResettableを実装している全オブジェクトをリセット
        MonoBehaviour[] allObjects = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            // Chochinは個別処理済みなのでスキップ
            if (obj.GetComponent<Chochin>() != null) continue;

            // SpikeTrapはBossStageGroundManagerが管理するのでスキップ
            if (obj.GetComponent<SpikeTrap>() != null) continue;

            if (obj is IResettable resettable)
            {
                resettable.ResetObject();
            }
        }


    }
}