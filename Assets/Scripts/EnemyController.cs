using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO:敵の種類に応じて継承して挙動を変える

public enum EnemyType
{
    xxx,
    yyy,
    EnemyType_Num
};

public class EnemyController : MonoBehaviour
{
    float fallSpeed;
    int hp;

    public EnemyType enemyType;
    public float fallSpeedBase;
    public int hpBase;

    public Sprite[] enemySprites;
    SpriteRenderer mainSpriteRenderer;

    // 撃破時のエフェクトと音
    public GameObject defeatEffect;
    public AudioClip defeatSE;
    static GameObject tempObject = null;
    static AudioSource audioSource = null;

    static GameObject gameController = null;

    // Start is called before the first frame update
    void Start()
    {
        // 開始時にイベントを飛ばす対象を登録しておく
        if (null == gameController)
        {
            gameController = GameObject.FindWithTag("GameController");
        }

        // 各種初期化
        fallSpeed = 3.0f * Random.value + fallSpeedBase;
        hp = hpBase;

        // 表示画像をランダムで選択(TODO:いずれは生成側で指定)
        mainSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        Sprite sprite = null;
        int index = Random.Range(0, enemySprites.Length);
        sprite = enemySprites[index];
        mainSpriteRenderer.sprite = sprite;
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

        if (0 >= hp)
        {
            // 撃破
            if (null == tempObject)
            {
                tempObject = new GameObject("EnemyController_Temp");
                GameObject.DontDestroyOnLoad(tempObject);
                audioSource = tempObject.AddComponent<AudioSource>();
            }
            audioSource.PlayOneShot(defeatSE);
            Instantiate<GameObject>(defeatEffect, transform.position, Quaternion.identity);

            // 敵を倒した通知
            EventHandlerExtention.SendEvent(new DefeatEnemyEventData(EnemyType.xxx));
            //gameController.SendMessage("IncreaseScore", SendMessageOptions.DontRequireReceiver);

            Destroy(this.gameObject);
        }

    }


}
