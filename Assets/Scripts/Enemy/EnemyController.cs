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
    protected float moveSpeed;
    // 旋回速度
    public float rotateSpeedBase;
    protected float rotateSpeed;
    // HP
    public int hpBase;
    protected int hp;

    // 移動できる画面範囲
    protected Rect gameArea;

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
    protected void Start()
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

    }

    void Update()
    {
        // ここは各自独自処理を行う
    }

    /***** Enemy共通処理 ****************************************************/
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
    protected bool Rotate2TargetAngle(float targetAngle, float rotateSpeed)
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
    protected void Rotate2Player(float rotateSpeed)
    {
        Vector2 posDiff = player.transform.position - this.transform.position;
        float targetAngle = Mathf.Atan2(posDiff.y, posDiff.x) * Mathf.Rad2Deg;
        Rotate2TargetAngle(targetAngle, rotateSpeed);

        return;
    }

    // まっすぐ進む
    protected void GoStraight(float speed)
    {
        transform.Translate(new Vector2(Mathf.Cos(GetNowAngle() * Mathf.Deg2Rad), Mathf.Sin(GetNowAngle() * Mathf.Deg2Rad)) * speed * Time.deltaTime, Space.World);
        return;
    }

    // 現在の角度取得(頭が向いている方向)
    protected float GetNowAngle()
    {
        return 90.0f + transform.rotation.eulerAngles.z;
    }

  
}
