using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // 弾の種類で決まるもの
    public float movSpeed; // 弾速
    public float rotateSpeed; // 回転速度
    public int attack;  // 弾の攻撃力

    float movAngle; // 進行方向(X軸に対する角度)

    GameObject gameController;

    // Bullet生成してから進行方向(角度)を設定
    public void SetAngle(float deg)
    {
        movAngle = deg;
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
        float speedX = movSpeed * Mathf.Sin(movAngle * Mathf.Deg2Rad);
        float speedY = (movSpeed + (Mathf.Cos(transform.rotation.eulerAngles.z * Mathf.Deg2Rad) * 2.0f)) * Mathf.Cos(movAngle * Mathf.Deg2Rad);
        transform.Translate(speedX * Time.deltaTime, speedY * Time.deltaTime, 0, Space.World);

        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
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
