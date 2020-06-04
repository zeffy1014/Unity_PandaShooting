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

    override protected void UpdateAction()
    {
        // ハエはまっすぐ進みながら… 
        GoStraight(moveSpeed);

        // 一定時間経過のたびに発射 -> ちょっとうざいので一旦止める
        shotElaspedTime += Time.deltaTime;
        if (shotInterval < shotElaspedTime)
        {
            //ShotBullet2Player();
            shotElaspedTime = 0.0f;
        }

        return;
    }

    override protected void OnAction()
    {
        // 蠅は状態遷移時に弾を撃ってくる
        state = EnemyState.Action;
        elaspedActionTime = 0.0f;

        // 奇数弾を放つ TODO:難易度によって数を変えたりしたい
        ShotMultipleBullet(3, 20);

        // 少し速度を落とす
        moveSpeed /= 2.0f;

        return;
    }
}