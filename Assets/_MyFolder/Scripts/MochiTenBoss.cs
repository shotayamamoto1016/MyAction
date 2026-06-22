using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MochiTenBoss : MonoBehaviour, IResettable
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

    [Header("アニメーション管理")]
    private int floatAnimIndex = 0; // クラス変数として管理

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
    public float stompGroundOffset = -0.5f; // 着地位置の調整

    [Header("踏みつけ連続攻撃設定")]
    public float stompStunDurationFirst = 0.5f; // 1回目・2回目用の短い隙
    private int stompChainRemaining = 0; // 残りの連続踏みつけ回数
    private bool isInStompChain = false;

    [Header("当たり判定")]
    //public Collider2D headCollider;     // 頭のColliderのみ別オブジェクト
    public Collider2D bodyCollider;     // 胴体Collider

    [Header("地面管理")]
    public BossStageGroundManager groundManager;

    [Header("Collider用子オブジェクト")]
    public Transform colliderObject; // Colliderがついている子オブジェクト

    [Header("状態別位置設定")]
    public Vector2 colliderLocalPosFloat = new Vector2(0.113f, 0.11f);
    public Vector2 colliderSizeFloat = new Vector2(4.456f, 5.52f);
    public CapsuleDirection2D colliderDirectionFloat = CapsuleDirection2D.Vertical;

    public Vector2 colliderLocalPosStomp = new Vector2(0.404f, -0.234f);
    public Vector2 colliderSizeStomp = new Vector2(4.566f, 3.22f);
    public CapsuleDirection2D colliderDirectionStomp = CapsuleDirection2D.Horizontal;

    private CapsuleCollider2D capsuleCollider;

    [Header("ダメージ設定")]
    public int maxHitCount = 3;          // 倒れるまでの被弾回数
    private int currentHitCount = 0;

    [Header("被弾アニメーション（4枚）")]
    public Sprite[] hitSprites;          // 1,2枚目：一回再生 / 3,4枚目：ループ
    public float hitAnimInterval = 0.15f;
    public float hitLoopDuration = 4f;   // 3,4枚目をループする時間
    public float hitLoopInterval = 0.3f;  // 3,4枚目ループの速度
    public float hitSpriteOffsetY = -0.3f; // 被弾アニメーション画像のYオフセット

    [Header("Rise設定")]
    public float riseStartOffsetY = 0f;

    [Header("怒り演出")]
    public Color angryColorTint = new Color(1f, 0.5f, 0.5f, 1f); // 赤みがかった色

    [Header("撃破演出（しょんぼり2枚→昇天3枚）")]
    public Vector2 defeatSitOffset = Vector2.zero;
    public Sprite[] defeatSitSprites;     // しょんぼり座り込み
    public float defeatSitAnimInterval = 0.2f;
    public float defeatSitHoldTime = 4f;  // 2枚目を表示する時間

    public Sprite[] defeatAscendSprites;  // 昇天3枚
    public float defeatAscendAnimInterval = 0.3f;
    public float defeatAscendSpeed = 1f;  // 上昇速度
    public float defeatFadeDuration = 1.5f; // 3枚目でフェードアウトする時間

    [Header("HP段階別ステータス設定")]
    public float riseSpeedHP3 = 2f;
    public float riseSpeedHP2 = 2.5f;
    public float riseSpeedHP1 = 3f;

    public float floatSpeedHP3 = 1f;
    public float floatSpeedHP2 = 1.5f;
    public float floatSpeedHP1 = 2f;

    public float spikeAttackIntervalHP3 = 10f;
    public float spikeAttackIntervalHP2 = 8f;
    public float spikeAttackIntervalHP1 = 6f;

    public float spikeWarningTimeHP3 = 2.0f;
    public float spikeWarningTimeHP2 = 1.75f;
    public float spikeWarningTimeHP1 = 1.5f;

    public float spikeUpTimeHP3 = 1.5f;
    public float spikeUpTimeHP2 = 1.75f;
    public float spikeUpTimeHP1 = 2.0f;

    public float stompStartTimeMinHP3 = 15f;
    public float stompStartTimeMinHP2 = 20f;
    public float stompStartTimeMinHP1 = 25f;

    public float stompStartTimeMaxHP3 = 20f;
    public float stompStartTimeMaxHP2 = 25f;
    public float stompStartTimeMaxHP1 = 30f;

    public float stompSpeedHP3 = 12f;
    public float stompSpeedHP2 = 14.5f;
    public float stompSpeedHP1 = 17f;

    public int spikeActiveCountHP3 = 6;
    public int spikeActiveCountHP2 = 7;
    public int spikeActiveCountHP1 = 8;

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

    private enum State { Waiting, Rising, Floating, SpikeAttack, StompAttack, Hit, Stunned, Dead }
    private State currentState = State.Waiting;

    [Header("撃破後の地面出現")]
    public PostBossGroundReveal postBossGroundReveal;

    [Header("デバッグ用")]
    public bool enableDebugKey = true; // テスト時のみtrueにする

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindWithTag("Player")?.transform;
        startPosition = transform.position;
        floatCenterPos = transform.position;

        if (colliderObject != null)
            capsuleCollider = colliderObject.GetComponent<CapsuleCollider2D>();

        if (riseSprites.Length > 0)
            spriteRenderer.sprite = riseSprites[0];

        SetColliderState(false);

        // HP3の初期ステータスを適用
        riseSpeed = riseSpeedHP3;
        floatSpeed = floatSpeedHP3;
        spikeAttackInterval = spikeAttackIntervalHP3;
        spikeWarningTime = spikeWarningTimeHP3;
        spikeUpTime = spikeUpTimeHP3;
        stompStartTimeMin = stompStartTimeMinHP3;
        stompStartTimeMax = stompStartTimeMaxHP3;
        stompSpeed = stompSpeedHP3;

        if (groundManager != null)
        {
            groundManager.spikeActiveCount = spikeActiveCountHP3;
        }
    }

    void SetColliderState(bool isStompPose)
    {
        if (capsuleCollider == null || colliderObject == null) return;

        if (isStompPose)
        {
            colliderObject.localPosition = colliderLocalPosStomp;
            capsuleCollider.size = colliderSizeStomp;
            capsuleCollider.direction = colliderDirectionStomp;
        }
        else
        {
            colliderObject.localPosition = colliderLocalPosFloat;
            capsuleCollider.size = colliderSizeFloat;
            capsuleCollider.direction = colliderDirectionFloat;
        }
    }

    void Update()
    {
        if (isDead) return;

        //// デバッグ:Tキーで即座に撃破演出を見られる 
        //if (enableDebugKey && Input.GetKeyDown(KeyCode.T))
        //{
        //    Debug.Log("デバッグ：撃破演出を強制再生");
        //    isDead = true;
        //    StartCoroutine(DefeatSequence());
        //    return;
        //}

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
                battleTimer = -9999f; 
                isInStompChain = true;
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

            // ボスBGMに切り替え
            if (GSound.Instance != null)
            {
                GSound.Instance.PlayBgm(SoundData.BgmType.Boss.ToString(), true);
            }

            StartCoroutine(RiseSequence());
        }
    }

    IEnumerator RiseSequence()
    {
        if (isDead) yield break;

        currentState = State.Rising;
        SetColliderState(false); // 浮遊用Colliderに戻す

        // Rise開始位置をオフセット調整 
        transform.position = new Vector3(
            transform.position.x,
            transform.position.y + riseStartOffsetY,
            transform.position.z);

        int animIndex = 0;
        float animTimer = 0f;

        // 上昇は今いるX座標のまま、Yだけ上昇する ← 強制リセットしない
        while (transform.position.y < startPosition.y + maxFlyHeight)
        {
            if (isDead) yield break;

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

        if (isDead) yield break;

        // 上昇完了後、floatSpeedでfloatCenterPosまで横移動して戻る 
        yield return StartCoroutine(ReturnToCenterX());

        if (isDead) yield break;

        currentState = State.Floating;
        StartCoroutine(FloatLoop());
    }

    // floatCenterPosのX座標まで、floatSpeedに応じた速度で戻る
    IEnumerator ReturnToCenterX()
    {
        float animTimer = 0f;

        while (Mathf.Abs(transform.position.x - floatCenterPos.x) > 0.05f)
        {
            float newX = Mathf.MoveTowards(
                transform.position.x, floatCenterPos.x,
                floatSpeed * 2f * Time.deltaTime);

            transform.position = new Vector3(
                newX, transform.position.y, transform.position.z);

            animTimer += Time.deltaTime;
            if (animTimer >= flyAnimInterval && floatSprites.Length > 0)
            {
                animTimer = 0f;
                spriteRenderer.sprite = floatSprites[floatAnimIndex];
                floatAnimIndex = (floatAnimIndex + 1) % floatSprites.Length;
            }

            yield return null;
        }

        transform.position = new Vector3(
            floatCenterPos.x, transform.position.y, transform.position.z);
    }

    IEnumerator FloatLoop()
    {
        float waveTimer = 0f;
        float animTimer = 0f;

        floatCenterPos = new Vector3(
            transform.position.x, transform.position.y, transform.position.z);

        while (currentState == State.Floating)
        {
            if (isDead) yield break;

            waveTimer += Time.deltaTime * floatSpeed;
            animTimer += Time.deltaTime;

            float x = floatCenterPos.x + Mathf.Sin(waveTimer) * floatRangeX;
            transform.position = new Vector3(
                x, floatCenterPos.y, floatCenterPos.z);

            if (animTimer >= flyAnimInterval && floatSprites.Length > 0)
            {
                animTimer = 0f;
                spriteRenderer.sprite = floatSprites[floatAnimIndex]; 
                floatAnimIndex = (floatAnimIndex + 1) % floatSprites.Length;
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

    //int GetStompAttackCount()
    //{
    //    // 残りの被弾可能回数を計算
    //    int remainingHits = maxHitCount - currentHitCount;

    //    Debug.Log("残りHP: " + remainingHits + " (maxHitCount:" + maxHitCount +
    //        " currentHitCount:" + currentHitCount + ")");

    //    if (remainingHits >= 3) return 1;
    //    if (remainingHits == 2) return 2;
    //    return 3;
    //}

    IEnumerator StompAttackSequence()
    {
        currentState = State.StompAttack;
        isAttacking = true;

        // 1回も連続設定がない場合は通常1回として扱う
        if (stompChainRemaining <= 0) stompChainRemaining = 1;

        if (player == null) yield break;

        // 1枚目→2枚目
        if (stompSprites.Length > 0)
        {
            spriteRenderer.sprite = stompSprites[0];
            yield return new WaitForSeconds(stompAnimInterval);
        }
        if (stompSprites.Length > 1)
        {
            spriteRenderer.sprite = stompSprites[1];
        }

        // じらし追従
        bool isChainContinuing = stompChainRemaining < GetInitialChainCount();
        float teaseTime = isChainContinuing ? 1.0f : 5f;
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

        // 最終位置決定
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
        float groundY = startPosition.y + stompGroundOffset;
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

        // 着地後3枚目に切り替え
        yield return new WaitForSeconds(landingDelayBeforeSprite3);

        if (stompSprites.Length > 2)
        {
            spriteRenderer.sprite = stompSprites[2];
            SetColliderState(true);
        }

        // スタン時間：連続踏みつけの最後の1回だけ長くする
        currentState = State.Stunned;
        isStunned = true;

        bool isLastInChain = (stompChainRemaining <= 1);
        float currentStunDuration = isLastInChain ?
            stompStunDuration : stompStunDurationFirst;

        float stunTimer = 0f;
        while (stunTimer < currentStunDuration)
        {
            if (currentState != State.Stunned) yield break; // 被弾したら抜ける
            stunTimer += Time.deltaTime;
            yield return null;
        }

        isStunned = false;
        SetColliderState(false);
        isAttacking = false;

        // 撃破された場合はここで処理を止める 
        if (isDead) yield break;

        // 連続踏みつけの残り回数を減らす
        stompChainRemaining--;
        Debug.Log("踏みつけ終了。残りチェイン: " + stompChainRemaining);

        if (stompChainRemaining > 0)
        {
            // まだ連続踏みつけが残っている → Riseしてからすぐ次の踏みつけへ
            currentState = State.Rising;
            StartCoroutine(RiseAndStompAgain());
        }
        else
        {
            isInStompChain = false;
            battleTimer = 0f; // 次の踏みつけまでのタイマーを再スタート
            nextStompTime = Random.Range(stompStartTimeMin, stompStartTimeMax);
            currentState = State.Rising;
            StartCoroutine(RiseSequence());
        }
    }

    // Riseした後すぐ次の踏みつけへ
    IEnumerator RiseAndStompAgain()
    {
        int animIndex = 0;
        float animTimer = 0f;

        while (transform.position.y < startPosition.y + maxFlyHeight)
        {
            // 撃破された場合はここで処理を止める 
            if (isDead) yield break;

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
        if (isDead) yield break;

        yield return StartCoroutine(ReturnToCenterX());

        if (isDead) yield break;

        // currentStateをFloatingにしない
        currentState = State.StompAttack;
        // Floatを挟まずすぐ次の踏みつけへ 
        StartCoroutine(StompAttackSequence());
    }

    // 現在の被弾段階での連続回数を取得
    int GetInitialChainCount()
    {
        int remainingHits = maxHitCount - currentHitCount;
        if (remainingHits == 2) return 2;
        if (remainingHits == 1) return 3;
        return 1;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        // 混乱アニメーション中はぽんたが死亡しない
        if (currentState == State.Hit) return;

        PlayerController playerController =
            collision.gameObject.GetComponent<PlayerController>();
        if (playerController == null) return;

        if (isStunned)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    // ぽんたを跳ね上げる
                    Rigidbody2D playerRb =
                        collision.gameObject.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        playerRb.linearVelocity = new Vector2(
                            playerRb.linearVelocity.x, 8f);
                    }
                    isStunned = false;

                    // 通常被弾の場合のみTakeDamageを呼ぶ
                    StopCoroutine(nameof(StompAttackSequence));
                    StartCoroutine(TakeDamage());
                    return;
                }
            }
        }

        if (!playerController.isDead && !playerController.isInvincible)
        {
            playerController.Die();
        }
    }

    IEnumerator DefeatSequence()
    {
        //isDead = true;
        //StopAllCoroutines();

        if (bodyCollider != null) bodyCollider.enabled = false;
        if (capsuleCollider != null) capsuleCollider.enabled = false;

        Debug.Log("もち天さま撃破！演出開始");

        // しょんぼり座り込みの位置に調整 
        Vector3 originalPos = transform.position;
        transform.position = new Vector3(
            originalPos.x + defeatSitOffset.x,
            originalPos.y + defeatSitOffset.y,
            originalPos.z);

        // しょんぼり座り込み1枚目
        if (defeatSitSprites.Length > 0)
        {
            spriteRenderer.sprite = defeatSitSprites[0];
            yield return new WaitForSeconds(defeatSitAnimInterval);
        }

        // しょんぼり座り込み2枚目
        if (defeatSitSprites.Length > 1)
        {
            spriteRenderer.sprite = defeatSitSprites[1];
            yield return new WaitForSeconds(defeatSitHoldTime);
        }

        // 昇天アニメーション、上昇しながら切り替え
        for (int i = 0; i < defeatAscendSprites.Length - 1; i++)
        {
            spriteRenderer.sprite = defeatAscendSprites[i];

            float elapsed = 0f;
            while (elapsed < defeatAscendAnimInterval)
            {
                transform.position += Vector3.up * defeatAscendSpeed * Time.deltaTime;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // 昇天アニメーション3枚目、上昇しながらフェードアウト
        if (defeatAscendSprites.Length > 2)
        {
            spriteRenderer.sprite = defeatAscendSprites[2];

            float fadeElapsed = 0f;
            Color startColor = spriteRenderer.color;

            while (fadeElapsed < defeatFadeDuration)
            {
                // 上昇を続ける
                transform.position += Vector3.up * defeatAscendSpeed * Time.deltaTime;

                // αを徐々に0にする
                float alpha = Mathf.Lerp(startColor.a, 0f, fadeElapsed / defeatFadeDuration);
                spriteRenderer.color = new Color(
                    startColor.r, startColor.g, startColor.b, alpha);

                fadeElapsed += Time.deltaTime;
                yield return null;
            }
        }

        // 完全に消える
        gameObject.SetActive(false);

        // 元のBGMに戻す
        if (GSound.Instance != null)
        {
            GSound.Instance.PlayBgm(SoundData.BgmType.Start.ToString(), true);
        }

        // 隠れていた地面を出現させる 
        if (postBossGroundReveal != null)
        {
            postBossGroundReveal.RevealGround();
        }

        Debug.Log("もち天さま完全に消滅");
    

}

    void OnDrawGizmosSelected()
    {
        if (colliderObject == null) return;

        CapsuleCollider2D col = colliderObject.GetComponent<CapsuleCollider2D>();
        if (col == null) return;

        Gizmos.color = Color.red;

        // 子オブジェクトのワールド座標を基準に描画
        Vector3 worldPos = colliderObject.position +
            new Vector3(col.offset.x, col.offset.y, 0f);

        Vector3 size = new Vector3(col.size.x, col.size.y, 0.1f);

        Gizmos.DrawWireCube(worldPos, size);
    }

    IEnumerator TakeDamage()
    {
        isStunned = false;
        currentHitCount++;

        Debug.Log("もち天さま被弾！ " + currentHitCount + "/" + maxHitCount);

        if (currentHitCount >= maxHitCount)
        {
            isDead = true;
            StartCoroutine(DefeatSequence());
            yield break;
        }

        currentState = State.Hit;

        // 怒りの色に変化
        float colorLerp = (float)currentHitCount / maxHitCount;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(Color.white, angryColorTint, colorLerp);
        }

        // Y座標を被弾アニメーション用にオフセット 
        Vector3 originalPos = transform.position;
        transform.position = new Vector3(
            originalPos.x, originalPos.y + hitSpriteOffsetY, originalPos.z);

        // 1,2枚目ループなしで1回再生
        if (hitSprites.Length > 0)
        {
            spriteRenderer.sprite = hitSprites[0];
            yield return new WaitForSeconds(hitAnimInterval);
        }
        if (hitSprites.Length > 1)
        {
            spriteRenderer.sprite = hitSprites[1];
            yield return new WaitForSeconds(hitAnimInterval);
        }

        // 3,4枚目ループする
        if (hitSprites.Length > 3)
        {
            float loopElapsed = 0f;
            int loopIndex = 2;

            while (loopElapsed < hitLoopDuration)
            {
                spriteRenderer.sprite = hitSprites[loopIndex];
                loopIndex = loopIndex == 2 ? 3 : 2;

                yield return new WaitForSeconds(hitLoopInterval); 
                loopElapsed += hitLoopInterval;
            }
        }

        // Y座標を元に戻す 
        transform.position = originalPos;

        // 攻撃を速くする
        ApplyAngryBoost();

        // タイマーをリセットしてFloatから再開 
        battleTimer = 0f;
        spikeTimer = 0f;
        nextStompTime = Random.Range(stompStartTimeMin, stompStartTimeMax);

        // 連続踏みつけのセットアップだけ行い、開始はしない
        int remainingHits = maxHitCount - currentHitCount;
        if (remainingHits == 2) stompChainRemaining = 2;
        else if (remainingHits <= 1) stompChainRemaining = 3;
        else stompChainRemaining = 1;

        isInStompChain = false; // まだ連続踏みつけは開始していない

        currentState = State.Rising;
        StartCoroutine(RiseSequence());
    }

    void ApplyAngryBoost()
    {
        int remainingHits = maxHitCount - currentHitCount;

        if (remainingHits == 2)
        {
            // HP2の状態
            riseSpeed = riseSpeedHP2;
            floatSpeed = floatSpeedHP2;
            spikeAttackInterval = spikeAttackIntervalHP2;
            spikeWarningTime = spikeWarningTimeHP2;
            spikeUpTime = spikeUpTimeHP2;
            stompStartTimeMin = stompStartTimeMinHP2;
            stompStartTimeMax = stompStartTimeMaxHP2;
            stompSpeed = stompSpeedHP2;

            if (groundManager != null)
            {
                groundManager.spikeActiveCount = spikeActiveCountHP2;
            }
        }
        else if (remainingHits <= 1)
        {
            // HP1の状態
            riseSpeed = riseSpeedHP1;
            floatSpeed = floatSpeedHP1;
            spikeAttackInterval = spikeAttackIntervalHP1;
            spikeWarningTime = spikeWarningTimeHP1;
            spikeUpTime = spikeUpTimeHP1;
            stompStartTimeMin = stompStartTimeMinHP1;
            stompStartTimeMax = stompStartTimeMaxHP1;
            stompSpeed = stompSpeedHP1;

            if (groundManager != null)
            {
                groundManager.spikeActiveCount = spikeActiveCountHP1;
            }
        }

        Debug.Log("もち天さま怒りモード！残りHIT: " + remainingHits);
    }

    public void ResetObject()
    {
        Debug.Log("MochiTenBoss ResetObject呼ばれた");

        StopAllCoroutines();

        isDead = false;
        isBattleStarted = false;
        isAttacking = false;
        isStunned = false;
        currentHitCount = 0;
        stompChainRemaining = 0;
        isInStompChain = false;
        battleTimer = 0f;
        spikeTimer = 0f;

        transform.position = startPosition;
        floatCenterPos = startPosition;

        currentState = State.Waiting;

        if (riseSprites.Length > 0)
            spriteRenderer.sprite = riseSprites[0];

        spriteRenderer.color = Color.white;

        if (bodyCollider != null) bodyCollider.enabled = true;
        SetColliderState(false);

        // ステータスをHP3に戻す
        riseSpeed = riseSpeedHP3;
        floatSpeed = floatSpeedHP3;
        spikeAttackInterval = spikeAttackIntervalHP3;
        spikeWarningTime = spikeWarningTimeHP3;
        spikeUpTime = spikeUpTimeHP3;
        stompStartTimeMin = stompStartTimeMinHP3;
        stompStartTimeMax = stompStartTimeMaxHP3;
        stompSpeed = stompSpeedHP3;

        if (groundManager != null)
        {
            groundManager.spikeActiveCount = spikeActiveCountHP3;
        }

        gameObject.SetActive(true);
    }

}