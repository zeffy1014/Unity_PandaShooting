using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonOperation : MonoBehaviour
{
    public PlayerController player;

    public bool isOperatePanel = false; // プレイ中の操作ボタンかどうか

    private void Start()
    {
        // プレイ中の操作ボタンだったら移動用タッチ範囲から外す
        if (isOperatePanel) TouchController.SetOperateArea(transform);
    }

    /* ボタン押下に対する各種処理群 */
    // タイトルに戻る
    public void ReturnTitle()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
    }

    // リトライする
    public void RetryGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    // 弾を撃つ
    public void ShotBullet()
    {
        player.ShotBullet();
    }
}
