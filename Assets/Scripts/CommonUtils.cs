using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

/*** メッセージシステム受信用のInterface ***/
// ゲーム制御
public interface IGameEventReceiver : IEventSystemHandler
{
    void OnGameOver();
    void OnBreakCombo();
    void OnIncreaseScore();
    void OnDamage(OperationTarget target, int damage);
}


/*** 共通enum定義 ***/
// ゲームステート
public enum GameState
{
    None,
    Ready,
    Play,
    GameOver
};

// 処理ターゲット
public enum OperationTarget
{
    None,
    Player,
    House
};


// 動作プラットフォーム判断
public class PlatformInfo
{
    static readonly bool isAndroid = Application.platform == RuntimePlatform.Android;
    static readonly bool isIOS = Application.platform == RuntimePlatform.IPhonePlayer;

    public static bool IsMobile() {
        // AndroidかiOSか、あるいはUnity RemoteだったらMobile扱いとする
    #if UNITY_EDITOR
        bool ret = UnityEditor.EditorApplication.isRemoteConnected;
        // Debug.Log("isRemoteConnected:" + ret);
    #else
        bool ret = isAndroid || isIOS;
    #endif
        return ret;
    }

}

