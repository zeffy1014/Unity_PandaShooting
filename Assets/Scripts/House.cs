using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour, IGameEventReceiver
{
    public HouseGauge hpGauge;

    /* Event受信処理 */
    // ダメージ発生
    public void OnDamage(OperationTarget target, int damage)
    {
        if (OperationTarget.House == target) hpGauge.OnDamage(damage);
    }

    // 何もしないものたち
    public void OnGameOver() { }
    public void OnBreakCombo() { }
    public void OnIncreaseScore() { }

    void Start()
    {
    }

    void Update()
    {
    }
}
