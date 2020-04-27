using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// ボタンの種類
public enum ButtonKind
{
    None,
    Action_Shot,  // 弾を発射
    Action_Slide, // スライドして物を投げる

    System_Retry, // リトライする
    System_Title, // タイトルへ戻る
};


public class ButtonOperation : MonoBehaviour
{
    public PlayerController player;

    public ButtonKind buttonKind;

    public Sprite popImage;
    public Sprite pushImage;

    public bool isOperatePanel = false; // プレイ中の操作ボタンかどうか

    private bool isDown = false; // 押された状態かどうか
    private bool isMobile = false;  // モバイル環境かどうか

    private void Start()
    {
        // プレイ中の操作ボタンだったら
        if (isOperatePanel)
        {
            // 移動用タッチ範囲から外す
            TouchController.SetOperateArea(transform);
            // モバイル環境だけ有効にする
            isMobile = PlatformInfo.IsMobile();
            this.gameObject.SetActive(isMobile);
        }
    }

    private void Update()
    {
        // Unity Remoteは起動直後に接続確認できないので再確認
        if (Application.isEditor && !isMobile && isOperatePanel)
        {
            isMobile = PlatformInfo.IsMobile();
            this.gameObject.SetActive(isMobile);
        }

        // 押されているかどうか
        if (isDown)
        {
            // 押され画像があればそれにする
            if (pushImage) this.GetComponent<Image>().sprite = pushImage;
            if (ButtonKind.Action_Shot == buttonKind)
            {
                // 弾を撃つ
                if (player) player.ShotBullet();
            }
        }
        else
        {
            // 離され画像があればそれにする
            if (popImage) this.GetComponent<Image>().sprite = popImage;
        }

    }

    /* ボタン押下に対する単発の各種処理群 */
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

    /* 各種ボタン操作に対し処理を行いたい場合 */
    // ボタン押し
    public void OnButtonDown()
    {
        isDown = true;

        if (ButtonKind.Action_Shot == buttonKind)
        {
            // 弾を撃つ
            if (player) player.ShotBullet();
        }
    }

    // ボタン離し
    public void OnButtonUp()
    {
        isDown = false;

        if (ButtonKind.Action_Shot == buttonKind)
        {
            // ボタンを離され表示にする
            if (popImage) this.GetComponent<Image>().sprite = popImage;
        }

    }

}
