using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MochiTenBoss : MonoBehaviour
{
    [Header("起動設定")]
    public float activateRangeX = 8f;   // ぽんたとの距離でボス戦開始

    [Header("浮遊の基準位置")]
    private Vector3 floatCenterPos; // 左右移動の中心位置

    [Header("飛行設定")]
    public float maxFlyHeight = 5f;     // 上昇する高さの上限
    public float riseSpeed = 3f;        // 上昇速度
    public float floatRangeX = 2f;      // 左右に動く幅
    public float floatSpeed = 1f;       // 左右の動く速さ

    [Header("浮遊アニメーション（前半3枚・後半3枚）")]
    public Sprite[] riseSprites;        // 上昇中の3枚
    public Sprite[] floatSprites;       // 浮遊中の3枚
    public float flyAnimInterval = 0.2f;

    [Header("トゲ攻撃設定")]
    public float spikeAttackInterval = 10f;
    public Sprite[] spikeAttackSprites; // 4枚・ループなし
    public float spikeAttackAnimInterval = 0.15f;
    public SpikeTrap[] spikeTraps;      // ステージに配置したトゲトラップ
    public float spikeWarningTime = 0.6f;
    public float spikeUpTime = 1f;

    [Header("踏みつけ攻撃設定")]
    public float stompStartTimeMin = 40f;
    public float stompStartTimeMax = 50f;
    public Sprite[] stompSprites;       // 3枚・ループなし
    public float stompAnimInterval = 0.15f;
    public float stompSpeed = 12f;
    public float stompStunDuration = 3f; // 着地後の隙
    public float landingDelayBeforeSprite3 = 0.3f;

    [Header("当たり判定")]
    public Collider2D headCollider;     // 頭のColliderのみ別オブジェクト
    public Collider2D bodyCollider;     // 胴体Collider

    [Header("地面管理")]
    public BossStageGroundManager groundManager;

    private SpriteRenderer spriteRenderer;
    private Transform player;
    private Vector3 startPosition;

    private bool isBattleStarted = false;
    private bool isDead = false;
    private bool isAttacking = false;
    private bool isStunned = false;

    private float spikeTimer = 0f;
    private float battleTimer = 0f;
    private float nextStompTime = 0f;

    private enum State { Waiting, Rising, Floating, SpikeAttack, StompAttack, Stunned, Dead }
    private State currentState = State.Waiting;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player")?.transform;
        startPosition = transform.position;
        floatCenterPos = transform.position;

        if (riseSprites.Length > 0)
            spriteRenderer.sprite = riseSprites[0];

        // 最初は頭のColliderだけ無効（地上にいる間は踏めない仕様にしたい場合は調整）
        if (headCollider != null) headCollider.enabled = false;
    }

    void Update()
    {
        if (isDead) return;

        if (!isBattleStarted)
        {
            CheckBattleStart();
            return;
        }

        battleTimer += Time.deltaTime;

        if (currentState == State.Floating)
        {
            spikeTimer += Time.deltaTime;

            if (spikeTimer >= spikeAttackInterval)
            {
                spikeTimer = 0f;
                StartCoroutine(SpikeAttackSequence());
            }
            else if (battleTimer >= nextStompTime)
            {
                StartCoroutine(StompAttackSequence());
            }
        }
    }

    void CheckBattleStart()
    {
        if (player == null) return;

        float distX = Mathf.Abs(player.position.x - transform.position.x);
        if (distX <= activateRangeX)
        {
            isBattleStarted = true;
            nextStompTime = Random.Range(stompStartTimeMin, stompStartTimeMax);
            StartCoroutine(RiseSequence());
        }
    }

    IEnumerator RiseSequence()
    {
        currentState = State.Rising;

        int animIndex = 0;
        float animTimer = 0f;

        // 上昇は今いるX座標のまま、Yだけ上昇する ← 強制リセットしない
        while (transform.position.y < startPosition.y + maxFlyHeight)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= flyAnimInterval && riseSprites.Length > 0)
            {
                animTimer = 0f;
                spriteRenderer.sprite = riseSprites[animIndex];
                animIndex = (animIndex + 1) % riseSprites.Length;
            }

            transform.position += Vector3.up * riseSpeed * Time.deltaTime;
            yield return null;
        }

        if (headCollider != null) headCollider.enabled = true;

        // 上昇完了後、floatSpeedでfloatCenterPosまで横移動して戻る ← 追加
        yield return StartCoroutine(ReturnToCenterX());

        currentState = State.Floating;
        StartCoroutine(FloatLoop());
    }

    // floatCenterPosのX座標まで、floatSpeedに応じた速度で戻る
    IEnumerator ReturnToCenterX()
    {
        while (Mathf.Abs(transform.position.x - floatCenterPos.x) > 0.05f)
        {
            float newX = Mathf.MoveTowards(
                transform.position.x, floatCenterPos.x,
                floatSpeed * 2f * Time.deltaTime); 

            transform.position = new Vector3(
                newX, transform.position.y, transform.position.z);
            yield return null;
        }

        transform.position = new Vector3(
            floatCenterPos.x, transform.position.y, transform.position.z);
    }

    IEnumerator FloatLoop()
    {
        float waveTimer = 0f;
        int animIndex = 0;
        float animTimer = 0f;

        // floatCenterPosを基準にする
        floatCenterPos = new Vector3(
            transform.position.x, transform.position.y, transform.position.z);

        while (currentState == State.Floating)
        {
            waveTimer += Time.deltaTime * floatSpeed;
            animTimer += Time.deltaTime;

            float x = floatCenterPos.x + Mathf.Sin(waveTimer) * floatRangeX;
            transform.position = new Vector3(
                x, floatCenterPos.y, floatCenterPos.z);

            if (animTimer >= flyAnimInterval && floatSprites.Length > 0)
            {
                animTimer = 0f;
                spriteRenderer.sprite = floatSprites[animIndex];
                animIndex = (animIndex + 1) % floatSprites.Length;
            }

            yield return null;
        }
    }

    IEnumerator SpikeAttackSequence()
    {
        currentState = State.SpikeAttack;
        isAttacking = true;

        for (int i = 0; i < spikeAttackSprites.Length; i++)
        {
            spriteRenderer.sprite = spikeAttackSprites[i];
            yield return new WaitForSeconds(spikeAttackAnimInterval);
        }

        // ランダムにトゲを出すブロックを選出 ← 変更
        List<SpikeTrap> targets = groundManager != null ?
            groundManager.GetRandomSpikeTargets() :
            new List<SpikeTrap>(spikeTraps);

        foreach (var trap in targets)
        {
            if (trap != null)
            {
                StartCoroutine(trap.ActivateSequence(spikeWarningTime, spikeUpTime));
            }
        }

        yield return new WaitForSeconds(spikeWarningTime + spikeUpTime);

        // floatCenterPosまでfloatSpeedで戻ってから再開
        yield return StartCoroutine(ReturnToCenterX());

        isAttacking = false;
        currentState = State.Floating;
        StartCoroutine(FloatLoop());
    }

    IEnumerator StompAttackSequence()
    {
        currentState = State.StompAttack;
        isAttacking = true;

        if (player == null) yield break;

        // 1枚目を表示してすぐ2枚目へ
        if (stompSprites.Length > 0)
        {
            spriteRenderer.sprite = stompSprites[0];
            yield return new WaitForSeconds(stompAnimInterval);
        }
        if (stompSprites.Length > 1)
        {
            spriteRenderer.sprite = stompSprites[1];
        }

        // 2枚目の状態でぽんたを5秒間追従しながらじらす
        float teaseTime = 5f;
        float elapsed = 0f;
        float followSpeed = 4f;

        while (elapsed < teaseTime)
        {
            if (player != null)
            {
                float targetX = player.position.x;
                float newX = Mathf.MoveTowards(
                    transform.position.x, targetX,
                    followSpeed * Time.deltaTime);

                transform.position = new Vector3(
                    newX, transform.position.y, transform.position.z);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 最後にぽんたの位置を確定して狙いを定める
        float finalTargetX = player != null ? player.position.x : transform.position.x;
        float aimMoveTime = 0.3f;
        float aimElapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 aimPos = new Vector3(
            finalTargetX, transform.position.y, transform.position.z);

        while (aimElapsed < aimMoveTime)
        {
            transform.position = Vector3.Lerp(startPos, aimPos, aimElapsed / aimMoveTime);
            aimElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = aimPos;

        // 真下に急降下
        float groundY = startPosition.y;
        while (transform.position.y > groundY)
        {
            transform.position += Vector3.down * stompSpeed * Time.deltaTime;

            if (transform.position.y <= groundY)
            {
                transform.position = new Vector3(
                    transform.position.x, groundY, transform.position.z);
                break;
            }
            yield return null;
        }

        // 地面に当たってから少し間を置いて3枚目に切り替え 
        yield return new WaitForSeconds(landingDelayBeforeSprite3);

        if (stompSprites.Length > 2)
        {
            spriteRenderer.sprite = stompSprites[2];
        }

        // 着地後3秒間スタン
        currentState = State.Stunned;
        isStunned = true;
        yield return new WaitForSeconds(stompStunDuration);
        isStunned = false;

        if (!isDead)
        {
            isAttacking = false;
            currentState = State.Rising;
            StartCoroutine(RiseSequence());
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerController playerController =
            collision.gameObject.GetComponent<PlayerController>();
        if (playerController == null) return;

        // スタン中に頭を踏まれたら撃破
        if (isStunned)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    // 頭のColliderと接触しているか確認
                    if (headCollider != null &&
                        collision.GetContact(0).collider == headCollider)
                    {
                        StartCoroutine(DefeatSequence());
                        return;
                    }
                }
            }
        }

        // それ以外は胴体・浮遊中含めて触れるとぽんた死亡
        if (!playerController.isDead && !playerController.isInvincible)
        {
            playerController.Die();
        }
    }

    IEnumerator DefeatSequence()
    {
        isDead = true;
        StopAllCoroutines();

        if (bodyCollider != null) bodyCollider.enabled = false;
        if (headCollider != null) headCollider.enabled = false;

        // 別途演出を後で追加
        Debug.Log("もち天さま撃破！演出は後で追加");

        yield return null;
    }
}