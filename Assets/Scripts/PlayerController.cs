using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// TODO:PlayerとEnemyをFlyerみたいなものにまとめてPlayerの移動などは継承して書きたい

public class PlayerController : MonoBehaviour
{
    // 弾を撃つ際の設定
    static float shotCycle = 0.2f; // 弾の連射間隔
    static float waitShotTime = 0.0f; // 現在の連射待ち時間

    public float touchMoveSense = 25f; //タッチ操作による移動距離調整用感度

    public GameObject damageEffect;

    public int defaultLife = 5;
    public int Life { get; set; }
    public bool Playing { get; set; } = false;
    public GameState State { get; set; } = GameState.None;

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

        Life = defaultLife;
        audioSource = GetComponent<AudioSource>();

        gameController = GameObject.FindWithTag("GameController");

    }

    // Update is called once per frame
    void Update()
    {
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
        if (GameState.Play == State)
        {
            // マウス左クリックで弾を出す
            if (Input.GetMouseButton(0))
            {
                ShotBullet();
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

    public void ShotBullet()
    {
        // 連射待ち時間経過しているか、発射OKだったら放つ
        if (waitShotTime >= shotCycle || true == BulletController.BulletGo)
        {
            Vector3 genPos = transform.position;
            genPos.y += 1.0f;
            Vector3 genRot = transform.rotation.eulerAngles;
            //genRot.z += 360.0f * Random.value;

            BulletController.ShotBullet(genPos, genRot, BulletKind.Player_Mikan);

            // 待ち時間クリア
            waitShotTime = 0.0f;
        }
        else
        {
            // 今回は発射せず待ち時間を増加
            waitShotTime += Time.deltaTime;
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ("Enemy" == other.gameObject.tag)
        {
            // エフェクトつける
            Instantiate(damageEffect, transform.position, Quaternion.identity);
            // 音も鳴らす
            audioSource.PlayOneShot(damageSE);

            Destroy(other.gameObject);

            Life--;
            lifePanel.UpdateLife(Life);

            // コンボ切れる
            ExecuteEvents.Execute<IGameEventReceiver>(
                target: gameController,
                eventData: null,
                functor: (receiver, eventData) => receiver.OnBreakCombo()
            );

            // ライフ無くなったらGameOver処理してもらう
            if (0 >= Life)
            {
                ExecuteEvents.Execute<IGameEventReceiver>(
                    target: gameController,
                    eventData: null,
                    functor: (receiver, eventData) => receiver.OnGameOver()
                );
            }
        }
    }

    public void ForceGameOver()
    {
        // エフェクトつける
        Instantiate(damageEffect, transform.position, Quaternion.identity);

        // ライフゼロになって強制終了
        Life = 0;
        lifePanel.UpdateLife(Life);
        gameObject.SetActive(false);
    }
}
