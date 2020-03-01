using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float movSpeed;
    public float rotateSpeed;
    public GameObject defeatEffect;

    public AudioClip defeatSE;
    AudioSource audioSource;

    GameObject gameController;

    // Start is called before the first frame update
    void Start()
    {
        // 開始時にGameControllerをFindしておく
        // これに対してSendMessageするのは直接依存しないで済む？
        gameController = GameObject.FindWithTag("GameController");

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // 魚の向きで速度に影響を出す
        transform.Translate(0, (movSpeed + (Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad)*2.0f)) * Time.deltaTime, 0, Space.World);
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ("Enemy" == other.gameObject.tag)
        {
            // エフェクトつける
            Instantiate<GameObject>(defeatEffect, transform.position, Quaternion.identity);

            // スコア加算する(あと音も鳴らす)
            gameController.SendMessage("IncreaseScore", SendMessageOptions.DontRequireReceiver);

            Destroy(other.gameObject);
            Destroy(gameObject);
        }
    }
}
