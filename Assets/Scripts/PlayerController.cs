using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float touchMoveSense = 2.5f; //タッチ操作による移動距離調整用感度

    public GameObject bullet;
    public GameObject damageEffect;
    public int defaultLife = 3;

    public int Life { get; set; }
    public bool Playing { get; set; } = false;
    public GameState.State State { get; set; } = GameState.State.None;

    public LifePanel lifePanel;

    public AudioClip shotSE;
    public AudioClip damageSE;
    AudioSource audioSource;

    public TouchController touchContoller;

    public Text posInfo;

    GameObject gameController;

    // 移動範囲制限のための画面範囲
    static float borderRatio = 0.95f;  //画面端に対する調整率
    Rect borderRect = new Rect();  //画面範囲用の矩形

    private void Start()
    {
        // 画面範囲設定(動的に)
        borderRect.xMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x * borderRatio;
        borderRect.yMin = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y * borderRatio;
        borderRect.xMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).x * borderRatio;
        borderRect.yMax = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).y * borderRatio;

        Debug.Log("left-bottom:(" + borderRect.xMin.ToString("f1") + ", " + borderRect.yMax.ToString("f1") + "), " + 
                  "right-top:("   + borderRect.xMax.ToString("f1") + ", " + borderRect.yMax.ToString("f1") + ")");

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
        if (GameState.State.Play == State)
        {
            // マウス左クリックで弾を出す
            if (Input.GetMouseButtonDown(0))
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
                // タッチによる移動量スクリーン幅・高さで割って0～1の割合にする
                Vector2 distRatio = new Vector2(touchPos.deltaPosition.x / Screen.width, touchPos.deltaPosition.y / Screen.height);
                Debug.Log("distRatio:" + distRatio.x.ToString("f2") + "," + distRatio.y.ToString("f2"));

                // 割合を移動範囲と感度設定とでかけ合わせて自機移動量を出す
                // Touch.deltaTimeで割って移動速度にしてからTime.deltaTimeをかけることで正式な移動量とする
                Vector2 distW = new Vector2();
                distW.x = distRatio.x * borderRect.width * touchMoveSense * (Time.deltaTime / touchPos.deltaTime);
                distW.y = distRatio.y * borderRect.height* touchMoveSense * (Time.deltaTime / touchPos.deltaTime);
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
        Vector3 genPos = transform.position;
        genPos.y += 1.0f;
        Vector3 genRot = transform.rotation.eulerAngles;
        genRot.z += 360.0f * Random.value;

        // 弾生成
        Instantiate<GameObject>(bullet, genPos, Quaternion.Euler(genRot));
        // 音も出す
        audioSource.PlayOneShot(shotSE);

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
            gameController.SendMessage("BreakCombo", SendMessageOptions.DontRequireReceiver);

            // ライフ無くなったらGameOver処理してもらう
            if (0 >= Life) gameController.SendMessage("GameIsOver", SendMessageOptions.DontRequireReceiver);
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
