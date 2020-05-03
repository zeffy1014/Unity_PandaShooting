using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*** メッセージシステム受信用のInterface ***/
interface IGameEventReceiver : IEventSystemHandler
{
    void OnGameOver();
    void OnBreakCombo();
    void OnDefeatEnemy(EnemyType enemyType);
    void OnHouseDamage(int damage);
    void OnShotFish(float lifeTime);
    void OnLostFish();
}

/*** イベント種別 ***/
enum SendEventType
{
    OnGameOver,      //ゲームオーバー通知
    OnBreakCombo,    //コンボ切れ通知
    OnDefeatEnemy,   //敵を撃破通知
    OnHouseDamage,   //家にダメージ通知
    OnShotFish,      //魚を放ちました通知
    OnLostFish,      //魚を失いました通知

    EventNum
};

/*** イベント情報 ***/
/*** イベント情報基底クラス ***/
abstract class SendEventDataBase
{
    protected SendEventType type;
    public SendEventDataBase() { }
    public SendEventType GetEventType() { return type; }
}
/*** 各種イベント情報クラス ***/
class GameOverEventData : SendEventDataBase //ゲームオーバー通知
{
    public GameOverEventData() { type = SendEventType.OnGameOver; }
}
class BreakComboEventData : SendEventDataBase //コンボ切れ通知
{
    public BreakComboEventData() { type = SendEventType.OnBreakCombo; }
}
class DefeatEnemyEventData : SendEventDataBase //敵を撃破通知
{
    public EnemyType EnemyType{ get; }
    public DefeatEnemyEventData(EnemyType enemyType)
    {
        type = SendEventType.OnDefeatEnemy;
        EnemyType = enemyType;
    }
}
class HouseDamageEventData : SendEventDataBase //家にダメージ通知
{
    public int Damage { get; }
    public HouseDamageEventData(int damage)
    {
        type = SendEventType.OnHouseDamage;
        Damage = damage;
    }
}
class ShotFishEventData : SendEventDataBase //魚を放ちました通知
{
    public float LifeTime { get; }
    public ShotFishEventData(float lifeTime)
    {
        type = SendEventType.OnShotFish;
        LifeTime = lifeTime;
    }
}
class LostFishEventData : SendEventDataBase //魚を失いました通知
{
    public LostFishEventData(float lifeTime) { type = SendEventType.OnLostFish; }
}


/*** イベント送受信モジュール ***/
class EventHandlerExtention
{
    static List<GameObject>[] listnerList = new List<GameObject>[(int)SendEventType.EventNum];
    static EventHandlerExtention()
    {
        for(int i=0; i< (int)SendEventType.EventNum; i++)
        {
            listnerList[i] = new List<GameObject>();
        }
    }

    public static void AddListner(GameObject go, SendEventType type)
    {
        listnerList[(int)type].Add(go);
    }

    /*** イベント送信 ***/
    public static void SendEvent(SendEventDataBase eventData)
    {
        // イベント種別と対応するListenerを取得
        SendEventType type = eventData.GetEventType();
        if (null == listnerList[(int)type]) return;

        // 各種処理
        void Callback(IGameEventReceiver receiver, BaseEventData data)
        {
            switch (type)
            {
                case SendEventType.OnGameOver:
                    receiver.OnGameOver();
                    break;
                case SendEventType.OnBreakCombo:
                    receiver.OnBreakCombo();
                    break;
                case SendEventType.OnDefeatEnemy:
                    try
                    {
                        // 派生クラスに変換してパラメータを取得
                        DefeatEnemyEventData ev = (DefeatEnemyEventData)eventData;
                        receiver.OnDefeatEnemy(ev.EnemyType);
                    }
                    catch (InvalidCastException)
                    {
                        // イベント種別は送信側で設定しないのでダウンキャスト失敗しない想定だが一応
                        Debug.Log("Invalid Event...");
                    }
                    break;
                case SendEventType.OnHouseDamage:
                    try
                    {
                        // 派生クラスに変換してパラメータを取得
                        HouseDamageEventData ev = (HouseDamageEventData)eventData;
                        receiver.OnHouseDamage(ev.Damage);
                    }
                    catch (InvalidCastException)
                    {
                        // イベント種別は送信側で設定しないのでダウンキャスト失敗しない想定だが一応
                        Debug.Log("Invalid Event...");
                    }
                    break;
                case SendEventType.OnLostFish:
                    receiver.OnLostFish();
                    break;
                case SendEventType.OnShotFish:
                    try
                    {
                        // 派生クラスに変換してパラメータを取得
                        ShotFishEventData ev = (ShotFishEventData)eventData;
                        receiver.OnShotFish(ev.LifeTime);
                    }
                    catch (InvalidCastException)
                    {
                        // イベント種別は送信側で設定しないのでダウンキャスト失敗しない想定だが一応
                        Debug.Log("Invalid Event...");
                    }
                    break;
                default:
                    break;
            }
            return;
        }
 
        foreach (GameObject listner in listnerList[(int)type])
        {
            if (null != listner)
            {
                ExecuteEvents.Execute<IGameEventReceiver>(
                    target: listner,
                    eventData: null,
                    functor: Callback
               );
            }
        }
    }

}
