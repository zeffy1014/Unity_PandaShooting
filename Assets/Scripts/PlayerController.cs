using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public float touchMoveSense = 5.0f; //タッチ操作による移動距離調整用感度

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

    GameObject gameController;

    // 移動範囲制限のための決め打ち画面範囲
    float screenWidth = 5.0f;
    float screenHeight = 10.0f;

    private void Start()
    {
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

        // 画面からは出さない範囲で自機移動
        if (screenWidth / 2 < mousePosW.x) mousePosW.x = screenWidth / 2;
        if (-screenWidth / 2 > mousePosW.x) mousePosW.x = -screenWidth / 2;
        if (screenHeight / 2 < mousePosW.y) mousePosW.y = screenHeight / 2;
        if (-screenHeight / 2 > mousePosW.y) mousePosW.y = -screenHeight / 2;

        transform.position = mousePosW;

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
                Vector2 newPos = new Vector2();
                newPos.x = touchPos.deltaPosition.x / touchMoveSense * Time.deltaTime + transform.position.x;
                newPos.y = touchPos.deltaPosition.y / touchMoveSense * Time.deltaTime + transform.position.y;

                //Debug.Log(transform.position + "->" + newPos);

                // 画面からは出さない範囲で自機移動
                if (screenWidth / 2 < newPos.x) newPos.x = screenWidth / 2;
                if (-screenWidth / 2 > newPos.x) newPos.x = -screenWidth / 2;
                if (screenHeight / 2 < newPos.y) newPos.y = screenHeight / 2;
                if (-screenHeight / 2 > newPos.y) newPos.y = -screenHeight / 2;

                transform.position = newPos;
            }
        }
        else
        {
            // 何もしない
        }

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
