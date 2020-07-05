using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingInfo : MonoBehaviour
{
    // デバッグモード
    [SerializeField] Toggle debugSwitch;

    static public bool DebugMode { get; private set; } = Application.isEditor;
    public void SetDebug(bool isDebug)
    {
        DebugMode = isDebug;
    }

    void Start()
    {
        // デバッグモードのToggle表示設定
        // ToggleはEditor使用時のみ表示する
        // bool validDebug = Application.isEditor;
        bool validDebug = true; // TODO:しばらくは常にON
        debugSwitch.gameObject.SetActive(validDebug);
        debugSwitch.isOn = validDebug;
        SetDebug(debugSwitch.isOn);

    }


}
