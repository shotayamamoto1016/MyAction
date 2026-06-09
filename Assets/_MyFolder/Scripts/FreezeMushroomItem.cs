using UnityEngine;
using System.Collections;

public class FreezeMushroomItem : MonoBehaviour, IResettable
{
    [Header("凍るエフェクト")]
    public float freezeDuration = 1.5f;

    private Vector3 startPosition;
    private bool isCollected = false;

    void Start()
    {
        startPosition = transform.position;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && !player.isDead)
            {
                isCollected = true;

                // 凍るSEを再生
                GSound.Instance.PlaySe(SoundData.SeType.Item_Freeze.ToString());

                player.FreezeAndDie(freezeDuration);
                gameObject.SetActive(false); // Destroyの代わりに非表示
            }
        }
    }

    public void ResetObject()
    {
        isCollected = false;
        transform.position = startPosition;
        gameObject.SetActive(true);
    }
}