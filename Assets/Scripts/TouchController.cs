using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : MonoBehaviour
{
    public static float distanceToCamera = 10.0f; //カメラとプレイヤーの距離

    static List<Rect> operateAreaList = new List<Rect>();  // 操作用ボタン範囲リスト

    int touchNum = 0; // 現在タッチされている数
    int moveTouchIndex = -1; // 移動用タッチのIndex
    int moveFingerID = -1;   // 移動用タッチのfingerId

    // 操作用ボタン範囲指定(移動用タッチ範囲外にするため)
    static public void SetOperateArea(Transform button)
    {
        Debug.Log("SetOperateArea input button pos: " + button.position);
        // 左下
        Vector3 lb = new Vector3(button.position.x + button.GetComponent<RectTransform>().rect.xMin, button.position.y + button.GetComponent<RectTransform>().rect.yMin);
        //Debug.Log("SetOperateArea input button left-bottom: " + lb);
        // 右上
        Vector3 rt = new Vector3(button.position.x + button.GetComponent<RectTransform>().rect.xMax, button.position.y + button.GetComponent<RectTransform>().rect.yMax);
        //Debug.Log("SetOperateArea input button right-top: " + rt);

        // 渡された範囲を保持する
        Rect rect = new Rect(lb.x, lb.y, rt.x - lb.x, rt.y - lb.y);
        operateAreaList.Add(rect);

        Debug.Log("SetOperateArea area: " + rect.position);
    }

    // 操作用ボタン範囲かどうか判定
    bool IsInOperateArea(Vector2  pos)
    {
        //Debug.Log("IsInOperateArea touch pos: " + pos);

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
                        Debug.Log("Finger for move has released.");
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
                    Debug.Log("Here comes a new finger for move! Index:" + i + ", FingerID:" + touch.fingerId );
                    moveTouchIndex = i;
                    moveFingerID = touch.fingerId;
                    return;
                }
            }
            else
            {
                if (Application.isEditor)
                {
                    // タッチ開始でも範囲外だったら保持しない(ログ出力用にEditorの場合だけ一応見ておく)
                    if (TouchPhase.Began == touch.phase)
                        Debug.Log("Touch has began, but out of range...");
                }
            }
        }
    }

    /* 移動用タッチ取得 */
    /* 引数  : [out]移動用タッチ情報を格納(戻り値がtrueの場合のみ使用すること)
    /* 戻り値: タッチ取得できたらtrue, できないならfalse */
    public bool GetMoveTouch(ref Touch retTouch)
    {
        if (-1 != moveTouchIndex)
        {
            // 移動用タッチ情報が取得できる
            retTouch = Input.GetTouch(moveTouchIndex);
            return true;
        }
        else
        {
            // 移動用タッチ情報が無いので無効値などセット
            retTouch.position = Vector2.zero;
            retTouch.phase = TouchPhase.Canceled;
            return false;
        }
    }

}

