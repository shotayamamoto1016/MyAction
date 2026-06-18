using UnityEngine;
using System.Collections;

public class CoroutineRunner : MonoBehaviour
{
    public static CoroutineRunner Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartRespawn(BreakBlock block, float delay)
    {
        StartCoroutine(RespawnRoutine(block, delay));
    }

    IEnumerator RespawnRoutine(BreakBlock block, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (block != null)
        {
            block.ResetObject();
        }
    }
}