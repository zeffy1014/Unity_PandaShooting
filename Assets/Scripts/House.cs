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
    public void OnHouseDamage(int damage)
    {
        hpGauge.OnDamage(damage);
    }

    // 何もしないものたち
    public void OnGameOver() { }
    public void OnBreakCombo() { }
    public void OnDefeatEnemy(EnemyType enemyType) { }
    public void OnShotFish(float lifeTime) { }
    public void OnLostFish() { }

    void Start()
    {
        // イベント受信登録
        EventHandlerExtention.AddListner(this.gameObject, SendEventType.OnHouseDamage);
    }

    void Update()
    {
        // 日時Text更新
        dateTime.text = DateTime.Now.ToString("yyyy-MM-dd ddd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
    }
}
