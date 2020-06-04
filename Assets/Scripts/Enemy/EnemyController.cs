using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO:敵の種類に応じて継承して挙動を変える

public enum EnemyType
{
    Fly,
    Mosquito,
    G,
    G_eggs,

    EnemyType_Num
};

// 入退場の状態
public enum EnemyState
{
    Enter,   // 入場
    Action,  // 戦闘
    Leave,   // 離脱

    EnemyState_Num
};

public class EnemyController : MonoBehaviour
{
    // 固定値
    const float ANGLE_GAP = 90.0f;

    // 共通で必要に応じて参照するもの
    static GameObject player = null;

    public EnemyType enemyType;

    // 移動速度
    public float moveSpeedBase;
    protected float moveSpeed;
    // 旋回速度
    public float rotateSpeedBase;
    protected float rotateSpeed;
    // HP
    public int hpBase;
    protected int hp;

    // 状態
    protected EnemyState state;

    // 出現からの経過時間
    protected float elaspedActionTime = 0.0f;

    // Generatorから渡される情報
    int id;                 // ユニークなID
    Vector2 appearPoint;    // 出現位置(画面外からの入場の場合はこの位置目指して入ってくる)
    float activityTime;     // 活動時間(この時間経過で離脱開始)
    EnemyExitType exitType; // 離脱パターン

    // 移動できる画面範囲
    protected Rect gameArea;

    // Destory時に呼んでほしいCallback
    public event Action destroyCallback = null;

    //public Sprite enemySprite;
    //SpriteRenderer mainSpriteRenderer;

    // ダメージ時のエフェクトと音(共通設定)
    static AudioClip damageSE;         // ダメージの音
    public Material damageMaterial;    // ダメージ中にスプライトに適用するマテリアル(白くするため)
    public Material normalMaterial;    // 通常時のマテリアル(もとに戻すため)
    static float flashTime = 0.1f; // ダメージ表示する時間

    // 撃破時のエフェクトと音(一応個別設定)
    public GameObject defeatEffect;
    public AudioClip defeatSE;

    static GameObject tempObject = null;
    static AudioSource audioSource = null;


    /***** MonoBehaviourイベント処理 ****************************************************/
    protected void Start()
    {
        // 各種初期化
        // 自機取得
        if (null == player) player = GameObject.FindWithTag("Player");

        // とりあえず背景スクロールスピードを加算する(移動ゼロの敵は背景と一緒にスクロールする)
        moveSpeed = moveSpeedBase + BGController.scrollSpeed;
        // もともと速度があるものはランダムで上乗せ(1.0-1.5倍)
        moveSpeed += (moveSpeed - BGController.scrollSpeed) * UnityEngine.Random.Range(0.0f, 0.5f);

        // 旋回速度は特にいじらない
        rotateSpeed = rotateSpeedBase;

        // HPもとりあえずそのまま
        hp = hpBase;

        // 初期状態は入場
        state = EnemyState.Enter;
        // そして当たり判定は入場中はOFF
        this.GetComponent<CapsuleCollider2D>().enabled = false;

    }

    void Update()
    {
        // 共通動作:状態に応じた関数を呼ぶ
        switch(state)
        {
            case EnemyState.Enter:
                UpdateEnter();
                break;
            case EnemyState.Action:
                elaspedActionTime += Time.deltaTime;
                if (activityTime > elaspedActionTime)
                {
                    // 活動時間内だったら各種Action
                    UpdateAction();
                }
                else
                {
                    // 活動時間に達していたら離脱状態へ移行
                    OnLeave();
                }
                break;
            case EnemyState.Leave:
                UpdateLeave();
                break;
            default:
                break;
        }
    }

    // 各種状態ごとのUpdate処理
    void UpdateEnter()
    {
        // 入場処理は共通: 出現位置まで移動
        if (true == Move2Target(appearPoint, moveSpeed, rotateSpeed))
        {
            // 移動完了したら真下を向く
            if (true == Rotate2TargetAngle(180.0f + ANGLE_GAP, rotateSpeed))
            {
                // 当たり判定を有効にして戦闘状態へ移行
                this.GetComponent<CapsuleCollider2D>().enabled = true;
                OnAction();
            }
        }

        return;
    }

    virtual protected void UpdateAction()
    {
        // 戦闘処理は派生先にて実装
    }

    void UpdateLeave()
    {
        float leaveAngle = default;

        // 退場処理は共通: 決まった方向へ離脱またはその場で消える
        switch(exitType)
        {
            case EnemyExitType.Spot:        // その場で消える
                // TODO:離脱エフェクトほしい
                Destroy(this.gameObject);
                return;
            case EnemyExitType.To_Left:     // 画面左へ離脱
                leaveAngle = 90.0f;
                break;
            case EnemyExitType.To_Right:    // 画面右へ離脱
                leaveAngle = -90.0f;
                break;
            case EnemyExitType.To_Top:      // 画面上へ離脱
                leaveAngle = 0.0f;
                break;
            case EnemyExitType.To_Buttom:   // 画面下へ離脱(defaultもここに振る)
            default:
                leaveAngle = 180.0f;
                break;
        }

        // 離脱する方向へ回転してから直進して消えていく
        if (true == Rotate2TargetAngle(leaveAngle + ANGLE_GAP, rotateSpeed))
        {
            GoStraight(moveSpeed);
        }

        return;
    }

    /***** Enemy共通処理 ****************************************************/
    // 生成情報を渡す
    public void SetGenerateInfo(int id, Vector2 appearPoint, float activityTime, EnemyExitType exitType)
    {
        // 各種値の正当性は渡す側で保証される前程で格納
        this.id = id;
        this.appearPoint = appearPoint;
        this.activityTime = activityTime;
        this.exitType = exitType;

        return;
    }

    // 画面範囲を渡す(生成時にセット)
    public void SetGameArea(Rect area)
    {
        gameArea = area;
    }

    // Destory時のCallback登録
    public void AddDestroyCallback(Action callBack)
    {
        destroyCallback = callBack;
    }

    // Object削除時
    public void OnDestroy()
    {
        if (null != destroyCallback) destroyCallback();
    }

    // ダメージ
    public void OnDamage(int bulletAtk)
    {
        hp -= bulletAtk;

        // AudioSource読み込み
        if (null == tempObject)
        {
            tempObject = new GameObject("EnemyController_Temp");
            GameObject.DontDestroyOnLoad(tempObject);
            audioSource = tempObject.AddComponent<AudioSource>();
        }

        if (0 >= hp)
        {
            // 撃破
            audioSource.PlayOneShot(defeatSE);
            Instantiate<GameObject>(defeatEffect, transform.position, Quaternion.identity);

            // 敵を倒した通知
            EventHandlerExtention.SendEvent(new DefeatEnemyEventData(enemyType));
            //gameController.SendMessage("IncreaseScore", SendMessageOptions.DontRequireReceiver);

            Destroy(this.gameObject);
        }
        else
        {
            // ダメージ
            //audioSource.PlayOneShot(damageSE);
            StartCoroutine(Flash(flashTime));
        }

    }

    // ちょっと光る
    IEnumerator Flash(float time)
    {
        this.gameObject.GetComponent<SpriteRenderer>().material = damageMaterial;
        yield return new WaitForSeconds(time);
        this.gameObject.GetComponent<SpriteRenderer>().material = normalMaterial;
    }


    /***** 移動・回転処理 ****************************************************/
    // ある位置に移動する(回転→移動→到着 完了したらtrueを返す)
    protected bool Move2Target(Vector2 targetPos, float moveSpeed, float rotateSpeed)
    {
        bool ret = false;

        // 既に目標位置にいたら終了
        if ((Vector2)transform.position == targetPos) return true;

        // まず目標位置に向く
        Vector2 posDiff = targetPos - (Vector2)this.transform.position;
        float targetAngle = Mathf.Atan2(posDiff.y, posDiff.x) * Mathf.Rad2Deg;
        if (true == Rotate2TargetAngle(targetAngle, rotateSpeed))
        {
            // 回転完了していたら移動(平行移動)
            if (true == Slide2TargetPos(targetPos, moveSpeed))
            {
                // 移動も完了していたらtrueを返す
                ret = true;
            }
        }

        return ret;
    }

    // ある角度に向かって回転する(回転終了したらtrueを返す)
    protected bool Rotate2TargetAngle(float targetAngle, float rotateSpeed)
    {
        bool ret = false;

        // 目標との角度差分(Deg)
        float angleDiff = Mathf.DeltaAngle(GetNowAngle(), targetAngle);

        /*
        // now->target が一定範囲内(5度以内)だったら回転しない
        if (5.0f > Mathf.Abs(angleDiff))
        {
            ret = true;
        }
        */

        // now->target がプラスだったら左回り(プラス回転)
        if (0.0f < angleDiff)
        {
            transform.Rotate(0.0f, 0.0f, rotateSpeed * Time.deltaTime);

            // now->target が0かマイナスに振れるようだったらtargetに合わせて回転終了
            angleDiff = Mathf.DeltaAngle(GetNowAngle(), targetAngle);
            if (0.0f >= angleDiff)
            {
                SetNowAngle(targetAngle);
                ret = true;
            }
        }
        // now->target がマイナスだったら右回り(マイナス回転)
        else
        {
            transform.Rotate(0.0f, 0.0f, -rotateSpeed * Time.deltaTime);

            // now->target が0かプラスに振れるようだったらtargetに合わせて回転終了
            angleDiff = Mathf.DeltaAngle(GetNowAngle(), targetAngle);
            if (0.0f <= angleDiff)
            {
                SetNowAngle(targetAngle);
                ret = true;
            }
        }

        return ret;
    }

    // 自機に向かって回転する
    protected void Rotate2Player(float rotateSpeed)
    {
        Vector2 posDiff = player.transform.position - this.transform.position;
        float targetAngle = Mathf.Atan2(posDiff.y, posDiff.x) * Mathf.Rad2Deg;
        Rotate2TargetAngle(targetAngle, rotateSpeed);

        return;
    }

    // まっすぐ進む
    protected void GoStraight(float speed)
    {
        transform.Translate(new Vector2(Mathf.Cos(GetNowAngle() * Mathf.Deg2Rad), Mathf.Sin(GetNowAngle() * Mathf.Deg2Rad)) * speed * Time.deltaTime, Space.World);
        return;
    }

    // 特定の位置に向かって平行移動する(移動完了したらtrueを返す)
    protected bool Slide2TargetPos(Vector2 targetPos, float moveSpeed)
    {
        bool ret = false;

        // 2点間の距離
        float posDist = Vector2.Distance(targetPos, this.transform.position);
        // このターンの移動量
        float moveDist = moveSpeed * Time.deltaTime;

        if (moveDist >= posDist)
        {
            // 移動量の方が距離と同じまたは大きい場合は目標位置に移動して完了
            transform.position = targetPos;
            ret = true;
        }
        else
        {
            // そうでない場合は一定距離移動して終了
            Vector2 posDiff = targetPos - (Vector2)this.transform.position;
            float targetAngle = Mathf.Atan2(posDiff.y, posDiff.x);
            transform.Translate(new Vector2(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle)) * moveSpeed * Time.deltaTime, Space.World);
        }

        return ret;

    }

    // 現在の角度取得(頭が向いている方向)
    protected float GetNowAngle()
    {
        return ANGLE_GAP + transform.eulerAngles.z;
    }

    // 現在の角度指定(頭が向いている方向)
    protected void SetNowAngle(float angle)
    {
        Vector3 setAngle = transform.eulerAngles;
        setAngle.z = angle - ANGLE_GAP;
        transform.eulerAngles = setAngle;

        return;
    }

    /***** 弾を出す処理 ****************************************************/
    // 指定された角度で弾を撃つ
    protected void ShotBullet(BulletKind kind, float deg, float speed)
    {
        // 発射位置と向き
        Vector3 genPos = this.transform.position;
        Vector3 genRot = this.transform.eulerAngles;

        BulletController.ShotBullet(genPos, genRot, kind, deg, speed);
    }

    // 自機に向かって弾を撃つ
    protected void ShotBullet2Player(BulletKind kind = BulletKind.Enemy_Point, float speed = -1.0f)
    {
        // 自機に対する角度を算出
        Vector2 posDiff = player.transform.position - this.transform.position;
        float targetAngle = Mathf.Atan2(posDiff.y, posDiff.x) * Mathf.Rad2Deg;

        ShotBullet(kind, targetAngle, speed);

        return;
    }

    // 複数弾(奇数弾・偶数弾)を撃つ
    // 弾の数と1つ1つの角度(デフォルト:30度だが弾数が多い場合は重ならないように調整が入る)を指定
    protected void ShotMultipleBullet(int bulletNum, float angle = 30.0f, BulletKind kind = BulletKind.Enemy_Point, float speed = -1.0f)
    {
        // 自機に対する角度を算出
        Vector2 posDiff = player.transform.position - this.transform.position;
        float targetAngle = Mathf.Atan2(posDiff.y, posDiff.x) * Mathf.Rad2Deg;

        // 必要ならば角度調整
        if (360.0f < bulletNum * angle) angle = 360.0f / bulletNum;

        // 最初の弾の角度
        float startAngle = targetAngle - angle * (bulletNum / 2);
        if (0 == bulletNum % 2)
        {
            // 偶数弾だったら半分ずらす
            startAngle -= (angle / 2);
        }

        for (int i=0; i<bulletNum; i++)
        {
            ShotBullet(kind, startAngle + (angle * i), speed);
        }

        return;
    }





    /***** 各種状態遷移処理 ****************************************************/
    virtual protected void OnAction()
    {
        state = EnemyState.Action;
        elaspedActionTime = 0.0f;
        return;
    }

    protected void OnLeave()
    {
        state = EnemyState.Leave;
        return;
    }
}
