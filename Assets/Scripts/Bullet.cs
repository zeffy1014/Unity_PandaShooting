using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bullet : MonoBehaviour
{
    // 弾の種類ごとに決まるもの(Prefabとして値を持っておく)
    public BulletKind bulletKind; // 弾の種類
    public float movSpeed; // 弾速
    public float rotateSpeed; // 回転速度
    public int attack;  // 弾の攻撃力

    public bool reflect = false;  // 壁で反射するかどうか

    public float lifeTime = 0.0f; // 弾の存在できる時間(0.0fは無限)
    public float elaspedTime = 0.0f; // 発射からの経過時間

    // 上記に加えてTagもPrefabで設定しておく

    // 発射時に個別に決めるもの
    float movAngle; // 進行方向(X軸に対する角度)

    GameObject player;


    // 進行方向(角度)を設定して放つ
    public void Shot(float deg)
    {
        movAngle = deg;
        float speedX = movSpeed * Mathf.Sin(movAngle * Mathf.Deg2Rad);
        float speedY = movSpeed * Mathf.Cos(movAngle * Mathf.Deg2Rad);
        this.GetComponent<Rigidbody2D>().AddForce(new Vector2(speedX, speedY) * movSpeed);

        this.GetComponent<Rigidbody2D>().AddTorque(rotateSpeed);

    }

    // Start is called before the first frame update
    void Start()
    {
        // 開始時にイベントを飛ばす対象を登録しておく
        player = GameObject.FindWithTag("Player");
        elaspedTime = 0.0f;

        if (BulletKind.Player_Sakana == bulletKind)
        {
            // 魚を放ちましたイベント出す
            EventHandlerExtention.SendEvent(new ShotFishEventData(3.0f));
/*            ExecuteEvents.Execute<IGameEventReceiver>(
               target: player,
               eventData: null,
               functor: (receiver, eventData) => receiver.OnShotFish(this.lifeTime)
           );*/
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (0.0f != lifeTime)
        {
            elaspedTime += Time.deltaTime;
            if (lifeTime <= elaspedTime)
            {
                // 弾の残存時間切れ
                Destroy(gameObject);
                if (BulletKind.Player_Sakana == bulletKind)
                {
                    // 魚を失いましたイベント出す
                    ExecuteEvents.Execute<IGameEventReceiver>(
                       target: player,
                       eventData: null,
                       functor: (receiver, eventData) => receiver.OnLostFish()
                   );
                }
            }
        }
        /*
        // 弾の向きで速度に影響を出す
        float speedX = movSpeed * Mathf.Sin(movAngle * Mathf.Deg2Rad);
        float speedY = (movSpeed + (Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad) * 2.0f)) * Mathf.Cos(movAngle * Mathf.Deg2Rad);
        transform.Translate(speedX * Time.deltaTime, speedY * Time.deltaTime, 0, Space.World);

        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
        */
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 自弾が敵にヒット
        if ("Enemy" == other.gameObject.tag && "Bullet" == this.gameObject.tag)
        {
            // ダメージを与える
            other.GetComponent<EnemyController>().OnDamage(attack);

            if (BulletKind.Player_Mikan == bulletKind)
            {
                // 次の弾発射OK
                BulletController.DestroyBullet(bulletKind);
                Destroy(gameObject);
            }
        }

    }
}
