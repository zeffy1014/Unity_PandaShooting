using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class House : MonoBehaviour, IGameEventReceiver
{
    public HouseGauge hpGauge;
    public Text dateTime;

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
    public void OnLostFish() { }

    void Start()
    {
        // 日時Text取得・更新
        // dateTime = this.GetComponentInChildren<Text>();
        // dateTime.text = DateTime.Now.ToString("yyyy-MM-dd ddd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
    }

    void Update()
    {
        // 日時Text更新
        dateTime.text = DateTime.Now.ToString("yyyy-MM-dd ddd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
    }
}
