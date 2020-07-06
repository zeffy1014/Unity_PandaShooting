using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniRx;

// TODO:PlayerとEnemyをFlyerみたいなものにまとめてPlayerの移動などは継承して書きたい

public class PlayerController : MonoBehaviour, IGameEventReceiver
{
    // 固定値
    const float ANGLE_GAP = 90.0f;

    // 弾を撃つ際の設定
    float shotCycle = 0.2f; // 弾の連射間隔
    float waitShotTime = 0.0f; // 現在の連射待ち時間

    // スライド操作の設定
    float slideCycle = 3.0f; // 再操作できるまでの時間
    float waitSlideTime = 3.0f; // 現在の再操作までの待ち時間(初期値は操作可能状態)
    float waitCatchTime = 0.5f; // 弾を回収できるまでの経過時間(この間は弾に触れても回収しない)
    float bulletLifeTime; // 弾を放ってから消えるまでの時間
    float bulletElaspedTime; // 弾を放ってからの経過時間
    bool lostBullet = false; // 弾を失っているかどうか(失ってからカウント開始)
    public Button slideButton; // スライド操作用ボタン表現をこちらでいじる

    public float touchMoveSense = 25f; //タッチ操作による移動距離調整用感度

    public GameObject damageEffect;

    // ライフ関連 ReactivePropertyで監視できるようにする
    public int defaultLife = 5;
    private ReactiveProperty<int> _lifeReactiveProperty = new ReactiveProperty<int>(default);
    public IReadOnlyReactiveProperty<int> lifeReactiveProperty { get { return _lifeReactiveProperty; } }

    public bool Playing { get; set; } = false;

    public LifePanel lifePanel;

    public AudioClip shotSE;
    public AudioClip damageSE;
    AudioSource audioSource;

    public TouchController touchContoller;

    public Text posInfo;

    GameObject gameController;

    // 移動範囲制限のための範囲設定
    Rect borderRect = new Rect();       //画面範囲用の矩形
    static float borderRatioV = 0.95f; //画面端に対する調整率(水平)
    static float borderRatioH = 0.85f; //画面端に対する調整率(垂直)
    public GameObject leftWall;         //左側の壁
    public GameObject rightWall;        //右側の壁

    // 右クリック操作用
    Vector2 startRightClickPos = Vector2.zero;


    /***** IGameEventReceiverイベント処理 ****************************************************/
    // 魚が放たれた
    public void OnShotFish(float lifeTime)
    {
        bulletLifeTime = (float)(object)lifeTime;
        bulletElaspedTime = 0.0f;
    }

    // 魚を失った
    public void OnLostFish()
    {
        // 待ち開始
        lostBullet = true;
        waitSlideTime = 0;
    }

    // その他：空実装
    public void OnGameOver() { }
    public void OnBreakCombo() { }
    public void OnDefeatEnemy(EnemyType type) { }
    public void OnHouseDamage(int damage) { }

    /***** MonoBehaviourイベント処理 ****************************************************/
    private void Start()
    {
        // 画面範囲設定(動的に) 垂直方向はスクリーン上下、水平方向は左右の壁
        borderRect.yMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y * borderRatioH;
        borderRect.yMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).y * borderRatioH;
        float borderShrinkV = (rightWall.transform.position.x - leftWall.transform.position.x) * (1.0f - borderRatioV);
        borderRect.xMin = leftWall.transform.position.x + borderShrinkV;
        borderRect.xMax = rightWall.transform.position.x - borderShrinkV;

        /*
        Debug.Log("left-bottom:(" + borderRect.xMin.ToString("f1") + ", " + borderRect.yMin.ToString("f1") + "), " + 
                  "right-top:("   + borderRect.xMax.ToString("f1") + ", " + borderRect.yMax.ToString("f1") + ")");
         */

        _lifeReactiveProperty.Value = defaultLife;

        audioSource = GetComponent<AudioSource>();

        gameController = GameObject.FindWithTag("GameController");

        // ゲージを初期化 ボタン自身Image->子オブジェクトImageで[1]指定 微妙...
        slideButton.GetComponentsInChildren<Image>()[1].fillAmount = 0.0f;

        // イベント受信用登録
        EventHandlerExtention.AddListner(this.gameObject, SendEventType.OnShotFish);
        EventHandlerExtention.AddListner(this.gameObject, SendEventType.OnLostFish);

        // 状態監視(初期値は無視する) AddToで破棄も考慮
        GameStateProperty.valueReactiveProperty.DistinctUntilChanged().Skip(1).Subscribe(x => ChangeState(x)).AddTo(this);
    }

    // Update is called once per frame
    void Update()
    {
        // 待ち時間更新
        if (shotCycle > waitShotTime)
        {
            waitShotTime += Time.deltaTime;
        }

        if (slideCycle > waitSlideTime && lostBullet)
        {
            waitSlideTime += Time.deltaTime;
            // 待ち時間中はゲージ表示
            slideButton.GetComponentsInChildren<Image>()[1].fillAmount = waitSlideTime / slideCycle;
            if (slideCycle <= waitSlideTime)
            {
                // ボタンをトーンアップしてゲージ消去
                slideButton.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f);
                slideButton.GetComponentsInChildren<Image>()[1].fillAmount = 0.0f;
                lostBullet = false;
                bulletLifeTime = 0.0f;
            }
        }
        if (0.0f != bulletLifeTime)
        {
            bulletElaspedTime += Time.deltaTime;
            if (bulletElaspedTime < bulletLifeTime)
            {
                // 弾を出している間はゲージ表示
                slideButton.GetComponentsInChildren<Image>()[1].fillAmount = (bulletLifeTime - bulletElaspedTime) / bulletLifeTime;
            }
        }

        // Android or iPhoneだったらタッチによる移動操作検出
        // それ以外はマウスによる移動操作　ただしUnity Remoteは携帯端末扱いでタッチ操作
        if (PlatformInfo.IsMobile())
        {
            OnTouchOperation();
        }
        else
        {
            OnMouseOperation();
        }
    }

    /***** Collider2Dイベント処理 ****************************************************/
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 敵または敵弾と接触
        if ("Enemy" == other.gameObject.tag || "Bullet_Enemy" == other.gameObject.tag)
        {
            // エフェクトつける
            Instantiate(damageEffect, transform.position, Quaternion.identity);
            // 音も鳴らす
            audioSource.PlayOneShot(damageSE);

            Destroy(other.gameObject);

            _lifeReactiveProperty.Value--;
            lifePanel.UpdateLife(lifeReactiveProperty.Value);

            // コンボ切れる
            EventHandlerExtention.SendEvent(new BreakComboEventData());

            // ライフ無くなったらGameOver状態へ遷移 -> ライフをGameControllerが監視して処理する
            /*
            if (0 >= lifeReactiveProperty.Value)
            {
                if (false == SettingInfo.DebugMode)
                {
                    //GameStateProperty.SetState(GameState.GameOver);
                    EventHandlerExtention.SendEvent(new GameOverEventData());
                }
            }
            */

            // TODO:一定時間無敵にする

        }
        // 魚を回収
        if ("Bullet" == other.gameObject.tag && BulletKind.Player_Sakana == other.GetComponent<Bullet>().bulletKind)
        {
            // 一定時間経過していたら回収
            if (waitCatchTime < other.GetComponent<Bullet>().elaspedTime)
            {
                Destroy(other.gameObject);

                // 待ち時間ゼロ
                waitSlideTime = slideCycle;
                slideButton.GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f);
                slideButton.GetComponentsInChildren<Image>()[1].fillAmount = 0.0f;
                lostBullet = false;
                bulletLifeTime = 0.0f;
            }
        }
    }

    /***** PlayerController個別処理 ****************************************************/
    private void OnMouseOperation()
    {
        // マウス位置(スクリーン座標)をワールド座標へ変換
        Vector3 mousePosW = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, TouchController.distanceToCamera));
        //Debug.Log(Input.mousePosition + "->" + mousePosW);

        // 画面からは出さない範囲(補正あり)で自機移動
        if (borderRect.xMax < mousePosW.x) mousePosW.x = borderRect.xMax;
        if (borderRect.xMin > mousePosW.x) mousePosW.x = borderRect.xMin;
        if (borderRect.yMax < mousePosW.y) mousePosW.y = borderRect.yMax;
        if (borderRect.yMin > mousePosW.y) mousePosW.y = borderRect.yMin;

        transform.position = mousePosW;

        // Debug用位置情報表示
        DispPosInfo(Input.mousePosition, mousePosW);

        // プレイ中のみの動作
        if (GameState.Play == GameStateProperty.GetState())
        {
            // マウス左クリックで弾を出す
            if (Input.GetMouseButton(0))
            {
                ShotBullet(BulletKind.Player_Mikan, GetNowAngle());
            }
            // マウス右クリック押し→離しで弾を出す
            if (Input.GetMouseButtonDown(1))
            {
                startRightClickPos = Input.mousePosition;
                //Debug.Log("Right Click Start: " + startRightClickPos);
            }
            if (Input.GetMouseButtonUp(1))
            {
                if (Vector2.zero != startRightClickPos)
                {
                    Vector2 endRightClickPos = Input.mousePosition;
                    float diffX = endRightClickPos.x - startRightClickPos.x;
                    float diffY = endRightClickPos.y - startRightClickPos.y;
                    float angle;
                    if (diffX == 0.0f && diffY == 0.0f)
                    {
                        // 自機の向きに発射
                        angle = GetNowAngle();
                    }
                    else
                    {
                        angle = Mathf.Atan2(diffY, diffX) * Mathf.Rad2Deg;
                    }

                    //Debug.Log("Right Click End: " + endRightClickPos);
                    //Debug.Log("  diff:(" + diffX + "," + diffY + "), angle:" + angle);
                    // 発射
                    ShotBullet(BulletKind.Player_Sakana, angle);

                    // 初期化
                    startRightClickPos = Vector2.zero;
                }
            }
        }
    }

    private void OnTouchOperation()
    {
        // 移動用タッチの情報取得
        Touch touchPos = new Touch();
        if (true == touchContoller.GetMoveTouch(ref touchPos))
        {
            // 移動していたら移動量(感度用設定値を加味)を加算して移動先座標を得る
            if (TouchPhase.Moved == touchPos.phase)
            {
                // タッチによる移動量の変換(画面の縦のサイズを基準に割合をとる)
                Vector2 distRatio = new Vector2(touchPos.deltaPosition.x / Screen.height, touchPos.deltaPosition.y / Screen.height);
                //Debug.Log("distRatio:" + distRatio.x.ToString("f2") + "," + distRatio.y.ToString("f2"));

                // 割合に感度設定をかけ合わせて自機移動量を出す
                // Touch.deltaTimeで割って移動速度にしてからTime.deltaTimeをかけることで正式な移動量とする
                //Vector2 distW = new Vector2();
                //distW.x = distRatio.x * touchMoveSense * (Time.deltaTime / touchPos.deltaTime);
                //distW.y = distRatio.y * touchMoveSense * (Time.deltaTime / touchPos.deltaTime);
                Vector2 distW = distRatio * touchMoveSense * (Time.deltaTime / touchPos.deltaTime);
                //Debug.Log("width:" + borderRect.width + ", height:" + borderRect.height + ", Sense:" + touchMoveSense + ", distW:" + distW);

                Vector2 newPos = (Vector2)transform.position + distW;

                // 画面からは出さない範囲で自機移動
                if (borderRect.xMax < newPos.x) newPos.x = borderRect.xMax;
                if (borderRect.xMin > newPos.x) newPos.x = borderRect.xMin;
                if (borderRect.yMax < newPos.y) newPos.y = borderRect.yMax;
                if (borderRect.yMin > newPos.y) newPos.y = borderRect.yMin;

                transform.position = newPos;

                // Debug用位置情報表示
                DispPosInfo(touchPos.position, newPos);
            }
        }
        else
        {
            // 何もしない
        }

    }

    // Debug用位置情報表示
    void DispPosInfo(Vector2 input, Vector2 pos)
    {
        // 文字列生成
        posInfo.text = "Input:" + input.x.ToString("f1") + ", " + input.y.ToString("f1") + "\nPos:" + pos.x.ToString("f1") + ", " + pos.y.ToString("f1");

        // 文字列移動
        float sideOffset = 0.0f;  // 横方向にずらす距離
        float heightOffset = 150.0f;  // 縦方向にずらす距離
        Vector2 dispPos = input + new Vector2(sideOffset, heightOffset);

        // 画面からは出さない範囲で調整
        if (Screen.width < dispPos.x + posInfo.GetComponent<RectTransform>().rect.width / 2) dispPos.x = Screen.width - posInfo.GetComponent<RectTransform>().rect.width / 2;
        if (0.0f > dispPos.x - posInfo.GetComponent<RectTransform>().rect.width / 2) dispPos.x = posInfo.GetComponent<RectTransform>().rect.width / 2;
        if (Screen.height < dispPos.y + posInfo.GetComponent<RectTransform>().rect.height / 2) dispPos.y = Screen.height - posInfo.GetComponent<RectTransform>().rect.height / 2;
        if (0.0f > dispPos.y - posInfo.GetComponent<RectTransform>().rect.height / 2) dispPos.y = posInfo.GetComponent<RectTransform>().rect.height / 2;

        posInfo.transform.position = dispPos;
    }

    public void ShotBullet(BulletKind kind)
    {
        ShotBullet(kind, GetNowAngle());
    }

    public void ShotBullet(BulletKind kind, float angle)
    {
        // 発射位置と向き
        Vector3 genPos = transform.position;
        genPos.y += 1.0f;
        Vector3 genRot = transform.rotation.eulerAngles;

        switch (kind)
        {
            case BulletKind.Player_Mikan:
                // 連射待ち時間経過しているか、発射OKだったら放つ
                if (waitShotTime >= shotCycle || true == BulletController.BulletGo)
                {
                    BulletController.ShotBullet(genPos, genRot, BulletKind.Player_Mikan, angle);

                    // 待ち時間クリア
                    waitShotTime = 0.0f;
                }
                break;
            case BulletKind.Player_Sakana:
                if (waitSlideTime >= slideCycle)
                {
                    genRot.z += 360.0f * Random.value; // 向きを少しランダムにいじる

                    BulletController.ShotBullet(genPos, genRot, BulletKind.Player_Sakana, angle);

                    // ボタンをトーンダウンする
                    slideButton.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f);

                    // 待ち時間クリア
                    waitSlideTime = 0.0f;
                }
                break;
            default:
                break;
        }
    }

    // State変化時の各処理入り口
    public void ChangeState(GameState state)
    {
        switch (state)
        {
            case GameState.GameOver:
                EntryGameOver();
                break;
            case GameState.Play:
            case GameState.Ready:
            default:
                break;
        }

        return;
    }

    void EntryGameOver()
    {
        // エフェクトつける
        Instantiate(damageEffect, transform.position, Quaternion.identity);

        // ライフゼロになって強制終了
        _lifeReactiveProperty.Value = 0;
        lifePanel.UpdateLife(lifeReactiveProperty.Value);
        gameObject.SetActive(false);
    }

    // 現在の角度取得(頭が向いている方向)
    protected float GetNowAngle()
    {
        return ANGLE_GAP + transform.eulerAngles.z;
    }
}
