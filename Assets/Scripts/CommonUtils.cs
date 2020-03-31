using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Windowsでマウスカーソルを動かしたい
/* とりあえず使わない
using System.Runtime.InteropServices;
#if UNITY_STANDALONE_WIN
public class Win32API
{
    [DllImport("User32.Dll")]
    public static extern long SetCursorPos(int x, int y);
}
#endif
*/

public class GameState : MonoBehaviour
{
    // ゲームステート
    public enum State
    {
        None,
        Ready,
        Play,
        GameOver
    };

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
    #else
        bool ret = isAndroid || isIOS;
    #endif
        return ret;
    }

}
