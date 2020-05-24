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

public class EnemyController : MonoBehaviour
{
    // 共通で必要に応じて参照するもの
    static GameObject player = null;

    public EnemyType enemyType;

    // 移動速度
    public float moveSpeedBase;
    float moveSpeed;

    // 旋回速度
    public float rotateSpeedBase;
    float rotateSpeed;

    // HP
    public int hpBase;
    int hp;

    // 移動できる画面範囲
    Rect gameArea;

    // G専用 壁際から回転する際の変数
    bool searchTarget = false;
    float targetAngle;
    struct nearWall
    {
        public bool N;
        public bool E;
        public bool W;
        public bool S;
    }
    nearWall nearWallCheck;

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
    void Start()
    {
        // 各種初期化
        // 自機取得
        if (null == player) player = GameObject.FindWithTag("Player");

        // とりあえず背景スクロールスピードを加算する(移動ゼロの敵は背景と一緒にスクロールする)
        moveSpeed = moveSpeedBase + BGController.scrollSpeed;
        // もともと速度があるものはランダムで上乗せ(1.0-1.5倍)
        moveSpeed += (moveSpeed - BGController.scrollSpeed) * Random.Range(0.0f, 0.5f);

        // 旋回速度は特にいじらない
        rotateSpeed = rotateSpeedBase;

        hp = hpBase;

        if (EnemyType.G == this.enemyType)
        {
            this.nearWallCheck.N = false;
            this.nearWallCheck.E = false;
            this.nearWallCheck.W = false;
            this.nearWallCheck.S = false;
        }
    }

    void Update()
    {
        // 種別ごとにやることが違う とりあえず書いていく
        switch (this.enemyType)
        {
            case EnemyType.Fly:
                // ハエはまっすぐ進みながら弾を撃ってくる
                GoStraight(this.moveSpeed);
                break;
            case EnemyType.G:
                // ゴキブリはランダムに向きを変えて直進　を繰り返す
                // 壁接近情報更新しつつ接近検出したらtrue
                if (false == searchTarget && true == IsApproachingWall(10.0f))
                {
                    searchTarget = true;
                    // 次に向かう角度(壁に対して離れる方向)を決める
                    float freeAngleMax = default;
                    float freeAngleMin = default;
                    GetFreeAngle(ref freeAngleMax, ref freeAngleMin, 5.0f);

                    // 既に壁から離れる方向を向いていたらそのまま(入場時を考慮)
                    if (freeAngleMax >= GetNowAngle() && freeAngleMin <= GetNowAngle())
                    {
                        searchTarget = false;
                    }
                    else
                    {
                        // 次に向かう角度(壁に対して離れる方向)を決める
                        targetAngle = Random.Range(freeAngleMin, freeAngleMax);
                    }
                }
                else if (true == searchTarget)
                {
                    // 目標角度に向かって回転
                    bool needRotate = Rotate2TargetAngle(this.targetAngle, this.rotateSpeed);
                    // もう回転しなくてよければ状態変更
                    if (false == needRotate) searchTarget = false;
                }
                else
                {
                    // 向いている方向に進む
                    GoStraight(this.moveSpeed);
                }
                break;
            case EnemyType.Mosquito:
                // 蚊は自機に近づいてくる
                Rotate2Player(this.rotateSpeed);
                GoStraight(this.moveSpeed);
                break;
            case EnemyType.G_eggs:
                // 特に何もしない(背景に合わせてスクロールのみ、今はまだ…)
                GoStraight(this.moveSpeed);
                break;
            default:
                break;
        }

    }

    /***** Enemy個別処理 ****************************************************/
    // 画面範囲を渡す(生成時にセット)
    public void SetGameArea(Rect area)
    {
        gameArea = area;
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

    // ある角度に向かって回転する(回転不要だったらfalseを返す)
    bool Rotate2TargetAngle(float targetAngle, float rotateSpeed)
    {
        bool ret = true;

        // 目標との角度差分(Deg)
        float angleDiff = Mathf.DeltaAngle(GetNowAngle(), targetAngle);

        // now->target が一定範囲内(5度以内)だったら回転しない
        if (5.0f > Mathf.Abs(angleDiff))
        {
            ret = false;
        }
        // now->target がプラスだったら左回り(プラス回転)
        else if (0.0f < angleDiff)
        {
            transform.Rotate(0.0f, 0.0f, rotateSpeed * Time.deltaTime);
        }
        // now->target がマイナスだったら右回り(マイナス回転)
        else
        {
            transform.Rotate(0.0f, 0.0f, -rotateSpeed * Time.deltaTime);
        }

        return ret;
    }

    // 自機に向かって回転する
    void Rotate2Player(float rotateSpeed)
    {
        Vector2 posDiff = player.transform.position - this.transform.position;
        float targetAngle = Mathf.Atan2(posDiff.y, posDiff.x) * Mathf.Rad2Deg;
        Rotate2TargetAngle(targetAngle, rotateSpeed);

        return;
    }

    // まっすぐ進む
    void GoStraight(float speed)
    {
        transform.Translate(new Vector2(Mathf.Cos(GetNowAngle() * Mathf.Deg2Rad), Mathf.Sin(GetNowAngle() * Mathf.Deg2Rad)) * speed * Time.deltaTime, Space.World);
        return;
    }

    // 現在の角度取得(頭が向いている方向)
    float GetNowAngle()
    {
        return 90.0f + transform.rotation.eulerAngles.z;
    }

    // 現在地が壁に接近したか確認・接近情報更新
    // 引数: 壁を0とした場合に画面幅・高さの何%の範囲まで接近していたら検出するか
    bool IsApproachingWall(float checkRate)
    {
        bool ret = false;

        // 引数範囲チェック
        if (0 > checkRate || 100 < checkRate)
        {
            // 不正入力
            return ret;
        }

        checkRate /= 100;
        float posX = this.transform.position.x;
        float posY = this.transform.position.y;

        // North接近確認
        if (gameArea.yMax > posY && (gameArea.yMax - gameArea.height * checkRate) <= posY)
        {
            if(!this.nearWallCheck.N)
            {
                this.nearWallCheck.N = true;
                ret = true;
            }
        }
        else if((gameArea.yMax - gameArea.height * checkRate) > posY) this.nearWallCheck.N = false;

        // East接近確認
        if (gameArea.xMax > posX && (gameArea.xMax - gameArea.width * checkRate) <= posX)
        {
            if (!this.nearWallCheck.E)
            {
                this.nearWallCheck.E = true;
                ret = true;
            }
        }
        else if ((gameArea.xMax - gameArea.width * checkRate) > posX) this.nearWallCheck.E = false;

        // West接近確認
        if (gameArea.xMin < posX && (gameArea.xMin + gameArea.width * checkRate) >= posX)
        {
            if (!this.nearWallCheck.W)
            {
                this.nearWallCheck.W = true;
                ret = true;
            }
        }
        else if ((gameArea.xMin + gameArea.width * checkRate) < posX) this.nearWallCheck.W = false;

        // South接近確認
        if (gameArea.yMin < posY && (gameArea.yMin + gameArea.height * checkRate) >= posY)
        {
            if (!this.nearWallCheck.S)
            {
                this.nearWallCheck.S = true;
                ret = true;
            }
        }
        else if ((gameArea.yMin + gameArea.height * checkRate) < posY) this.nearWallCheck.S = false;

        return ret;
    }

    // 壁から離れる角度を取得
    // 返却用の最大最小角度と、遊びを持たせるために狭める角度(play)
    void GetFreeAngle(ref float angleMax, ref float angleMin, float play)
    {
        angleMax = 360.0f;
        angleMin = 0.0f;

        // どの壁に近い状態かで第1～第4象限で進める方向を決める
        bool quadrant1 = true; //0-90deg
        bool quadrant2 = true; //90-180deg
        bool quadrant3 = true; //180-270deg
        bool quadrant4 = true; //270-360deg

        if (this.nearWallCheck.N) quadrant1 = quadrant2 = false;
        if (this.nearWallCheck.E) quadrant1 = quadrant4 = false;
        if (this.nearWallCheck.W) quadrant2 = quadrant3 = false;
        if (this.nearWallCheck.S) quadrant3 = quadrant4 = false;

        // パターンとしては8種類
        if (true == quadrant1)
        {
            if (true == quadrant2)
            {
                angleMin = 0.0f;
                angleMax = 180.0f;
            }
            else if (true == quadrant4)
            {
                angleMin = -90.0f;
                angleMax = 90.0f;
            }
            else
            {
                angleMin = 0.0f;
                angleMax = 90.0f;
            }
        }
        else if (true == quadrant2)
        {
            if (true == quadrant3)
            {
                angleMin = 90.0f;
                angleMax = 270.0f;
            }
            else
            {
                angleMin = 90.0f;
                angleMax = 180.0f;
            }
        }
        else if (true == quadrant3)
        {
            if (true == quadrant4)
            {
                angleMin = 180.0f;
                angleMax = 360.0f;
            }
            else
            {
                angleMin = 180.0f;
                angleMax = 270.0f;
            }
        }
        else if (true == quadrant4)
        {
            angleMin = 270.0f;
            angleMax = 360.0f;
        }
        else
        {
            // 壁から離れるのは上記8パターン以外はない　あとは全方向フリーで
        }

        // 遊びを考慮して範囲を少しせばめる
        angleMin += play;
        angleMax -= play;

        return;
    }
}
