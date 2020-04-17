using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDestryoer : MonoBehaviour
{
    public bool isDeadLine = false;          // 触れたらゲームオーバー
    public bool isDestroyBulletLine = true;  // 触れたら弾だけ削除
    public bool isDestroyLine = true;        // 触れたらなんでも削除

    GameObject gameController;
    GameObject bulletController;

    void Start()
    {
        // 開始時にGameControllerをFindしておく
        // これに対してSendMessageするのは直接依存しないで済む？
        gameController = GameObject.FindWithTag("GameController");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 弾だったら破棄のお知らせ
        if ("Bullet" == other.tag) {
            if (isDestroyBulletLine || isDestroyLine)
            {
                BulletController.DestroyBullet();
                Destroy(other.gameObject);
            }
        }
        else {
            if (isDestroyLine)
            {
                Destroy(other.gameObject);

                // コンボ切れる
                gameController.SendMessage("BreakCombo", SendMessageOptions.DontRequireReceiver);
            }
        }

        // ゲームオーバーのトリガーとなるオブジェクトに触れた場合
        if (isDeadLine)
        {
            // 報告
            gameController.SendMessage("GameIsOver", SendMessageOptions.DontRequireReceiver);
        }
    }
}
