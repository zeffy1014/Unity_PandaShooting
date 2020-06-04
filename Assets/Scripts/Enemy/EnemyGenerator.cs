using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/*** 離脱パターン ***/
public enum EnemyExitType
{
    Spot,         // その場で消える
    To_Top,       // 画面上へ出ていく
    To_Buttom,    // 画面下へ出ていく
    To_Left,      // 画面左へ出ていく
    To_Right,     // 画面右へ出ていく
}

/*** 敵を生成する際の各種情報 ***/
public class EnemyGenerateInfo
{
    // 入力されるべき値の数をとりあえず固定値で持っておく
    static readonly int infoNum = 11;

    // カンマ区切りの文字列を順にメンバに格納する
    public EnemyGenerateInfo(string str)
    {
        string[] input = str.Split(',');
        // まずは入力された情報の数をチェック
        if (infoNum != input.Length)
        {
            Debug.Log("Invalid Input Num...Input:" + input.Length + " != " + infoNum);
            return;
        }
        // 超絶決め打ちで格納していく
        /***
         * 文字列の構成
         * [0]ID(int)
         * [1]何の敵を出すか(EnemyType)
         * [2]生成時間(float)
         * [3][4]生成位置(Vector2)
         * [5][6]出現位置(Vector2)
         * [7]活動時間(float)
         * [8]離脱パターン(EnemyExitType)
         * [9]早回し対象かどうか(bool:0 or 1)
         * [10]早回し対象の場合の生成時間上限(float)
         */
        try
        {
            this.Id = int.Parse(input[0]);
            this.Type = (EnemyType)int.Parse(input[1]);
            this.GenerateTime = float.Parse(input[2]);

            float gx = float.Parse(input[3]);
            float gy = float.Parse(input[4]);
            float ax = float.Parse(input[5]);
            float ay = float.Parse(input[6]);
            this.GeneratePoint = new Vector2(gx, gy);
            this.AppearPoint = new Vector2(ax, ay);

            this.ActivityTime = float.Parse(input[7]);
            this.ExitType = (EnemyExitType)int.Parse(input[8]);
            this.Bonus = bool.Parse(input[9]);
            if (Bonus) this.BonusTime = float.Parse(input[10]);

            // ここまで入力できたら情報セット成功とする
            SuccessInput = true;
        }
        catch (Exception e)
        {
            // 何らか入力不正で例外発生した場合
            Debug.Log(e);
            return;
        }
    }

    // 文字列による情報セットが成功しているか
    public bool SuccessInput { get; private set; } = false;

    // 生成が終わった後の削除用フラグ
    public bool DeleteOK { get; set; } = false;

    /***Enemy側に渡す情報***/
    public int Id { get; private set; }                 // ユニークなID
    public Vector2 AppearPoint { get; private set; }    // 出現位置(画面外からの入場の場合はこの位置目指して入ってくる)
    public float ActivityTime { get; private set; }     // 活動時間(この時間経過で離脱開始)
    public EnemyExitType ExitType { get; private set; } // 離脱パターン
    // public int Option { get; private set; }          // 何種類かパターンがある敵のパターン指定値

    /***Generator側のみで使用する情報***/
    public EnemyType Type { get; private set; }         // 何の敵を出すか
    public float GenerateTime { get; private set; }     // 生成時間
    public Vector2 GeneratePoint { get; private set; }  // 生成位置
    public bool Bonus { get; private set; }             // 早回しボーナス出現かどうか
    public float BonusTime { get; private set; }        // (true==bonusのみ)出現できる時間の上限
}

/*** EnemyGenerator ***/
public class EnemyGenerator : MonoBehaviour
{
    GameObject[] enemyPrefabs;
    static string[] enemyPrefabPath = new string[(int)EnemyType.EnemyType_Num]
    {
        "Prefabs/Enemy/EnemyPrefab Fly",
        "Prefabs/Enemy/EnemyPrefab Mosquito",
        "Prefabs/Enemy/EnemyPrefab G",
        "Prefabs/Enemy/EnemyPrefab G-eggs",
    };

    // 生成エリア指定用
    public GameArea gameArea;
    Vector2 minPos;
    Vector2 maxPos;
    bool getArea = false;

    // 敵生成シナリオ
    List<EnemyGenerateInfo> generateScenario;

    // ステージ開始からの経過時間
    bool countStart = false;
    float elaspedTime = 0.0f;

    // 現在の敵の数(生成時に+1, 撃破/離脱時に-1するだけ)
    int enemyNum = 0;

    /***** MonoBehaviourイベント処理 ****************************************************/
    private void Awake()
    {
        // とりあえず敵の種類の数だけ配列確保(あとで実際に使用するものだけ適宜Loadする)
        enemyPrefabs = new GameObject[(int)EnemyType.EnemyType_Num];

        generateScenario = new List<EnemyGenerateInfo>();

        // 敵生成シナリオを読み込む
        LoadScenario();

        // 経過時間初期化
        countStart = false;
        elaspedTime = 0.0f;
    }

    private void Start()
    {
        // Debug.Log("EnemyGenerator Start");
    }

    private void Update()
    {
        if(countStart) elaspedTime += Time.deltaTime;
        PlayScenario();
        
    }

    /***** EnemyGenerator個別処理 ****************************************************/
    public void GameStart()
    {
        // 経過時間カウント開始(そのうち全体管理情報とするかも)
        countStart = true;
        elaspedTime = 0.0f;

        //Invoke("GenEnemy", 1.0f);
        // InvokeRepeating("GenEnemy", 1, Random.value);

    }

    void GenEnemy()
    {
        // とりあえず決め打ちの範囲でランダム生成
        GameObject enemy = Instantiate(
            GetEnemyPrefab((EnemyType)UnityEngine.Random.Range(0, (int)EnemyType.EnemyType_Num)),   // Random.Range(0, 10) can return a value between 0 and 9
            gameArea.GetPosFromRate(new Vector3(UnityEngine.Random.value, UnityEngine.Random.Range(1.0f, 1.1f))),
            Quaternion.Euler(0.0f, 0.0f, 180.0f)
        );
        enemy.GetComponent<EnemyController>().SetGameArea(gameArea.GetGameAreaRect());

        // 次のEnemy生成
        Invoke("GenEnemy", 0.5f + 1.0f * UnityEngine.Random.value);
    }

    // 敵生成シナリオ読み込み
    bool LoadScenario()
    {
        // TODO:ファイルから読み込む想定だがまずはベタで記載する
        /***
         * 文字列の構成
         * [0]ID(int)
         * [1]何の敵を出すか(EnemyType)
         * [2]生成時間(float)
         * [3][4]生成位置(Vector2)
         * [5][6]出現位置(Vector2)
         * [7]活動時間(float)
         * [8]離脱パターン(EnemyExitType)
         * [9]早回し対象かどうか(bool:0 or 1)
         * [10]早回し対象の場合の生成時間上限(float)
         */
        string[] str = new string[10]
        {
            "1,0,3.0,0.1,0.9,0.8,0.8,3.0,1,False,0.0",
            "2,0,4.0,0.2,0.9,0.85,0.8,3.0,1,False,0.0",
            "3,0,5.0,0.3,0.9,0.9,0.8,3.0,2,False,0.0",
            "4,0,6.0,0.4,0.9,0.5,0.8,3.0,3,False,0.0",
            "5,0,7.0,0.5,0.9,0.6,0.8,3.0,4,False,0.0",
            "6,0,8.0,0.3,0.9,0.7,0.6,3.0,0,False,0.0",
            "7,0,8.0,0.3,0.9,0.75,0.6,3.0,0,False,0.0",
            "8,0,9.0,0.3,0.9,0.80,0.6,3.0,0,False,0.0",
            "9,0,9.0,0.3,0.9,0.85,0.6,3.0,3,False,0.0",
            "10,0,10.0,0.3,0.9,0.90,0.6,3.0,4,False,0.0",
        };
        for (int i=0; i<str.Length; i++)
        {
            SetString2EnemyGenerateInfo(str[i]);
        }

        /*
        // CSVファイルからの読み込み
        StreamReader sReader = new StreamReader(@"xxx.csv");
        // 1行目を読み飛ばす
        sReader.ReadLine();
        // 2行目以降を格納
        while (!sReader.EndOfStream)
        {
            // 1行読んで生成情報に格納・リスト追加
            SetString2EnemyGenerateInfo(sReader.ReadLine());
        }
        */

        return true;
    }

    // 文字列をカンマ区切りでEnemyGenerateInfoに先頭から格納
    bool SetString2EnemyGenerateInfo(string str)
    {
        bool ret = false;

        // 文字列から生成情報を作成
        var genInfo = new EnemyGenerateInfo(str);

        // 正しく情報セットできていたらリストへ追加
        if (genInfo.SuccessInput)
        {
            generateScenario.Add(genInfo);
            ret = true;
        }
        else
        {
            Debug.Log("Invalid Input Info...");
        }

        return ret;
    }

    // 敵生成シナリオ再生
    void PlayScenario()
    {
        // 生成時間に達しているものを一通り抽出して処理する
        // 生成したらシナリオから削除し、敵の数(enemyNum)を+1する(特に現在何のIDの敵がいるかまでは意識しない)
        // 早回し対象の場合は時間上限までenemyNumを監視し0になったら生成する

        int generateNum = 0;  // 最終的に生成した敵の数

        // 生成時間に達しているものを抽出
        var generateList = generateScenario.Where(info => info.GenerateTime <= elaspedTime);
        // 抽出したものを順次処理
        foreach (var generateInfo in generateList)
        {
            // 早回し対象は残存敵が無ければ生成、残っていたら今回はパス
            if (generateInfo.Bonus && 0 < enemyNum)
            {
                // 早回し受付時間に達していたら時間切れ
                if (elaspedTime >= generateInfo.BonusTime)
                {
                    // あとでまとめて削除する対象にする
                    generateInfo.DeleteOK = true;
                }
                continue;
            }

            // Let's生成
            GameObject enemy = Instantiate(
                GetEnemyPrefab(generateInfo.Type),
                gameArea.GetPosFromRate(generateInfo.GeneratePoint),
                Quaternion.Euler(0.0f, 0.0f, 180.0f)  // とりあえず下向きで生成
                );
            enemy.GetComponent<EnemyController>().SetGameArea(gameArea.GetGameAreaRect());
            generateNum++;

            // 各種情報を渡してあげる
            enemy.GetComponent<EnemyController>().SetGenerateInfo(
                id: generateInfo.Id,
                appearPoint: gameArea.GetPosFromRate(generateInfo.AppearPoint),
                activityTime: generateInfo.ActivityTime,
                exitType: generateInfo.ExitType
                );

            // Destroy時に敵の数を減らすようにevent登録
            enemy.GetComponent<EnemyController>().destroyCallback += () => { if (0 < enemyNum) enemyNum--; };

            // あとでまとめて削除する対象にする
            generateInfo.DeleteOK = true;

        }

        // もともとのListから該当する敵の情報を抜く(ID一致する要素を削除)
        generateScenario.RemoveAll(info => true == info.DeleteOK);

        // 今回生成した敵の数を上乗せする
        enemyNum += generateNum;

        return;
    }

    // 敵プレファブを取得(未取得ならロード)
    GameObject GetEnemyPrefab(EnemyType type)
    {
        if (null == enemyPrefabs[(int)type])
        {
            // ロードする
            enemyPrefabs[(int)type] = (GameObject)Resources.Load(enemyPrefabPath[(int)type]);
        }

        return enemyPrefabs[(int)type];
    }


}
