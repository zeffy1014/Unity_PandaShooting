using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameArea : MonoBehaviour
{
    bool getArea = false;  // ゲームエリア取得済みかどうか
    Rect gameArea;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // ゲームエリア取得
    void GetArea()
    {
        if (!getArea)
        {
            // 生成対象となるエリアの座標を取得(マスク用Sprite Maskの各種設定値から計算…)
            float ppu = this.GetComponent<SpriteMask>().sprite.pixelsPerUnit;
            Rect rect = this.GetComponent<SpriteMask>().sprite.rect;
            Vector2 position = this.transform.position;
            Vector2 scale = this.transform.localScale;

            // 元画像のPixel per unitから表示上サイズを求めてScale加味し、Positionでずらす
            gameArea.xMin = ((rect.width / ppu) * scale.x * (-0.5f)) + position.x;
            gameArea.yMin = ((rect.height / ppu) * scale.y * (-0.5f)) + position.y;
            gameArea.xMax = ((rect.width / ppu) * scale.x * (0.5f)) + position.x;
            gameArea.yMax = ((rect.height / ppu) * scale.y * (0.5f)) + position.y;

            Debug.Log("Get GameArea!! min:" + gameArea.min + ", max:" + gameArea.max);
            getArea = true;
        }
    }

    // 縦横の割合指定で座標取得
    public Vector3 GetPosFromRate(Vector3 posRate)
    {
        GetArea();

        /*
        if (0.0f > posRate.x || 1.0f < posRate.x || 0.0f > posRate.y || 1.0f < posRate.y)
        {
            // 範囲指定エラー
            Debug.Log("Input rate out of range...");
            return Vector3.zero;
        }
        */

        Vector3 retPos;
        retPos.x = (gameArea.xMax - gameArea.xMin) * posRate.x + gameArea.xMin;
        retPos.y = (gameArea.yMax - gameArea.yMin) * posRate.y + gameArea.yMin;
        retPos.z = 0.0f;

        // Debug.Log("ret:" + retPos);
        return retPos;
    }

}
