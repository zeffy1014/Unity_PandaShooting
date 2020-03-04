using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDestryoer : MonoBehaviour
{
    public bool isDeadLine = false;

    GameObject gameController;

    void Start()
    {
        // 開始時にGameControllerをFindしておく
        // これに対してSendMessageするのは直接依存しないで済む？
        gameController = GameObject.FindWithTag("GameController");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(other.gameObject);

        // コンボ切れる
        gameController.SendMessage("BreakCombo", SendMessageOptions.DontRequireReceiver);

        // ゲームオーバーのトリガーとなるオブジェクトに触れた場合
        if (isDeadLine)
        {
            // 報告
            gameController.SendMessage("GameIsOver", SendMessageOptions.DontRequireReceiver);
        }
    }
}
