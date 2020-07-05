using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

using UnityEngine.EventSystems;

/*** 共通enum定義 ***/
// ゲームステート
public enum GameState
{
    None,
    Ready,
    Play,
    GameOver
};

public class GameStateProperty
{
    static public ReactiveProperty<GameState> valueReactiveProperty = new ReactiveProperty<GameState>(GameState.None);

    // 値を設定する
    static public void SetState(GameState state)
    {
        valueReactiveProperty.Value = state;
    }

    // 値を取得する
    static public GameState GetState()
    {
        return valueReactiveProperty.Value;
    }

}

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

