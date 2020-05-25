using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EC_Mosquito : EnemyController
{
    new void Start()
    {
        base.Start();
    }

    void Update()
    {
        // 蚊は自機に近づいてくる
        Rotate2Player(this.rotateSpeed);
        GoStraight(this.moveSpeed);
        return;
    }
}
