using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class GameController : MonoBehaviour, IGameEventReceiver
{
    const float DisplayGoSec = 1.5f;
    const float CountDownSpan = 0.1f;

    public int baseScore;
    public int bonusScore;

    public PlayerController player;
    public EnemyGenerator generator;
    public HouseGauge house;

    public Text scoreLabel;
    public Text comboLabel;
    public Text stateLabel;
    public Button shotButton;
    public Button slideButton;
    public Button retryButton;
    public Button titleButton;
    public Text posInfo;

    int score = 0;
    int combo = 0;
    float counter = DisplayGoSec;

    public AudioClip gamestartSE;
    public AudioClip gameoverSE;
    public AudioClip defeatSE;
    AudioSource audioSource;

    private bool isMobile = false;  // モバイル環境かどうか

    /***** IGameEventReceiverイベント処理 ****************************************************/
    // コンボ切れ
    public void OnBreakCombo()
    {
        combo = 0;
        comboLabel.text = "Combo : " + combo;
    }

    // ゲームオーバー…
    public void OnGameOver()
    {
        // Debug Mode中でなければゲームオーバー
        //if (!debugMode.isOn) if (GameState.Play == state) player.ForceGameOver();
    }

    // 敵を撃破
    public void OnDefeatEnemy(EnemyType type)
    {
        // TODO:敵の種類からスコアを出す
        int increasScore = 100;
        IncreaseScore(increasScore);
    }

    // その他：空実装
    public void OnHouseDamage(int damage) { }
    public void OnShotFish(float lifeTime) { }
    public void OnLostFish() { }

    /***** MonoBehaviourイベント処理 ****************************************************/
    void Start()
    {
        // 状態監視(初期値は無視する) AddToで破棄も考慮
        GameStateProperty.valueReactiveProperty.DistinctUntilChanged().Skip(1).Subscribe(x => ChangeState(x)).AddTo(this);
        player.lifeReactiveProperty.DistinctUntilChanged().Skip(1).Subscribe(x => ChangePlayerLife(x)).AddTo(this);
        house.lifeReactiveProperty.DistinctUntilChanged().Skip(1).Subscribe(x => ChangeHouseLife(x)).AddTo(this);

        // 開始時はReady状態
        GameStateProperty.SetState(GameState.Ready);

        audioSource = GetComponent<AudioSource>();

        // イベント受信登録
        EventHandlerExtention.AddListner(this.gameObject, SendEventType.OnGameOver);
        EventHandlerExtention.AddListner(this.gameObject, SendEventType.OnBreakCombo);
        EventHandlerExtention.AddListner(this.gameObject, SendEventType.OnDefeatEnemy);

        // ハイスコア無かったら0で作成
        if (!PlayerPrefs.HasKey("HighScore")) PlayerPrefs.SetInt("HighScore", 0);

        // カーソルを出さないようにする
        Cursor.lockState = CursorLockMode.Confined;

        // Debug用位置情報はデバッグモードの場合のみ表示
        posInfo.gameObject.SetActive(SettingInfo.DebugMode);

        // 操作ボタンはモバイル環境だけ有効にする
        // TODO:しばらくは常にON
        // isMobile = PlatformInfo.IsMobile();
        isMobile = true;
        shotButton.gameObject.SetActive(isMobile);
        slideButton.gameObject.SetActive(isMobile);
    }

    private void Update()
    {
        // Unity Remoteは起動直後に接続確認できないので再確認
        if (Application.isEditor && !isMobile)
        {
            isMobile = PlatformInfo.IsMobile();
            shotButton.gameObject.SetActive(isMobile);
            slideButton.gameObject.SetActive(isMobile);
        }
    }

    void LateUpdate()
    {
        // 状態ごとにイベント監視
        switch (GameStateProperty.GetState())
        {
            case GameState.Ready:
                // ラベルを点滅
                Color color = stateLabel.color;
                color.a = Mathf.Sin(Time.time * 5.0f);
                stateLabel.color = color;

                // 何か押したらスタート
                if (Input.anyKey) GameStateProperty.SetState(GameState.Play);
                break;
            case GameState.Play:
                // デバッグモード解除時の死亡確認
                if (false == SettingInfo.DebugMode)
                {
                    if (0 >= player.lifeReactiveProperty.Value) GameStateProperty.SetState(GameState.GameOver);
                }
                break;
            case GameState.GameOver:
                 break;
        }
    }

    /***** GameController個別処理 ****************************************************/
    // スコア加算
    public void IncreaseScore(int increaseScore)
    {
        // ゲームオーバーでなければ加算
        if (GameState.GameOver != GameStateProperty.GetState())
        {
            // スコアとコンボ追加
            // スコア計算式: 敵の種類に応じた得点 + コンボ数*ボーナス点
            score += (increaseScore + combo * bonusScore);
            scoreLabel.text = "Score : " + score;
            combo++;
            comboLabel.text = "Combo : " + combo;

            // ハイスコア更新
            if (PlayerPrefs.GetInt("HighScore") < score) PlayerPrefs.SetInt("HighScore", score);

            // 音も鳴らす
            //audioSource.PlayOneShot(defeatSE);
        }
    }

    // 一定時間GO表示する
    IEnumerator DisplayGO()
    {
        counter = DisplayGoSec;

        Color color = stateLabel.color;
        color.a = 1.0f;
        stateLabel.color = color;

        stateLabel.gameObject.SetActive(true);
        stateLabel.text = "スタート！";

        while (0 < counter)
        {
            yield return new WaitForSeconds(CountDownSpan);
            counter -= CountDownSpan;
        }

        // 表示完了で消去
        stateLabel.gameObject.SetActive(false);
        stateLabel.text = "";
    }


    /***** 各種状態遷移処理 ****************************************************/
    // State変化時の各処理入り口
    public void ChangeState(GameState state)
    {
        switch (state)
        {
            case GameState.GameOver:
                EntryGameOver();
                break;
            case GameState.Play:
                EntryPlay();
                break;
            case GameState.Ready:
                EntryReady();
                break;
            default:
                break;
        }

        return;
    }
    void EntryReady()
    {
        // ラベルを更新
        scoreLabel.text = "Score : " + 0;
        comboLabel.text = "Combo : " + 0;

        stateLabel.gameObject.SetActive(true);
        stateLabel.text = "よーい";

        // ボタンは非表示
        retryButton.gameObject.SetActive(false);
        titleButton.gameObject.SetActive(false);

        // カーソル非表示
        Cursor.visible = false;

    }

    void EntryPlay()
    {
        generator.GameStart();

        // 音をならす
        audioSource.PlayOneShot(gamestartSE);

        // 一定時間文字表示してから消す
        StartCoroutine(DisplayGO());

        // カーソル非表示
        Cursor.visible = false;

    }

    void EntryGameOver()
    {
        // ラベルを更新
        stateLabel.gameObject.SetActive(true);
        stateLabel.text = "そこまで！";

        // 音をならす
        audioSource.PlayOneShot(gameoverSE);

        // ハイスコアを保存(念のため)
        PlayerPrefs.Save();

        // ボタンを表示する
        retryButton.gameObject.SetActive(true);
        titleButton.gameObject.SetActive(true);

        // カーソルを表示する
        Cursor.visible = true;
    }

    // 監視対象のLife変化時
    public void ChangePlayerLife(int life)
    {
        if (0 >= life)
        {
            if (false == SettingInfo.DebugMode) // デバッグモードだったらそのまま
            {
                // ライフ0になったらGameOver状態へ遷移する
                GameStateProperty.SetState(GameState.GameOver);
            }
        }
        return;
    }

    public void ChangeHouseLife(int life)
    {
        if (0 >= life)
        {
            if (false == SettingInfo.DebugMode) // デバッグモードだったらそのまま
            {
                // ライフ0になったらGameOver状態へ遷移する
                GameStateProperty.SetState(GameState.GameOver);
            }
        }
        return;
    }

}
