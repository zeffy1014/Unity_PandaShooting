using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 弾の種類
public enum BulletKind
{
    None,
    Player_Mikan,
    Player_Sakana,
    Player_Kaju,

    Enemy_Point,
    Enemy_Homing,
};

public class BulletController : MonoBehaviour
{
    static GameObject bulletPrefab = null;  // 弾のPrefab

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
        bulletPrefab = (GameObject)Resources.Load("Prefabs/BulletPrefab");
        shotSE = (AudioClip)Resources.Load("Audio/swish1_1");
        return;
    }

    static public void ShotBullet(Vector3 pos, Vector3 rot, BulletKind kind)
    {
        // TODO:弾の種類に応じた処理を行う(今はPlayerのShotのみ)
        //Debug.Log("BulletNum:" + BulletNum);

        // 弾生成
        if (null == bulletPrefab) LoadResource();
        Instantiate<GameObject>(bulletPrefab, pos, Quaternion.Euler(rot));
        // 音も出す
        GetObject().GetComponent<AudioSource>().PlayOneShot(shotSE);

        BulletGo = false;


    }

    static public void DestroyBullet()
    {
        // 次の弾発射OK
        BulletGo = true;
    }

}
