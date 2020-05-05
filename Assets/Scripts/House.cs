using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class House : MonoBehaviour, IGameEventReceiver
{
    public HouseGauge hpGauge;
    public Text dateTime;

    /***** IGameEventReceiverイベント処理 ****************************************************/
    // ダメージ発生
    public void OnHouseDamage(int damage)
    {
        hpGauge.OnDamage(damage);
    }

    // その他：空実装
    public void OnGameOver() { }
    public void OnBreakCombo() { }
    public void OnDefeatEnemy(EnemyType enemyType) { }
    public void OnShotFish(float lifeTime) { }
    public void OnLostFish() { }


    /***** MonoBehaviourイベント処理 ****************************************************/
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
