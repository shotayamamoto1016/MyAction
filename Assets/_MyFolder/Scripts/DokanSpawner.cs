using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DokanSpawner : MonoBehaviour, IResettable
{
    [Header("土管設定")]
    public Sprite dokanSprite;          // 土管の画像

    [Header("スポーン設定")]
    public GameObject koromochiPrefab;  // KoromochiLeftのPrefab
    public float spawnIntervalFast = 0.6f; // ONの時のスポーン間隔
    public float spawnIntervalSlow = 3f;   // OFFの時のスポーン間隔
    public int maxKoromochi = 5;        // 最大スポーン数
    public Transform spawnPoint; // スポーン位置を直接指定 
    
    private bool isOn = false;
    private bool isSpawning = false;
    private float currentInterval = 0.6f;
    private List<GameObject> spawnedKoromochis = new List<GameObject>();
    
    void Start()
    {
        currentInterval = spawnIntervalFast;

        // spawnPointが設定されていない場合は自分の位置を使う
        if (spawnPoint == null)
        {
            GameObject sp = new GameObject("SpawnPoint");
            sp.transform.SetParent(transform);
            sp.transform.localPosition = new Vector3(0f, 1f, 0f);
            spawnPoint = sp.transform;
        }
    }

    public void SetOn(bool value)
    {
        isOn = value;

        // ONとOFFでスポーン間隔を変更
        currentInterval = isOn ? spawnIntervalFast : spawnIntervalSlow;

        if (!isSpawning)
        {
            StartCoroutine(SpawnRoutine());
        }
    }


    IEnumerator SpawnRoutine()
    {
        isSpawning = true;

        while (true)
        {
            CleanupList();
            if (spawnedKoromochis.Count < maxKoromochi)
            {
                SpawnKoromochi();
            }

            // 現在のインターバルで待機
            yield return new WaitForSeconds(currentInterval);
        }
    }
    void SpawnKoromochi()
    {
        if (koromochiPrefab == null) return;

        // spawnPointの位置にスポーン
        Vector3 spawnPos = spawnPoint != null ?
            spawnPoint.position : transform.position + Vector3.up;

        GameObject koromochi = Instantiate(
            koromochiPrefab, spawnPos, Quaternion.identity);

        spawnedKoromochis.Add(koromochi);
    }

    void CleanupList()
    {
        spawnedKoromochis.RemoveAll(k => k == null || !k.activeSelf);
    }

    public void ResetObject()
    {
        StopAllCoroutines();
        isSpawning = false;
        isOn = false;
        currentInterval = spawnIntervalFast;

        foreach (var koromochi in spawnedKoromochis)
        {
            if (koromochi != null)
            {
                koromochi.SetActive(false);
            }
        }
        spawnedKoromochis.Clear();

        // リセット後再開
        StartCoroutine(SpawnRoutine());
    }
}