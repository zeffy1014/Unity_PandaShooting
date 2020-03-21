using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : MonoBehaviour
{
    static List<Rect> operateAreaList = new List<Rect>();  // 操作用ボタン範囲リスト

    int touchNum = 0; // 現在タッチされている数
    int moveTouchIndex = -1; // 移動用タッチのIndex
    int moveFingerID = -1;   // 移動用タッチのfingerId

    // 操作用ボタン範囲指定(移動用タッチ範囲外にするため)
    static public void SetOperateArea(Transform button)
    {
        Debug.Log(button.localPosition);

        // 渡されたオブジェクトの範囲を覚えておく
        Rect rect = new Rect();
        rect.xMin = button.localPosition.x + button.GetComponent<RectTransform>().rect.xMin;
        rect.yMin = button.localPosition.y + button.GetComponent<RectTransform>().rect.yMin;
        rect.xMax = button.localPosition.x + button.GetComponent<RectTransform>().rect.xMax;
        rect.yMax = button.localPosition.y + button.GetComponent<RectTransform>().rect.yMax;
        operateAreaList.Add(rect);
    }

    // 操作用ボタン範囲かどうか判定
    bool IsInOperateArea(Vector2  pos)
    {
        bool ret = false;
        foreach (Rect area in operateAreaList)
        {
            if (area.xMin <= pos.x && area.xMax >= pos.x && area.yMin <= pos.y && area.yMax >= pos.y)
            {
                Debug.Log("(" + pos.x + "," + pos.y + "):This position is Operation Area.");
                ret = true;
                break;
            }
        }
        return ret;
    }

    // 毎フレームタッチ情報を取得する
    private void Update()
    {
        touchNum = Input.touchCount;

        // 操作用ボタン範囲内でPhase:Beganの指があったら移動用としてfingerIdとタッチ情報を保持
        // 同時にBeganとなる指があった場合はindex若い方を優先
        // 以降そのfingerIdのタッチを移動用タッチとする
        // 新たなBeganの指があった場合
        //   移動用タッチの指が継続していたら無視
        //   移動用タッチの指が離れていたら(Endもしくはタッチ無し)新たなBeganのタッチを移動用として保持

        // 移動用タッチがすでにある場合はそれを確認
        if (-1 != moveTouchIndex && -1 != moveFingerID)
        {
            Touch touch = Input.GetTouch(moveTouchIndex);
            if (moveFingerID == touch.fingerId)
            {
                switch (touch.phase)
                {
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        // 移動用タッチを維持して終了
                        return;
                    case TouchPhase.Began:
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                    default:
                        // 移動用タッチ情報をクリアして先に進む
                        moveTouchIndex = moveFingerID = -1;
                        break;
                }
            }
            else
            {
                // 移動用タッチ情報をクリアして先に進む
                moveTouchIndex = moveFingerID = -1;
            }
        }

        // ここからは移動用タッチ情報があるか取得する
        for (int i=0; i<touchNum; i++)
        {
            Touch touch = Input.GetTouch(i);
            // タッチ範囲チェック
            if (false == IsInOperateArea(touch.position))
            {
                // タッチ開始だったらそれを移動用タッチとして保持して終了
                if(TouchPhase.Began == touch.phase)
                {
                    moveTouchIndex = i;
                    moveFingerID = touch.fingerId;
                    return;
                }
            }
        }
    }

    /* タッチ操作 */
    public static TouchPhase GetTouchInfo(int index)
    {
        if (PlatformInfo.IsMobile())
        {
            if (Input.GetMouseButtonDown(index)) { return TouchPhase.Began; }
            if (Input.GetMouseButton(index)) { return TouchPhase.Moved; }
            if (Input.GetMouseButtonUp(index)) { return TouchPhase.Ended; }
        }
        else
        {
            if (Input.touchCount > 0)
            {
                return Input.GetTouch(index).phase;
            }
        }
        return TouchPhase.Canceled;
    }
    /*
    /// <summary>
    /// タッチポジションを取得(エディタと実機を考慮)
    /// </summary>
    /// <returns>タッチポジション。タッチされていない場合は (0, 0, 0)</returns>
    public static Vector3 GetTouchPosition()
    {
    if (AppConst.IsEditor)
    {
    TouchInfo touch = AppUtil.GetTouch();
    if (touch != TouchInfo.None) { return Input.mousePosition; }
    }
    else
    {
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);
        TouchPosition.x = touch.position.x;
        TouchPosition.y = touch.position.y;
        return TouchPosition;
    }
    }
        return Vector3.zero;
    }

    /// <summary>
    /// タッチワールドポジションを取得(エディタと実機を考慮)
    /// </summary>
    /// <param name='camera'>カメラ</param>
    /// <returns>タッチワールドポジション。タッチされていない場合は (0, 0, 0)</returns>
    public static Vector3 GetTouchWorldPosition(Camera camera)
    {
        return camera.ScreenToWorldPoint(GetTouchPosition());
    }*/

}

