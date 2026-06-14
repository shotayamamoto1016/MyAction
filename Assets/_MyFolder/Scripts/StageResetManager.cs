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

    //// チェックポイント復活時に呼ばれる
    //public void ResetStage()
    //{
    //    // 隠しブロックを初期化
    //    HiddenBlock[] hiddenBlocks = FindObjectsByType<HiddenBlock>(FindObjectsSortMode.None);
    //    foreach (var block in hiddenBlocks)
    //    {
    //        block.ResetBlock();
    //    }

    //    // 崩れるブロックを初期化
    //    CrumbleBlock[] crumbleBlocks = FindObjectsByType<CrumbleBlock>(FindObjectsSortMode.None);
    //    foreach (var block in crumbleBlocks)
    //    {
    //        block.ResetBlock();
    //    }

    //    // CollapseBlockを初期化 
    //    CollapseBlock[] collapseBlocks = FindObjectsByType<CollapseBlock>(
    //        FindObjectsInactive.Include, FindObjectsSortMode.None);
    //    foreach (var block in collapseBlocks)
    //    {
    //        block.ResetBlock();
    //    }

    //    // うらめし提灯を初期化（倒していないものだけ復活）
    //    Chochin[] chochins = FindObjectsByType<Chochin>(FindObjectsSortMode.None);
    //    foreach (var chochin in chochins)
    //    {
    //        if (CheckpointManager.instance != null &&
    //            CheckpointManager.instance.IsChochinDefeated(chochin.GetInstanceID()))
    //        {
    //            // 倒済みの提灯は消す
    //            Destroy(chochin.gameObject);
    //        }
    //        else
    //        {
    //            // 倒していない提灯は初期化
    //            chochin.ResetChochin();
    //        }
    //    }

    //    // ころもちを復活 
    //    Koromochi[] koromochis = FindObjectsByType<Koromochi>(
    //        FindObjectsInactive.Include, FindObjectsSortMode.None);
    //    foreach (var koromochi in koromochis)
    //    {
    //        koromochi.ResetKoromochi();
    //    }
    //}

    public void ResetStage()
    {
        // カラスの弾を全て非表示にする 
        CrowFireball[] fireballs = FindObjectsByType<CrowFireball>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var fireball in fireballs)
        {
            fireball.gameObject.SetActive(false);
        }

        // IResettableを実装している全オブジェクトをリセット
        MonoBehaviour[] allObjects = FindObjectsByType<MonoBehaviour>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            if (obj is IResettable resettable)
            {
                resettable.ResetObject();
            }
        }

        // うらめし提灯は個別処理（倒済みは復活させない）
        Chochin[] chochins = FindObjectsByType<Chochin>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var chochin in chochins)
        {
            if (CheckpointManager.instance != null &&
                CheckpointManager.instance.IsChochinDefeated(
                    chochin.GetInstanceID()))
            {
                Destroy(chochin.gameObject);
            }
        }
    }
}