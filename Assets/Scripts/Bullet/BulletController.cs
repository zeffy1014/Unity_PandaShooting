using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 弾の種類
public enum BulletKind
{
    Player_Mikan,
    Player_Sakana,
    Player_Kaju,

    Enemy_Point,
    Enemy_Homing,

    BulletKind_Num
};

public class BulletController : MonoBehaviour
{
    static GameObject[] bulletPrefab = null;  // 弾のPrefab
    static string[] bulletPrefabPath = new string[(int)BulletKind.BulletKind_Num]
    {
        "Prefabs/Bullet/BulletPrefab Player_Shot0a",
        "Prefabs/Bullet/BulletPrefab Player_Shot1a",
        "Prefabs/Bullet/BulletPrefab", // ダミー登録
        "Prefabs/Bullet/BulletPrefab Enemy_Shot0",
        "Prefabs/Bullet/BulletPrefab", // ダミー登録
    };

    static public bool BulletGo { get; private set; } = false; // 待たずに発射してよいかどうか

    static AudioClip shotSE;
    static GameObject tempObject = null;
    static AudioSource audioSource = null;

    static GameObject GetObject()
    {
        if (null == tempObject)
        {
            tempObject = new GameObject("BulletController_Temp");
            GameObject.DontDestroyOnLoad(tempObject);
            audioSource = tempObject.AddComponent<AudioSource>();
        }

        return tempObject;
    }

    static void LoadResource()
    {
        bulletPrefab = new GameObject[(int)BulletKind.BulletKind_Num];
        for (int i=0; i<(int)BulletKind.BulletKind_Num; i++)
        {
            bulletPrefab[i] = (GameObject)Resources.Load(bulletPrefabPath[i]);
        }
        shotSE = (AudioClip)Resources.Load("Audio/swish1_1");
        return;
    }

    // 弾を発射
    static public void ShotBullet(Vector3 pos, Vector3 rot, BulletKind kind, float angle, float speed = -1.0f)
    {
        BulletGo = false;

        // TODO:弾の種類に応じた処理を行う(今はPlayerのShotのみ)
        //Debug.Log("BulletNum:" + BulletNum);

        // 弾生成
        if (null == bulletPrefab) LoadResource();
        GameObject bullet = Instantiate<GameObject>(bulletPrefab[(int)kind], pos, Quaternion.Euler(rot));
        if (0 > speed)
        {
            // デフォルト設定の速度で発射
            bullet.GetComponent<Bullet>().Shot(angle);
        }
        else
        {
            // 指定速度で発射
            bullet.GetComponent<Bullet>().Shot(angle, speed);
        }

        // 音も出す
        GetObject().GetComponent<AudioSource>().PlayOneShot(shotSE);

    }

    static public void DestroyBullet(BulletKind kind)
    {
        if (BulletKind.Player_Mikan == kind)
        {
            // 次の弾発射OK
            BulletGo = true;
        }
    }

}
