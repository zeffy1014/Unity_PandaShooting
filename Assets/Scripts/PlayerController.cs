using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Runtime.InteropServices;

// Windowsでマウスカーソルを動かしたい
/* とりあえず使わない
#if UNITY_STANDALONE_WIN
public class Win32API
{
    [DllImport("User32.Dll")]
    public static extern long SetCursorPos(int x, int y);
}
#endif
*/

public class PlayerController : MonoBehaviour
{
    public float distanceToCamera = 10.0f; //カメラとプレイヤーの距離

    public GameObject bullet;
    public GameObject damageEffect;
    public int defaultLife = 3;

    public int Life { get; set; }
    public bool Playing { get; set; } = false;
    public GameState.State State { get; set; } = GameState.State.None;

    public LifePanel lifePanel;

    public AudioClip shotSE;
    public AudioClip damageSE;
    AudioSource audioSource;

    GameObject gameController;

    // 移動範囲制限のための決め打ち画面範囲
    float screenWidth = 5.0f;
    float screenHeight = 10.0f;

    private void Start()
    {
        Life = defaultLife;
        audioSource = GetComponent<AudioSource>();

        gameController = GameObject.FindWithTag("GameController");

    }

    // Update is called once per frame
    void Update()
    {
        // EditorまたはWindowsだったらマウス入力
        // Androidだったらタッチ入力
        #if UNITY_EDITOR
            OnMouseOperation();
        #elif UNITY_STANDALONE_WIN
            OnMouseOperation();
        #elif UNITY_ANDROID
            OnTouchOperation();
        #else
            // Do Nothing
        #endif
    }

    private void OnMouseOperation()
    {
        // マウス位置に従い自機移動
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distanceToCamera));
        Debug.Log(mousePos);

        // 画面からは出さない範囲で
        if (screenWidth / 2 < mousePos.x) mousePos.x = screenWidth / 2;
        if (-screenWidth / 2 > mousePos.x) mousePos.x = -screenWidth / 2;
        if (screenHeight / 2 < mousePos.y) mousePos.y = screenHeight / 2;
        if (-screenHeight / 2 > mousePos.y) mousePos.y = -screenHeight / 2;

        transform.position = mousePos;

        // プレイ中のみの動作
        if (GameState.State.Play == State)
        {
            // マウス左クリックで弾を出す
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 genPos = transform.position;
                genPos.y += 1.0f;
                Vector3 genRot = transform.rotation.eulerAngles;
                genRot.z += 360.0f * Random.value;

                // 弾生成
                Instantiate<GameObject>(bullet, genPos, Quaternion.Euler(genRot));
                // 音も出す
                audioSource.PlayOneShot(shotSE);
            }
        }
    }

    private void OnTouchOperateion()
    {

    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if ("Enemy" == other.gameObject.tag)
        {
            // エフェクトつける
            Instantiate(damageEffect, transform.position, Quaternion.identity);
            // 音も鳴らす
            audioSource.PlayOneShot(damageSE);

            Destroy(other.gameObject);

            Life--;
            lifePanel.UpdateLife(Life);

            // コンボ切れる
            gameController.SendMessage("BreakCombo", SendMessageOptions.DontRequireReceiver);

            if (0 >= Life) gameObject.SetActive(false); // Destroy(gameObject);
        }
    }

    public void ForceGameOver()
    {
        // エフェクトつける
        Instantiate(damageEffect, transform.position, Quaternion.identity);

        // ライフゼロになって強制終了
        Life = 0;
        lifePanel.UpdateLife(Life);
        gameObject.SetActive(false);
    }
}
