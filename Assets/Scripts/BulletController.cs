using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    static GameObject bulletPrefab = null;  // 弾のPrefab
    static float shotCycle = 0.2f; // 弾の連射間隔
    static float waitShotTime = 0.0f; // 現在の連射待ち時間
    static bool bulletGo = false; // 待たずに発射してよいかどうか

    public float movSpeed;
    public float rotateSpeed;
    public GameObject defeatEffect;

    GameObject gameController;

    static AudioClip shotSE;
    static AudioClip defeatSE;
    static GameObject tempObject = null;
    static AudioSource audioSource = null;

    static GameObject GetObject()
    {
        if (null == tempObject)
        {
            tempObject = new GameObject("Bullet");
            GameObject.DontDestroyOnLoad(tempObject);
            audioSource = tempObject.AddComponent<AudioSource>();
        }

        return tempObject;
    }

    static void LoadResource()
    {
        bulletPrefab = (GameObject)Resources.Load("Prefabs/BulletPrefab");
        shotSE = (AudioClip)Resources.Load("Audio/swish1_1");
        defeatSE = (AudioClip)Resources.Load("Audio/cracker2");

        return;
    }

    static public void ShotBullet(Vector3 pos, Vector3 rot)
    {
        //Debug.Log("BulletNum:" + BulletNum);
        // 連射待ち時間経過しているか、発射OKだったら放つ
        if (waitShotTime >= shotCycle || true == bulletGo)
        {
            // 弾生成
            if (null == bulletPrefab) LoadResource();
            Instantiate<GameObject>(bulletPrefab, pos, Quaternion.Euler(rot));
            // 音も出す
            GetObject().GetComponent<AudioSource>().PlayOneShot(shotSE);

            // 待ち時間クリア
            waitShotTime = 0.0f;
            bulletGo = false;
        }
        else
        {
            // 今回は発射せず待ち時間を増加
            waitShotTime += Time.deltaTime;
        }
    }

    static public void DestroyBullet()
    {
        // 次の弾発射OK
        bulletGo = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        // 開始時にGameControllerをFindしておく
        // これに対してSendMessageするのは直接依存しないで済む？
        gameController = GameObject.FindWithTag("GameController");

    }

    // Update is called once per frame
    void Update()
    {
        // 弾の向きで速度に影響を出す
        transform.Translate(0, (movSpeed + (Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad)*2.0f)) * Time.deltaTime, 0, Space.World);
        //transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ("Enemy" == other.gameObject.tag)
        {
            // エフェクトつける
            Instantiate<GameObject>(defeatEffect, transform.position, Quaternion.identity);

            // スコア加算する(あと音も鳴らす)
            gameController.SendMessage("IncreaseScore", SendMessageOptions.DontRequireReceiver);

            audioSource.PlayOneShot(defeatSE);

            // 次の弾発射OK
            bulletGo = true;

            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
