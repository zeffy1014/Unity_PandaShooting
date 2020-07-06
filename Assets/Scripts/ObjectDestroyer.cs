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

    void Start()
    {
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ("Bullet" == other.gameObject.tag) {
            // 反射しない弾だったら廃棄
            if ((isDestroyBulletLine || isDestroyLine) && !other.GetComponent<Bullet>().reflect)
            {
                BulletController.DestroyBullet(other.GetComponent<Bullet>().bulletKind);
                Destroy(other.gameObject);
            }
        }
        else {
            if (isDestroyLine)
            {
                Destroy(other.gameObject);
            }
        }

        // 敵が家ダメージのオブジェクトに触れた場合
        if (isDamageHouseLine)
        {
            if("Enemy" == other.gameObject.tag)
            {
                // TODO:ダメージはとりあえず300固定、ただ敵に応じて決めたい
                EventHandlerExtention.SendEvent(new HouseDamageEventData(300));
                EventHandlerExtention.SendEvent(new BreakComboEventData());
            }
        }

        // ゲームオーバーのトリガーとなるオブジェクトに触れた場合
        if (isDeadLine)
        {
            GameStateProperty.SetState(GameState.GameOver);
            Destroy(other.gameObject);
        }
    }
}
