using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*** 入退場パターン ***/
public enum EnemyEntranceType
{
    Spot,         // 指定座標に現れる
    From_Top,     // 画面上から入ってくる
    From_Buttom,  // 画面下から入ってくる
    From_Left,    // 画面左から入ってくる
    From_Right,   // 画面右から入ってくる
}
public enum EnemyExitType
{
    Spot,         // その場で消える
    To_Top,       // 画面上へ出ていく
    To_Buttom,    // 画面下へ出ていく
    To_Left,      // 画面左へ出ていく
    To_Right,     // 画面右へ出ていく
}

/*** 敵を生成する際の各種情報 ***/
struct EnemyGenerateInfo
{
    int id;  // ユニークなID

    Vector2 appearPoint;  // 出現位置(画面外からの入場の場合はこの位置目指して入ってくる)

    EnemyEntranceType entranceType;  // 入場パターン
    EnemyExitType exitType;  // 退場パターン

    float activityTime;  // 活動時間(この時間経過で退場開始)

    List<int> preDefeatId;  // 事前に撃破しておく必要がある敵のID(早回し対応など)
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
    public GameObject gameArea;
    Vector2 minPos;
    Vector2 maxPos;
    bool getArea = false;

    // 敵生成シナリオ
    List<EnemyGenerateInfo> generateScenario;

    // ステージ開始からの経過時間
    float elaspedTime = 0.0f;

    EnemyGenerator()
    {
        // とりあえず敵の種類の数だけ配列確保(あとで実際に使用するものだけ適宜Loadする)
        enemyPrefabs = new GameObject[(int)EnemyType.EnemyType_Num];

        // 敵生成シナリオを読み込む
        LoadScenario();

    }

    /***** MonoBehaviourイベント処理 ****************************************************/
    private void Start()
    {
    }

    private void Update()
    {
        elaspedTime += Time.deltaTime;
        
    }

    /***** EnemyGenerator個別処理 ****************************************************/
    public void GameStart()
    {
        elaspedTime = 0.0f;

        Invoke("GenEnemy", 1.0f);
        // InvokeRepeating("GenEnemy", 1, Random.value);
    }

    void GenEnemy()
    {
        // とりあえず決め打ちの範囲でランダム生成
        GameObject enemy = Instantiate(
            GetEnemyPrefab((EnemyType)Random.Range(0, (int)EnemyType.EnemyType_Num)),   // Random.Range(0, 10) can return a value between 0 and 9
            ConvertGenPos(new Vector3(Random.value, Random.Range(1.0f, 1.1f))), 
            Quaternion.identity
        );

        // 次のEnemy生成
        Invoke("GenEnemy", 0.5f + 1.0f * Random.value);
    }

    // 敵生成シナリオ読み込み
    bool LoadScenario()
    {
        // TODO:ファイルから読み込む想定だがまずはベタで記載する

        return true;
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


    // 生成位置を座標変換
    Vector3 ConvertGenPos(Vector3 genPosRate)
    {
        if (!getArea)
        {
            // 生成対象となるエリアの座標を取得(マスク用Sprite Maskの各種設定値から計算…)
            float ppu = gameArea.GetComponent<SpriteMask>().sprite.pixelsPerUnit;
            Rect rect = gameArea.GetComponent<SpriteMask>().sprite.rect;
            Vector2 position = gameArea.transform.position;
            Vector2 scale = gameArea.transform.localScale;

            // 元画像のPixel per unitから表示上サイズを求めてScale加味し、Positionでずらす
            minPos.x = ((rect.width / ppu) * scale.x * (-0.5f)) + position.x;
            minPos.y = ((rect.height / ppu) * scale.y * (-0.5f)) + position.y;
            maxPos.x = ((rect.width / ppu) * scale.x * (0.5f)) + position.x;
            maxPos.y = ((rect.height / ppu) * scale.y * (0.5f)) + position.y;

            Debug.Log("Get Generate Enemy Area!! min:" + minPos + ", max:" + maxPos);
            getArea = true;
        }

        Vector3 retPos;
        retPos.x = (maxPos.x - minPos.x) * genPosRate.x + minPos.x;
        retPos.y = (maxPos.y - minPos.y) * genPosRate.y + minPos.y;
        retPos.z = 0.0f;

        // Debug.Log("ret:" + retPos);
        return retPos;
    }
}
