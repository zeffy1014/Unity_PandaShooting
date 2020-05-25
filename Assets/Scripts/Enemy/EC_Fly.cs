using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EC_Fly : EnemyController
{
    // 弾関連
    public float shotInterval = 1.0f;  // 発射間隔
    float shotElaspedTime = 0.0f;  // 前回発射からの経過時間

    new void Start()
    {
        base.Start();
    }

    void Update()
    {
        // TODO:ハエはまっすぐ進みながら弾を撃ってくる
        GoStraight(this.moveSpeed);

        return;
    }
}
