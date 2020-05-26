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
        // ハエはまっすぐ進みながら弾を撃ってくる
        GoStraight(this.moveSpeed);

        // 一定時間経過のたびに発射
        shotElaspedTime += Time.deltaTime;
        if (shotInterval < shotElaspedTime)
        {
            ShotBullet2Player();
            shotElaspedTime = 0.0f;
        }

        return;
    }
}
