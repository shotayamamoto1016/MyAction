using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrogDefeatWall : MonoBehaviour
{
    [Header("崩壊ブロック設定")]
    public List<GameObject> wallBlocks; // 崩壊させるブロック群
    public float collapseInterval = 0.1f; // ブロック間の崩壊間隔

    [Header("破片設定")]
    public Sprite fragmentSprite;
    public int fragmentCount = 4;
    public float fragmentForce = 5f;
    public float fragmentLifeTime = 0.8f;

    public void CollapseWall()
    {
        StartCoroutine(CollapseSequence());
    }

    IEnumerator CollapseSequence()
    {
        foreach (var block in wallBlocks)
        {
            if (block != null && block.activeSelf)
            {
                SpawnFragments(block.transform.position);
                block.SetActive(false);
            }
            yield return new WaitForSeconds(collapseInterval);
        }
    }

    void SpawnFragments(Vector3 position)
    {
        if (fragmentSprite == null) return;

        for (int i = 0; i < fragmentCount; i++)
        {
            GameObject fragment = new GameObject("Fragment");
            fragment.transform.position = position;
            fragment.transform.localScale = Vector3.one * 0.4f;

            SpriteRenderer sr = fragment.AddComponent<SpriteRenderer>();
            sr.sprite = fragmentSprite;

            Rigidbody2D rb = fragment.AddComponent<Rigidbody2D>();

            float angle = 45f + (i * 90f);
            float randomAngle = angle + Random.Range(-30f, 30f);
            Vector2 direction = new Vector2(
                Mathf.Cos(randomAngle * Mathf.Deg2Rad),
                Mathf.Sin(randomAngle * Mathf.Deg2Rad));

            rb.linearVelocity = direction * fragmentForce;
            rb.angularVelocity = Random.Range(-360f, 360f);

            Destroy(fragment, fragmentLifeTime);
        }
    }
}