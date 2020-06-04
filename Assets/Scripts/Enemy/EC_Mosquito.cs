using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EC_Mosquito : EnemyController
{
    new void Start()
    {
        base.Start();
    }

    override protected void UpdateAction()
    {
        // 蚊は自機に近づいてくる
        Rotate2Player(this.rotateSpeed);
        GoStraight(this.moveSpeed);
        return;
    }
}
