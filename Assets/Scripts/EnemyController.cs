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
    public EnemyType enemyType;

    // 移動速度
    public float fallSpeedBase;
    float fallSpeed;

    // HP
    public int hpBase;
    int hp;

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

    // Start is called before the first frame update
    void Start()
    {
        // 各種初期化
        // とりあえず背景スクロールスピードを加算する(移動ゼロの敵は背景と一緒にスクロールする)
        fallSpeed = fallSpeedBase + BGController.scrollSpeed;
        // もともと速度があるものはランダムで上乗せ(1.0-1.5倍)
        fallSpeed += (fallSpeedBase - BGController.scrollSpeed) * Random.Range(0.0f, 0.5f);

        hp = hpBase;

    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0, -fallSpeed * Time.deltaTime, 0, Space.World);

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

}
