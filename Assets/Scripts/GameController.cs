using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    const float DisplayGoSec = 1.5f;
    const float CountDownSpan = 0.1f;

    // ゲームステート
    enum State
    {
        Ready,
        Play,
        GameOver
    };

    State state;

    public PlayerController player;
    public EnemyGenerator generator;
    public Text scoreLabel;
    public Text stateLabel;

    int score = 0;
    float counter = DisplayGoSec;

    public AudioClip gamestartSE;
    public AudioClip gameoverSE;
    public AudioClip defeatSE;
    AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        // 開始時はReady状態
        EntryReady();

        audioSource = GetComponent<AudioSource>();
    }

    void LateUpdate()
    {
        // 状態ごとにイベント監視
        switch (state)
        {
            case State.Ready:
                // ラベルを点滅
                Color color = stateLabel.color;
                color.a = Mathf.Sin(Time.time * 5.0f);
                stateLabel.color = color;

                // キーボード押すかタッチしたらゲームスタート
                if ((Input.GetButtonDown("Fire1"))
                    || (Input.GetKeyDown(KeyCode.Space))
                    || (Input.GetKeyDown(KeyCode.LeftArrow))
                    || (Input.GetKeyDown(KeyCode.RightArrow)))
                {
                    EntryPlay();
                }
                break;
            case State.Play:
                // 死亡したらゲームオーバー
                if (0 >= player.Life) EntryGameOver();
                break;
            case State.GameOver:
                // タッチしたらシーン再読み込み
                if ((Input.GetButtonDown("Fire1"))
                    || (Input.GetKeyDown(KeyCode.Space))
                    || (Input.GetKeyDown(KeyCode.LeftArrow))
                    || (Input.GetKeyDown(KeyCode.RightArrow)))
                {
                    ReloadScene();
                }
                break;
        }
    }

    // シーンを再読み込み
    void ReloadScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Sceneひとつだけなので0で
    }

    // スコア加算
    public void IncreaseScore()
    {
        // ゲームオーバーでなければ加算
        if (State.GameOver != state)
        {
            score++;
            scoreLabel.text = "Score : " + score;

            // 音も鳴らす
            audioSource.PlayOneShot(defeatSE);
        }
    }

    // ゲームオーバー…
    void GameIsOver()
    {
        if (State.Play == state) player.ForceGameOver();
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

    /* 各種状態入場処理 */
    void EntryReady()
    {
        state = State.Ready;

        // ラベルを更新
        scoreLabel.text = "Score : " + 0;

        stateLabel.gameObject.SetActive(true);
        stateLabel.text = "よーい";
    }

    void EntryPlay()
    {
        state = State.Play;
        player.Playing = true;
        generator.GameStart();

        // 音をならす
        audioSource.PlayOneShot(gamestartSE);

        // 一定時間文字表示してから消す
        StartCoroutine(DisplayGO());
    }

    void EntryGameOver()
    {
        state = State.GameOver;

        // ラベルを更新
        stateLabel.gameObject.SetActive(true);
        stateLabel.text = "そこまで！";

        // 音をならす
        audioSource.PlayOneShot(gameoverSE);
    }
}
