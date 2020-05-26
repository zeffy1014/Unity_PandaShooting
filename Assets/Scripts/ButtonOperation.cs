using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.EventSystems;

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

    // ドラッグ操作検出用
    private Vector2 startDragPoint = Vector2.zero;  // ドラッグ開始地点

    private void Start()
    {
        // プレイ中の操作ボタンだったら
        if (isOperatePanel)
        {
            // 移動用タッチ範囲から外す
            TouchController.SetOperateArea(transform);
        }
    }

    void Update()
    {
        // 押されているかどうか
        if (isDown)
        {
            // 押され画像があればそれにする
            if (pushImage) this.GetComponent<Image>().sprite = pushImage;
            if (ButtonKind.Action_Shot == buttonKind)
            {
                // 弾を撃つ
                if (player) player.ShotBullet(BulletKind.Player_Mikan, player.transform.eulerAngles.z);
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
    public void OnButtonDown(PointerEventData data)
    {
        isDown = true;
        Debug.Log("OnButtonDown, pointerId:" + data.pointerId + ", button:" + this.buttonKind);

        switch (buttonKind)
        {
            case ButtonKind.Action_Shot:
                // 弾を撃つ
                if (player) player.ShotBullet(BulletKind.Player_Mikan);
                break;
            default:
                break;
        }
    }

    // ボタン離し
    public void OnButtonUp(PointerEventData data)
    {
        isDown = false;
        Debug.Log("OnButtonUp, pointerId:" + data.pointerId + ", button:" + this.buttonKind);

        switch (buttonKind)
        {
            case ButtonKind.Action_Shot:
                // ボタンを離され表示にする -> Updateで対応
                // if (popImage) this.GetComponent<Image>().sprite = popImage;
                break;
            case ButtonKind.Action_Slide:
                if (Vector2.zero != startDragPoint)
                {
                    float diffX = data.position.x - startDragPoint.x;
                    float diffY = data.position.y - startDragPoint.y;
                    float shotAngle = Mathf.Atan2(diffY, diffX) * Mathf.Rad2Deg;
                    Debug.Log("shotAngle:" + shotAngle);

                    if (player) player.ShotBullet(BulletKind.Player_Sakana, shotAngle);
                }
                // 初期化
                startDragPoint = Vector2.zero;
                break;
            default:
                break;
        }

    }

    // ドラッグ開始
    public void OnStartDrag(PointerEventData data)
    {
        Debug.Log("OnStartDrag, pointerId:" + data.pointerId + ", button:" + this.buttonKind);
        if (ButtonKind.Action_Slide == buttonKind)
        {
            startDragPoint = data.position;
        }
    }

    // ドラッグ終了
    public void OnEndDrag(PointerEventData data)
    {
        Debug.Log("OnEndDrag, pointerId:" + data.pointerId + ", button:" + this.buttonKind);
    }

}
