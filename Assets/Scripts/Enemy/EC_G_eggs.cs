using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EC_G_eggs : EnemyController
{
    new void Start()
    {
        base.Start();
    }

    void Update()
    {
        // TODO:特に何もしない(背景に合わせてスクロールのみ、今はまだ…)
        GoStraight(this.moveSpeed);

        return;
    }
}
