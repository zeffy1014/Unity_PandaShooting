using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float movSpeed;
    public float rotateSpeed;
    public int attack;

    GameObject gameController;

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
        transform.Translate(0, (movSpeed + (Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad) * 2.0f)) * Time.deltaTime, 0, Space.World);
        //transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 自弾が敵にヒット
        if ("Enemy" == other.gameObject.tag && "Bullet" == this.gameObject.tag)
        {
            // ダメージを与える
            other.GetComponent<EnemyController>().OnDamage(attack);

            // 次の弾発射OK
            BulletController.DestroyBullet();

            Destroy(gameObject);
        }
    }
}
