using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour, IGameEventReceiver
{
    const float DisplayGoSec = 1.5f;
    const float CountDownSpan = 0.1f;

    GameState state = GameState.None;

    public int baseScore;
    public int bonusScore;

    public PlayerController player;
    public EnemyGenerator generator;
    public Text scoreLabel;
    public Text comboLabel;
    public Text stateLabel;
    public Button shotButton;
    public Button slideButton;
    public Button retryButton;
    public Button titleButton;
    public Toggle debugMode;
    public Text posInfo;

    int score = 0;
    int combo = 0;
    float counter = DisplayGoSec;

    public AudioClip gamestartSE;
    public AudioClip gameoverSE;
    public AudioClip defeatSE;
    AudioSource audioSource;

    private bool isMobile = false;  // モバイル環境かどうか

    // Start is called before the first frame update
    void Start()
    {
        // 開始時はReady状態
        EntryReady();

        audioSource = GetComponent<AudioSource>();

        // イベント受信登録
        EventHandlerExtention.AddListner(this.gameObject, SendEventType.OnGameOver);
        EventHandlerExtention.AddListner(this.gameObject, SendEventType.OnBreakCombo);
        EventHandlerExtention.AddListner(this.gameObject, SendEventType.OnDefeatEnemy);

        // ハイスコア無かったら0で作成
        if (!PlayerPrefs.HasKey("HighScore")) PlayerPrefs.SetInt("HighScore", 0);

        // カーソルを出さないようにする
        Cursor.lockState = CursorLockMode.Confined;

        // Debug ModeのToggleはEditor使用時のみ表示する
        //bool validDebug = Application.isEditor;
        bool validDebug = true; // TODO:しばらくは常にON
        debugMode.gameObject.SetActive(validDebug);
        debugMode.GetComponent<Toggle>().isOn = validDebug;

        // Debug用位置情報もDebug Modeの場合のみ表示
        posInfo.gameObject.SetActive(validDebug);

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
        switch (state)
        {
            case GameState.Ready:
                // ラベルを点滅
                Color color = stateLabel.color;
                color.a = Mathf.Sin(Time.time * 5.0f);
                stateLabel.color = color;

                // 何か押したらスタート
                if (Input.anyKey) EntryPlay();
                break;
            case GameState.Play:
                // 死亡したらゲームオーバー(Debug Modeでなければ)
                if (0 >= player.Life && !debugMode.isOn) EntryGameOver();
                break;
            case GameState.GameOver:
                 break;
        }
    }

    // スコア加算
    public void IncreaseScore(int increaseScore)
    {
        // ゲームオーバーでなければ加算
        if (GameState.GameOver != state)
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

    /* 各種Event受信処理 */
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
        if (!debugMode.isOn) if (GameState.Play == state) player.ForceGameOver();
    }

    // スコア加算
    public void OnDefeatEnemy(EnemyType type)
    {
        // TODO:敵の種類からスコアを出す
        int increasScore = 100;
        IncreaseScore(increasScore);
    }
    
    // 何もしない
    public void OnHouseDamage(int damage) { }
    public void OnShotFish(float lifeTime) { }
    public void OnLostFish() { }

    /* 各種状態入場処理 */
    void EntryReady()
    {
        state = GameState.Ready;
        player.State = GameState.Ready;

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
        state = GameState.Play;
        player.State = GameState.Play;
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
        state = GameState.GameOver;
        player.State = GameState.GameOver;

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
}
