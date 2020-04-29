using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectDestroyer : MonoBehaviour
{
    // TODO:このあたりは別途整理しなおしたい…
    // どんな
    public bool isDeadLine = false;          // 触れたらゲームオーバー
    public bool isDestroyBulletLine = true;  // 触れたら弾だけ削除
    public bool isDestroyLine = true;        // 触れたらなんでも削除
    public bool isDamageHouseLine = false;   // 触れたら家にダメージ

    GameObject gameController;
    GameObject house;
    GameObject player;

    void Start()
    {
        // 開始時にイベントを飛ばす対象を登録しておく
        gameController = GameObject.FindWithTag("GameController");
        house = GameObject.FindWithTag("House");
        player = GameObject.FindWithTag("Player");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 弾だったら破棄のお知らせ
        if ("Bullet" == other.tag) {
            if (isDestroyBulletLine || isDestroyLine)
            {
                if (BulletKind.Player_Sakana == other.GetComponent<Bullet>().bulletKind)
                {
                    // 魚を失いましたイベント出す
                    ExecuteEvents.Execute<IGameEventReceiver>(
                       target: player,
                       eventData: null,
                       functor: (receiver, eventData) => receiver.OnLostFish()
                   );
                }

                BulletController.DestroyBullet(other.GetComponent<Bullet>().bulletKind);
                Destroy(other.gameObject);
            }
        }
        else {
            if (isDestroyLine)
            {
                Destroy(other.gameObject);

                ExecuteEvents.Execute<IGameEventReceiver>(
                    target: gameController,
                    eventData: null,
                    functor: (receiver, eventData) => receiver.OnBreakCombo()
                );
            }
        }

        // 敵が家ダメージのオブジェクトに触れた場合
        if (isDamageHouseLine)
        {
            if("Enemy" == other.tag)
            {
                // TODO:ダメージはとりあえず300固定、ただ敵に応じて決めたい
                ExecuteEvents.Execute<IGameEventReceiver>(
                    target: house,
                    eventData: null,
                    functor: (receiver, eventData) => receiver.OnDamage(OperationTarget.House, 300)
                );
            }
        }

        // ゲームオーバーのトリガーとなるオブジェクトに触れた場合
        if (isDeadLine)
        {
            ExecuteEvents.Execute<IGameEventReceiver>(
                target: gameController,
                eventData: null,
                functor: (receiver, eventData) => receiver.OnGameOver()
            );
        }
    }
}
