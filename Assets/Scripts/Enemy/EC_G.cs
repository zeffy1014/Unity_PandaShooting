using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EC_G : EnemyController
{
    // G専用 壁際から回転する際の変数
    bool searchTarget = false;
    float targetAngle;
    struct nearWall
    {
        public bool N;
        public bool E;
        public bool W;
        public bool S;
    }
    nearWall nearWallCheck;

    new void Start()
    {
        // 壁接近状態初期化
        this.nearWallCheck.N = false;
        this.nearWallCheck.E = false;
        this.nearWallCheck.W = false;
        this.nearWallCheck.S = false;

        base.Start();
    }

    override protected void UpdateAction()
    {
        // ゴキブリはランダムに向きを変えて直進　を繰り返す
        // 壁接近情報更新しつつ接近検出したらtrue
        if (false == searchTarget && true == IsApproachingWall(10.0f))
        {
            searchTarget = true;
            // 次に向かう角度(壁に対して離れる方向)を決める
            float freeAngleMax = default;
            float freeAngleMin = default;
            GetFreeAngle(ref freeAngleMax, ref freeAngleMin, 5.0f);

            // 既に壁から離れる方向を向いていたらそのまま(入場時を考慮)
            if (freeAngleMax >= GetNowAngle() && freeAngleMin <= GetNowAngle())
            {
                searchTarget = false;
            }
            else
            {
                // 次に向かう角度(壁に対して離れる方向)を決める
                targetAngle = Random.Range(freeAngleMin, freeAngleMax);
            }
        }
        else if (true == searchTarget)
        {
            // 目標角度に向かって回転
            bool needRotate = Rotate2TargetAngle(this.targetAngle, this.rotateSpeed);
            // もう回転しなくてよければ状態変更
            if (true == needRotate) searchTarget = false;
        }
        else
        {
            // 向いている方向に進む
            GoStraight(this.moveSpeed);
        }

        return;
    }

    /*****G専用内部関数*****/
    // 現在地が壁に接近したか確認・接近情報更新
    // 引数: 壁を0とした場合に画面幅・高さの何%の範囲まで接近していたら検出するか
    protected bool IsApproachingWall(float checkRate)
    {
        bool ret = false;

        // 引数範囲チェック
        if (0 > checkRate || 100 < checkRate)
        {
            // 不正入力
            return ret;
        }

        checkRate /= 100;
        float posX = this.transform.position.x;
        float posY = this.transform.position.y;

        // North接近確認
        if (gameArea.yMax > posY && (gameArea.yMax - gameArea.height * checkRate) <= posY)
        {
            if (!this.nearWallCheck.N)
            {
                this.nearWallCheck.N = true;
                ret = true;
            }
        }
        else if ((gameArea.yMax - gameArea.height * checkRate) > posY) this.nearWallCheck.N = false;

        // East接近確認
        if (gameArea.xMax > posX && (gameArea.xMax - gameArea.width * checkRate) <= posX)
        {
            if (!this.nearWallCheck.E)
            {
                this.nearWallCheck.E = true;
                ret = true;
            }
        }
        else if ((gameArea.xMax - gameArea.width * checkRate) > posX) this.nearWallCheck.E = false;

        // West接近確認
        if (gameArea.xMin < posX && (gameArea.xMin + gameArea.width * checkRate) >= posX)
        {
            if (!this.nearWallCheck.W)
            {
                this.nearWallCheck.W = true;
                ret = true;
            }
        }
        else if ((gameArea.xMin + gameArea.width * checkRate) < posX) this.nearWallCheck.W = false;

        // South接近確認
        if (gameArea.yMin < posY && (gameArea.yMin + gameArea.height * checkRate) >= posY)
        {
            if (!this.nearWallCheck.S)
            {
                this.nearWallCheck.S = true;
                ret = true;
            }
        }
        else if ((gameArea.yMin + gameArea.height * checkRate) < posY) this.nearWallCheck.S = false;

        return ret;
    }

    // 壁から離れる角度を取得
    // 返却用の最大最小角度と、遊びを持たせるために狭める角度(play)
    protected void GetFreeAngle(ref float angleMax, ref float angleMin, float play)
    {
        angleMax = 360.0f;
        angleMin = 0.0f;

        // どの壁に近い状態かで第1～第4象限で進める方向を決める
        bool quadrant1 = true; //0-90deg
        bool quadrant2 = true; //90-180deg
        bool quadrant3 = true; //180-270deg
        bool quadrant4 = true; //270-360deg

        if (this.nearWallCheck.N) quadrant1 = quadrant2 = false;
        if (this.nearWallCheck.E) quadrant1 = quadrant4 = false;
        if (this.nearWallCheck.W) quadrant2 = quadrant3 = false;
        if (this.nearWallCheck.S) quadrant3 = quadrant4 = false;

        // パターンとしては8種類
        if (true == quadrant1)
        {
            if (true == quadrant2)
            {
                angleMin = 0.0f;
                angleMax = 180.0f;
            }
            else if (true == quadrant4)
            {
                angleMin = -90.0f;
                angleMax = 90.0f;
            }
            else
            {
                angleMin = 0.0f;
                angleMax = 90.0f;
            }
        }
        else if (true == quadrant2)
        {
            if (true == quadrant3)
            {
                angleMin = 90.0f;
                angleMax = 270.0f;
            }
            else
            {
                angleMin = 90.0f;
                angleMax = 180.0f;
            }
        }
        else if (true == quadrant3)
        {
            if (true == quadrant4)
            {
                angleMin = 180.0f;
                angleMax = 360.0f;
            }
            else
            {
                angleMin = 180.0f;
                angleMax = 270.0f;
            }
        }
        else if (true == quadrant4)
        {
            angleMin = 270.0f;
            angleMax = 360.0f;
        }
        else
        {
            // 壁から離れるのは上記8パターン以外はない　あとは全方向フリーで
        }

        // 遊びを考慮して範囲を少しせばめる
        angleMin += play;
        angleMax -= play;

        return;
    }


}
